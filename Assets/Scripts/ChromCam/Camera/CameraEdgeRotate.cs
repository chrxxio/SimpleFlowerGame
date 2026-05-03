using UnityEngine;

public class CameraEdgeRotate : MonoBehaviour
{
    public Transform orbitCenter;
    public float duration = 0.4f;

    private float timer;
    private bool rotating;
    private float targetAngle;

    public bool IsFinished => !rotating;

    public void StartRotation(int direction)
    {
        rotating = true;
        timer = 0f;
        targetAngle = 90f * direction;
    }

    void Update()
    {
        if (!rotating) return;

        float step = (targetAngle / duration) * Time.deltaTime;

        transform.RotateAround(
            orbitCenter.position,
            Vector3.up,
            step
        );

        timer += Time.deltaTime;

        if (timer >= duration)
        {
            rotating = false;
        }
    }
}