using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CameraStart startCam;
    public CameraFollowRoot followCam;
    public CameraEdgeRotate edgeCam;
    public CameraFlowerOrbit flowerCam;

    public Transform root;
    public Transform flower;

    private enum CameraState
    {
        Start,
        Follow,
        EdgeRotate,
        Flower
    }

    private CameraState currentState;

    void Start()
    {
        SetState(CameraState.Start);

        // hook edge rotation callback
        edgeCam.onComplete = () => SetState(CameraState.Follow);
    }

    void Update()
    {
        if (currentState == CameraState.Start && startCam.isFinished)
        {
            // handoff (NO SNAP)
            followCam.transform.position = transform.position;
            followCam.transform.rotation = transform.rotation;

            SetState(CameraState.Follow);
        }
    }

    public void TriggerEdgeRotation(float direction)
    {
        SetState(CameraState.EdgeRotate);
        edgeCam.TriggerRotation(direction);
    }

    public void TriggerFlowerReveal()
    {
        SetState(CameraState.Flower);
    }

    void SetState(CameraState newState)
    {
        currentState = newState;

        startCam.enabled = false;
        followCam.enabled = false;
        edgeCam.enabled = false;
        flowerCam.enabled = false;

        switch (newState)
        {
            case CameraState.Start:
                startCam.enabled = true;
                break;

            case CameraState.Follow:
                followCam.target = root;
                followCam.ResetOffset();
                followCam.enabled = true;
                break;

            case CameraState.EdgeRotate:
                edgeCam.target = root;
                edgeCam.enabled = true;
                break;

            case CameraState.Flower:
                flowerCam.target = flower;
                flowerCam.enabled = true;
                break;
        }
    }
}