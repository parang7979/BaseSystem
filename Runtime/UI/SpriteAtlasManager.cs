using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace Parang.UI
{
    public class SpriteAtlasManager : Singleton<SpriteAtlasManager>
    {
        static private readonly string _spriteAtlasPath = "Assets/Bundle/Sprite/{0}.spriteatlasv2";
        private Dictionary<string, SpriteAtlas> atlases;

        protected override void Awake()
        {
            base.Awake();
            atlases = new Dictionary<string, SpriteAtlas>();
            UnityEngine.U2D.SpriteAtlasManager.atlasRequested += LoadRequest;
        }

        async void LoadRequest(string key, Action<SpriteAtlas> callback)
        {
            var atlas = await LoadAtlas(key);
            callback?.Invoke(atlas);
        }

        async public UniTask<SpriteAtlas> LoadAtlas(string key)
        {
            if (!atlases.TryGetValue(key, out var atlas))
            {
                atlas = await Addressables.LoadAssetAsync<SpriteAtlas>(string.Format(_spriteAtlasPath, key));
                atlases.Add(key, atlas);
            }
            return atlas;
        }

        public Sprite GetSprite(string key, string sprite)
        {
            if (atlases.TryGetValue(key, out var atlas))
                return atlas.GetSprite(sprite);
            return null;
        }

        static public UniTask Load(string key)
        {
            return Instance.LoadAtlas(key);
        }

        static public Sprite Get(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var strs = name.Split('.');
            if (strs.Length < 2) return null;
            return Instance.GetSprite(strs[0], strs[1]);
        }
    }
}