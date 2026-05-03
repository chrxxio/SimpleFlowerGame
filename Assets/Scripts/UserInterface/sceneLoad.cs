using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneLoad : MonoBehaviour
{
    public void LoadMyScene(int scene)
    {
        SceneManager.LoadScene(scene); // replace with your scene name
    }
}
