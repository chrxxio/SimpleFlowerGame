using UnityEngine;
using UnityEngine.UI;

public class CollectibleCounter : MonoBehaviour
{
    [Header("Collectibles")]
    public int totalCollectibles = 5;
    private int collectedCount = 0;

    [Header("UI Images")]
    public Image[] collectibleImages;

    void Start()
    {
        UpdateUI();
    }

    public void AddCollectible()
    {
        collectedCount++;

        if (collectedCount > totalCollectibles)
            collectedCount = totalCollectibles;

        UpdateUI();
    }

    void UpdateUI()
    {
        for (int i = 0; i < collectibleImages.Length; i++)
        {
            if (collectibleImages[i] != null)
            {
                collectibleImages[i].enabled = i < collectedCount;
            }
        }
    }
}