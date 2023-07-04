using UnityEngine;

namespace Parang.Util
{
    static public class MathExtenstion
    {
        static public Vector3 Clamp(this Rect rect, Vector3 pos)
        {
            var p = new Vector2(pos.x, pos.z);
            if (!rect.Contains(p))
            {
                p.x = Mathf.Clamp(p.x, rect.xMin, rect.xMax);
                p.y = Mathf.Clamp(p.y, rect.yMin, rect.yMax);
                return new Vector3(p.x, 0f, p.y);
            }
            return pos;
        }
    }
}
