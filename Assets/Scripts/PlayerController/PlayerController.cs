using UnityEngine;
using UnityEngine.InputSystem;

namespace Train.Player
{
    using Manager;

    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody m_rb;
        [SerializeField] private Camera m_camera;

        [Header("Camera")]
        [SerializeField] private Vector3 m_cameraOffset = new(0f, 9.5f, -3f);
        [SerializeField] private Vector3 m_cameraEulerRotation = new(80f, 0f, 0f);

        [Header("Movement")]
        [SerializeField, Min(0f)] private float m_moveSpeed = 5f;

        [Header("Turning")]
        [SerializeField] private float m_maxTurnSpeed;

        [Header("Boost")]
        [SerializeField, Min(0f)] private float m_boostSpeed = 10f;
        [SerializeField, Min(0f)] private float m_boostAcceleration;
        [SerializeField, Min(0f)] private float m_boostDeceleration;

        // movement
        private Plane m_playerLevel;
        private Vector2 m_mousePos;
        private Vector3 m_targetPos;
        private float m_currentSpeed;

        // boosting
        private bool m_isBoosting = false;

        // movement properties
        public float CurrentSpeed => m_currentSpeed;
        public float MoveSpeed => m_moveSpeed;
        public float BoostSpeed => m_boostSpeed;

        // events
        public event System.Action OnBeginBoosting;
        public event System.Action OnEndBoosting;

        private void Awake()
        {
            m_playerLevel = new Plane(Vector3.up, -transform.position.y);
            m_camera.transform.rotation = Quaternion.Euler(m_cameraEulerRotation);
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

            // get target direction
            Vector3 dirToTarget = (m_targetPos - m_rb.position).normalized;

            // Rotate with max turn speed
            if (dirToTarget != Vector3.zero)
                transform.rotation = Quaternion.RotateTowards(m_rb.rotation, Quaternion.LookRotation(dirToTarget), m_maxTurnSpeed * Time.fixedDeltaTime);

            // move in forward direction
            if (m_isBoosting)
            {
                m_currentSpeed = Mathf.Min(m_currentSpeed + m_boostAcceleration * Time.fixedDeltaTime, m_boostSpeed);
            }
            else
            {
                m_currentSpeed = Mathf.Max(m_currentSpeed - m_boostDeceleration * Time.fixedDeltaTime, m_moveSpeed);
            }
            Vector3 velocity = m_currentSpeed * transform.forward;
            m_rb.linearVelocity = velocity;
            
        }

        public void OnMouseMove(InputAction.CallbackContext context)
        {
            m_mousePos = context.ReadValue<Vector2>();
        }

        public void OnPress(InputAction.CallbackContext context)
        {
            if (context.performed)
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
