using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

namespace Parang
{
    public class SoundManager : Singleton<SoundManager>
    {
        public AudioMixer Mixer;
        public string MasterVolume = "MasterVolume";
        public AudioSource BGM;
        public string BGMVolume = "BGMVolume";
        public AudioSource Effect;
        public string EffectVolume = "EffectVolume";
        public string TouchSound;

        private string _currentBgmPath;
        private Dictionary<string, AudioClip> _clips;

        protected override void Awake()
        {
            base.Awake();
            _currentBgmPath = null;
            _clips = new Dictionary<string, AudioClip>();
        }

        public void SetMasterVolume(float value)
        {
            value = Mathf.Clamp(value, 0.0001f, 1f);
            Mixer.SetFloat(MasterVolume, Mathf.Log10(value) * 20);
        }

        public void SetBGMVolume(float value)
        {
            value = Mathf.Clamp(value, 0.0001f, 1f);
            Mixer.SetFloat(BGMVolume, Mathf.Log10(value) * 20);
        }

        public void SetEffectVolume(float value)
        {
            value = Mathf.Clamp(value, 0.0001f, 1f);
            Mixer.SetFloat(EffectVolume, Mathf.Log10(value) * 20);
        }

        public async void PlayBGM(string path)
        {
            if (path == _currentBgmPath) return;
            StopBgm();
            _currentBgmPath = path;
            if (!string.IsNullOrEmpty(path))
            {
                BGM.clip = await Addressables.LoadAssetAsync<AudioClip>($"Assets/Bundle/Sound/BGM/{path}");
                BGM.Play();
            }
        }

        public void StopBgm()
        {
            if (BGM.clip != null)
            {
                BGM.Stop();
                Addressables.Release(BGM.clip);
                BGM.clip = null;
            }
        }

        public async UniTask LoadSound(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!_clips.ContainsKey(path))
            {
                // 일단 공간만 만들어서 중복로딩을 막음
                _clips.Add(path, null);
                var clip = await Addressables.LoadAssetAsync<AudioClip>($"Assets/Bundle/Sound/Effect/{path}");
                _clips[path] = clip;
            }
        }

        public async UniTask<AudioClip> GetEffect(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (!_clips.TryGetValue(path, out var clip))
            {
                // 일단 공간만 만들어서 중복로딩을 막음
                _clips.Add(path, null);
                clip = await Addressables.LoadAssetAsync<AudioClip>($"Assets/Bundle/Sound/Effect/{path}");
                _clips[path] = clip;
            }
            return clip;
        }

        public async void PlayEffect(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!_clips.TryGetValue(path, out var clip))
            {
                // 일단 공간만 만들어서 중복로딩을 막음
                _clips.Add(path, null);
                clip = await Addressables.LoadAssetAsync<AudioClip>($"Assets/Bundle/Sound/Effect/{path}");
                _clips[path] = clip;
            }
            if (clip != null)
                Effect.PlayOneShot(clip);
        }

        public void CleanUp()
        {
            foreach (var c in _clips) Addressables.Release(c);
            _clips.Clear();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
                BGM.Stop();
            else
                BGM.Play();
        }

        private void Update()
        {
            if (string.IsNullOrEmpty(TouchSound)) return;
            if (Input.GetMouseButtonDown(0))
            {
                PlayEffect(TouchSound);
            }
        }
    }
}
