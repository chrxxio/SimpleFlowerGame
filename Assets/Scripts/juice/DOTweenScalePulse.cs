using UnityEngine;
using DG.Tweening;

public class DOTweenScalePulse : MonoBehaviour
{
    public Vector3 targetScale = new Vector3(1.2f, 1.2f, 1.2f);
    public float duration = 0.5f;

    void Start()
    {
        transform.DOScale(targetScale, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
