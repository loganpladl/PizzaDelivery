using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField]
    AudioSource rewindAudioSource;

    [SerializeField]
    AudioSource soundEffectSource;

    [SerializeField]
    AudioClip knockSound;

    [SerializeField]
    AudioClip loopPlaySound;

    [SerializeField]
    AudioClip loopPauseSound;

    [SerializeField]
    AudioSource musicAudioSource;

    [SerializeField]
    AudioClip victorySound;

    [SerializeField]
    AudioClip levelStartSound;

    [SerializeField]
    AudioClip musicClipBlueUniverse;

    [SerializeField]
    AudioClip musicClipRedUniverse;

    [SerializeField]
    AudioClip musicClipGreenUniverse;

    [SerializeField]
    AudioClip universeShiftSound;

    [SerializeField]
    float BlueUniverseMusicVolume = .5f;

    [SerializeField]
    float RedUniverseMusicVolume = .5f;

    [SerializeField]
    float GreenUniverseMusicVolume = .5f;

    private void Awake()
    {
        if (Instance != null)
        {
            GameObject.Destroy(this);
        }
        else Instance = this;
    }

    public void PlayRewindAudio()
    {
        rewindAudioSource.time = 0;
        rewindAudioSource.Play();
    }

    public void StopRewindAudio()
    {
        rewindAudioSource.Stop();
    }

    public void PlayMusic()
    {
        musicAudioSource.Play();
    }

    public void PauseMusic()
    {
        musicAudioSource.Pause();
    }

    public void StopMusic()
    {
        musicAudioSource.Stop();
    }

    // Use the music and volume associated with the blue universe
    public void SetMusicBlueUniverse()
    {
        musicAudioSource.clip = musicClipBlueUniverse;
        musicAudioSource.volume = BlueUniverseMusicVolume;
    }

    public void SetMusicRedUniverse()
    {
        musicAudioSource.clip = musicClipRedUniverse;
        musicAudioSource.volume = RedUniverseMusicVolume;
    }

    public void SetMusicGreenUniverse()
    {
        musicAudioSource.clip = musicClipGreenUniverse;
        musicAudioSource.volume = GreenUniverseMusicVolume;
    }

    public void PlayLevelStartSound()
    {
        soundEffectSource.PlayOneShot(levelStartSound, .4f);
    }

    public void PlayLoopPauseSound()
    {
        soundEffectSource.PlayOneShot(loopPauseSound, .3f);
    }

    public void PlayLoopPlaySound()
    {
        soundEffectSource.PlayOneShot(loopPlaySound, .5f);
    }

    public void PlayUniverseShiftSound()
    {
        soundEffectSource.PlayOneShot(universeShiftSound, .5f);
    }

    public void PlayKnockSound()
    {
        soundEffectSource.PlayOneShot(knockSound, .6f);
    }

    public void PlayVictorySound()
    {
        soundEffectSource.PlayOneShot(victorySound, .4f);
    }
}
