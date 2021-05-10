using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabAudioManager : MonoBehaviour
{
    public AudioClip shrink1;
    public AudioClip loop1;
    public AudioClip shrink2;
    public AudioClip loop2;
    public AudioClip shrink3;

    public AudioSource shrinkSource;
    public AudioSource loopSource;
    public AudioSource hairThud;
    public AudioSource narration;

    public List<AudioClip> narrationClips;

    private bool inShrink;
    private bool noMoreLooping;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (inShrink)
        {
            // If the shrink music has just finished, we want to start the loop and pre-load the next shrink music
            if (!shrinkSource.isPlaying && !noMoreLooping)
            {
                loopSource.Play();
                inShrink = false;
                if (shrinkSource.clip.Equals(shrink1)) shrinkSource.clip = shrink2;
                else shrinkSource.clip = shrink3;
            }
        }
    }

    // Expected to be called in order, 1 then 2 then 3
    public void StartShrinkPhase1()
    {
        shrinkSource.Play();
        narration.Stop();
        inShrink = true;
        PlayNextNarrativeClip();
    }

    public void StartShrinkPhase2()
    {
        loopSource.Stop();
        narration.Stop();
        shrinkSource.Play();
        inShrink = true;
        PlayNextNarrativeClip();

        loopSource.clip = loop2;
    }

    public void StartShrinkPhase3()
    {
        loopSource.Stop();
        narration.Stop();
        shrinkSource.volume = 0.2f;
        shrinkSource.Play();
        inShrink = true;
        noMoreLooping = true;
    }

    public void HairThud()
    {
        hairThud.Play();
    }

    public void PlayNextNarrativeClip()
    {
        StartCoroutine(StartNarrativeAfterCurrentShrink());
    }

    private IEnumerator StartNarrativeAfterCurrentShrink()
    {
        if (shrinkSource.isPlaying) yield return new WaitUntil(() => !shrinkSource.isPlaying);
        if (narration.isPlaying) narration.Stop();
        if (narrationClips.Count > 0)
        {
            narration.clip = narrationClips[0];
            narrationClips.RemoveAt(0);
        }
        narration.Play();    
    }
}
