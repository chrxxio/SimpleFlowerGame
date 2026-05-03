//using UnityEngine;
//using UnityEngine.InputSystem;

//[RequireComponent(typeof(SphereCollider))]
//public class PlayerController : MonoBehaviour {
//    [SerializeField] private float moveSpeed = 5f;
//    [SerializeField] private float turnSpeed = 120f;
//    [SerializeField] private LayerMask obstacleLayers = ~0;
//    [SerializeField] private float skinWidth = 0.02f;

//    private SphereCollider sphere;

//    void Awake() {
//        sphere = GetComponent<SphereCollider>();
//    }

//    void Update() {
//        Keyboard kb = Keyboard.current;
//        if (kb == null) return;

//        // Rotation
//        float turn = 0f;
//        if (kb.aKey.isPressed) turn += 1f;
//        if (kb.dKey.isPressed) turn -= 1f;
//        transform.Rotate(0f, 0f, turn * turnSpeed * Time.deltaTime);

//        // Movement
//        Vector3 desiredMove = transform.up * moveSpeed * Time.deltaTime;
//        Vector3 actualMove = ResolveMovement(desiredMove);
//        transform.position += actualMove;
//    }

//    Vector3 ResolveMovement(Vector3 motion) {
//        float distance = motion.magnitude;
//        if (distance < 0.0001f) return Vector3.zero;

//        Vector3 direction = motion / distance;
//        float radius = sphere.radius * transform.lossyScale.x;
//        Vector3 origin = transform.position + sphere.center;

//        if (Physics.SphereCast(origin, radius, direction, out RaycastHit hit,
//                               distance + skinWidth, obstacleLayers,
//                               QueryTriggerInteraction.Ignore)) {
//            float allowed = Mathf.Max(0f, hit.distance - skinWidth);
//            return direction * allowed;
//        }

//        return motion;
//    }
//}

using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(SphereCollider))]
public class PlayerController : MonoBehaviour {

    [SerializeField] private CameraController cameraController;


    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 120f;
    [SerializeField] private LayerMask obstacleLayers = ~0;
    [SerializeField] private float skinWidth = 0.02f;

    [Header("Tower")]
    [SerializeField] private Transform tower;
    [SerializeField] private Collider towerCollider;
    [SerializeField] private float wrapCooldown = 0.1f;
    [SerializeField] private float wrapInset = 0.05f;

    [Header("Debug")]
    [SerializeField] private bool verboseLogging = true;

    private SphereCollider sphere;
    private float playerRadius;
    private int currentFace = -1;
    private float wrapCooldownTimer = 0f;

    void Awake() {
        sphere = GetComponent<SphereCollider>();
        playerRadius = sphere.radius * transform.lossyScale.x;
    }

    void Update() {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // === INPUT ===
        float turn = 0f;
        if (kb.aKey.isPressed) turn += 1f;
        if (kb.dKey.isPressed) turn -= 1f;
        transform.Rotate(0f, 0f, turn * turnSpeed * Time.deltaTime);

        // === MOVEMENT ===
        Vector3 desiredMove = transform.up * moveSpeed * Time.deltaTime;
        Vector3 actualMove = ResolveMovement(desiredMove);
        transform.position += actualMove;

        // === TOWER LOGIC ===
        if (tower == null || towerCollider == null) return;

        if (wrapCooldownTimer > 0f)
            wrapCooldownTimer -= Time.deltaTime;

        // Wrap detection
        if (currentFace != -1 && wrapCooldownTimer <= 0f) {
            int crossDirection = CheckEdgeCrossing(currentFace);
            if (crossDirection != 0) {
                int oldFace = currentFace;
                int newFace = GetWrappedFace(oldFace, crossDirection);

                LogDiagnostic("PRE-WRAP", oldFace);

                WrapAroundTower(crossDirection);
                LogDiagnostic("POST-ROTATE", oldFace);

                if (cameraController != null)
                {
                    Debug.Log("CAMERA ROTATE TRIGGERED: " + crossDirection);
                    cameraController.TriggerEdgeRotation(crossDirection);
                }

                SnapToFace(newFace);
                PullAwayFromOldFace(oldFace);
                LogDiagnostic("POST-SNAP", newFace);

                currentFace = newFace;
                wrapCooldownTimer = wrapCooldown;

                Debug.Log($"WRAP: face {oldFace} -> face {newFace}, " +
                          $"crossDir={crossDirection} ({(crossDirection > 0 ? "CCW" : "CW")})");



                return;
            }
        }

        // Maintain surface adherence on the current face — does NOT redetect face
        if (currentFace != -1) {
            SnapToFace(currentFace);
        } else {
            // First frame: auto-detect the initial face
            currentFace = SnapToTowerSurface();
        }
    }

    Vector3 ResolveMovement(Vector3 motion) {
        float distance = motion.magnitude;
        if (distance < 0.0001f) return Vector3.zero;

        Vector3 direction = motion / distance;
        float radius = sphere.radius * transform.lossyScale.x;
        Vector3 origin = transform.position + sphere.center;

        if (Physics.SphereCast(origin, radius, direction, out RaycastHit hit,
                               distance + skinWidth, obstacleLayers,
                               QueryTriggerInteraction.Ignore)) {
            float allowed = Mathf.Max(0f, hit.distance - skinWidth);
            return direction * allowed;
        }

        return motion;
    }

