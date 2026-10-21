using UnityEngine;

namespace Train.Destroyables
{
    [RequireComponent(typeof(Collider))]
    public class DestroyableDetector : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Destroyable destroyable))
            {
                destroyable.Destroy();
            }
        }
    }
}
