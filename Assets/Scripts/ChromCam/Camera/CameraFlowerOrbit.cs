using UnityEngine;

public class CameraFlowerOrbit : MonoBehaviour
{
    public Transform target;

    public float radius = 3.5f; // tighter than before
    public float height = 2f;

    public float minSpeed = 5f;
    public float maxSpeed = 25f;

    private float angle;
    private float timer;

    public void ResetOrbit()
    {
        if (!target) return;

        // Direction from target → camera
        Vector3 dir = transform.position - target.position;

        // Flatten horizontal direction
        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z).normalized;

        // Calculate starting angle from current position
        angle = Mathf.Atan2(flatDir.z, flatDir.x) * Mathf.Rad2Deg;

        // Reset timer so speed ramps nicely
        timer = 0f;
    }

    void LateUpdate()
    {
        if (!target) return;

        timer += Time.deltaTime;

        float speed = Mathf.Lerp(minSpeed, maxSpeed, timer);

        angle += speed * Time.deltaTime;

        float rad = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(rad) * radius,
            height,
            Mathf.Sin(rad) * radius
        );

        transform.position = target.position + offset;
        transform.LookAt(target);
    }
}