using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [SerializeField] AudioSource track1;
    [SerializeField] AudioSource track2;
    [SerializeField] AudioSource transition;

    bool firstTrackIsPlaying = true;

    private void Awake()
    {
        Messenger.AddListener(GameEvent.COMBAT_STATE_CHANGED, OnCombatStateChanged);
    }

    private void OnDestroy()
    {
        Messenger.RemoveListener(GameEvent.COMBAT_STATE_CHANGED, OnCombatStateChanged);
    }

    public void ChangeTracks()
    {
        if (firstTrackIsPlaying)
        {
            StartCoroutine(Swap(track1, track2));
        }
        else
        {
            StartCoroutine(Swap(track2, track1));
        }

        firstTrackIsPlaying = !firstTrackIsPlaying;
    }

    private IEnumerator Swap(AudioSource fromTrack, AudioSource toTrack)
    {
        float duration = 0.4f;
        float currentTime = 0;
        float startVol = fromTrack.volume;
        float targetVol = toTrack.volume;

        if (transition != null)
        {
            transition.Play();
            yield return new WaitForSeconds(0.1f);
        }
        
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            fromTrack.volume = Mathf.Lerp(startVol, targetVol, currentTime / duration);
            toTrack.volume = Mathf.Lerp(targetVol, startVol, currentTime / duration);
            yield return null;
        }
        yield break;
    }

    private void OnCombatStateChanged()
    {
        ChangeTracks();
    }
}