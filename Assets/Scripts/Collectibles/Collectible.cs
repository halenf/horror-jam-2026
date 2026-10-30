using UnityEngine;

namespace Train.Collectibles
{
    using Manager;
    using Player;
    using System;

    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class Collectible : MonoBehaviour
    {
        private enum CollectibleState
        {
            Idle,
            Notified,
            BeingEaten
        }

        [Header("References")]
        [SerializeField] protected Rigidbody m_rb;
        [SerializeField] protected Animator m_anim;
        [SerializeField] protected AudioSource m_audioSource;

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
        private CollectibleState m_state;
        private float m_stateTimer;
        private float m_collectDelay;
        private Transform m_beingEatenTarget;

        private void Update()
        {
            m_stateTimer += Time.deltaTime;

            if (m_state == CollectibleState.BeingEaten)
            {
                // if should be collected
                if (m_stateTimer >= m_collectDelay)
                {
                    Collect();
                    return;
                }

                transform.SetPositionAndRotation(m_beingEatenTarget.position - m_beingEatenTarget.forward, m_beingEatenTarget.rotation * Quaternion.Euler(new(-90f, 0, 180f)));
            }
        }

        private void FixedUpdate()
        {
            if (m_state == CollectibleState.Notified)
            {
                Vector3 dirVector = transform.position - m_player.transform.position;
                dirVector.y = 0f; // flatten, no Y-axis movement
                Vector3 dirNormalised = dirVector.normalized;

                // rotate towards movement direction
                m_rb.MoveRotation(Quaternion.RotateTowards(m_rb.rotation, Quaternion.LookRotation(dirVector), m_turnSpeed * Time.fixedDeltaTime));

                // move away from player
                float speed = GetMovementSpeed();
                m_rb.linearVelocity = speed * dirNormalised;
                //m_rb.MovePosition(m_rb.position + speed * Time.fixedDeltaTime * dirNormalised);

                if (m_anim != null)
                {
                    m_anim.SetFloat("MoveSpeed", speed / m_maxMoveSpeed);
                }
            }
        }

        /// <summary>
        /// Let the Collectible know that the player is close by
        /// </summary>
        public virtual void Notify(PlayerController player)
        {
            if (m_state == CollectibleState.Idle)
            {
                m_player = player;
                m_state = CollectibleState.Notified;
                m_stateTimer = 0f;

                if (m_anim != null)
                {
                    m_anim.SetBool("IsRunning", true);
                }
            }
        }

        /// <summary>
        /// Being eaten, in Update() waits until timer hits delay then gets collected.
        /// </summary>
        public virtual void BeginCollect(float delay, Transform target)
        {
            if (m_state == CollectibleState.Notified)
            {
                m_state = CollectibleState.BeingEaten;
                m_rb.isKinematic = true;
                m_stateTimer = 0f;
                m_collectDelay = delay;
                m_beingEatenTarget = target;

                // play sound
                if (m_audioSource != null)
                {
                    m_audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.08f);
                    m_audioSource.Play();
                }

                if (m_anim != null)
                {
                    m_anim.SetTrigger("GetEaten");
                }
            }
        }
        /// <summary>
        /// Collect without being eaten, or called after finished being eaten.
        /// </summary>
        public virtual void Collect()
        {
            m_state = CollectibleState.Idle;
            m_rb.isKinematic = false;
            TimerScript.Instance.AddTime(m_value);
            onCollected.Invoke(this);
        }

        /// <summary>
        /// Reset values after being respawned.
        /// </summary>
        public void OnRespawn()
        {
            m_stateTimer = 0f;
        }

        private float GetMovementSpeed()
        {
            float t = Mathf.Min(m_stateTimer / m_moveSpeedDecayDuration, 1f);
            return m_moveSpeedDecayFunction.Evaluate(t) * (m_maxMoveSpeed - m_minMoveSpeed) + m_minMoveSpeed;
        }
    }
}
