using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Train.Player
{
    using System.Collections;
    using System.Threading.Tasks;
    using Manager;
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody m_rb;
        [SerializeField] private CapsuleCollider m_col;
        [SerializeField] private Camera m_camera;
        [SerializeField] private CarriageController m_carriages;

        [Header("Camera")]
        [SerializeField] private Vector3 m_cameraOffset = new(0f, 9.5f, -3f);
        [SerializeField] private Vector3 m_cameraEulerRotation = new(80f, 0f, 0f);

        [Header("Movement")]
        [SerializeField, Min(0f)] private float m_moveSpeed = 5f;
        [SerializeField] private float m_maxTurnSpeed;

        [Header("Boost")]
        [SerializeField, Min(0f)] private float m_boostSpeed = 10f;
        [SerializeField, Min(0f)] private float m_boostAcceleration;
        [SerializeField, Min(0f)] private float m_boostDeceleration;

        [Header("Wall Flip")]
        [Tooltip("When the Player collides with one of its Carriages, it becomes disabled for this amount of time, and then is reenabled.")]
        [SerializeField, Min(0f)] private float m_collideDisableDuration = 2f;
        [Tooltip("How much time you lose when you flip from hitting something.")]
        [SerializeField, Min(0f)] private float m_timeLossOnFlip = 2.5f;
        [SerializeField, Min(0f)] private float m_wallFlipCastRadius = 0.5f;
        [SerializeField, Min(0f)] private float m_wallFlipCastMaxDistance = 3f;
        [SerializeField] private LayerMask m_wallFlipRaycastMask;

        [Header("Game Over")]
        [SerializeField] private ParticleSystem m_deathVFX;
        [Tooltip("The deceleration the player experiences while they are in the grace period before they Game Over.")]
        [SerializeField, Min(0f)] private float m_gracePeriodDeceleration = 6f;

        // movement
        private Plane m_playerLevel;
        private Vector2 m_mousePos;
        private Vector3 m_targetPos;
        private float m_currentSpeed;

        // boosting
        private bool m_isBoosting = false;

        // game over
        private float m_gracePeriodCountdown;

        // movement properties
        public float CurrentSpeed => m_rb.linearVelocity.magnitude;
        public float MoveSpeed => m_moveSpeed;
        public float BoostSpeed => m_boostSpeed;
        public float MaxTurnSpeed => m_maxTurnSpeed;

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
            UpdateMovement();
        }

        #region Input
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
        #endregion

        #region Movement
        private void UpdateMovement()
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

            // determine speed
            // if in grace period before game over, slow down to 0
            if (m_gracePeriodCountdown > 0f)
            {
                m_currentSpeed = Mathf.Max(m_currentSpeed - m_gracePeriodDeceleration * Time.fixedDeltaTime, 0f);
            }
            // normal game movement speed stuff
            else
            {
                if (m_isBoosting)
                {
                    m_currentSpeed = Mathf.Min(m_currentSpeed + m_boostAcceleration * Time.fixedDeltaTime, m_boostSpeed);
                }
                else
                {
                    m_currentSpeed = Mathf.Max(m_currentSpeed - m_boostDeceleration * Time.fixedDeltaTime, m_moveSpeed);
                }
            }

            // move in forward direction
            Vector3 velocity = m_currentSpeed * transform.forward;
            m_rb.linearVelocity = velocity;
        }

        /// <summary>
        /// Flip direction based on surface normal when hitting object.
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionStay(Collision collision)
        {
            // use raycast to get surface normal.
            // use raycast to check for smaller objects, OR
            // use capsule cast to check area
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, m_wallFlipCastMaxDistance, m_wallFlipRaycastMask, QueryTriggerInteraction.Ignore))
            {
                Reflect(hit.normal);
            }
            else
            {
                // not using || (OR) to avoid needing to declare this before if statement, not always using them
                Vector3 p1 = transform.position + m_col.center + 0.5f * -m_col.height * Vector3.forward;
                Vector3 p2 = p1 + Vector3.forward * m_col.height;
                if (Physics.CapsuleCast(p1, p2, m_wallFlipCastRadius, transform.forward, out hit, m_wallFlipCastMaxDistance, m_wallFlipRaycastMask, QueryTriggerInteraction.Ignore))
                {
                    Reflect(hit.normal);
                }
            }

            void Reflect(Vector3 normal)
            {
                // disable all carriages for a few seconds
                m_carriages.DisableAllCollision(m_collideDisableDuration);

                // lose time
                TimerScript.Instance.LoseTime(m_timeLossOnFlip);

                // flip direction
                transform.forward = Vector3.Reflect(transform.forward, normal);
            }
        }
        #endregion

        #region Game Over
        public void StartSlowingDown(float gracePeriodDuration)
        {
            m_gracePeriodCountdown = gracePeriodDuration;
        }
        public void StopSlowingDown()
        {
            m_gracePeriodCountdown = 0f;
        }

        public void OnGameEnd()
        {
            Instantiate(m_deathVFX, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
            m_carriages.DisableAllObjects();
        }
        #endregion

#if UNITY_EDITOR
        [Header("Editor-Only")]
        [SerializeField] private bool m_drawGizmos = true;

        private void OnDrawGizmos()
        {
            if (!m_drawGizmos)
                return;

            Handles.matrix = transform.localToWorldMatrix;
            Handles.color = Color.lawnGreen;

            Vector3 p1 = m_col.center + 0.5f * -m_col.height * Vector3.forward + m_wallFlipCastRadius * Vector3.forward;
            Vector3 p2 = p1 + Vector3.forward * m_col.height - 2f * m_wallFlipCastRadius * Vector3.forward;

            Handles.DrawWireDisc(p1, Vector3.up, m_wallFlipCastRadius);
            Handles.DrawWireDisc(p1, Vector3.right, m_wallFlipCastRadius);
            Handles.DrawWireDisc(p1, Vector3.forward, m_wallFlipCastRadius);

            Handles.DrawWireDisc(p2, Vector3.up, m_wallFlipCastRadius);
            Handles.DrawWireDisc(p2, Vector3.right, m_wallFlipCastRadius);
            Handles.DrawWireDisc(p2, Vector3.forward, m_wallFlipCastRadius);

            Vector3 castOffset = Vector3.forward * m_wallFlipCastMaxDistance;

            Handles.DrawWireDisc(p1 + castOffset, Vector3.up, m_wallFlipCastRadius);
            Handles.DrawWireDisc(p1 + castOffset, Vector3.right, m_wallFlipCastRadius);
            Handles.DrawWireDisc(p1 + castOffset, Vector3.forward, m_wallFlipCastRadius);

            Handles.DrawWireDisc(p2 + castOffset, Vector3.up, m_wallFlipCastRadius);
            Handles.DrawWireDisc(p2 + castOffset, Vector3.right, m_wallFlipCastRadius);
            Handles.DrawWireDisc(p2 + castOffset, Vector3.forward, m_wallFlipCastRadius);
        }
#endif
    }
}
