using UnityEngine;

namespace Train.Manager
{
    public class TimerScript : MonoBehaviour
    {
        [SerializeField] private float m_startTime = 60f;

        [SerializeField] private float m_maxTime = 120f;

        private float m_gameTime = 0f;

        private float m_timeMultiplier = 1f;

        public float GameTime { get => m_gameTime; private set { } }
        public float GameTimeMultiplier { get => m_timeMultiplier; private set { } }

        public static TimerScript Instance;


        private void Awake()
        {
            Instance = this;
            m_gameTime = m_startTime;
        }

        // Update is called once per frame
        void Update()
        {
            m_gameTime -= Time.deltaTime * m_timeMultiplier;
        }

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
            m_timeMultiplier = multiplierToSet;
        }

        public void AddTimeMultiplier(float multiplierToAdd)
        {
            m_timeMultiplier += multiplierToAdd;
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
    }
}


