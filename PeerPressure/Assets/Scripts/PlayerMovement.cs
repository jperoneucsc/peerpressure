using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public float gravity = -9.81f;
    public float interactRange = 10f;
    public LayerMask interactableLayer;
    public float smoothMoveDuration = 1f; // Duration for smooth movement

    public GameObject crosshairPrefab; // Assign the crosshair prefab in the inspector
    public Transform dartboard; // Assign the dartboard transform in the inspector
    public Transform bullseye; // Assign the bullseye transform in the inspector
    public float bullseyeRadius = 1f; // Radius to check if the crosshair is on the bullseye
    public float crosshairMovementSpeed = 1f; // Speed of crosshair movement
    public float crosshairMovementAmplitude = 1f; // Amplitude of crosshair movement

    private CharacterController controller;
    private Vector3 velocity;
    private Transform cameraTransform;
    private bool isControlLocked = false; // Flag to lock/unlock player controls
    private Vector3 targetPosition; // Target position for smooth movement
    private Quaternion targetRotation; // Target rotation for smooth movement
    private float moveStartTime; // Time when movement starts
    private GameObject crosshairInstance; // Instance of the crosshair

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;

        // Hide and lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Instantiate crosshair and deactivate it initially
        if (crosshairPrefab != null)
        {
            crosshairInstance = Instantiate(crosshairPrefab);
            crosshairInstance.SetActive(false);
        }
    }

    void Update()
    {
        if (!isControlLocked)
        {
            HandleMovement();
            HandleMouseLook();
        }

        HandleInteraction();
        SmoothMoveToTarget();

        if (isControlLocked && crosshairInstance != null)
        {
            MoveCrosshair();

            if (Input.GetKeyDown(KeyCode.Space)) // Replace with the key you want to use to stop the crosshair
            {
                EndDartGame();
            }
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        controller.Move(move * moveSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        transform.Rotate(Vector3.up * mouseX);
        cameraTransform.Rotate(Vector3.left * mouseY);
    }

    void HandleInteraction()
    {
        RaycastHit hit;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red);

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Interacted with: " + hit.collider.gameObject.name);
                hit.collider.gameObject.SendMessage("OnInteract", SendMessageOptions.DontRequireReceiver);
                StartDartGame(hit.collider.transform.GetChild(0).transform);
            }
        }
    }

    public void StartDartGame(Transform dartboardPosition)
    {
        // Lock player controls
        isControlLocked = true;

        // Set target position and rotation for smooth movement
        targetPosition = dartboardPosition.position;
        targetRotation = dartboardPosition.rotation;
        moveStartTime = Time.time; // Record the start time of the movement

        // Activate crosshair and position it in front of the camera
        if (crosshairInstance != null)
        {
            crosshairInstance.SetActive(true);
            crosshairInstance.transform.position = cameraTransform.position + cameraTransform.forward * 5f; // Adjust distance as needed
        }

        Debug.Log("Dart game started. Player controls are locked.");
    }

    void SmoothMoveToTarget()
    {
        if (isControlLocked)
        {
            float elapsedTime = (Time.time - moveStartTime) / smoothMoveDuration;
            if (elapsedTime < 1f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, elapsedTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, elapsedTime);
            }
            else
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
            }
        }
    }

    void MoveCrosshair()
    {
        if (crosshairInstance != null)
        {
            float movementTime = Time.time * crosshairMovementSpeed;
            float x = Mathf.Sin(movementTime) * crosshairMovementAmplitude;
            float z = Mathf.Cos(movementTime) * crosshairMovementAmplitude;

            crosshairInstance.transform.position = dartboard.position + new Vector3(x, 0, z);
        }
    }

    void EndDartGame()
    {
        // Unlock player controls
        isControlLocked = false;

        // Deactivate crosshair
        if (crosshairInstance != null)
        {
            crosshairInstance.SetActive(false);
        }

        Debug.Log("Dart game ended. Player controls are unlocked.");

        // Optionally, add logic here to check if the crosshair hit the bullseye
        CheckCrosshairHit();
    }

    void CheckCrosshairHit()
    {
        if (crosshairInstance != null)
        {
            if (Vector3.Distance(crosshairInstance.transform.position, bullseye.position) <= bullseyeRadius)
            {
                Debug.Log("Hit the bullseye!");
                // Handle bullseye hit logic
            }
            else
            {
                Debug.Log("Missed the bullseye!");
                // Handle miss logic
            }
        }
    }
}
