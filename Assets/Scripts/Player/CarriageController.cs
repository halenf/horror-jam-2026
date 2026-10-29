using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

namespace Train.Player
{
    public class CarriageController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController m_player;
        [SerializeField] private FollowerController m_rootFollower;
        [SerializeField] private FollowerController m_carriagePrefab;

        [Header("Fields")]
        [SerializeField, Min(0f)] private float m_followDistance = 1.3f;

        private List<FollowerController> m_createdCarriages = new();

        private void Start()
        {
            m_createdCarriages.Add(m_rootFollower);
        }

        [ContextMenu("Add Carriage")]
        public void AddCarriage()
        {
            // get reference to previous carriage
            FollowerController prevCarriage = m_createdCarriages[^1];

            // determine spawn offset from player's current position and rotation
            Quaternion rotation = prevCarriage.transform.rotation;
            Vector3 offset = prevCarriage.transform.position + rotation * new Vector3(0f, 0f, -m_followDistance);

            // create new Carriage object
            FollowerController newCarriage = Instantiate(m_carriagePrefab, offset, rotation, transform);
            newCarriage.gameObject.layer = LayerMask.NameToLayer("Player");

            // disable the new Carriage as it will be enabled when it gains a follower
            newCarriage.enabled = false;

            // set the new carriage as the follower of the previous one and start it
            m_createdCarriages.Add(newCarriage);
            prevCarriage.Begin(m_player, newCarriage);
        }

        public void DisableAllCarriages(float disableDuration)
        {
            foreach (FollowerController f in m_createdCarriages)
            {
                f.OnCollideWithPlayer(disableDuration);
            }
        }

#if UNITY_EDITOR
        public void AddCarriage_EditorOnly(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                AddCarriage();
            }
        }
#endif
    }
}
