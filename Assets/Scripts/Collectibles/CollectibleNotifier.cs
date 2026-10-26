using UnityEngine;

namespace Train.Collectibles
{
    using Player;

    [RequireComponent(typeof(Collider))]
    public class CollectibleNotifier : MonoBehaviour
    {
        [SerializeField] private PlayerController m_player;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Collectible collectible))
            {
                collectible.Notify(m_player);
            }
        }
    }
}
