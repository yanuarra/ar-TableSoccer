using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioHandler : MonoBehaviour
{
    public AudioSource audio_Bgm;
    public AudioSource audio_Sfx;
    public AudioMixer audioMixer;
    public AudioSource audio_Vo;

    [Header("Audio Clip")]
    [SerializeField]
    private AudioClip clip_ButtonHighlight;

    [SerializeField]
    private AudioClip clip_ButtonClicked;

    public static AudioHandler Instance { get; private set; }

    private Coroutine voMonitorCoroutine;

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
        if (!audio_Vo.isPlaying)
        {
            audio_Bgm.volume = 0.2f;
        }
    }

    public void ToggleMuteAllAudio()
    {
        audio_Bgm.mute = !audio_Bgm.mute;
        audio_Vo.mute = !audio_Vo.mute;
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

    public void PlayVoPlanet(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("Audio clip not found!");
            return;
        }
        audio_Vo.clip = clip;
        LeanTween.value(audio_Vo.volume, 0.75f, 3f).setOnUpdate((float val) => audio_Vo.volume = val);
        //audio_Vo.volume = 0.75f;
        audio_Vo.Play();
        audio_Bgm.volume = 0.1f;

        if (voMonitorCoroutine != null)
        {
            StopCoroutine(voMonitorCoroutine);
        }
        voMonitorCoroutine = StartCoroutine(MonitorVoAudio());
    }

    public void StopVoPlanet()
    {
        //audio_Vo.Stop();
        LeanTween.value(audio_Vo.volume, 0f, 3f).setOnUpdate((float val) => audio_Vo.volume = val);
        if (voMonitorCoroutine != null)
        {
            StopCoroutine(voMonitorCoroutine);
            voMonitorCoroutine = null;
        }

        audio_Bgm.volume = 0.5f;
        //StartCoroutine(FadeOutAudio(audio_Vo, 3f));
    }

    private IEnumerator FadeOutAudio(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float currentVolume = audioSource.volume;
        LeanTween.value(currentVolume, 0f, 3f);
        while (audioSource.volume > 0)
        {
            //audioSource.volume -= startVolume * Time.deltaTime / duration;
            audio_Vo.volume = currentVolume;
            yield return null;
        }

        audio_Vo.Stop();

        //audio_Bgm.volume = 0.5f;
    }

    private IEnumerator MonitorVoAudio()
    {
        while (audio_Vo.isPlaying)
        {
            yield return null;
        }
        audio_Bgm.volume = 1f;
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