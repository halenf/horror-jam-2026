using UnityEngine;

namespace Train.Manager
{
    using Player;

    public class TimerScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController m_player;
        [SerializeField] private CarriageController m_carriages;

        [Header("Time Levels")]
        [SerializeField, Min(0f)] private float m_startTime = 30f;
        [SerializeField, Min(0f)] private float m_startMaxTime = 45f;

        [Header("Levels")]
        [SerializeField, Min(0f)] private float m_maxTimePerLevel = 15f;
        [Tooltip("Once a new level is reached, the duration of time where the timer is paused before the max timer increases.")]
        [SerializeField, Min(0f)] private float m_levelUpSafetyDuration = 5f;
        [SerializeField, Min(1f)] private float m_boostMultiplier = 2f;

        private int m_currentLevel = 0;
        private float m_currentTime;
        private float m_currentMaxTime;
        private float m_levelUpSafetyCountdown;

        private float m_timeExtraMultiplier = 1f;
        private bool m_isBoosting = false;
        // If this is true, the timer will not tick down
        private bool m_paused;

        public float GameTime { get => m_currentTime; }
        public float MaxGameTime { get => m_currentMaxTime;}
        public float GameTimeMultiplier { get => m_timeExtraMultiplier; }

        public static TimerScript Instance;
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            m_currentTime = m_startTime;
            m_currentMaxTime = m_startMaxTime;
        }

        // Update is called once per frame
        void Update()
        {
            // if not paused
            if (!m_paused)
            {
                float boost = (m_isBoosting) ? m_boostMultiplier : 1f;

                m_currentTime -= Time.deltaTime * boost * m_timeExtraMultiplier;

                if(m_currentTime <= 0)
                {
                    StartCoroutine(m_player.OnGameEnd());
                    FindAnyObjectByType<UIScript>().OnGameOver();
                }
            }
            // if in safety time
            else if (m_levelUpSafetyCountdown > 0f)
            {
                m_currentMaxTime += m_maxTimePerLevel / m_levelUpSafetyDuration * Time.deltaTime;

                m_levelUpSafetyCountdown -= Time.deltaTime;
                if (m_levelUpSafetyCountdown <= 0f)
                {
                    // finish Level Up
                    m_carriages.AddCarriage();
                    m_currentMaxTime = m_startMaxTime + m_currentLevel * m_maxTimePerLevel;
                    UnpauseTimer();
                }
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

        public void CheckLevelUp()
        {
            if (m_currentTime >= m_currentMaxTime)
            {
                m_currentLevel++;
                m_levelUpSafetyCountdown = m_levelUpSafetyDuration;
                PauseTimer();
            }
        }

        /// <summary>
        /// adds an amount of time to the timer (wow!)
        /// </summary>
        /// <param name="timeToAdd">amount of time to add</param>
        /// <param name="capped">should the added time be capped to the current max value</param>
        public void AddTime(float timeToAdd)
        {
            if (!m_paused)
            {
                m_currentTime = Mathf.Min(m_currentTime + timeToAdd, m_currentMaxTime);
                CheckLevelUp();
            }
        }
        public void LoseTime(float timeToLose)
        {
            if (!m_paused)
            {
                m_currentTime = Mathf.Max(m_currentTime - timeToLose, 0f);
            }
        }

        public void SetTime(float timeToSet, bool capped = true)
        {
            float previousTime = m_currentTime;

            m_currentTime = timeToSet;

            if (capped)
            {
                m_currentTime = OverHealAdjust(previousTime, m_currentTime);
            }

            CheckLevelUp();
        }

        public void AddMaxTime(float maxTimeToAdd)
        {
            m_currentMaxTime += maxTimeToAdd;
        }


        public void SetMaxTime(float maxTimeToSet)
        {
            m_currentMaxTime = maxTimeToSet;
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
            float cappedTime = Mathf.Clamp(m_currentTime, 0f, m_currentMaxTime);

            if (cappedTime > previousTime) { return cappedTime; }

            return (currentTime > m_currentMaxTime) ? previousTime : currentTime;
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


