using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [Tooltip("Tag to find player if no target is assigned")]
    [SerializeField] private string targetTag = "Player";

    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 3.5f, -6f);
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Look Settings")]
    [SerializeField] private bool lookAtTarget = true;
    [SerializeField] private Vector3 lookOffset = new Vector3(0, 1f, 0); // Look a bit above the player

    private Vector3 desiredPosition;

    private void Start()
    {
        // Find player by tag if no target is assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(targetTag);
            if (player != null)
            {
                target = player.transform;
                Debug.Log("Camera found player target by tag: " + targetTag);
            }
            else
            {
                Debug.LogWarning("Camera could not find a target with tag: " + targetTag);
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Calculate desired position (behind and above player)
        desiredPosition = target.position + offset;

        // Smoothly move the camera
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Make camera look at player
        if (lookAtTarget)
        {
            transform.LookAt(target.position + lookOffset);
        }
    }

    // Optional: Visualize camera path in editor
    private void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
            Gizmos.DrawWireSphere(target.position + lookOffset, 0.25f);
        }
    }
}