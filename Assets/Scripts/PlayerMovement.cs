using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float laneChangeSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpDuration = 0.5f;

    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 2f;
    [SerializeField] private int startingLane = 1; // 0 = left, 1 = center, 2 = right

    // Touch detection
    private float touchStartX;
    private float touchEndX;
    private float minSwipeDistance = 50f; // Minimum swipe distance in pixels

    // Movement state
    private Vector3 targetPosition;
    private int currentLane;
    private bool isChangingLanes = false;
    private bool isJumping = false;

    // Components
    private Rigidbody rb;
    private Animator animator; // Optional: if character has animations

    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>(); // Optional

        // Initialize lane position
        currentLane = startingLane;
        UpdateTargetPosition();

        // Set initial position
        transform.position = targetPosition;
    }

    void Update()
    {
        // Handle touch input for lane changes
        HandleTouchInput();
    }

    void FixedUpdate()
    {
        // Automatic forward movement
        Vector3 forwardMovement = Vector3.forward * forwardSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMovement);

        // Lane changing movement (if not at target position)
        if (isChangingLanes)
        {
            Vector3 movementDirection = (targetPosition - transform.position).normalized;
            Vector3 laneMovement = movementDirection * laneChangeSpeed * Time.fixedDeltaTime;

            // Calculate distance to target
            float distanceToTarget = Vector3.Distance(
                new Vector3(transform.position.x, 0, 0),
                new Vector3(targetPosition.x, 0, 0)
            );

            // If close enough to target or would overshoot
            if (distanceToTarget < laneMovement.magnitude)
            {
                // Snap to exact lane position
                Vector3 newPosition = transform.position;
                newPosition.x = targetPosition.x;
                transform.position = newPosition;
                isChangingLanes = false;
            }
            else
            {
                // Move toward target lane
                rb.MovePosition(rb.position + laneMovement);
            }
        }
    }

    private void HandleTouchInput()
    {
        // Skip input processing if currently changing lanes
        if (isChangingLanes) return;

        // Process touch input for mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartX = touch.position.x;
                    break;

                case TouchPhase.Ended:
                    touchEndX = touch.position.x;
                    float swipeDistance = touchEndX - touchStartX;

                    // Determine if swipe was significant enough
                    if (Mathf.Abs(swipeDistance) > minSwipeDistance)
                    {
                        if (swipeDistance > 0 && currentLane < 2) // Swipe right
                        {
                            ChangeLane(1);
                        }
                        else if (swipeDistance < 0 && currentLane > 0) // Swipe left
                        {
                            ChangeLane(-1);
                        }
                    }
                    break;
            }
        }

        // For testing in editor with keyboard
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentLane > 0)
        {
            ChangeLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && currentLane < 2)
        {
            ChangeLane(1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && !isJumping)
        {
            StartCoroutine(Jump());
        }
#endif
    }

    private void ChangeLane(int direction)
    {
        currentLane += direction;
        UpdateTargetPosition();
        isChangingLanes = true;

        // Optional: Play animation if you have one
        // if (animator != null) animator.SetTrigger("SidestepTrigger");
    }

    private void UpdateTargetPosition()
    {
        // Calculate the target x position based on current lane
        float targetX = (currentLane - 1) * laneDistance; // -1 for left, 0 for center, 1 for right
        targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);
    }

    // Optional: Add jump functionality for future use
    private IEnumerator Jump()
    {
        if (isJumping) yield break;

        isJumping = true;
        // Optional: Trigger jump animation
        // if (animator != null) animator.SetTrigger("JumpTrigger");

        float jumpStartTime = Time.time;
        float jumpEndTime = jumpStartTime + jumpDuration;
        Vector3 startPos = transform.position;

        while (Time.time < jumpEndTime)
        {
            float timeProgress = (Time.time - jumpStartTime) / jumpDuration;

            // Parabolic jump curve
            float heightProgress = Mathf.Sin(timeProgress * Mathf.PI);
            float currentHeight = heightProgress * jumpHeight;

            // Set the new position with jump height
            Vector3 newPos = transform.position;
            newPos.y = startPos.y + currentHeight;
            transform.position = newPos;

            yield return null;
        }

        // Make sure we land exactly at the starting height
        Vector3 landPos = transform.position;
        landPos.y = startPos.y;
        transform.position = landPos;

        isJumping = false;
    }
}