using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class PlayerSoundEffects : MonoBehaviour
{
    public  AudioSource audioSource;
    public  AudioClip[] sounds;

    event EventHandler OnPickedUp;
    event EventHandler OnLetGo;

    private void SuccessfullyPickedUp(GameObject pickedUpGO)
    {
        if (OnPickedUp != null)
        {
            OnPickedUp(pickedUpGO, null);
            //Debug.Log("PICKED UP");
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }


    // 0 = Pickup, 1 = Drop, 
    public  void PlaySoundEffect(int soundIndex)
    {
        audioSource.PlayOneShot(sounds[soundIndex]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}



