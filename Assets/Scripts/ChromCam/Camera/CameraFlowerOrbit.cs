using UnityEngine;

public class CameraFlowerOrbit : MonoBehaviour
{
    public Transform target;

    public float radius = 3.5f;
    public float height = 2f;

    public float minSpeed = 5f;
    public float maxSpeed = 25f;

    public float orbitDuration = 3f; // time before screenshot ready

    private float angle;
    private float timer;
    private float orbitTimer = 0f;

    private bool hasCompleted = false;

    public bool HasCompleted()
    {
        return hasCompleted;
    }

    public void ResetOrbit()
    {
        if (!target) return;

        Vector3 dir = transform.position - target.position;
        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z).normalized;

        angle = Mathf.Atan2(flatDir.z, flatDir.x) * Mathf.Rad2Deg;
        timer = 0f;
        orbitTimer = 0f;
        hasCompleted = false;
    }

    void LateUpdate()
    {
        if (!target) return;

        timer += Time.deltaTime;
        orbitTimer += Time.deltaTime;

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

        // ✅ ORBIT COMPLETE CHECK
        if (!hasCompleted && orbitTimer >= orbitDuration)
        {
            hasCompleted = true;
            Debug.Log("📸 ORBIT COMPLETE - READY FOR SCREENSHOT");
        }
    }
}