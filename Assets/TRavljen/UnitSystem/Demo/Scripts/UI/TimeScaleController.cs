using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TRavljen.UnitSystem.Demo
{
    
    public class TimeScaleController : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private Button upButton;
        [SerializeField] private Button downButton;
        [SerializeField] private Button resetButton;

        private void Awake()
        {
            upButton.onClick.AddListener(SpeedUp);
            downButton.onClick.AddListener(SpeedDown);
            resetButton.onClick.AddListener(Reset);
        }

        private void SpeedUp() => UpdateTimeScale(Mathf.Min(Time.timeScale + 0.5f, 5f));

        private void SpeedDown() => UpdateTimeScale(Mathf.Max(0, Time.timeScale - 0.5f));

        private void Reset() => UpdateTimeScale(1);

        private void UpdateTimeScale(float scale)
        {
            Time.timeScale = scale;
            text.text = "Time scale: " + scale.ToString("0.00");
        }
        
    }
    
}
