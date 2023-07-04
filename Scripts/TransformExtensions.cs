using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Parang
{
    static public class TransformExtensions
    {
        static public void ResetLocal(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        static public void SetLocalY(this Transform t, float height)
        {
            var pos = t.localPosition;
            pos.y = height;
            t.localPosition = pos;
        }

        static public void AddLocalY(this Transform t, float height)
        {
            var pos = t.localPosition;
            pos.y += height;
            t.localPosition = pos;
        }

        static public Vector3 SetY(this Vector3 pos, float height)
        {
            pos.y = height;
            return pos;
        }

        static public void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform tr in gameObject.transform)
                tr.gameObject.SetLayerRecursively(layer);
        }

        static public void SetActive(this Component obj, bool active)
        {
            obj.gameObject.SetActive(active);
        }

        static public void SetActive(this IEnumerable<Component> objs, bool active)
        {
            foreach (var o in objs) o.gameObject.SetActive(active);
        }

        static public void SetActive(this IEnumerable<GameObject> objs, bool active)
        {
            foreach (var o in objs) o.SetActive(active);
        }

        public static Transform FindRecursively(this Transform current, string name)
        {
            if (current.name == name)
                return current;
            for (int i = 0; i < current.childCount; ++i)
            {
                var child = current.GetChild(i);
                Transform found = FindRecursively(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        public static void GetComponentsRecursively<T>(this Transform current, ref List<T> ret)
        {
            var comp = current.GetComponent<T>();
            if (comp != null) ret.Add(comp);
            for (int i = 0; i < current.childCount; ++i)
            {
                var child = current.GetChild(i);
                GetComponentsRecursively<T>(child, ref ret);
            }
        }

        public static void SetTexts(this TextMeshProUGUI[] texts, string text)
        {
            foreach (var t in texts)
                t.text = text;
        }

        public static Vector3 RandomPosition(this Vector3 center, float radius)
        {
            var randomPos = Random.insideUnitCircle * radius;
            return center + new Vector3(randomPos.x, 0.5f, randomPos.y);
        }

        public static void EnableEffect(this ParticleSystem particle, bool enable)
        {
            if (particle == null) return;
            if (enable)
            {
                if (!particle.isPlaying) particle.Play();
            }
            else
            {
                if (particle.isPlaying) particle.Stop();
            }
        }

        public static void ShotEffect(this ParticleSystem particle)
        {
            EnableEffect(particle, false);
            EnableEffect(particle, true);
        }
    }
}
