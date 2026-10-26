using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Train.Manager
{
    public class UIScript : MonoBehaviour
    {
        public static UIScript Instance;

        [SerializeField] private Slider m_Timer;
        [SerializeField] private TextMeshProUGUI m_timerText;

        [SerializeField] private Color m_timerBaseColour;
        [SerializeField] private float m_timerFlashingTime = 30f;
        private float m_timerFlashCooldown = 0f;
        [SerializeField] private float m_shakeMultiplier;

        private float lerpTime = 0f;



        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            float gameTime = TimerScript.Instance.GameTime;
            float maxGameTime = TimerScript.Instance.MaxGameTime;

            //short for calculator
            float calc = gameTime / maxGameTime;

            lerpTime = Mathf.Lerp(lerpTime, calc, Time.deltaTime);

            m_Timer.value = lerpTime;

            var ts = TimeSpan.FromSeconds(gameTime);

            m_timerText.text = string.Format("{0:00}:{1:00}:{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds);

            //flash timer as a warning if below threshold
            if (gameTime < m_timerFlashingTime)
            {
                if (m_timerFlashCooldown <= 0f)
                {
                    m_timerFlashCooldown = 5f * (gameTime / m_timerFlashingTime);

                    m_timerText.color = Color.red;
                }

                m_timerFlashCooldown -= Time.deltaTime;
            }

            m_timerText.color = Color.Lerp(m_timerText.color, m_timerBaseColour, Time.deltaTime * 2f);
        }
    }
}

