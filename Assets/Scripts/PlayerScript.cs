using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(SphereCollider))]
public class PlayerController : MonoBehaviour
{

    [SerializeField] private CameraController cameraController;
    [SerializeField] private RootController rootController;


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

    [Header("Stem Trail")]
    [SerializeField] private StemTrail stemTrail;

    [Header("Debug")]
    [SerializeField] private bool verboseLogging = true;

    private SphereCollider sphere;
    private float playerRadius;
    private int currentFace = -1;
    private float wrapCooldownTimer = 0f;

    void Awake()
    {
        sphere = GetComponent<SphereCollider>();
        playerRadius = sphere.radius * transform.lossyScale.x;
    }

    void Update()
    {
        FlowerMagnet magnet = FindObjectOfType<FlowerMagnet>();

        if (magnet != null && magnet.IsActive())
        {
            return;
        }

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // === INPUT ===
        float turn = 0f;
        if (kb.aKey.isPressed) turn += 1f;
        if (kb.dKey.isPressed) turn -= 1f;
        transform.Rotate(0f, 0f, turn * turnSpeed * Time.deltaTime);


        // === MOVEMENT ===
        Vector3 desiredMove;

        // 🔥 MAGNET OVERRIDE
        if (rootController != null && rootController.IsMagnetActive())
        {
            Transform flower = rootController.flowerTarget;

            if (flower != null)
            {
                Vector3 toFlower = (flower.position - transform.position);

                Vector3 pull = toFlower.normalized * moveSpeed * 3f;

                float turnInput = 0f;
                if (Keyboard.current.aKey.isPressed) turnInput += 1f;
                if (Keyboard.current.dKey.isPressed) turnInput -= 1f;

                Vector3 side = transform.right * turnInput * moveSpeed * 0.5f;

                desiredMove = (pull + side) * Time.deltaTime;
            }
            else
            {
                desiredMove = Vector3.zero;
            }
        }
        else
        {
            // normal climbing
            desiredMove = transform.up * moveSpeed * Time.deltaTime;
        }
        if (rootController != null && rootController.IsMagnetActive())
        {
            // 🔥 IGNORE COLLISION WHEN MAGNET ACTIVE
            transform.position += desiredMove;
        }
        else
        {
            Vector3 actualMove = ResolveMovement(desiredMove);
            transform.position += actualMove;
        }
        // 🚫 STOP tower logic during magnet
        if (rootController != null && rootController.IsMagnetActive())
        {
            return;
        }
        // === TOWER LOGIC ===
        if (tower == null || towerCollider == null) return;

        if (wrapCooldownTimer > 0f)
            wrapCooldownTimer -= Time.deltaTime;

        // Wrap detection
        if (currentFace != -1 && wrapCooldownTimer <= 0f &&
        (rootController == null || !rootController.IsMagnetActive()))
        {
            int crossDirection = CheckEdgeCrossing(currentFace);
            if (crossDirection != 0)
            {
                int oldFace = currentFace;
                int newFace = GetWrappedFace(oldFace, crossDirection);

                LogDiagnostic("PRE-WRAP", oldFace);

                // Compute corner BEFORE rotating the player, since we need the player's
                // current Y position in tower-local space (which is preserved through
                // the wrap, but cleaner to compute once here)
                Vector3 cornerPos = ComputeCornerWorldPos(oldFace, newFace);

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

                if (stemTrail != null) stemTrail.OnPlayerWrapped(cornerPos);

                return;
            }
        }

        // Top-edge detection
        if (currentFace != -1)
{
    Bounds localBounds = GetTowerLocalBounds();
    Vector3 localPos = tower.InverseTransformPoint(transform.position);
    Vector3 relPos = localPos - localBounds.center;

            if (relPos.y >= localBounds.extents.y)
            {
                if (magnet != null)
                {
                    magnet.ActivateMagnet();
                }
            }
        }

        // Maintain surface adherence on the current face — does NOT redetect face
        if (currentFace != -1)
        {
            if (rootController == null || !rootController.IsMagnetActive())
            {
                SnapToFace(currentFace);
            }
        }
        else
        {
            // First frame: auto-detect the initial face
            currentFace = SnapToTowerSurface();
        }
    }

    Vector3 ResolveMovement(Vector3 motion)
    {
        float distance = motion.magnitude;
        if (distance < 0.0001f) return Vector3.zero;

        Vector3 direction = motion / distance;
        float radius = sphere.radius * transform.lossyScale.x;
        Vector3 origin = transform.position + sphere.center;

        if (Physics.SphereCast(origin, radius, direction, out RaycastHit hit,
                               distance + skinWidth, obstacleLayers,
                               QueryTriggerInteraction.Ignore))
        {
            float allowed = Mathf.Max(0f, hit.distance - skinWidth);
            return direction * allowed;
        }

        return motion;
    }

