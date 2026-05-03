using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class ScreenshotCapture : MonoBehaviour
{
    public CameraFlowerOrbit orbitCam;
    public GameObject saveButtonUI;
    public GameObject savedPopup;

    private bool readyToCapture = false;
    private bool hasCaptured = false;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DownloadImage(byte[] array, int length, string filename);
#endif

    void Start()
    {
        if (saveButtonUI != null)
            saveButtonUI.SetActive(false);

        if (savedPopup != null)
            savedPopup.SetActive(false); // 👈 REQUIRED
    }

    void Update()
    {
        if (orbitCam == null) return;

        if (!readyToCapture && orbitCam.HasCompleted())
        {
            readyToCapture = true;

            if (saveButtonUI != null)
                saveButtonUI.SetActive(true);

            Debug.Log("📸 Ready to capture");
        }
    }

    public void OnSaveButtonPressed()
    {
        if (!readyToCapture || hasCaptured) return;

        hasCaptured = true;

        if (saveButtonUI != null)
            saveButtonUI.SetActive(false);

        StartCoroutine(Capture());
    }

    IEnumerator Capture()
    {
        yield return new WaitForEndOfFrame();

        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        Destroy(tex);

        string filename = "Flower_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";

#if UNITY_WEBGL && !UNITY_EDITOR
        DownloadImage(png, png.Length, filename);
        Debug.Log("📸 Saved to Downloads!");
#else
        string path = Application.dataPath + "/" + filename;
        System.IO.File.WriteAllBytes(path, png);
        Debug.Log("📸 Saved locally: " + path);
#endif

        Debug.Log("POPUP TRIGGERED");
        // ✅ SHOW POPUP
        if (savedPopup != null)
        {
            savedPopup.SetActive(true);
            Invoke(nameof(HidePopup), 2f);
        }
    }
    void HidePopup()
    {
        if (savedPopup != null)
            savedPopup.SetActive(false);
    }
}