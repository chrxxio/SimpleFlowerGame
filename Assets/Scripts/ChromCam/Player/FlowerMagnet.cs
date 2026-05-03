using UnityEngine;
using System.Collections;

public class FlowerMagnet : MonoBehaviour
{
    public Transform player;
    public Transform centerPoint;
    public CameraController cameraController;

    public CameraFollowRoot followCam;
    public CameraFlowerOrbit flowerOrbitCam;

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
    public bool IsActive()
    {
        return isActive;
    }

    public void ActivateMagnet()
    {
        if (isActive || hasSnapped) return; // 🔥 ADD THIS

        isActive = true;
        Debug.Log("🌸 MAGNET ACTIVATED");
    }

    private bool hasSnapped = false;
    public bool HasSnapped()
    {
        return hasSnapped;
    }

    void Update()
    {
        if (!player || !centerPoint) return;
        if (!isActive) return;

        magnetTimer += Time.deltaTime;

        Vector3 targetPos = centerPoint.position;
        Vector3 toFlower = targetPos - player.position;
        float distance = toFlower.magnitude;

        // SNAP
        if (distance < snapDistance && !hasSnapped)
        {
            hasSnapped = true;

            player.position = targetPos;

            Debug.Log("🌸 SNAPPED TO CENTER");

            isActive = false;

            // 🔥 HARD LOCK PLAYER
            PlayerController playerScript = player.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.enabled = false;
            }

            // 🔥 SWITCH CAMERA
            StartCoroutine(SwitchCameraAfterDelay(0.3f));

            return;
        }

        // 🎯 Normalize distance (0 = at center, 1 = far away)
        float normalized = Mathf.Clamp01(distance / 10f);

        // 🔥 Ease curve (slow far away, stronger near center)
        float strength = Mathf.Lerp(0.5f, 1.5f, 1f - normalized);

        // Direction
        Vector3 dir = toFlower.normalized;

        // Pull
        Vector3 pull = dir * pullStrength * strength * Time.deltaTime;

        // 🎮 Side control fades as you get closer
        float input = Input.GetAxis("Horizontal");
        float sideFactor = Mathf.Lerp(sideInfluence, 0.1f, 1f - normalized);
        Vector3 side = player.right * input * sideFactor * Time.deltaTime;

        // Final move
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