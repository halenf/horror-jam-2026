using UnityEngine;

namespace Train.Player
{
    public class TrainEyeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator m_anim;

        [Header("Fields")]
        [SerializeField, Min(0f)] private float m_blinkPeriodLower;
        [SerializeField, Min(0f)] private float m_blinkPeriodUpper;

        private float m_blinkCountdown;

        private void Start()
        {
            m_blinkCountdown = GetBlinkPeriod();
        }

        private void Update()
        {
            if (m_blinkCountdown > 0f)
            {
                m_blinkCountdown -= Time.deltaTime;
            }
            else
            {
                Blink();
                m_blinkCountdown = GetBlinkPeriod();
            }
        }

        private void Blink()
        {
            if (Random.value >= 0.5f)
                m_anim.Play("Blink");
            else
                m_anim.Play("Look");
        }

        private float GetBlinkPeriod()
        {
            return Random.Range(m_blinkPeriodLower, m_blinkPeriodUpper);
        }

        private void OnValidate()
        {
            if (m_blinkPeriodUpper < m_blinkPeriodLower || m_blinkPeriodLower > m_blinkPeriodUpper)
            {
                m_blinkPeriodUpper = m_blinkPeriodLower;
            }
        }
    }
}
