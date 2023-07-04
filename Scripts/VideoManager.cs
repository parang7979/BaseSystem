using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Video;

namespace Parang
{
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoManager : Singleton<VideoManager>
    {
        public RenderTexture Texture { get; private set; }

        private VideoPlayer _player;
        private bool _isPlaying;
        private CancellationTokenSource _token;

        protected override void Awake()
        {
            base.Awake();
            _player = GetComponent<VideoPlayer>();
            Texture = new RenderTexture(1080, 1920, 24, RenderTextureFormat.ARGB32);
            _player.targetTexture = Texture;
            _player.playOnAwake = false;
            _isPlaying = false;
            _token = null;
        }

        public async UniTask Play(string path)
        {
            if (_isPlaying) return;

            _isPlaying = true;
            _token = new CancellationTokenSource();
            var clip = await Addressables.LoadAssetAsync<VideoClip>($"Assets/Bundle/Video/{path}");
            _player.clip = clip;
            _player.Play();
            await UniTask.Delay((int)(clip.length * 1000), cancellationToken: _token.Token);
            _player.Stop();
            _player.clip = null;
            Addressables.Release(clip);
            _token = null;
            _isPlaying = false;
        }

        public void Stop()
        {
            _token?.Cancel();
            _token = null;
        }
    }
}
