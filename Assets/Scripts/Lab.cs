using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus;


public class Lab : MonoBehaviour
{
    public DialogUI mainUI;
    public DialogUI tinyUI;
    
    public Transform[] grabbables;

    public ToScale[] objectsToScale;

    bool continueShrink;

    public Renderer layer1Render;
    public Material oldMaterial;
    public Material newMaterial;
    bool bol;

    Coroutine playthrough;
    public LabAudioManager AudioManager;

    private void Awake()
    {
        foreach(ToScale t in objectsToScale)
        {
            t.SetupTransition();
        }
    }

    void Update()
    {
        // Scale room
        foreach (ToScale t in objectsToScale)
        {
            t.Elapse();
        }

        if (Input.GetKeyDown("i"))
        {
            bol = !bol;
        }
    }

    public void SelectedChapter(int newChapter)
    {
        // Selected chapter using canvas buttons
        if (playthrough == null && !ChapterSelector.main.loading)
        {
            // Play Lab Chapter
            if (newChapter == 0)
            {
                playthrough = StartCoroutine(Playthrough());
            }
            else // Load other chapter
            {
                ChapterSelector.main.StartCoroutine(ChapterSelector.main.LoadChapter(newChapter));
            }
        }
    }

    public void ContinueShrink()
    {
        continueShrink = true;
        tinyUI.Resume();
    }

    IEnumerator Playthrough()
    {
        // Start main UI
        mainUI.Resume();
        yield return new WaitForSeconds(1f);
        AudioManager.PlayNextNarrativeClip();

        // Wait until user has grabbed Ruler
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Ruler.instance.InHand() != -1);

        // Hand index which doesn't hold the ruler
        int otherHand = ((Ruler.instance.InHand() + 1) % 2);
        // Lock ruler hand
        Player.hands[Ruler.instance.InHand()].SetLocked(true);
        // Drop anything in other hand
        Player.hands[otherHand].ForceRelease(Player.hands[otherHand].grabbedObject);

        // Remove Grabbable-ness from other grabbables
        foreach (Transform t in grabbables)
        {
            Destroy(t.GetComponent<CustomGrabbable>());
        }

        yield return new WaitForSeconds(1);

        // Play audio and skip UI narration for now
        AudioManager.StartShrinkPhase1();
        mainUI.skipAudio = true;

        // Shrink to tissue sample
        Ruler.instance.Play();
        yield return new WaitForSeconds(objectsToScale[0].Play());

        // Freeze grabbables
        foreach (Transform t in grabbables)
        {
            t.GetComponent<Rigidbody>().isKinematic = true;
        }

        // Start tiny UI
        tinyUI.Resume();

        // Wait for user interaction
        yield return new WaitUntil(() => continueShrink);
        yield return new WaitForSeconds(0.25f);

        // Shrink
        Ruler.instance.Play();
        objectsToScale[0].Play();
        objectsToScale[1].Play();
        AudioManager.StartShrinkPhase2();
        // Enable second layer
        objectsToScale[2].objectToScale.gameObject.SetActive(true);
        // Set layer for hands and Ruler to "Infront"
        if (Player.hands[0]) { Player.hands[0].transform.GetComponentInChildren<SkinnedMeshRenderer>().gameObject.layer = 10; }
        if (Player.hands[1]) { Player.hands[1].transform.GetComponentInChildren<SkinnedMeshRenderer>().gameObject.layer = 10; }
        foreach(Transform t in Ruler.instance.transform.GetComponentsInChildren<Transform>()) { t.gameObject.layer = 10; }

        yield return new WaitForSeconds(20f);
        // Expand second layer 
        yield return new WaitForSeconds(objectsToScale[2].Play());

        // Rerender tissue layer 2
        foreach(Renderer r in objectsToScale[2].objectToScale.GetComponentsInChildren<Renderer>())
        {
            // Set new material
            r.material = newMaterial;
            // Remove from infront layer
            r.gameObject.layer = 15;
        }
        // Disable render on layer 1
        layer1Render.enabled = false;

        // Set layer for hands to default
        if (Player.hands[0]) { Player.hands[0].transform.GetComponentInChildren<SkinnedMeshRenderer>().gameObject.layer = 0; }
        if (Player.hands[1]) { Player.hands[1].transform.GetComponentInChildren<SkinnedMeshRenderer>().gameObject.layer = 0; }
        foreach (Transform t in Ruler.instance.transform.GetComponentsInChildren<Transform>()) { t.gameObject.layer = 0; }

        // Reset waiting for shrink
        continueShrink = false;
        // Start tiny UI
        tinyUI.Resume();

        yield return new WaitUntil(() => continueShrink);
        yield return new WaitForSeconds(1f);

        // Expand second layer 
        Ruler.instance.Play();
        AudioManager.StartShrinkPhase3();
        yield return new WaitForSeconds(objectsToScale[2].Play() - 2);

        OVRScreenFade.instance.fadeColor = new Color(0.44f, 0.11f, 0.2f);
        OVRScreenFade.instance.fadeTime = 2;
        OVRScreenFade.instance.FadeOut();

        yield return new WaitForSeconds(2.5f);

        SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
    }

    public void DropHair()
    {
        StartCoroutine(DropHairCoroutine());
    }

    private IEnumerator DropHairCoroutine()
    {
        // Drop Hair
        AudioManager.narration.Stop();
        yield return new WaitForSeconds(2.0f);
        objectsToScale[3].objectToScale.gameObject.SetActive(true);
        objectsToScale[3].Play();
        yield return new WaitForSeconds(0.7f);
        AudioManager.HairThud();
        yield return new WaitForSeconds(1.8f);
        tinyUI.Resume();
        AudioManager.PlayNextNarrativeClip();
    }
}
