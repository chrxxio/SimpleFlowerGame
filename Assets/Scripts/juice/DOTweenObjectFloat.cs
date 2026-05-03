using DG.Tweening;
using UnityEngine;

public class DOTweenObjectFloat : MonoBehaviour
{
    public float floatAmount = 0.5f;   // how high it moves
    public float duration = 1f;        // speed

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;

        transform.DOMoveY(startPos.y + floatAmount, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
