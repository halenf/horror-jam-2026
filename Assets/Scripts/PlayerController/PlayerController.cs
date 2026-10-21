using UnityEngine;
using UnityEngine.InputSystem;

namespace Train.PlayerController
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody m_rb;
        [SerializeField] private Camera m_camera;

        [Header("Camera")]
        [SerializeField] private Vector3 m_cameraOffset = new(0, 9.5f, -3f);

        [Header("Movement")]
        [SerializeField, Min(0f)] private float m_moveSpeed;

        [Header("Boost")]
        [SerializeField, Range(1f, 10f)] private float m_boostSpeedMultiplier;

        // movement
        private static Plane m_playerLevel = new(Vector3.up, 0f);
        private Vector2 m_mousePos;
        private Vector3 m_targetPos;
        private float m_currentSpeed;

        // boosting
        private bool m_isBoosting = false;

        public float CurrentSpeed => m_currentSpeed;
        public float MoveSpeed => m_moveSpeed;
        public float BoostSpeed => m_moveSpeed * m_boostSpeedMultiplier;

        public event System.Action OnBeginBoosting;
        public event System.Action OnEndBoosting;

        private void Awake()
        {
            m_playerLevel = new Plane(Vector3.up, -transform.position.y);
        }

        private void LateUpdate()
        {
            m_camera.transform.position = transform.position + m_cameraOffset;
        }

        private void FixedUpdate()
        {
            // get intersect of mouse pos and y = 0
            Ray ray = m_camera.ScreenPointToRay(m_mousePos);
            if (m_playerLevel.Raycast(ray, out float enter))
            {
                m_targetPos = ray.GetPoint(enter);
            }

            // move by transform
            //Vector3 newPos = Vector3.MoveTowards(m_rb.position, m_targetPos, m_moveSpeed * Time.fixedDeltaTime);
            //m_rb.MovePosition(newPos);

            // move by velocity
            Vector3 dirToTarget = (m_targetPos - m_rb.position).normalized;
            m_currentSpeed = m_moveSpeed * (m_isBoosting ? m_boostSpeedMultiplier : 1f);
            Vector3 velocity = m_currentSpeed * dirToTarget;
            m_rb.linearVelocity = velocity;

            // turn towards target
            transform.forward = dirToTarget;
        }

        public void OnMouseMove(InputAction.CallbackContext context)
        {
            m_mousePos = context.ReadValue<Vector2>();
        }

        public void OnPress(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                m_isBoosting = true;
                OnBeginBoosting.Invoke();
            }
            else if (context.canceled)
            {
                m_isBoosting = false;
                OnEndBoosting.Invoke();
            }
        }
    }
}
