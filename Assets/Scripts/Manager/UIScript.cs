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
        [SerializeField] private GameObject m_gameOverUI;

        [SerializeField, Range(0f, 8f)] private float m_sliderUpdateSpeed = 1f;
        [SerializeField] private Color m_timerTextBaseColour;
        [SerializeField] private Color m_timerTextFlashColour;
        [SerializeField] private float m_timerTextFlashingTime = 30f;
        private float m_timerFlashCooldown = 0f;
        [SerializeField] private float m_shakeSize;

        [SerializeField] private Gradient m_healthGradient;
        [SerializeField] private Image m_fill;

        [SerializeField] private float m_carriageSizeConstant; //manual calculation cause I can't be bothered doing math

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

            lerpTime = Mathf.Lerp(lerpTime, calc, Time.deltaTime * m_sliderUpdateSpeed);

            m_timer.value = lerpTime;

            //this shit
            var ts = TimeSpan.FromSeconds(gameTime);

            m_timerText.text = "<mspace=15>" + string.Format("{0:00}:{1:00}:{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds);

            //flash timer as a warning if below threshold
            if (gameTime < m_timerTextFlashingTime)
            {
                if (m_timerFlashCooldown <= 0f)
                {
                    m_timerFlashCooldown = 5f * (gameTime / m_timerTextFlashingTime);

                    m_timerText.color = m_timerTextFlashColour;
                }

                m_timerFlashCooldown -= Time.deltaTime;


                m_timerTransform.localPosition = new Vector3(Random.Range(-m_shakeSize, m_shakeSize), Random.Range(-m_shakeSize, m_shakeSize)) * (1f - (gameTime / m_timerTextFlashingTime));
            }

            m_timerText.color = Color.Lerp(m_timerText.color, m_timerTextBaseColour, Time.deltaTime * 2f);

            Color mixedColor = m_healthGradient.Evaluate(calc);

            m_fill.color = mixedColor;
        }

        public void AddCarraige()
        {
            m_timerTransform.sizeDelta = new Vector2(m_timerTransform.sizeDelta.x + m_carriageSizeConstant, m_timerTransform.sizeDelta.y);
        }
        public void OnGameOver()
        {
            m_timer.gameObject.SetActive(false);
            m_gameOverUI.SetActive(true);
        }
    }
}

