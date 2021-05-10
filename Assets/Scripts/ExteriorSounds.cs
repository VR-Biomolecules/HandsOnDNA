using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExteriorSounds : MonoBehaviour
{

    public AudioSource audioSource;
    public List<TimedSound> timedSounds = new List<TimedSound>();
    float timer;
    void Update()
    {
        timer += Time.deltaTime;
        foreach (TimedSound timedSound in timedSounds)
        {
            if (timedSound.nextPlayTime < timer)
            {
                //Play sound
                audioSource.PlayOneShot(timedSound.audioClip);
                //Reset sound timer
                timedSound.nextPlayTime += Random.Range(timedSound.lengthRange.x, timedSound.lengthRange.y);
            }
        }
    }
}

[System.Serializable]
public class TimedSound
{
    public string name;
    public Vector2 lengthRange = new Vector2 ();
    public float nextPlayTime;
    public AudioClip audioClip;
}
