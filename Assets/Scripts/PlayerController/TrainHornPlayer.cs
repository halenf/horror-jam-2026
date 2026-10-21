using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

namespace Train.PlayerController
{
    public class TrainHornPlayer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AudioSource m_hornAudioSource;
        [SerializeField] private AudioMixer m_hornMixer;

        [Header("Playback")]
        [SerializeField, Min(0f)] private float m_fadeInSpeed = 1f;
        [SerializeField, Min(0f)] private float m_fadeOutSpeed = 1f;
        [SerializeField, Range(0.5f, 2f)] private float m_randomPitchRangeLower = 0.5f;
        [SerializeField, Range(0.5f, 2f)] private float m_randomPitchRangeUpper = 1.5f;

        private bool m_isHorny = false;

        public bool HornStillPlaying => m_hornAudioSource.volume > 0f;

        private void Update()
        {                        
            if (m_isHorny)
            {
                if (m_hornAudioSource.volume < 1f)
                {
                    m_hornAudioSource.volume = Mathf.Min(1f, m_hornAudioSource.volume + m_fadeInSpeed * Time.deltaTime);
                }
            }
            else if (m_hornAudioSource.volume > 0f)
            {
                m_hornAudioSource.volume = Mathf.Max(0f, m_hornAudioSource.volume - m_fadeOutSpeed * Time.deltaTime);
                if (m_hornAudioSource.volume == 0f)
                {
                    m_hornAudioSource.Stop();
                }
            }
        }

        private void OnValidate()
        {
            if (m_randomPitchRangeUpper < m_randomPitchRangeLower || m_randomPitchRangeLower > m_randomPitchRangeUpper)
            {
                m_randomPitchRangeUpper = m_randomPitchRangeLower;
            }
        }

        private void PlayHorn()
        {
            float randomPitch = Random.Range(m_randomPitchRangeLower, m_randomPitchRangeUpper);
            m_hornMixer.SetFloat("HornPitch", randomPitch);
            m_hornAudioSource.Play();
        }

        public void OnHornPress(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                m_isHorny = true;
                if (!m_hornAudioSource.isPlaying)
                    PlayHorn();
            }
            else if (context.canceled)
            {
                m_isHorny = false;
            }
        }
    }
}
