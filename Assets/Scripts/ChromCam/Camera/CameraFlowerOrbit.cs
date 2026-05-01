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