using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject indicator;

    private GameObject indicatorInstance;

    public int remainingHints = 0;
    private bool indicatorVisible = false;

    private Vector3 hintTarget;


    private void Awake()
    {
        indicatorInstance = Instantiate(indicator, new Vector3(0,0,0), Quaternion.identity);
        indicatorInstance.SetActive(false);
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleMovement();

        // TODO
        TeleportToNPC();

        if (remainingHints > 0)
        {
            if (Input.GetKey(Consts.HINT_KEY) && !indicatorVisible)
            {
                EnableHint();
                remainingHints--;
            }
            if (indicatorVisible)
                UpdateHintPosition();
        }
    }


    /// <summary>
    /// Updates the player transform to reflect its movement
    /// </summary>
    /// <remarks>
    /// The player moves either relative to the camera or to itself
    /// depending on whether the player is freelooking (behind itself for example).
    /// </remarks>
    private void HandleMovement()
    {
        if (FreeLooking())
        {
            ApplyRotation();
            ApplyTranslation(mainCamera.transform);
        }
        else
        {
            ApplyTranslation(transform);
        }
    }

    /// <summary>
    /// Detects if the user is freelooking.
    /// </summary>
    /// <remarks>
    /// If user is holding down LMB, the camera can rotate around the player
    /// without affecting its direction.
    /// </remarks>
    private bool FreeLooking()
    {
        return Input.GetMouseButton(1);
    }

    /// <summary>
    /// Applies the camera's rotation around the y axis to the player.
    /// </summary>
    private void ApplyRotation()
    {
        Quaternion rotation = mainCamera.transform.rotation;
        rotation.z = 0f;
        rotation.x = 0f;

        transform.rotation = rotation;
    }

    private void ApplyTranslation(Transform tr)
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward = tr.forward;
        Vector3 right = tr.right;

        // Ignore y data and normalize to preserve movement speed
        forward.y = 0;
        right.y = 0;
        forward = forward.normalized;
        right = right.normalized;

        // Transform to local space
        Vector3 forwardRelative = v * forward;
        Vector3 rightRelative = h * right;

        Vector3 direction = (forwardRelative + rightRelative).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void TeleportToNPC()
    {
        if (Input.GetKey(Consts.TELEPORT_KEY))
        {
            transform.position = new Vector3(0, transform.position.y, 0);
        }
    }

    private void EnableHint()
    {
        GameObject closest = GameController.GetClosestObject(transform.position);
        if (closest != null)
        {
            hintTarget = closest.transform.position;
            indicatorInstance.SetActive(true);
            indicatorVisible = true;
            StartCoroutine(ExpireHint());
        }
    }

    private void UpdateHintPosition()
    {
        Vector3 direction = hintTarget - transform.position;
        Vector3 pos = transform.position + direction.normalized * 2.0f;
        indicatorInstance.transform.position = pos;
    }

    private IEnumerator ExpireHint()
    {
        yield return new WaitForSeconds(2);
        if (indicatorVisible)
        {
            indicatorVisible = false;
            indicatorInstance.SetActive(false);
        }
    }
}
