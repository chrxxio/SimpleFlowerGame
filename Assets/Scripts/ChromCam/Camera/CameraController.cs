using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public CameraFollowRoot followCam;

    [Header("Flower Sequence")]
    public CameraFlowerOrbit flowerOrbit;
    public Transform flowerTarget;
    public float transitionDuration = 1.5f;


    // 🔥 EDGE ROTATION (UNCHANGED)
    public void TriggerEdgeRotation(int direction)
    {
        if (followCam != null)
        {
            followCam.RotateCamera(direction);
            Debug.Log("EDGE ROTATE: " + direction);
        }
    }

    public void TriggerFlowerSequence()
    {
        StartCoroutine(TransitionToFlower());
    }

    IEnumerator TransitionToFlower()
    {
        Transform cam = followCam.transform;

        // disable follow behavior
        followCam.enabled = false;

        Vector3 startPos = cam.position;
        Quaternion startRot = cam.rotation;

        // 🔥 cinematic end position (tweak this)
        Vector3 endPos = flowerTarget.position + new Vector3(0, 2f, -4f);
        Quaternion endRot = Quaternion.LookRotation(flowerTarget.position - endPos);

        float time = 0f;

        while (time < transitionDuration)
        {
            float t = time / transitionDuration;
            t = Mathf.SmoothStep(0f, 1f, t); // smooth ease

            cam.position = Vector3.Lerp(startPos, endPos, t);
            cam.rotation = Quaternion.Slerp(startRot, endRot, t);

            time += Time.deltaTime;
            yield return null;
        }

        cam.position = endPos;
        cam.rotation = endRot;

        // 🔥 enable orbit AFTER transition
        if (flowerOrbit != null)
            flowerOrbit.enabled = true;
    }
}