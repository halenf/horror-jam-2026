using UnityEngine;

namespace Train.Collectibles
{
    [RequireComponent(typeof(Collider))]
    public class CollectibleDetector : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Collectible collectible))
            {
                collectible.Collect();
            }
        }
    }
}
