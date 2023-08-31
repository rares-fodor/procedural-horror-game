using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class EnemyBehavior : MonoBehaviour
{
    private enum EnemyState { Despawned, Despawning, Inactive, Patrolling, Chasing, Evade, Stopped }
    private EnemyState enemyState = EnemyState.Despawned;

    private NavMeshAgent enemyAgent;

    // Game progress counter
    private int gameProgress;

    // References to points of progress
    private List<GameObject> stones;
    
    // Last spawn target
    private Vector3 spawnPoint;

    // Default agent speed (as set in editor)
    private float agentSpeed;

    [SerializeField] private GameObject player;
    [SerializeField] private float patrolDetectionRange = 10;
    [SerializeField] private float chaseDetectionRange = 10;
    [SerializeField] private float evadeRange = 10;
    [SerializeField] private float evadeSpeedModifier = 0.95f;
    [SerializeField] private float spawnDistance = 40;
    [SerializeField] private float huntEvadeRangeModifier = 15;
    [SerializeField] private float huntDuration = 10f;
    [SerializeField] private float huntChance = 0.3f;


    // Spawn range
    private float spawnRadius = 15f;

    // Set detectionRange to either patrolDetectionRange or chaseDetectionRange
    private float detectionRange;

    // Hunt attempt timers (how long until the game starts trying to start hunts again)
    private float succesfulHuntAttemptCooldown = 60f;
    private float unsuccesfulHuntAttemptCooldown = 10f;

    // Set to true to allow hunting
    private bool randomHuntsEnabled = false;

    // Set to false after progressing to hunt stage
    private bool shouldDespawn = true;

    // Set to true if the game progressed while the enemy is chasing
    private bool progressedInChase = false;

    private void Awake()
    {
        enemyAgent = gameObject.GetComponent<NavMeshAgent>();
        gameProgress = 0;
        agentSpeed = enemyAgent.speed;
        GameController.GameProgressedEvent.AddListener(OnGameProgressed);
        GameController.StoneLocationChangedEvent.AddListener(OnStoneLocationsChanged);
    }

    private void OnDestroy()
    {
        GameController.GameProgressedEvent.RemoveListener(OnGameProgressed);
        GameController.StoneLocationChangedEvent.RemoveListener(OnStoneLocationsChanged);
    }

    /// <summary>
    /// Tests whether the player has entered the enemy's trigger collider.
    /// Invokes a game event to signal the player's death if necessary.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player killed!");
            GameController.PlayerDied.Invoke();
        }
    }

    /// <summary>
    /// Handles behavior depending on state.
    /// </summary>
    private void Update()
    {
        if (enemyAgent.enabled == true)
        {
            CheckPlayerInRange();
        }

        if (randomHuntsEnabled && (enemyState == EnemyState.Despawned || enemyState == EnemyState.Patrolling))
        {
            AttemptHunt();
        }

        switch (enemyState)
        {
            case EnemyState.Despawned:
                break;
            case EnemyState.Despawning:
                break;
            case EnemyState.Inactive:
                break;
            case EnemyState.Patrolling:
                NextPatrolDestinationIfStopped();
                break;
            case EnemyState.Chasing:
                if (!CheckEvaded())
                {
                    ChasePlayer();
                }
                break;
            case EnemyState.Evade:
                LoseInterest();
                break;
        }
    }

    /// <summary>
    /// Handles game progression logic, is invoked every time the player collects a stone
    /// </summary>
    private void OnGameProgressed()
    {
        Debug.Log("Game progressed");
        gameProgress++;

        // Remove closest stone (it was collected since the game progressed)
        stones.Remove(GetClosestStone(player.transform.position));

        // Begin enemy activity
        if (gameProgress == 1)
        {
            ChangeState(EnemyState.Patrolling);
            SpawnAtClosestStone();
        }

        // Move towards next stones
        else if (gameProgress >= 2 && gameProgress < 4)
        {
            // If enemy is chasing wait for succesful evade to warp to next stone for patrol
            if (enemyState != EnemyState.Patrolling)
            {
                progressedInChase = true;
            } else
            {
                SpawnAtClosestStone();
            }
        }
        
        // Enable random chance chases
        else if (gameProgress >= 4 && gameProgress < 6)
        {
            randomHuntsEnabled = true;
            shouldDespawn = false;
        }
        
        // Guarantee a permanent chase unless player is blessed by the mage
        else if (gameProgress >= 6)
        {
            huntChance = 1.0f;
            huntDuration = 5000f;
        }
    }

    /// <summary>
    /// Updates a local copy of the list of references to points of progress.
    /// Gets invoked after the level generator is done placing them on the map
    /// </summary>
    /// <param name="stones">The game's units of progress (player finds and collects them)</param>
    private void OnStoneLocationsChanged(List<GameObject> stones)
    {
        this.stones = stones;
    }

    /// <summary>
    /// State transition wrapper
    /// </summary>
    /// <param name="state">State to transition to</param>
    private void ChangeState(EnemyState state)
    {
        Debug.Log(state);

        switch (state)
        {
            case EnemyState.Inactive:
                GoToInactive();
                break;
            case EnemyState.Patrolling:
                GoToPatrolling();
                break;
            case EnemyState.Despawned:
                GoToDespawn();
                break;
            case EnemyState.Despawning:
                GoToDespawning();
                break;
            case EnemyState.Chasing:
                GoToChase();
                break;
            case EnemyState.Evade:
                GoToEvade();
                break;
        }
    }

    // The following methods adjust the enemy's properties according to the
    // state it is about to transition into, and then finalize the transition.

    /// <summary>
    /// Ensures movement speed is reset, lowers the detection range
    /// and tranitions to patrolling.
    /// </summary>
    private void GoToPatrolling()
    {
        enemyAgent.speed = agentSpeed;
        detectionRange = patrolDetectionRange;
        enemyState = EnemyState.Patrolling;
    }


    /// <summary>
    /// Disables the agent to prevent unwanted repositioning from the navigation
    /// system. Moves the enemy to an unseen position and transitions to despawn.
    /// </summary>
    private void GoToDespawn()
    {
        enemyAgent.enabled = false;

        Vector3 despawnPos = gameObject.transform.position;
        despawnPos.y = 500;
        gameObject.transform.position = despawnPos;

        enemyState = EnemyState.Despawned;
    }

    /// <summary>
    /// Lowers the detection range and transitions to despawning.
    /// </summary>
    /// <remarks>
    /// The detection range is lowered to give the impression
    /// that the enemy is losing track of the player
    /// </remarks>
    private void GoToDespawning()
    {
        detectionRange = patrolDetectionRange;
        enemyState = EnemyState.Despawning;
    }

    /// <summary>
    /// Interrupts the despawn coroutine if needed, enables the agent,
    /// increases the detection range, resets the speed and transitions
    /// to chasing.
    /// </summary>
    /// <remarks>
    /// If the enemy is in the process of despawning the coroutine should be
    /// stopped to avoid the enemy despawning while chasing.
    /// </remarks>
    private void GoToChase()
    {
        if (enemyState == EnemyState.Despawning)
        {
            // Cancel despawn behavior
            Debug.Log("Canceling despawn!");
            StopCoroutine("StopAndDespawn");
            enemyAgent.isStopped = false;
        }
        enemyAgent.enabled = true;
        detectionRange = chaseDetectionRange;
        enemyAgent.speed = agentSpeed;
        enemyState = EnemyState.Chasing;
    }


    /// <summary>
    /// Lowers enemy speed and transitions to evade.
    /// </summary>
    /// <remarks>
    /// Lowers the enemy speed to give the impression that the enemy is looking
    /// around for the player. Gives the player time to evade.
    /// </remarks>
    private void GoToEvade()
    {
        // Lower speed (pretend to lose interest in player)
        enemyAgent.speed = agentSpeed * evadeSpeedModifier;
        enemyState = EnemyState.Evade;
    }

    /// <summary>
    /// Transitions to inactive
    /// </summary>
    private void GoToInactive()
    {
        enemyState = EnemyState.Inactive;
    }

    /// <summary>
    /// Wait until the enemy has despawned and then warp it to the closest
    /// point of progress.
    /// </summary>
    /// <remarks>
    /// Should be called only if the game progresses while the enemy is chasing
    /// </remarks>
    private IEnumerator WaitAndWarp()
    {
        while (true)
        {
            if (enemyState == EnemyState.Despawned)
                break;
        }

        SpawnAtClosestStone();
        yield return null;
    }

    /// <summary>
    /// Disable hunting for the given <paramref name="cooldown"/> time.
    /// </summary>
    /// <param name="cooldown">Time to wait until enabling hunts again.</param>
    private IEnumerator HuntCooldown(float cooldown)
    {
        randomHuntsEnabled = false;
        yield return new WaitForSeconds(cooldown);
        randomHuntsEnabled = true;
    }

    /// <summary>
    /// Stops the agent at it's last destination for a few seconds. If the
    /// enemy should despawn it picks a new destinations and despawns after
    /// a few more seconds. Otherwise it transitions back to patrol.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ComeOutOfChase()
    {
        enemyAgent.speed = agentSpeed;

        enemyAgent.isStopped = true;
        yield return new WaitForSeconds(4);
        enemyAgent.isStopped = false;

        if (shouldDespawn)
        {
            // Move towards a point as despawn happens
            NextPatrolDestinationIfStopped();

            ChangeState(EnemyState.Despawning);
            yield return new WaitForSeconds(4);
            ChangeState(EnemyState.Despawned);

            // Account for game progressing while the enemy was chasing
            // this handles the case where the player collects a stone while
            // being chased. The next closest stone should still
            // have the enemy patrolling it.
            if (progressedInChase)
            {
                SpawnAtClosestStone();
                progressedInChase = false;
            }
        }
        else
        {
            spawnPoint = enemyAgent.transform.position;
            ChangeState(EnemyState.Patrolling);
        }
    }

    /// <summary>
    /// Greatly increases the evade range and gradually lowers it back to normal across
    /// the hunt duration time. Quits if player leaves the evasion range.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChaseWhileShrinkingRange()
    {
        float timer = 0f;
        float huntEvadeRange = evadeRange * huntEvadeRangeModifier;
        float startingEvadeRange = evadeRange;

        while (timer < huntDuration)
        {
            evadeRange = Mathf.Lerp(huntEvadeRange, startingEvadeRange, timer / huntDuration);
            timer += Time.deltaTime;

            if (CheckEvaded())
                break;

            yield return null;
        }

        // Reset evade range
        evadeRange = startingEvadeRange;
    }

    /// <summary>
    /// Attempt to begin a hunt and calls the apropriate cooldown timer given the outcome
    /// </summary>
    private void AttemptHunt()
    {
        if (Random.value <= huntChance)
        {
            Debug.Log("Hunt attempt SUCCESFUL!");
            StartHunt();
            StartCoroutine(HuntCooldown(succesfulHuntAttemptCooldown));
        } else
        {
            Debug.Log("Hunt attempt failed! Retrying in a few seconds!");
            StartCoroutine(HuntCooldown(unsuccesfulHuntAttemptCooldown));
        }
    }

    /// <summary>
    /// Enable the NavMeshAgent and warp to a random point around the next closest
    /// point of progress to the player.
    /// </summary>
    private void SpawnAtClosestStone()
    {
        if (enemyAgent != null)
        {
            GameObject closestStone = GetClosestStone(player.transform.position);
            if (closestStone != null)
            {
                Vector3 target = GetRandomPoint(closestStone.transform.position);
                Debug.Log($"Warping to {target} close to stone at {closestStone.transform.position}");

                // Set spawn point
                spawnPoint = target;

                enemyAgent.Warp(target);
                enemyAgent.enabled = true;

                ChangeState(EnemyState.Patrolling);
            }
        }
    }

    /// <summary>
    /// Selects a new random patrol point if the agent arrived at its destination.
    /// </summary>
    private void NextPatrolDestinationIfStopped()
    {
        if (enemyAgent.remainingDistance <= enemyAgent.stoppingDistance)
        {
            Vector3 target = GetRandomPoint(spawnPoint);

            Debug.DrawRay(target, Vector3.up, Color.blue, 1.0f);
            enemyAgent.SetDestination(target);
        }
    }

    /// <summary>
    /// Verifies whether the player is within the detection range
    /// </summary>
    private void CheckPlayerInRange()
    {
        if (DistanceToPlayer() <= detectionRange && enemyState != EnemyState.Chasing)
        {
            ChangeState(EnemyState.Chasing);
            ChasePlayer();
        }
    }
    
    /// <summary>
    /// Update agent destination to the player position
    /// </summary>
    private void ChasePlayer()
    {
        enemyAgent.SetDestination(player.transform.position);
    }

    /// <summary>
    /// Verifies whether the player has moved farther than the evade range
    /// and transitions to the evade state.
    /// </summary>
    private bool CheckEvaded()
    {
        if (DistanceToPlayer() >= evadeRange)
        {
            ChangeState(EnemyState.Evade);
            return true;
        }
        return false;
    }

    /// <summary>
    /// If the enemy has arrived at the last point where the player was
    /// within the evade range, it transitions out of the evade state and
    /// into the inactive state in preparation for the despawn/return to 
    /// patrol behavior.
    /// </summary>
    private void LoseInterest()
    {
        if (enemyAgent.remainingDistance <= enemyAgent.stoppingDistance)
        {
            Debug.Log("Losing interest!");
            ChangeState(EnemyState.Inactive);
            StartCoroutine("ComeOutOfChase");
        }
    }
    
    /// <summary>
    /// Starts a hunt by warping the agent behind the 
    /// camera and forcing a chase.
    /// </summary>
    private void StartHunt()
    {
        SpawnBehindCamera();

        ChangeState(EnemyState.Chasing);
        
        // Start lowering range until player evades
        StartCoroutine("ChaseWhileShrinkingRange");
    }
    
    /// <summary>
    /// Warp the agent at a random point in an area with a given radius
    /// and the center at least the given distance behind the camera.
    /// </summary>
    /// <remarks>
    /// Spawns the enemy off-screen.
    /// </remarks>
    private void SpawnBehindCamera()
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 origin = cameraTransform.position - cameraTransform.forward * spawnDistance;
        Vector3 target = GetRandomPoint(origin);

        enemyAgent.Warp(target);
    }

    /// <summary>
    /// Warps the agent at a random point on a disc
    /// with the given radius around <paramref name="origin"/>
    /// </summary>
    /// <param name="origin"></param>
    /// <returns></returns>
    private Vector3 GetRandomPoint(Vector3 origin)
    {
        // Get a random point in an area around the origin
        Vector2 spawnOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 target = new Vector3(origin.x + spawnOffset.x, 0, origin.z + spawnOffset.y);
        
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas))
            return hit.position;

        return Vector3.zero;
    }

    /// <summary>
    /// Computes the distance from the enemy to player
    /// </summary>
    /// <returns>The distance to the player</returns>
    private float DistanceToPlayer()
    {
        return Vector3.Distance(player.transform.position, gameObject.transform.position);
    }

    /// <summary>
    /// Returns a reference to the closest point of progress to the player
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private GameObject GetClosestStone(Vector3 target)
    {
        if (stones != null && stones.Count > 0)
        {
            GameObject closest = stones[0];
            float leastDistance = Vector3.Distance(stones[0].transform.position, target);

            for (int i = 1; i < stones.Count; i++)
            {
                float currentDistance = Vector3.Distance(stones[i].transform.position, target);
                if (currentDistance < leastDistance)
                {
                    leastDistance = currentDistance;
                    closest = stones[i];
                }
            }

            return closest;
        }
        return null;
    }
}
