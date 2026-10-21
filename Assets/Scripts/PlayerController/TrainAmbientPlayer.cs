using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Train.Player
{
    public class TrainAmbientPlayer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController m_player;
        [SerializeField] private AudioSource m_source1;
        [SerializeField] private AudioSource m_source2;
        [SerializeField] private AudioMixer m_mixer;

        [Header("Heartbeat Playback")]
        [SerializeField, Range(0f, 1.5f)] private float m_beatDelayLower;
        [SerializeField, Range(0f, 1.5f)] private float m_beatDelayUpper;
        [SerializeField, Range(0f, 5f)] private float m_beatFrequencyLower;
        [SerializeField, Range(0f, 5f)] private float m_beatFrequencyUpper;
        [SerializeField, Range(0.5f, 2f)] private float m_randomBeatPitchLower = 0.5f;
        [SerializeField, Range(0.5f, 2f)] private float m_randomBeatPitchUpper = 1.5f;

        private float m_frequencyCountdown;
        private float m_delayCountdown;

        private bool m_isBeating;

        private float m_speedLower;
        private float m_speedUpper;

        private void Start()
        {
            m_speedLower = m_player.MoveSpeed;
            m_speedUpper = m_player.BoostSpeed;
            m_frequencyCountdown = GetBeatFrequency();
        }

        private void Update()
        {
            // if time to start beats
            if (m_frequencyCountdown <= 0f)
            {
                // if already beating
                if (m_isBeating)
                {
                    // if time to play second beat
                    if (m_delayCountdown <= 0f)
                    {
                        // play last beat and reset frequency countdown
                        m_isBeating = false;
                        m_mixer.SetFloat("HeartbeatPitch", GetRandomBeatPitch());
                        m_source2.Play();
                        m_frequencyCountdown = GetBeatFrequency();
                    }
                    // if not time to play second beat
                    else
                    {
                        m_delayCountdown -= Time.deltaTime;
                    }
                }
                // if not yet beating
                else
                {
                    // start beating
                    m_isBeating = true;
                    m_mixer.SetFloat("HeartbeatPitch", GetRandomBeatPitch());
                    m_source1.Play();
                    m_delayCountdown = GetBeatDelay();
                }
            }
            // if not yet time to start beats
            else
            {
                m_frequencyCountdown -= Time.deltaTime;
            }
        }

        private void OnEnable()
        {
            m_player.OnBeginBoosting += OnBeginBoosting;
            m_player.OnEndBoosting += OnEndBoosting;
        }
        private void OnDisable()
        {
            m_player.OnBeginBoosting -= OnBeginBoosting;
            m_player.OnEndBoosting -= OnEndBoosting;
        }

        // gets progress inverted
        private float GetSpeedProgress()
        {
            return (m_player.CurrentSpeed - m_speedUpper) / (m_speedLower - m_speedUpper);
        }
        private float GetBeatDelay()
        {
            return m_beatDelayLower + (m_beatDelayUpper - m_beatDelayLower) * GetSpeedProgress();
        }
        // upper and lower swapped for functionality
        private float GetBeatFrequency()
        {
            return m_beatFrequencyLower + (m_beatFrequencyUpper - m_beatFrequencyLower) * GetSpeedProgress();
        }

        // on begin boost, subtract difference to lower countdowns
        private void OnBeginBoosting()
        {
            if (m_frequencyCountdown > 0)
            {
                m_frequencyCountdown -= m_beatFrequencyUpper - m_beatFrequencyLower;
            }
            if (m_delayCountdown > 0)
            {
                m_delayCountdown -= m_beatFrequencyUpper - m_beatFrequencyLower;
            }
        }
        // on end boost, add difference to increase countdowns
        private void OnEndBoosting()
        {
            if (m_frequencyCountdown > 0)
            {
                m_frequencyCountdown += m_beatFrequencyUpper - m_beatFrequencyLower;
            }
            if (m_delayCountdown > 0)
            {
                m_delayCountdown += m_beatFrequencyUpper - m_beatFrequencyLower;
            }
        }

        private float GetRandomBeatPitch()
        {
            return Random.Range(m_randomBeatPitchLower, m_randomBeatPitchUpper);
        }
    }
}
