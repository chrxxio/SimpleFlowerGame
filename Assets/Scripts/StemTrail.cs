using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class StemTrail : MonoBehaviour {
    [SerializeField] private Transform player;
    [SerializeField] private float pointSpacing = 0.3f;

    private LineRenderer lr;
    private Vector3 lastCommittedPos;

    void Awake() {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 0;
    }

    void Start() {
        if (player == null) {
            Debug.LogError("StemTrail: player reference not set.");
            enabled = false;
            return;
        }

        // Drop the first committed point at the player's starting position,
        // plus a "live" point that will track the player.
        CommitPoint(player.position);
        AppendLivePoint(player.position);
    }

    void Update() {
        if (player == null) return;

        // Always update the live (last) point to match the player's position
        int liveIndex = lr.positionCount - 1;
        lr.SetPosition(liveIndex, player.position);

        // If the player has moved far enough from the last committed point,
        // promote the live point and add a new live one
        float distance = Vector3.Distance(player.position, lastCommittedPos);
        if (distance >= pointSpacing) {
            CommitPoint(player.position);
            AppendLivePoint(player.position);
        }
    }

    /// <summary>
    /// Called by the player when a wrap occurs. Inserts a corner point so the line
    /// bends cleanly at the tower's edge instead of cutting a diagonal across it.
    /// </summary>
    public void OnPlayerWrapped(Vector3 cornerWorldPos) {
        if (player == null) return;

        // The live point is currently sitting at the player's pre-wrap position
        // (Update updated it this frame). Append the corner point next, which makes
        // the previous live point implicitly permanent (no longer the tip).
        int newIndex = lr.positionCount;
        lr.positionCount = newIndex + 1;
        lr.SetPosition(newIndex, cornerWorldPos);

        // Append a new live point at the player's post-wrap position
        AppendLivePoint(player.position);

        // Reset spacing reference to the new player position so the next commit
        // happens after a full pointSpacing of new movement
        lastCommittedPos = player.position;
    }

    void CommitPoint(Vector3 worldPos) {
        int newIndex = lr.positionCount;
        lr.positionCount = newIndex + 1;
        lr.SetPosition(newIndex, worldPos);
        lastCommittedPos = worldPos;
    }

    void AppendLivePoint(Vector3 worldPos) {
        int newIndex = lr.positionCount;
        lr.positionCount = newIndex + 1;
        lr.SetPosition(newIndex, worldPos);
    }
}