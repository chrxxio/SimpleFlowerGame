using DG.Tweening;
using UnityEngine;

public class DOTweenRotateBetween : MonoBehaviour
{
    public Vector3 minRotation;
    public Vector3 maxRotation;
    public float duration = 2f;

    void Start()
    {
        transform.rotation = Quaternion.Euler(minRotation);

        transform.DORotate(maxRotation, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}