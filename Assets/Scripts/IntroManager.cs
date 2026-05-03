using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class IntroSequence : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private Transform mound;
    [SerializeField] private GameObject aPrompt;
    [SerializeField] private GameObject dPrompt;
    [SerializeField] private Transform player;
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private StemTrail stemTrail;

    [Header("Pulsate")]
    [SerializeField] private float pulsateScaleIncrease = 0.2f;
    [SerializeField] private float pulsateDuration = 0.3f;

    [Header("Sprout")]
    [SerializeField] private float sproutDuration = 0.6f;

    private enum State { AwaitA1, AwaitD1, AwaitA2, AwaitD2, Pulsating, Sprouting, Playing }
    private State state = State.AwaitA1;
    private State stateAfterPulse;

    void Start() {
        if (aPrompt != null) aPrompt.SetActive(true);
        if (dPrompt != null) dPrompt.SetActive(false);
        if (playerRenderer != null) playerRenderer.enabled = false;
        if (playerController != null) playerController.enabled = false;
        if (stemTrail != null) stemTrail.enabled = false;
    }

    void Update() {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (state == State.AwaitA1 && kb.aKey.wasPressedThisFrame) {
            if (aPrompt != null) aPrompt.SetActive(false);
            stateAfterPulse = State.AwaitD1;
            state = State.Pulsating;
            StartCoroutine(PulsateThen(OnPulseComplete));
        } else if (state == State.AwaitD1 && kb.dKey.wasPressedThisFrame) {
            if (dPrompt != null) dPrompt.SetActive(false);
            stateAfterPulse = State.AwaitA2;
            state = State.Pulsating;
            StartCoroutine(PulsateThen(OnPulseComplete));
        } else if (state == State.AwaitA2 && kb.aKey.wasPressedThisFrame) {
            if (aPrompt != null) aPrompt.SetActive(false);
            stateAfterPulse = State.AwaitD2;
            state = State.Pulsating;
            StartCoroutine(PulsateThen(OnPulseComplete));
        } else if (state == State.AwaitD2 && kb.dKey.wasPressedThisFrame) {
            if (dPrompt != null) dPrompt.SetActive(false);
            stateAfterPulse = State.Sprouting;
            state = State.Pulsating;
            StartCoroutine(PulsateThen(OnPulseComplete));
        }
    }

    void OnPulseComplete() {
        state = stateAfterPulse;

        switch (state) {
            case State.AwaitD1:
            case State.AwaitD2:
                if (dPrompt != null) dPrompt.SetActive(true);
                break;
            case State.AwaitA2:
                if (aPrompt != null) aPrompt.SetActive(true);
                break;
            case State.Sprouting:
                StartCoroutine(Sprout());
                break;
        }
    }

    IEnumerator PulsateThen(System.Action onComplete) {
        if (mound == null) { onComplete?.Invoke(); yield break; }

        Vector3 baseScale = mound.localScale;
        float t = 0f;
        while (t < pulsateDuration) {
            t += Time.deltaTime;
            float pulse = Mathf.Sin(t / pulsateDuration * Mathf.PI); // 0 -> 1 -> 0
            mound.localScale = baseScale * (1f + pulsateScaleIncrease * pulse);
            yield return null;
        }
        mound.localScale = baseScale;
        onComplete?.Invoke();
    }

    IEnumerator Sprout() {
        Vector3 endPos = player.position;
        Vector3 startPos = mound.position;

        player.position = startPos;
        if (playerRenderer != null) playerRenderer.enabled = true;
        if (stemTrail != null) stemTrail.enabled = true;

        // Wait one frame so StemTrail's Start() runs with the player at startPos
        yield return null;

        float t = 0f;
        while (t < sproutDuration) {
            t += Time.deltaTime;
            player.position = Vector3.Lerp(startPos, endPos, t / sproutDuration);
            yield return null;
        }
        player.position = endPos;

        if (playerController != null) playerController.enabled = true;
        state = State.Playing;
    }
}