using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

namespace Train.Player
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

        private HornyState m_hornyState = HornyState.Waiting;

        public bool HornStillPlaying => m_hornAudioSource.volume > 0f;

        private void Update()
        {
            switch (m_hornyState)
            {
                case HornyState.Waiting:
                    return;
                case HornyState.Horny:
                    if (m_hornAudioSource.volume < 1f)
                    {
                        m_hornAudioSource.volume = Mathf.Min(1f, m_hornAudioSource.volume + m_fadeInSpeed * Time.deltaTime);
                    }
                    // still holding button but sound is finished playing
                    if (!m_hornAudioSource.isPlaying)
                    {
                        m_hornAudioSource.volume = 0f;
                        m_hornyState = HornyState.Waiting;
                    }
                    break;
                case HornyState.Aftercare:
                    if (m_hornAudioSource.volume > 0f)
                    {
                        m_hornAudioSource.volume = Mathf.Max(0f, m_hornAudioSource.volume - m_fadeOutSpeed * Time.deltaTime);
                        if (m_hornAudioSource.volume == 0f)
                        {
                            StopHorn();
                        }
                    }
                    break;
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
            m_hornyState = HornyState.Horny;
            float randomPitch = Random.Range(m_randomPitchRangeLower, m_randomPitchRangeUpper);
            m_hornMixer.SetFloat("HornPitch", randomPitch);
            m_hornAudioSource.Play();
        }
        private void StopHorn()
        {
            m_hornyState = HornyState.Waiting;
            m_hornAudioSource.Stop();
        }

        public void OnHornPress(InputAction.CallbackContext context)
        {
            if (context.performed && m_hornyState == HornyState.Waiting)
            {
                PlayHorn();
            }
            else if (context.canceled && m_hornyState == HornyState.Horny)
            {
                m_hornyState = HornyState.Aftercare;
            }
        }

        private enum HornyState
        {
            Waiting,
            Horny,
            Aftercare
        }
    }
}
