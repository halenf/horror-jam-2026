using UnityEngine;

namespace Train.Collectibles
{
    using Manager;
    using Player;
    using System;

    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class Collectible : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected Rigidbody m_rb;
        [SerializeField] protected Animator m_anim;

        [Header("Timer")]
        [SerializeField] protected float m_value = 3f;

        [Header("Movment")]
        [SerializeField, Min(0f)] protected float m_maxMoveSpeed = 3f;
        [SerializeField, Min(0f)] protected float m_minMoveSpeed = 0.5f;
        [SerializeField, Min(0f)] protected float m_moveSpeedDecayDuration = 3f;
        [SerializeField] protected AnimationCurve m_moveSpeedDecayFunction;
        [SerializeField, Min(0f)] protected float m_turnSpeed = 200f;

        public event Action<Collectible> onCollected;

        private PlayerController m_player;
        private bool m_notifiedOfPlayer = false;
        private float m_notifiedLifetime;

        private void Update()
        {
            if (m_notifiedOfPlayer)
            {
                m_notifiedLifetime += Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            if (m_notifiedOfPlayer)
            {
                Vector3 dirVector = transform.position - m_player.transform.position;
                dirVector.y = 0f; // flatten, no Y-axis movement
                Vector3 dirNormalised = dirVector.normalized;

                // move away from player
                float speed = GetMovementSpeed();
                m_rb.linearVelocity = speed * Time.fixedDeltaTime * dirNormalised;
                //m_rb.MovePosition(m_rb.position + speed * Time.fixedDeltaTime * dirNormalised);

                // rotate towards movement direction
                m_rb.MoveRotation(Quaternion.RotateTowards(m_rb.rotation, Quaternion.LookRotation(dirVector), m_turnSpeed * Time.fixedDeltaTime));

                if (m_anim != null)
                {
                    m_anim.SetFloat("MoveSpeed", speed / m_maxMoveSpeed);
                }
            }
        }

        public virtual void Initialise(float value)
        {
            m_value = value;
        }

        /// <summary>
        /// Let the Collectible know that the player is close by
        /// </summary>
        public virtual void Notify(PlayerController player)
        {
            m_player = player;
            m_notifiedOfPlayer = true;

            if (m_anim != null)
            {
                m_anim.SetBool("IsRunning", true);
            }
        }

        /// <summary>
        /// Destroy the Collectible and add its value to the timer
        /// </summary>
        public virtual void Collect()
        {
            TimerScript.Instance.AddTime(m_value);

            // despawn
            onCollected.Invoke(this);
        }

        public void OnRespawn()
        {
            m_notifiedOfPlayer = false;
            m_notifiedLifetime = 0f;
        }

        private float GetMovementSpeed()
        {
            float t = Mathf.Min(m_notifiedLifetime / m_moveSpeedDecayDuration, 1f);
            return m_moveSpeedDecayFunction.Evaluate(t) * (m_maxMoveSpeed - m_minMoveSpeed) + m_minMoveSpeed;
        }
    }
}
