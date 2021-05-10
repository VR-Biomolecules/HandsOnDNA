using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Controls the background and narration audio for the scene 
 */
public class CellAudioManager : MonoBehaviour
{
    public AudioSource shrinkOutro;
    public AudioSource cellBackground;
    public AudioSource activityCompletion;

    public AudioSource narration;
    public List<AudioClip> scene2Clips;
    public List<AudioClip> scene3Clips;

    private List<AudioClip> currentClips;

    public bool playNarration = true;

    void Start()
    {
        StartCoroutine(PlayMusic());
    }

    private IEnumerator PlayMusic()
    {
        shrinkOutro.Play();
        yield return new WaitForSeconds(shrinkOutro.clip.length);
        cellBackground.Play();
    }

    public void PlayNextNarrativeClip()
    {
        if (playNarration) StartCoroutine(PlayNarrativeWhenReady());
    }

    private IEnumerator PlayNarrativeWhenReady()
    {
        if (shrinkOutro.isPlaying)
        {
            yield return new WaitUntil(() => shrinkOutro.clip.length - shrinkOutro.time < 16);
            shrinkOutro.volume = 0.1f;
        }
        if (narration.isPlaying) narration.Stop();
        if (currentClips.Count > 0)
        {
            narration.clip = currentClips[0];
            currentClips.RemoveAt(0);
        }
        narration.Play();    
    }

    public void SwitchToChapter2()
    {
        currentClips = scene2Clips;
    }

    public void SwitchToChapter3()
    {
        currentClips = scene3Clips;
    }
}