    /// <summary>
    /// Returns +1 for CCW-edge crossing, -1 for CW-edge crossing, 0 for no crossing.
    /// </summary>
    int CheckEdgeCrossing(int face) {
        Bounds localBounds = GetTowerLocalBounds();
        Vector3 localPos = tower.InverseTransformPoint(transform.position);
        Vector3 relPos = localPos - localBounds.center;
        Vector3 extents = localBounds.extents;

        float inFaceCoord;
        float threshold;
        int ccwSign;

        switch (face) {
            case 0: inFaceCoord = relPos.z; threshold = extents.z; ccwSign = -1; break;
            case 1: inFaceCoord = relPos.z; threshold = extents.z; ccwSign = 1; break;
            case 2: inFaceCoord = relPos.x; threshold = extents.x; ccwSign = 1; break;
            case 3: inFaceCoord = relPos.x; threshold = extents.x; ccwSign = -1; break;
            default: return 0;
        }

        if (inFaceCoord > threshold) return ccwSign;
        if (inFaceCoord < -threshold) return -ccwSign;
        return 0;
    }

    int GetWrappedFace(int oldFace, int direction) {
        int[] ccwNext = { 3, 2, 0, 1 };
        int[] cwNext = { 2, 3, 1, 0 };
        return direction > 0 ? ccwNext[oldFace] : cwNext[oldFace];
    }

    void WrapAroundTower(int rotationDirection) {
        float angle = 90f * rotationDirection;
        transform.Rotate(tower.up, angle, Space.World);
    }

    void SnapToFace(int face) {
        Bounds localBounds = GetTowerLocalBounds();
        Vector3 center = localBounds.center;
        Vector3 extents = localBounds.extents;

        Vector3 localPos = tower.InverseTransformPoint(transform.position);
        Vector3 relPos = localPos - center;

        switch (face) {
            case 0: relPos.x = extents.x + playerRadius / tower.lossyScale.x; break;
            case 1: relPos.x = -extents.x - playerRadius / tower.lossyScale.x; break;
            case 2: relPos.z = extents.z + playerRadius / tower.lossyScale.z; break;
            case 3: relPos.z = -extents.z - playerRadius / tower.lossyScale.z; break;
        }

        transform.position = tower.TransformPoint(relPos + center);
    }

    void PullAwayFromOldFace(int oldFace) {
        Bounds localBounds = GetTowerLocalBounds();
        Vector3 center = localBounds.center;
        Vector3 extents = localBounds.extents;

        Vector3 localPos = tower.InverseTransformPoint(transform.position);
        Vector3 relPos = localPos - center;

        switch (oldFace) {
            case 0: relPos.x = extents.x - wrapInset; break;
            case 1: relPos.x = -extents.x + wrapInset; break;
            case 2: relPos.z = extents.z - wrapInset; break;
            case 3: relPos.z = -extents.z + wrapInset; break;
        }

        transform.position = tower.TransformPoint(relPos + center);
    }

    int SnapToTowerSurface() {
        Bounds localBounds = GetTowerLocalBounds();
        Vector3 center = localBounds.center;
        Vector3 extents = localBounds.extents;

        Vector3 localPos = tower.InverseTransformPoint(transform.position);
        Vector3 relPos = localPos - center;

        float xRatio = Mathf.Abs(relPos.x) / extents.x;
        float zRatio = Mathf.Abs(relPos.z) / extents.z;

        int face;
        if (xRatio >= zRatio)
            face = (relPos.x >= 0f) ? 0 : 1;
        else
            face = (relPos.z >= 0f) ? 2 : 3;

        SnapToFace(face);
        return face;
    }

    Bounds GetTowerLocalBounds() {
        if (towerCollider is MeshCollider mc && mc.sharedMesh != null)
            return mc.sharedMesh.bounds;
        if (towerCollider is BoxCollider bc)
            return new Bounds(bc.center, bc.size);

        return new Bounds(Vector3.zero, Vector3.one);
    }

    void LogDiagnostic(string label, int faceContext) {
        if (!verboseLogging) return;

        Bounds localBounds = GetTowerLocalBounds();
        Vector3 localPos = tower.InverseTransformPoint(transform.position);
        Vector3 relPos = localPos - localBounds.center;
        Vector3 extents = localBounds.extents;

        Debug.Log(
            $"[{label}] face={faceContext} | " +
            $"world.pos={Format(transform.position)} | " +
            $"local.pos={Format(localPos)} | " +
            $"rel.pos={Format(relPos)} | " +
            $"extents={Format(extents)} | " +
            $"world.up={Format(transform.up)} | " +
            $"world.right={Format(transform.right)} | " +
            $"world.fwd={Format(transform.forward)} | " +
            $"tower.pos={Format(tower.position)} | " +
            $"tower.up={Format(tower.up)} | " +
            $"tower.scale={Format(tower.lossyScale)}"
        );
    }

    static string Format(Vector3 v) => $"({v.x:F2},{v.y:F2},{v.z:F2})";
}