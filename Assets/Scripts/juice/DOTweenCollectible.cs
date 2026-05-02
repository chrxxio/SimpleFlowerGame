using UnityEngine;
using DG.Tweening;

public class DOTweenCollectible : MonoBehaviour
{
    [Header("Rotation")]
    public Vector3 rotationAxis = Vector3.up;
    public float rotationDuration = 2f; // time for full 360

    [Header("Floating")]
    public float floatHeight = 0.5f;
    public float floatDuration = 1.5f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;

        // 🔄 Continuous rotation
        transform
            .DORotate(rotationAxis * 360f, rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);

        // 🌊 Floating up and down
        transform
            .DOMoveY(startPos.y + floatHeight, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
