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

            m_Timer.value = calc;

            var ts = TimeSpan.FromSeconds(gameTime);


            m_timerText.text = string.Format("{0:00}:{1:00}:{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds);


        }
    }
}

