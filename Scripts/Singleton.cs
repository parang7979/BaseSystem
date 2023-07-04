using UnityEngine;

namespace Parang
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
                if (destroy)
                {
                    return null;
                }
                else
                {
                    if (instance == null)
                    {
                        instance = (T)FindObjectOfType(typeof(T));
                        // 동적 생성 허용하지않음
                        /* if (instance == null)
                        {
                            var obj = new GameObject(typeof(T).ToString() + " (Singleton)");
                            instance = obj.AddComponent<T>();
                            destroy = false;
                        } */
                    }
                    return instance;
                }
            }
        }

        private static T instance = null;
        private static bool destroy = false;

        [SerializeField]
        private bool dontDestroyOnLoad = true;

        protected virtual void Awake()
        {
            instance = null;
            destroy = false;
            if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationQuit()
        {
            instance = null;
            destroy = true;
        }

        protected virtual void OnDestroy()
        {
            instance = null;
            destroy = true;
        }
    }
}