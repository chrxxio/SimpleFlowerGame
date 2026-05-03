using UnityEngine;
using System.Collections;
using DG.Tweening;

public class FlowerMagnet : MonoBehaviour
{
    public Transform player;
    public Transform centerPoint;
    public CameraController cameraController;

    public CameraFollowRoot followCam;
    public CameraFlowerOrbit flowerOrbitCam;
    public Camera mainCamera;

    [Header("Camera Timing")]
    public float cameraSwitchDelay = 1.5f;

    private float magnetTimer = 0f;
    private bool cameraSwitched = false;

    [Header("Magnet Settings")]
    public float pullStrength = 7f;
    public float maxSpeed = 3f;
    public float sideInfluence = 0.4f;
    public float snapDistance = 0.5f;

    private bool isActive = false;
    private bool hasSnapped = false;

    public bool IsActive() => isActive;
    public bool HasSnapped() => hasSnapped;

    public void ActivateMagnet()
    {
        if (isActive || hasSnapped) return;

        isActive = true;
        Debug.Log("🌸 MAGNET ACTIVATED");
    }

    public void StopAnimation()
    {
        DOTween.KillAll();
    }

    void SwitchToOrthoZoomOut()
    {
        if (mainCamera == null) return;

        mainCamera.orthographic = true;

        // 🎯 POSITION (pull back + lift up)
        mainCamera.transform.position = new Vector3(
            player.position.x,
            player.position.y + 6f,   // height
            player.position.z - 10f    // pull back
        );

        // 🎯 ROTATION (tilt toward horizon)
        mainCamera.transform.rotation = Quaternion.Euler(20f, 0f, 0f);

        // 🎯 ZOOM OUT
        DOTween.To(
            () => mainCamera.orthographicSize,
            x => mainCamera.orthographicSize = x,
            28f,   // adjust this if needed
            1.2f
        ).SetEase(Ease.OutCubic);
    }

    void Update()
    {
        if (!player || !centerPoint) return;
        if (!isActive) return;

        magnetTimer += Time.deltaTime;

        Vector3 targetPos = centerPoint.position;
        Vector3 toFlower = targetPos - player.position;
        float distance = toFlower.magnitude;

        // 🌸 SNAP
        if (distance < snapDistance && !hasSnapped)
        {
            hasSnapped = true;

            StopAnimation();

            player.position = targetPos;
            Debug.Log("🌸 SNAPPED TO CENTER");

            isActive = false;

            // Lock player
            PlayerController playerScript = player.GetComponent<PlayerController>();
            if (playerScript != null)
                playerScript.enabled = false;

            // Camera logic
            SwitchToOrthoZoomOut();
            StartCoroutine(SwitchCameraAfterDelay(0.3f));

            return;
        }

        // Movement logic
        float normalized = Mathf.Clamp01(distance / 10f);
        float strength = Mathf.Lerp(0.5f, 1.5f, 1f - normalized);

        Vector3 dir = toFlower.normalized;

        Vector3 pull = dir * pullStrength * strength * Time.deltaTime;

        float input = Input.GetAxis("Horizontal");
        float sideFactor = Mathf.Lerp(sideInfluence, 0.1f, 1f - normalized);
        Vector3 side = player.right * input * sideFactor * Time.deltaTime;

        Vector3 move = pull + side;
        move = Vector3.ClampMagnitude(move, maxSpeed * Time.deltaTime);

        player.position += move;
        player.LookAt(targetPos);
    }

    IEnumerator SwitchCameraAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (followCam != null)
            followCam.enabled = false;

        if (flowerOrbitCam != null)
        {
            flowerOrbitCam.enabled = true;
            flowerOrbitCam.ResetOrbit();
        }

        Debug.Log("🎥 CAMERA SWITCHED AFTER SNAP");
    }
}