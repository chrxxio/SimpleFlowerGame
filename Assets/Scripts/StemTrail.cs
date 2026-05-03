using UnityEngine;

public class StemTrail : MonoBehaviour {
    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private float segmentRadius = 0.5f;
    [SerializeField] private float playerRadius = 0.5f;
    [SerializeField] private Transform stemContainer; // optional parent for hierarchy tidiness

    private Vector3 lastSpawnPos;
    private float spacing;        // = 2 * segmentRadius
    private float spawnOffset;    // = playerRadius + segmentRadius

    void Start() {
        spacing = 2f * segmentRadius;
        spawnOffset = playerRadius + segmentRadius;

        // Drop the first segment immediately behind the player at spawn
        SpawnSegment();
        lastSpawnPos = transform.position;
    }

    void Update() {
        float distanceSinceLast = Vector3.Distance(transform.position, lastSpawnPos);
        if (distanceSinceLast >= spacing) {
            SpawnSegment();
            lastSpawnPos = transform.position;
        }
    }

    void SpawnSegment() {
        // Spawn behind the player, opposite the direction they're facing
        Vector3 spawnPos = transform.position - transform.up * spawnOffset;
        GameObject segment = Instantiate(segmentPrefab, spawnPos, Quaternion.identity, stemContainer);
    }
}