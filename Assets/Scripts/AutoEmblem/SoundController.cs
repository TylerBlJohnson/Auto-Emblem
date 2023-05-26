using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    private static SoundController instance;
    private AudioSource sfxSource;
    private float sfxVolumeMaster = 1.0f;

    public void SetSfxVolume(float value)
    {
        sfxVolumeMaster = Mathf.Clamp(value, 0.0f, 1.0f);
    }
    public float GetSfxVolume()
    {
        return sfxVolumeMaster;
    }

    public static SoundController Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySfx(AudioClip clip, float volume = 1.0f)
    {
        sfxSource.PlayOneShot(clip, volume * sfxVolumeMaster);
    }
}
