using UnityEngine;
using System;

public class CameraEdgeRotate : MonoBehaviour
{
    public Transform target;        // player/root
    public Transform orbitCenter;   // LevelCenter

    public float duration = 0.4f;
    public float rotationAmount = 90f;

    private bool isRotating = false;
    private float timer = 0f;

    private Quaternion startRot;
    private Quaternion endRot;

    public Action onComplete; 

    public void TriggerRotation(float direction)
    {
        if (isRotating) return;

        isRotating = true;
        timer = 0f;

        startRot = transform.rotation;
        endRot = Quaternion.Euler(
            0,
            transform.eulerAngles.y + (rotationAmount * direction),
            0
        );
    }

    void Update()
    {
        if (!isRotating) return;

        timer += Time.deltaTime;
        float t = timer / duration;

        transform.RotateAround(
            orbitCenter.position,
            Vector3.up,
            (rotationAmount / duration) * Time.deltaTime
        );

        transform.LookAt(target);

        if (t >= 1f)
        {
            isRotating = false;
            onComplete?.Invoke(); 
        }
    }
}