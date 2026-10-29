using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Train.Manager
{
    public class UIScript : MonoBehaviour
    {
        public static UIScript Instance;

        [SerializeField] private Slider m_timer;
        private RectTransform m_timerTransform;
        [SerializeField] private TextMeshProUGUI m_timerText;

        [SerializeField] private Color m_timerBaseColour;
        [SerializeField] private Color m_timerFlashColour;
        [SerializeField] private float m_timerFlashingTime = 30f;
        private float m_timerFlashCooldown = 0f;
        [SerializeField] private float m_shakeSize;

        private float lerpTime = 0f;



        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            Instance = this;
            m_timerTransform = (RectTransform)m_timer.transform;
        }

        // Update is called once per frame
        void Update()
        {
            float gameTime = TimerScript.Instance.GameTime;
            float maxGameTime = TimerScript.Instance.MaxGameTime;

            //short for calculator
            float calc = gameTime / maxGameTime;

            lerpTime = Mathf.Lerp(lerpTime, calc, Time.deltaTime);

            m_timer.value = lerpTime;

            //this shit
            var ts = TimeSpan.FromSeconds(gameTime);

            m_timerText.text = "<mspace=15>" + string.Format("{0:00}:{1:00}:{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds);

            //flash timer as a warning if below threshold
            if (gameTime < m_timerFlashingTime)
            {
                if (m_timerFlashCooldown <= 0f)
                {
                    m_timerFlashCooldown = 5f * (gameTime / m_timerFlashingTime);

                    m_timerText.color = m_timerFlashColour;
                }

                m_timerFlashCooldown -= Time.deltaTime;


                m_timerTransform.localPosition = new Vector3(Random.Range(-m_shakeSize, m_shakeSize), Random.Range(-m_shakeSize, m_shakeSize)) * (1f - (gameTime / m_timerFlashingTime));
            }

            m_timerText.color = Color.Lerp(m_timerText.color, m_timerBaseColour, Time.deltaTime * 2f);
        }
    }
}

