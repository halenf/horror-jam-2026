using UnityEngine;

namespace Train.Manager
{
    using Player;

    public class TimerScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController m_player;

        [Header("Fields")]
        [SerializeField] private float m_startTime = 60f;
        [SerializeField] private float m_maxTime = 120f;
        [SerializeField] private float m_boostMultiplier = 2f;

        private float m_timeExtraMultiplier = 1f;
        private float m_gameTime = 0f;
        private bool m_isBoosting = false;
        // If this is true, the timer will not tick down
        private bool m_paused;

        public float GameTime { get => m_gameTime; private set { } }
        public float MaxGameTime { get => m_maxTime; private set { } }
        public float GameTimeMultiplier { get => m_timeExtraMultiplier; private set { } }

        public static TimerScript Instance;
        private void Awake()
        {
            Instance = this;
            m_gameTime = m_startTime;
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_paused)
            {
                float boost = (m_isBoosting) ? m_boostMultiplier : 1f;

                m_gameTime -= Time.deltaTime * boost * m_timeExtraMultiplier;
            }
        }

        private void OnEnable()
        {
            m_player.OnBeginBoosting += StartBoosting;
            m_player.OnEndBoosting += StopBoosting;

            m_paused = false;
        }

        private void OnDisable()
        {
            m_player.OnBeginBoosting -= StartBoosting;
            m_player.OnEndBoosting -= StopBoosting;

            m_paused = true;
        }


        private void StartBoosting() { m_isBoosting = true; }
        private void StopBoosting() { m_isBoosting = false; }

        /// <summary>
        /// adds an amount of time to the timer (wow!)
        /// </summary>
        /// <param name="timeToAdd">amount of time to add</param>
        /// <param name="capped">should the added time be capped to the current max value</param>
        public void AddTime(float timeToAdd, bool capped = true)
        {
            float previousTime = m_gameTime;

            m_gameTime += timeToAdd;

            if (capped)
            {
                m_gameTime = OverHealAdjust(previousTime, m_gameTime);
            }
        }

        public void SetTime(float timeToSet, bool capped = true)
        {
            float previousTime = m_gameTime;

            m_gameTime = timeToSet;

            if (capped)
            {
                m_gameTime = OverHealAdjust(previousTime, m_gameTime);
            }
        }

        public void AddMaxTime(float maxTimeToAdd)
        {
            m_maxTime += maxTimeToAdd;
        }


        public void SetMaxTime(float maxTimeToSet)
        {
            m_maxTime = maxTimeToSet;
        }

        public void SetMultiplier(float multiplierToSet)
        {
            m_timeExtraMultiplier = multiplierToSet;
        }

        public void AddTimeMultiplier(float multiplierToAdd)
        {
            m_timeExtraMultiplier += multiplierToAdd;
        }

        /// <summary>
        /// prevents overhealing from reducing overheal to zero from capped additions
        /// </summary>
        private float OverHealAdjust(float previousTime, float currentTime)
        {
            float cappedTime = Mathf.Clamp(m_gameTime, 0, m_maxTime);

            if (cappedTime > previousTime) { return cappedTime; }

            return (currentTime > m_maxTime) ? previousTime : currentTime;
        }

        public void PauseTimer()
        {
            m_paused = true;
        }

        public void UnpauseTimer()
        {
            m_paused = false;
        }
    }
}