    /// <summary>
    /// Returns +1 for CCW-edge crossing, -1 for CW-edge crossing, 0 for no crossing.
    /// </summary>
    int CheckEdgeCrossing(int face)
    {
        Bounds localBounds = GetTowerLocalBounds();
        Vector3 localPos = tower.InverseTransformPoint(transform.position);
        Vector3 relPos = localPos - localBounds.center;
        Vector3 extents = localBounds.extents;

        float inFaceCoord;
        float threshold;
        int ccwSign;

        switch (face)
        {
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

    int GetWrappedFace(int oldFace, int direction)
    {
        int[] ccwNext = { 3, 2, 0, 1 };
        int[] cwNext = { 2, 3, 1, 0 };
        return direction > 0 ? ccwNext[oldFace] : cwNext[oldFace];
    }

    void WrapAroundTower(int rotationDirection)
    {
        float angle = 90f * rotationDirection;
        transform.Rotate(tower.up, angle, Space.World);
    }

    void SnapToFace(int face)
    {
        Bounds localBounds = GetTowerLocalBounds();
        Vector3 center = localBounds.center;
        Vector3 extents = localBounds.extents;

        Vector3 localPos = tower.InverseTransformPoint(transform.position);
        Vector3 relPos = localPos - center;

        switch (face)
        {
            case 0: relPos.x = extents.x + playerRadius / tower.lossyScale.x; break;
            case 1: relPos.x = -extents.x - playerRadius / tower.lossyScale.x; break;
            case 2: relPos.z = extents.z + playerRadius / tower.lossyScale.z; break;
            case 3: relPos.z = -extents.z - playerRadius / tower.lossyScale.z; break;
        }

        transform.position = tower.TransformPoint(relPos + center);
    }

    void PullAwayFromOldFace(int oldFace)
    {
        Bounds localBounds = GetTowerLocalBounds();
        Vector3 center = localBounds.center;
        Vector3 extents = localBounds.extents;

        Vector3 localPos = tower.InverseTransformPoint(transform.position);
        Vector3 relPos = localPos - center;

        switch (oldFace)
        {
            case 0: relPos.x = extents.x - wrapInset; break;
            case 1: relPos.x = -extents.x + wrapInset; break;
            case 2: relPos.z = extents.z - wrapInset; break;
            case 3: relPos.z = -extents.z + wrapInset; break;
        }

        transform.position = tower.TransformPoint(relPos + center);
    }

    int SnapToTowerSurface()
    {
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

    Bounds GetTowerLocalBounds()
    {
        if (towerCollider is MeshCollider mc && mc.sharedMesh != null)
            return mc.sharedMesh.bounds;
        if (towerCollider is BoxCollider bc)
            return new Bounds(bc.center, bc.size);

        return new Bounds(Vector3.zero, Vector3.one);
    }

    /// <summary>
    /// Computes the world-space position of the corner edge between two faces, at the
    /// player's current local Y. Used by StemTrail to bend the line cleanly at corners.
    /// </summary>
    Vector3 ComputeCornerWorldPos(int oldFace, int newFace)
    {
        Bounds localBounds = GetTowerLocalBounds();
        Vector3 center = localBounds.center;
        Vector3 extents = localBounds.extents;

        Vector3 playerLocal = tower.InverseTransformPoint(transform.position);
        float localY = playerLocal.y;

        // Determine corner X and Z signs based on which faces are involved
        bool cornerPlusX = (oldFace == 0 || newFace == 0);
        bool cornerPlusZ = (oldFace == 2 || newFace == 2);

        float cornerX = (cornerPlusX ? extents.x : -extents.x) + center.x;
        float cornerZ = (cornerPlusZ ? extents.z : -extents.z) + center.z;

        // Push outward by player radius along both axes so the corner sits at the
        // same effective distance from the tower as the player's surface
        float xPush = playerRadius / tower.lossyScale.x * (cornerPlusX ? 1f : -1f);
        float zPush = playerRadius / tower.lossyScale.z * (cornerPlusZ ? 1f : -1f);

        Vector3 cornerLocal = new Vector3(cornerX + xPush, localY, cornerZ + zPush);
        return tower.TransformPoint(cornerLocal);
    }

    void LogDiagnostic(string label, int faceContext)
    {
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