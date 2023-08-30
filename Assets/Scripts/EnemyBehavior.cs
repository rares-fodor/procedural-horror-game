using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    private enum EnemyState { Despawned, Despawning, Inactive, Patrolling, Hunting, Aggressive, Chasing, Evade }
    private EnemyState enemyState = EnemyState.Despawned;

    private int gameProgress;
    private List<GameObject> stones;

    private NavMeshAgent enemyAgent;

    [SerializeField] private GameObject player;
    [SerializeField] private float patrolDetectionRange = 10;
    [SerializeField] private float chaseDetectionRange = 10;
    [SerializeField] private float evadeRange = 10;
    [SerializeField] private float spawnDistance = 40;
    [SerializeField] private float chaseDetectionModifier = 5;
    [SerializeField] private float huntEvadeModifier = 15;


    // Is true when the despawn timer begins counting down
    // will be invalidated by the player beginning a chase during the despawn
    private bool isDespawning;

    // Default agent speed (as set in editor)
    private float agentSpeed;

    // Spawn range
    private float spawnRadius = 15f;

    // Last spawn/teleport target
    private Vector3 spawnPoint;

    // Set to true to allow hunting
    private bool randomHuntsEnabled = false;

    // Hunt duration
    private float huntLength = 10;

    private float detectionRange;


    private void Awake()
    {
        enemyAgent = gameObject.GetComponent<NavMeshAgent>();
        gameProgress = 0;
        agentSpeed = enemyAgent.speed;
    }

    private void Start()
    {
        GameController.GameProgressedEvent.AddListener(OnGameProgressed);
        GameController.StoneLocationChangedEvent.AddListener(OnStoneLocationsChanged);
    }


    private void OnDestroy()
    {
        GameController.GameProgressedEvent.RemoveListener(OnGameProgressed);
    }

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
            case EnemyState.Hunting:
                break;
            case EnemyState.Aggressive:
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
    private void OnGameProgressed()
    {
        Debug.Log("Game progressed");
        gameProgress++;

        // Remove closest stone (it was collected since the game progressed)
        stones.Remove(GetClosestStone(player.transform.position));

        if (gameProgress >= 1 && gameProgress < 4)
        {
            ChangeState(EnemyState.Patrolling);
            SpawnAtClosestStone();
        }
        else if (gameProgress >= 4 && gameProgress < 6)
        {
            // Enable random chance chases
            randomHuntsEnabled = true;
        }
        else if (gameProgress >= 6)
        {
            enemyState = EnemyState.Aggressive;
            Debug.Log("Aggressive");
        }
    }

    private void OnStoneLocationsChanged(List<GameObject> stones)
    {
        this.stones = stones;
    }

    private void AttemptHunt()
    {
        if (Random.value <= 0.3)
        {
            Debug.Log("Hunt attempt SUCCESFUL!");
            StartHunt();
        } else
        {
            Debug.Log("Hunt attempt failed! Retrying in a few seconds!");
            StartCoroutine(RetryTimer());
        }
    }

    private IEnumerator RetryTimer()
    {
        ChangeState(EnemyState.Inactive);
        yield return new WaitForSeconds(2);
        ChangeState(EnemyState.Despawned);
    }

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

                enemyAgent.enabled = true;
                enemyAgent.Warp(target);
            }
        }
    }

    private void NextPatrolDestinationIfStopped()
    {
        if (enemyAgent.remainingDistance <= enemyAgent.stoppingDistance)
        {
            Vector3 target = GetRandomPoint(spawnPoint);

            Debug.DrawRay(target, Vector3.up, Color.blue, 1.0f);
            enemyAgent.SetDestination(target);
        }
    }

    private void CheckPlayerInRange()
    {
        // Verify whether the player is within the detection range and potentially initiate chase
        if (DistanceToPlayer() <= detectionRange && enemyState != EnemyState.Chasing)
        {
            ChangeState(EnemyState.Chasing);
            ChasePlayer();
        }
    }

    private void ChasePlayer()
    {
        enemyAgent.SetDestination(player.transform.position);
    }

    private bool CheckEvaded()
    {
        if (DistanceToPlayer() >= evadeRange)
        {
            ChangeState(EnemyState.Evade);
            return true;
        }
        return false;
    }

    private void LoseInterest()
    {
        if (enemyAgent.remainingDistance <= enemyAgent.stoppingDistance)
        {
            Debug.Log("Losing interest!");

            // While this state is active the coroutine is also still running and
            // can be safely interrupted by a chase
            ChangeState(EnemyState.Despawning);
            StartCoroutine("StopAndDespawn");
        }
    }

    private IEnumerator StopAndDespawn()
    {
        // Reset speed
        enemyAgent.speed = agentSpeed;
        
        enemyAgent.isStopped = true;
        yield return new WaitForSeconds(4);
        enemyAgent.isStopped = false;

        // Move towards a point as despawn happens
        NextPatrolDestinationIfStopped();

        // Wait before despawning
        yield return new WaitForSeconds(4);
        ChangeState(EnemyState.Despawned);
    }

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
            case EnemyState.Hunting:
                StartHunt();
                break;
        }
    }

    private void GoToPatrolling()
    {
        enemyAgent.speed = agentSpeed;
        detectionRange = patrolDetectionRange;
        enemyState = EnemyState.Patrolling;
    }

    private void GoToDespawn()
    {
        // Move enemy at an unreachable and invisible position
        // while it is despawned
        Vector3 despawnPos = gameObject.transform.position;
        despawnPos.y = 500;
        gameObject.transform.position = despawnPos;

        enemyState = EnemyState.Despawned;
    }

    private void GoToDespawning()
    {
        detectionRange = patrolDetectionRange;
        enemyState = EnemyState.Despawning;
    }

    private void GoToChase()
    {
        if (enemyState == EnemyState.Despawning)
        {
            // Cancel despawn behavior
            Debug.Log("Canceling despawn!");
            StopCoroutine("StopAndDespawn");
            enemyAgent.isStopped = false;
        }
        detectionRange = chaseDetectionRange;
        enemyAgent.speed = agentSpeed;
        enemyState = EnemyState.Chasing;
    }

    private void GoToEvade()
    {
        // Lower speed (pretend to lose interest in player)
        enemyAgent.speed = agentSpeed * 0.95f;
        enemyState = EnemyState.Evade;
    }

    private void GoToInactive()
    {
        enemyState = EnemyState.Inactive;
    }

    private void StartHunt()
    {
        SpawnBehindCamera();

        ChangeState(EnemyState.Chasing);
        
        // Start lowering range until player evades
        StartCoroutine("ChaseWhileShrinkingRange");
    }

    private IEnumerator ChaseWhileShrinkingRange()
    {
        float timer = 0f;
        float startingEvadeRange = evadeRange * huntEvadeModifier;

        while (timer < huntLength)
        {
            evadeRange = Mathf.Lerp(startingEvadeRange, evadeRange, timer / huntLength);
            timer += Time.deltaTime;

            if (CheckEvaded())
                break;

            yield return null;
        }
    }

    private void SpawnBehindCamera()
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 target = cameraTransform.position - cameraTransform.forward * spawnDistance;
        Vector2 offset = Random.insideUnitCircle * spawnRadius;

        enemyAgent.Warp(new Vector3(target.x + offset.x, 0.5f, target.z + offset.y));
    }

    private float DistanceToPlayer()
    {
        return Vector3.Distance(player.transform.position, gameObject.transform.position);
    }

    private Vector3 GetRandomPoint(Vector3 origin)
    {
        // Get a random point in an area around the origin
        Vector2 spawnOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 target = new Vector3(origin.x + spawnOffset.x, 0, origin.z + spawnOffset.y);
        
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas))
            return hit.position;

        return Vector3.zero;
    }

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
