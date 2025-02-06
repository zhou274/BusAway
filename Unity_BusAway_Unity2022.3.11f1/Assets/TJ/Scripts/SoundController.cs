using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace TJ.Scripts
{
    public class SoundController : MonoBehaviour
    {
        public static SoundController Instance;
        public AudioSource audioSource;
        public AudioClip tapSound, buttonSound, hitSound, sort, full, win, fail, nocoinPOP;
        
        public AudioClip moving;

        private bool isFullSoundPlaying = false;
        private void Awake()
        {
            Instance = this;
        }

        public void PlayOneShot(AudioClip clip, float volume)
        {
            audioSource.PlayOneShot(clip, volume);
        }
        public void PlayOneShot(AudioClip clip)
        {
            audioSource.PlayOneShot(clip);
        }

        public void PlayFullSound()
        {
            if (!isFullSoundPlaying)
            {
                audioSource.PlayOneShot(full);
                isFullSoundPlaying = true;
                DOVirtual.DelayedCall(full.length, ()=>isFullSoundPlaying = false);
            }
        }
    }
}