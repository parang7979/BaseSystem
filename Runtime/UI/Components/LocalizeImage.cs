using System;
using UnityEngine;

namespace Parang.UI
{
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class LocalizeImage : MonoBehaviour
    {
        static public Func<SystemLanguage> Localizer;

        public Sprite Korean;
        public Sprite Japanese;

        private UnityEngine.UI.Image _image;
        private Sprite _original;

        private void Awake()
        {
            _image = GetComponent<UnityEngine.UI.Image>();
            _original = _image.sprite;
        }

        private void OnEnable()
        {
            Localize();
        }

        public void Localize()
        {
            if (_image != null)
            {
                var language = Localizer?.Invoke() ?? SystemLanguage.Korean;
                switch (language)
                {
                    case SystemLanguage.Japanese:
                        _image.sprite = Japanese;
                        break;

                    default:
                        _image.sprite = Korean;
                        break;
                }
            }
        }
    }
}
