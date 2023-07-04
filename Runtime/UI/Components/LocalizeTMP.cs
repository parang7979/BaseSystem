using System;
using UnityEngine;

namespace Parang.UI
{
    [RequireComponent(typeof(TMPro.TMP_Text))]
    public class LocalizeTMP : MonoBehaviour
    {
        static public Func<string, string> Localizer;

        private TMPro.TMP_Text _text;
        private string _original;

        private void Awake()
        {
            _text = GetComponent<TMPro.TMP_Text>();
            _original = _text.text;
        }

        private void OnEnable()
        {
            Localize();
        }

        public void Localize()
        {
            if (_text != null)
                _text.text = Localizer?.Invoke(_original) ?? _original;
        }
    }
}
