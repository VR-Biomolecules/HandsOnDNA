using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus;


public class ChapterSelector : MonoBehaviour
{
    public static ChapterSelector main;

    public Transform player;

    public float fadeTime = 1;

    internal bool loading;

    private void Awake()
    {
        // No player has been activated yet 
        if (!main)
        {
            main = this;

            player.gameObject.SetActive(true);
            DontDestroyOnLoad(player);
            transform.parent = player;
        }
        else // Player already exists
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Pressed key to load chapter
        for (int i = 0; i < 3; i++)
        {
            if (Input.GetKeyDown("" + (i+1)))
            {
                StartCoroutine(LoadChapter(i));
            }
        }
    }

    public IEnumerator LoadChapter(int newChapter)
    {
        loading = true;

        // Fade to black
        OVRScreenFade.instance.fadeColor = Color.black;
        OVRScreenFade.instance.fadeTime = fadeTime;
        OVRScreenFade.instance.FadeOut();
        // Wait until screen is black
        yield return new WaitForSeconds(fadeTime + 0.5f);

        // Let go of any objects in hands
        for (int i = 0; i < 2; i++)
        {
            Player.hands[i].SetLocked(false);
            Player.hands[i].ForceRelease(Player.hands[i].grabbedObject);
        }

        // Loading Chapter 1
        if (newChapter == 0)
        {
            // Load lab scene
            SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);

            // Wait until scene fully loaded
            yield return new WaitUntil(() => SceneManager.GetActiveScene().buildIndex == 0);
        }
        else // Loading Chapter 2 or 3
        {
            // Allow Cell script to prepare scene for correct chapter
            Cell.skipToChapter = newChapter;

            // Load cell scene
            SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);

            // Wait until scene fully loaded
            yield return new WaitUntil(() => SceneManager.GetActiveScene().buildIndex == 1);
        }

        yield return new WaitForSeconds(0.5f);

        // Fade to clear
        OVRScreenFade.instance.FadeIn();

        loading = false;
    }
}
