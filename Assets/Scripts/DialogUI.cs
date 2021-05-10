using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogUI : MonoBehaviour
{
    [System.Serializable]
    public struct UITransition
    {
        // If true, will pause before this element, waiting for mainUI.Resume() to be called (from the execution of the previous transition)
        public bool startPaused;
        // The component used to edit the alpha (transparency) of the image/text
        public CanvasGroup canvasGroup;
        // How the canvasGroup alpha changes over time
        public AnimationCurve fadeCurve;
        // The amount of time (after the fade is complete) to wait before moving on to the next transition
        public float waitTime;
        // The audio clip to play when this transition starts
        public AudioClip narrationClip;
        // Which objects should toggle, at the start of the fade (if fading in) or at the end of the fade (if fading out)
        public List<GameObject> toggleObjects;
        // The amount of time this transition has elapsed for (stops at 0 if 'paused' is enabled). Maximum value = length(fadeCurve) + waitTime
        internal float elapsedTime;
    }
    // Array of transitions to play
    public UITransition[] transitions;
    // Which transition is currently playing (or paused on)
    internal int index;
    // UI is paused
    public bool paused;
    // When index exceeds transition length, starts from transition 0 again.
    public bool loop;

    // If true, audio is not played by any transition
    public bool skipAudio;
    // The audio source component to play sounds from (for all transitions)
    public AudioSource audioSource;

    private void Start()
    {
        StartCoroutine(Play());
    }

    void Update()
    {
        // A valid transition needs to be played (not paused)
        if (index < transitions.Length && !paused)
        {
            // Elapse time the transition has been running for
            transitions[index].elapsedTime += Time.deltaTime;

            // Set alpha based on transition fadeCurve and elapsedTime
            transitions[index].canvasGroup.alpha = transitions[index].fadeCurve.Evaluate(transitions[index].elapsedTime);
        }
    }

    private bool canResume = true;

    public void Resume()
    {
        if (canResume)
        {
            Debug.Log($"DialogUI.Resume\t{name}\t{Time.timeSinceLevelLoad}");
            StartCoroutine(ResumeCoroutine());
            StartCoroutine(PauseResuming());
        }
        else
        {
            Debug.Log("Tried to resume when paused");
        }
    }

    //Little hack to stop multi button presses messing up the dialogUI running
    private IEnumerator PauseResuming()
    {
        canResume = false;
        yield return new WaitForSeconds(2.0f);
        canResume = true;
    }

    public void PlaySound(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }

    IEnumerator ResumeCoroutine()
    {
        // Transition index is not out of range
        if (index < transitions.Length)
        {
            // Wait until paused is true (as Resume() may be called slightly before the current transition is actually paused)
            yield return new WaitUntil(() => paused);
            // Unpause
            paused = false;
        }
    }

    IEnumerator Play()
    {
        // Transition index is not out of range
        while (index < transitions.Length)
        {
            // Pause at start
            if (transitions[index].startPaused)
            {
                paused = true;
            }
            // Reset elapsed time
            transitions[index].elapsedTime = 0;

            // Wait until current transition is unpaused. The first unpause starts the UI and fades it in
            yield return new WaitUntil(() => !paused);

            // Toggle gameobjects (before fading in)
            if (Math.Abs(transitions[index].fadeCurve.keys[transitions[index].fadeCurve.keys.Length - 1].value - 1) < 0.01)
            {
                foreach (GameObject g in transitions[index].toggleObjects)
                {
                    //Debug.Log(g.name + " is active? " + g.activeSelf);
                    g.SetActive(!g.activeSelf);
                    //Debug.Log(g.name + " is active? " + g.activeSelf);
                }
            }

            // Play Narration clip
            if (transitions[index].narrationClip && !skipAudio) { audioSource.PlayOneShot(transitions[index].narrationClip); }

            // Wait for transition fade to complete + waitTime
            yield return new WaitForSeconds(transitions[index].fadeCurve.keys[transitions[index].fadeCurve.keys.Length-1].time + transitions[index].waitTime);

            // Toggle gameobjects (after fading out)
            if (Math.Abs(transitions[index].fadeCurve.keys[transitions[index].fadeCurve.keys.Length - 1].value) < 0.01) //this is permissive because it's not constant in all transitions
            {
                foreach (GameObject g in transitions[index].toggleObjects)
                {
                    g.SetActive(!g.activeSelf);
                }
            }

            // Loop to start
            if (loop && index == transitions.Length-1)
            {
                index = 0;
            }
            else // Move on to next transition
            {
                index++;
            }
        }
    }
}
