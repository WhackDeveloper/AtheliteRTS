using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TRavljen.UnitSystem.Demo
{
    class ResourceTextElement : MonoBehaviour
    {
        [SerializeField]
        private Text text;

        public void SetText(string text)
        {
            this.text.text = text;
        }
    }
}