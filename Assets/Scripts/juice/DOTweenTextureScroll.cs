using UnityEngine;
using DG.Tweening;

public class DOTweenTextureScroll : MonoBehaviour
{
    [Header("Material")]
    public Renderer targetRenderer;
    public string textureProperty = "_MainTex";

    [Header("Scroll")]
    public Vector2 scrollAmount = new Vector2(1f, 0f);
    public float duration = 2f;

    private Material mat;
    private Tween scrollTween;

    void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        // .material creates an instance so you don't edit the shared material asset
        mat = targetRenderer.material;

        scrollTween = mat.DOOffset(scrollAmount, textureProperty, duration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
    }

    void OnDestroy()
    {
        scrollTween?.Kill();

        if (mat != null)
            Destroy(mat);
    }
}
