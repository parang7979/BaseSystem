using System.Collections.Generic;
using UnityEngine;

namespace Parang.UI
{
    static public class UIExtensions
    {
        static public void SetText(this IEnumerable<TMPro.TextMeshProUGUI> texts, string text)
        {
            foreach (var t in texts) t.text = text;
        }

        static public void SetColor(this IEnumerable<TMPro.TextMeshProUGUI> texts, Color color)
        {
            foreach (var t in texts) t.color = color;
        }
    }
}
