using UnityEngine;
using DG.Tweening;


public class DOTweenCameraShake : MonoBehaviour
{
    public Camera cam;

    public float duration = 0.3f;
    public float strength = 0.3f;
    public int vibrato = 20;
    private float randomness = 90;
    private bool fadeOut = true;
    private Tween currentShake;

    public void Shake()
    {
        // Kill existing shake so they don’t stack weirdly
        currentShake?.Kill();

        currentShake = cam.DOShakePosition(
            duration,
            strength,
            vibrato,
            randomness,
            fadeOut
        );
    }
}
