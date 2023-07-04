using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Parang
{
    public class Pool<T> : Singleton<T> where T : MonoBehaviour
    {
        public bool AllowInstantiate = true;

        private readonly Dictionary<string, PoolBehaviour> originals = new();
        private readonly Dictionary<string, List<PoolBehaviour>> pools = new();

        private Transform pool;
        private SceneInstance instance;

        protected override void Awake()
        {
            base.Awake();
            var go = new GameObject("pool");
            go.transform.SetParent(transform, false);
            go.SetActive(false);
            pool = go.transform;
        }

        async public UniTask LoadPool(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!originals.ContainsKey(path))
            {
                var obj = await Addressables.InstantiateAsync(path);
                var poolable = obj.GetComponent<PoolBehaviour>();
                if (poolable == null)
                {
                    Debug.LogError($"LoadPool{typeof(T)} failed {path} : not found PoolBehaviour");
                    Addressables.Release(obj);
                    return;
                }
                poolable.SaveTransform();
                poolable.Path = path;
                poolable.TimeStamp = DateTime.UtcNow.Ticks;
                poolable.IsReleased = true;
                poolable.OnLoadPool();
                obj.transform.SetParent(pool);
                originals.Add(path, poolable);
                pools.Add(path, new List<PoolBehaviour>());
            }
        }

        async public UniTask LoadPool(IEnumerable<string> pathes)
        {
            foreach (var path in pathes) await LoadPool(path);
        }

        virtual public V Instantiate<V>(string path, Transform parents) where V : PoolBehaviour
        {
            if (string.IsNullOrEmpty(path)) return null;

            // 새로 생성하지 않고 원본을 사용함 (UI에서 사용)
            if (!AllowInstantiate)
            {
                if (originals.TryGetValue(path, out var obj))
                {
                    var poolable = obj.GetComponent<V>();
                    if (poolable == null) return null;
                    poolable.transform.SetParent(parents, false);
                    poolable.ResetTransform();
                    poolable.IsReleased = false;
                    return poolable;
                }
                return null;
            }

            if (pools.TryGetValue(path, out var pool))
            {
                var poolable = pool.FirstOrDefault(x => x.IsReleased);
                if (poolable != null)
                {
                    // 풀에 사용가능한게 있는 경우
                    poolable.ResetTransform();
                    poolable.transform.SetParent(parents, false);
                    poolable.TimeStamp = DateTime.UtcNow.Ticks;
                    poolable.IsReleased = false;
                    return poolable as V;
                }
            }

            if (originals.TryGetValue(path, out var o))
            {
                // 풀에 사용가능한게 없는 경우
                var obj = Instantiate(o.gameObject);
                var poolable = obj.GetComponent<V>();
                if (poolable == null)
                {
                    Destroy(obj);
                    return null;
                }
                poolable.CopyFrom(o);
                poolable.transform.SetParent(parents, false);
                poolable.ResetTransform();
                poolable.Path = path;
                poolable.TimeStamp = DateTime.UtcNow.Ticks;
                poolable.IsReleased = false;
                pools[path].Add(poolable);
                return poolable;
            }
            return null;
        }

        virtual public void Release(GameObject obj)
        {
            var poolable = obj.GetComponent<PoolBehaviour>();
            Release(poolable);
        }

        virtual public void Release(PoolBehaviour poolable)
        {
            if (poolable != null
                && !poolable.IsReleased
                && originals.ContainsKey(poolable.Path))
            {
                poolable.ResetTransform();
                poolable.transform.SetParent(pool, false);
                poolable.StopAllCoroutines();
                poolable.IsReleased = true;
            }
        }

        public void UnloadPool(string path)
        {
            if (pools.TryGetValue(path, out var pool))
                foreach (var p in pool) Destroy(p);
            pools.Remove(path);

            if (originals.TryGetValue(path, out var o))
                Addressables.Release(o);
            originals.Remove(path);
        }

        public void UnloadPool()
        {
            foreach (var pool in pools.Values)
                foreach (var p in pool) Destroy(p);
            pools.Clear();

            foreach (var o in originals.Values)
                Addressables.Release(o);
            originals.Clear();
        }

        public async UniTask<Scene> LoadScene(string path, bool active)
        {
            await UnloadScene();
            instance = await Addressables.LoadSceneAsync(path, LoadSceneMode.Additive);
            if (active && instance.Scene.IsValid())
                SceneManager.SetActiveScene(instance.Scene);
            return instance.Scene;
        }

        public async UniTask UnloadScene()
        {
            if (instance.Scene.isLoaded)
                instance = await Addressables.UnloadSceneAsync(instance);
        }
    }
}
