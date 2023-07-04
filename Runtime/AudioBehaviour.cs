using UnityEngine;

namespace Parang
{
    public class AudioBehaviour : PoolBehaviour
    {
        public AudioSource AudioSource;

        virtual protected void Awake()
        {
            if (AudioSource != null)
                AudioSource.spatialBlend = 1f;
        }

        async public void PlaySound(string path)
        {
            if (SoundManager.Instance == null || AudioSource == null || !gameObject.activeInHierarchy) return;

            var clip = await SoundManager.Instance.GetEffect(path);
            if (clip != null)
            {
                AudioSource.PlayOneShot(clip);
            }
        }
    }
}
