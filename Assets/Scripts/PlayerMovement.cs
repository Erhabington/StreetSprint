using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float laneChangeSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float slideDuration = 0.5f;

    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 2f;
    [SerializeField] private int startingLane = 1;

    private float touchStartX;
    private float touchEndX;
    private float touchStartY;
    private float minSwipeDistance = 50f;

    private Vector3 targetPosition;
    private int currentLane;
    private bool isChangingLanes = false;
    private bool isJumping = false;
    private bool isSliding = false;

    private Rigidbody rb;
    private Animation anim;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animation>();

        currentLane = startingLane;
        UpdateTargetPosition();
        transform.position = targetPosition;

        if (anim != null && anim.GetClip("Run") != null)
        {
            anim.Play("Run");
        }
    }

    void Update()
    {
        HandleTouchInput();
    }

    void FixedUpdate()
    {
        Vector3 forwardMovement = Vector3.forward * forwardSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMovement);

        if (isChangingLanes)
        {
            Vector3 movementDirection = (targetPosition - transform.position).normalized;
            Vector3 laneMovement = movementDirection * laneChangeSpeed * Time.fixedDeltaTime;

            float distanceToTarget = Vector3.Distance(
                new Vector3(transform.position.x, 0, 0),
                new Vector3(targetPosition.x, 0, 0)
            );

            if (distanceToTarget < laneMovement.magnitude)
            {
                Vector3 newPosition = transform.position;
                newPosition.x = targetPosition.x;
                transform.position = newPosition;
                isChangingLanes = false;
            }
            else
            {
                rb.MovePosition(rb.position + laneMovement);
            }
        }
    }

    private void HandleTouchInput()
    {
        if (isChangingLanes) return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartX = touch.position.x;
                    touchStartY = touch.position.y;
                    break;

                case TouchPhase.Ended:
                    touchEndX = touch.position.x;
                    float touchEndY = touch.position.y;

                    float swipeX = touchEndX - touchStartX;
                    float swipeY = touchEndY - touchStartY;

                    if (Mathf.Abs(swipeX) > Mathf.Abs(swipeY))
                    {
                        // Horizontal swipe
                        if (Mathf.Abs(swipeX) > minSwipeDistance)
                        {
                            if (swipeX > 0 && currentLane < 2)
                            {
                                ChangeLane(1);
                            }
                            else if (swipeX < 0 && currentLane > 0)
                            {
                                ChangeLane(-1);
                            }
                        }
                    }
                    else
                    {
                        // Vertical swipe
                        if (swipeY > minSwipeDistance && !isJumping)
                        {
                            StartCoroutine(Jump());
                        }
                        else if (swipeY < -minSwipeDistance && !isSliding)
                        {
                            StartCoroutine(Slide());
                        }
                    }

                    break;
            }
        }

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
        else if (Input.GetKeyDown(KeyCode.DownArrow) && !isSliding)
        {
            StartCoroutine(Slide());
        }
#endif
    }

    private void ChangeLane(int direction)
    {
        currentLane += direction;
        UpdateTargetPosition();
        isChangingLanes = true;
    }

    private void UpdateTargetPosition()
    {
        float targetX = (currentLane - 1) * laneDistance;
        targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);
    }

    private IEnumerator Jump()
    {
        isJumping = true;

        if (anim != null && anim.GetClip("Runtojumpspring") != null)
        {
            anim.Play("Runtojumpspring");
        }

        float jumpStartTime = Time.time;
        float jumpEndTime = jumpStartTime + jumpDuration;
        Vector3 startPos = transform.position;

        while (Time.time < jumpEndTime)
        {
            float timeProgress = (Time.time - jumpStartTime) / jumpDuration;
            float heightProgress = Mathf.Sin(timeProgress * Mathf.PI);
            float currentHeight = heightProgress * jumpHeight;

            Vector3 newPos = transform.position;
            newPos.y = startPos.y + currentHeight;
            transform.position = newPos;

            yield return null;
        }

        Vector3 landPos = transform.position;
        landPos.y = startPos.y;
        transform.position = landPos;

        if (anim != null && anim.GetClip("Run") != null)
        {
            anim.CrossFade("Run");
        }

        isJumping = false;
    }

    private IEnumerator Slide()
    {
        isSliding = true;

        if (anim != null && anim.GetClip("Runtoslide") != null)
        {
            anim.Play("Runtoslide");
        }

        yield return new WaitForSeconds(slideDuration);

        if (anim != null && anim.GetClip("Run") != null)
        {
            anim.CrossFade("Run");
        }

        isSliding = false;
    }
}
