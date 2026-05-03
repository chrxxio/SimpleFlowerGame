using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class CircleInverseFade : MonoBehaviour
{
    [Header("Material")]
    public Material circleMaskMaterial;

    [Header("Settings")]
    public float maxRadius = 1f;     // fully open
    public float minRadius = 0f;     // fully closed
    public float duration = 0.8f;

    [Header("Behavior")]
    public bool playFadeInOnStart = true; // <-- THIS is your toggle
    public int sceneToLoad;

    private bool fading;

    void Start()
    {

        if (playFadeInOnStart)
        {
            // Start closed (black), then open
            circleMaskMaterial.SetFloat("_Radius", minRadius);

            circleMaskMaterial
                .DOFloat(maxRadius, "_Radius", duration)
                .SetEase(Ease.OutQuad);
        }
        else
        {
            // Start fully open (normal gameplay view)
            circleMaskMaterial.SetFloat("_Radius", maxRadius);
        }
    }

    public void FadeOutToScene()
    {
        if (fading) return;
        fading = true;

        // Shrink hole → black covers screen
        circleMaskMaterial
            .DOFloat(minRadius, "_Radius", duration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                SceneManager.LoadScene(sceneToLoad);
            });
    }
}