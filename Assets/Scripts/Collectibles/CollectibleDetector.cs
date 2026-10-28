using UnityEngine;

namespace Train.Collectibles
{
    [RequireComponent(typeof(Collider))]
    public class CollectibleDetector : MonoBehaviour
    {
        [SerializeField] private Animator m_teethAnimator;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Collectible collectible))
            {
                collectible.Collect();
                m_teethAnimator.Play("Eat");
            }
        }
    }
}
