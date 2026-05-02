using UnityEngine;

public class RootController : MonoBehaviour
{
    public Transform levelCenter;   // center of tower
    public CameraController cameraController;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float verticalSpeed = 2f;

    [Header("Tower Settings")]
    public float radius = 3f;       // distance from center
    public float edgeThreshold = 0.95f;

    private float currentAngle = 0f;

    void Start()
    {
        // Initialize angle based on starting position
        Vector3 dir = (transform.position - levelCenter.position).normalized;
        currentAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A / D
        float vertical = Input.GetAxis("Vertical");     // W / S

        // Move UP (growth)
        transform.position += Vector3.up * vertical * verticalSpeed * Time.deltaTime;

        // Rotate around tower
        if (Mathf.Abs(horizontal) > 0.01f)
        {
            float rotationAmount = horizontal * moveSpeed * 100f * Time.deltaTime;
            currentAngle += rotationAmount;

            // Apply circular movement
            float rad = currentAngle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Sin(rad) * radius,
                transform.position.y,
                Mathf.Cos(rad) * radius
            );

            transform.position = new Vector3(
                levelCenter.position.x + offset.x,
                transform.position.y,
                levelCenter.position.z + offset.z
            );

            transform.LookAt(levelCenter.position + Vector3.up * transform.position.y);
        }

        DetectEdge(horizontal);
    }

    void DetectEdge(float input)
        {
            if (Mathf.Abs(input) < 0.1f) return;

            // Snap angle to 90° blocks
            float snapped = Mathf.Round(currentAngle / 90f) * 90f;
            float difference = Mathf.Abs(currentAngle - snapped);

            if (difference < 2f) // near edge
            {
                float direction = Mathf.Sign(input); // ✅ FIX

                cameraController.TriggerEdgeRotation(direction);

                // lock exactly to edge
                currentAngle = snapped;
            }
        }
}