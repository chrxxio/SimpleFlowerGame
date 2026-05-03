using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public CollectibleCounter collectibleCounter;
    [SerializeField] private AudioClip CollectedSFX;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.Instance.PlaySFX(CollectedSFX);
            collectibleCounter.AddCollectible();
            Destroy(gameObject);
        }
    }
}