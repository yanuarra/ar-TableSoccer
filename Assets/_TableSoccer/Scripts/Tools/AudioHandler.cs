using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioHandler : MonoBehaviour
{
    public AudioSource audio_Bgm;
    public AudioSource audio_Sfx;
    public AudioMixer audioMixer;
    [Header("Audio Clip")]
    [SerializeField]
    private AudioClip clip_ButtonHighlight;
    [SerializeField]
    private AudioClip clip_ButtonClicked;
    public static AudioHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void PlayAudioBgm(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("Audio clip not found!");
            return;
        }
        audio_Bgm.clip = clip;
        audio_Bgm.Play();
    }

    public void ToggleMuteAllAudio()
    {
        audio_Bgm.mute = !audio_Bgm.mute;
        audio_Sfx.mute = !audio_Sfx.mute;
    }

    public void StopAudioBgm()
    {
        audio_Bgm.Stop();
    }

    public void AdjustSFXVolume(float volume)
    {
        audioMixer.SetFloat("sfxVolume", volume);
    }

    public void ToggleLoop(bool isLooping)
    {
        audio_Sfx.loop = isLooping;
    }

    public void PlayAudioSfx(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("Audio clip not found!");
            return;
        }
        audio_Sfx.clip = clip;
        audio_Sfx.Play();
    }

    private IEnumerator FadeOutAudio(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float currentVolume = audioSource.volume;
        LeanTween.value(currentVolume, 0f, 3f);
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.Stop();
        audio_Bgm.volume = 0.5f;
    }

    public void StopAudioSfx()
    {
        audio_Sfx.Stop();
    }

    public void PlayButtonHighlight()
    {
        PlayAudioSfx(clip_ButtonHighlight);
    }

    public void PlayButtonClicked()
    {
        PlayAudioSfx(clip_ButtonClicked);
    }
}