using System;
using System.Collections;
using MoleculeTypes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class Cell : MonoBehaviour
{
    public static int skipToChapter = -1;

    public DialogUI chapterOnePointFiveUI;
    public DialogUI chapterTwoUI;
    public DialogUI chapterThreeUI;
    public DialogUI freeplayUI;

    public ToScale[] objectsToScale;

    [FormerlySerializedAs("dna")] public GameObject smashableDna;
    
    DNAStrand firstStrand;
    DNAStrand secondStrand;
    public DNAStrand brokenStrand;

    public GameObject batsParent;

    public GameObject chapterOnePointFiveObj;
    public GameObject chapterTwoObj;
    public GameObject chapterThreeObj;
    public Vector3 scaleOfThreeRepsModels = new Vector3(0.5f, 0.5f, 0.5f);
    public Transform threeRepBaSStartPosition;

    // Stored references so we can reset their positions between 3 reps questions
    public DNADoubleHelix vdwModelIn3Reps;
    public DNADoubleHelix basModelIn3Reps;
    public DNADoubleHelix cartoonModelIn3Reps;

    public BoxDetectorManager chapOnePointFiveBoxManager;
    public MultitargetBoxDetectorManager chapThreeBoxManager;
    public HelixPositionDetector helixDetector; // for reordering the bases scene
    public CellAudioManager cellAudioManager;

    public static Material BondMat;
    public static Material GreyMat;
    public Material BondMatHook;
    public Material GreyMatHook;

    public Material phosOrange;
    public Material sugarBlue;
    public Material nBaseGreen;

    void Start()
    {
        BondMat = BondMatHook;
        GreyMat = GreyMatHook;

        chapterTwoObj.SetActive(true);
        chapterTwoObj.SetActive(false);
        
        // Loaded scene from editor
        if (!ChapterSelector.main)
        {
            SceneManager.LoadScene(0);
            return;
        }

        // Setup Transitions
        foreach (ToScale t in objectsToScale)
        {
            t.SetupTransition();
        }

        // Selected Chapter
        if (skipToChapter != -1)
        {
            // Skip shrink
            objectsToScale[0].Skip();

            // Load Chapter 2
            if (skipToChapter == 1)
            {
                cellAudioManager.SwitchToChapter2();
                StartCoroutine(ChapterOnePointFive());
            }
            // Load Chapter 3
            else if (skipToChapter == 2)
            {
                cellAudioManager.SwitchToChapter3();
                StartCoroutine(ChapterThree());
            }
            else // Load Freeplay
            {
                cellAudioManager.playNarration = false;
                StartCoroutine(Freeplay());
            }

            skipToChapter = -1;
        }
        else // Came naturally from chapter 1
        {
            cellAudioManager.SwitchToChapter2();
            StartCoroutine(Playthrough());
        }
    }

    void Update()
    {
        // Scale
        foreach (ToScale t in objectsToScale)
        {
            t.Elapse();
        }
    }

    /**
     * Initiates playthrough of the cell scene by finishing the shrink. 
     */
    IEnumerator Playthrough()
    {
        // Fade to scene
        OVRScreenFade.instance.FadeIn();
        Ruler.instance.Play();
        // "Shrink" player to platform
        yield return new WaitForSeconds(objectsToScale[0].Play() - 1);

        StartCoroutine(ChapterOnePointFive());
    }

    /**
     * Called by the Next button at the end of each chapter.
     * - Ends the previous UI component
     * - Fades in/out chapter objects
     * - Starts chapter coroutines
     */
    public void NextChapter(int nextChapter)
    {
        if (nextChapter == 2) // Start 3 reps scene
        {
            // End old UI
            chapterOnePointFiveUI.Resume();

            //Scale up the vdw and the cartoon models
            foreach(Transform scaleUp in chapterTwoObj.transform)
            {
                StartCoroutine(LerpToPos(scaleUp, scaleOfThreeRepsModels, 2f, "scale"));
            }
            
            //Move and scale the BaS model into position as well
            StartCoroutine(LerpToPos(chapterOnePointFiveObj.transform, threeRepBaSStartPosition.position, 2f, "pos"));
            StartCoroutine(LerpToPos(chapterOnePointFiveObj.transform, scaleOfThreeRepsModels, 2f, "scale"));
            StartCoroutine(LerpToPos(chapterOnePointFiveObj.transform, threeRepBaSStartPosition.rotation.eulerAngles, 2f, "rot"));
            
            chapterOnePointFiveObj.GetComponent<DNADoubleHelix>().spining = false;
            chapterOnePointFiveObj.GetComponent<DNADoubleHelix>().interactive = true;
            chapterOnePointFiveObj.transform.GetChild(0).gameObject.SetActive(true); //Turn on the labeller
            
            StartCoroutine(ChapterTwo());
            StartCoroutine(SaveNewPosition(basModelIn3Reps, 2.0f));
        }
        else if (nextChapter == 3) // Start smash and put together scene
        {

            // End old UI
            chapterTwoUI.Resume();

            //Fade out objects
            foreach (Transform scaleUp in chapterTwoObj.transform)
            {
                StartCoroutine(LerpToScaleOut(scaleUp, new Vector3(0.0001f, 0.0001f, 0.0001f), 2f));
            }
            StartCoroutine(LerpToScaleOut(chapterOnePointFiveObj.transform, new Vector3(0.0001f, 0.0001f, 0.0001f), 2f));

            cellAudioManager.SwitchToChapter3();
            StartCoroutine(ChapterThree());
        }
    }

    /**
     * Prompt a DNA helix to cache a new start position after the given time
     */
    private IEnumerator SaveNewPosition(DNADoubleHelix helix, float time)
    {
        yield return new WaitForSeconds(time);
        helix.CacheNewStartPosition();
    }

    /**
     * Execution of the cell intro and identify the elements scene
     */
    public IEnumerator ChapterOnePointFive()
    {
        yield return new WaitForSeconds(1);
        chapterOnePointFiveObj.SetActive(true);

        // Ruler exists
        if (Ruler.instance != null)
        {
            // Unlock ruler hand
            Player.hands[Ruler.instance.InHand()].SetLocked(false);
            // Set new rigibody settings
            Ruler.instance.GetComponent<Rigidbody>().useGravity = false;
            Ruler.instance.GetComponent<Rigidbody>().drag = 5;
            Ruler.instance.GetComponent<Rigidbody>().angularDrag = 5;
            Ruler.instance.CacheNewInitialPos();
        }

        // Start UI and narration
        chapterOnePointFiveUI.Resume();
        cellAudioManager.PlayNextNarrativeClip();
        //backToLabUI.Resume();
    }

    public void ChapterOnePointFiveSkipButton()
    {
        chapOnePointFiveBoxManager.AnswerAllBoxes();
    }

    /**
     * Execution of the three representations scene
     */
    public IEnumerator ChapterTwo()
    {
        yield return new WaitForSeconds(1);
        chapterTwoObj.SetActive(true);
        
        if (Ruler.instance)
        {
            Ruler.instance.gameObject.SetActive(false);
        }
        
        yield return new WaitForSeconds(3);
        // Start UI
        chapterTwoUI.Resume();
        cellAudioManager.PlayNextNarrativeClip();
        //backToLabUI.Resume();

    }

    /**
     * Reset the question scene and start the next question playing during scene 2
     */
    public void Chapter2NextQuestion()
    {
        vdwModelIn3Reps.ResetToStartPosition();
        basModelIn3Reps.ResetToStartPosition();
        cartoonModelIn3Reps.ResetToStartPosition();
        chapterTwoUI.Resume();
        cellAudioManager.PlayNextNarrativeClip();
    }


    /**
     * Begin execution of the smash apart and put back together scene, chapter 3
     */
    public IEnumerator ChapterThree()
    {
        yield return new WaitForSeconds(2);
        EnableDnaAndBat();
        
        if (Ruler.instance)
        {
            Ruler.instance.gameObject.SetActive(true);
        }
        
        // Start UI and audio
        chapterThreeUI.Resume();
        cellAudioManager.PlayNextNarrativeClip();
    }

    /**
     * Begin DNA Freeplay/Sandbox scene
     */
    public IEnumerator Freeplay()
    {
        yield return new WaitForSeconds(1);
        
        EnableDnaAndBat();

        // Start UI
        freeplayUI.Resume();
        //backToLabUI.Resume();
    }

    /**
     * Enable the bat and the smashable DNA model, including ghosting the second strand.
     */
    void EnableDnaAndBat()
    {
        chapterThreeObj.SetActive(true);
        DNADoubleHelix helix = smashableDna.GetComponent<DNADoubleHelix>();
        helix.interactive = true;
        firstStrand = helix.strandA;
        secondStrand = helix.strandB;

        secondStrand.SetGhostMode(true);

        // Fade-in Bat
        batsParent.SetActive(true);
        objectsToScale[1].Play();
    }

    /**
     * Called at the beginning of the scene where the student starts reconstructing the strand,
     * beginning with a single nucleotide. 
     */
    public void StartMoleculeReassemblyScene()
    {
        StartCoroutine(PrepareDNAForReassemblyScene());
        batsParent.SetActive(false);
        chapThreeBoxManager.GiveMoleculesBack(); // Switch Box manager to feeding mode

        // Continue UI and narration
        chapterThreeUI.Resume();
        cellAudioManager.PlayNextNarrativeClip();
    }

    private IEnumerator PrepareDNAForReassemblyScene()
    {
        // Get the big DNA strand back in place, ghost the whole thing, and remove the bits being replaced
        yield return StartCoroutine(ResetDNACoroutine());
        firstStrand.SetGhostMode(false);
        foreach (Molecule molecule in firstStrand.GetAllMolecules())
        {
            // I know which bases were used to make the reconstructable strand, so keep them out here
            int molNum = int.Parse(molecule.name.Substring(0, 1));
            if (molNum == 7 || molNum == 6 || molNum == 5 || molNum == 4)
            {
                molecule.gameObject.SetActive(false);
            }
            else if (molNum == 3 && molecule.GetType() == typeof(Sugar))
            {
                Sugar sugar3 = (Sugar) molecule;
                sugar3.threePrimeBond.gameObject.SetActive(false);
            }
        }
    }

    /**
     * Called at the beginning of the scene where the user needs to reorder the bases and
     * connect the final strand back into the DNA double helix.
     */
    public void StartReplaceTheDNAScene()
    {
        firstStrand.SetGhostMode(true);
        
        // Display the complementary bases on the ghost strand, and label them
        foreach (NBase molecule in secondStrand.Bases)
        {
            int molNum = int.Parse(molecule.name.Substring(0, 2));
            if (molNum == 16 || molNum == 17 || molNum == 18 || molNum == 19)
            {
                molecule.Unfade();
                molecule.labeller.enabled = true;
                molecule.labeller.xOffsetOverride = -0.1f;
                molecule.labeller.yOffsetOverride = 0.001f;
                molecule.labeller.zOffsetOverride = -0.4f;
                molecule.labelRenderer.enabled = true;
                molecule.alwaysShowLabel = true;
            }
        }

        // Then set the backbone to grey to make it obvious
        foreach (Phosphate secondStrandPhosphate in secondStrand.Phosphates)
        {
            secondStrandPhosphate.SetGreyMode();
        }

        foreach (Sugar secondStrandSugar in secondStrand.Sugars)
        {
            secondStrandSugar.SetGreyMode();
        }

        // Let broken strand reorder bases again
        foreach (Bond bond in brokenStrand.gBonds)
        {
            bond.bondJoint.breakForce = 1500;
        }

        // Have the broken strand display labels when held
        foreach (NBase nBase in brokenStrand.Bases)
        {
            nBase.showLabelWhileConnected = true;
        }
        
        // Set broken strand backbone to grey
        foreach (Phosphate heldPhosphate in brokenStrand.Phosphates)
        {
            heldPhosphate.SetGreyMode();
        }

        foreach (Sugar heldSugar in brokenStrand.Sugars)
        {
            heldSugar.SetGreyMode();
        }
        
        // Set the rest of the remaining helix backbone to grey
        foreach (Phosphate phosphate in firstStrand.Phosphates)
        {
            phosphate.SetGreyMode();
        }

        foreach (Sugar sugar in firstStrand.Sugars)
        {
            sugar.SetGreyMode();
        }

        // Get orbs on the broken helix to start up
        helixDetector.ActivateSignalOrbs();
        
        chapterThreeUI.Resume();
        cellAudioManager.PlayNextNarrativeClip();
    }

    /**
     * Called when the user finished reordering the bases correctly and pushed the strand into place.
     */
    public void FinishHelixPlacement()
    {
        // Deactivate broken strand 
        brokenStrand.gameObject.SetActive(false);
        helixDetector.gameObject.SetActive(false);

        // And reactivate the mirroring mols in the smashable DNA strand
        StartCoroutine(ActivateMolsNextFrame());

        // turn labels off
        foreach (NBase molecule in secondStrand.Bases)
        {
            int molNum = int.Parse(molecule.name.Substring(0, 2));
            if (molNum == 16 || molNum == 17 || molNum == 18 || molNum == 19)
            {
                // molecule.sugar.Unfade();
                // molecule.sugar.fivePrimePhosphate.Unfade();
                // molecule.sugar.threePrimePhosphate.Unfade();
                molecule.labeller.enabled = false;
                molecule.labelRenderer.enabled = false;
                molecule.alwaysShowLabel = false;
            }
        }
        
        chapterThreeUI.Resume();
        cellAudioManager.PlayNextNarrativeClip();
    }

    private IEnumerator ActivateMolsNextFrame()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        foreach (Molecule molecule in firstStrand.GetAllMolecules())
        {
            // I know which bases were used to make the reconstructable strand, so keep them out here
            int molNum = int.Parse(molecule.name.Substring(0, 1));
            if (molNum == 7 || molNum == 6 || molNum == 5 || molNum == 4)
            {
                molecule.gameObject.SetActive(true);
                // if (!(molecule.GetType() == typeof(NBase))) molecule.SetGhostMode(true);
            } else if (molNum == 3 && molecule.GetType() == typeof(Sugar))
            {
                ((Sugar) molecule).threePrimeBond.gameObject.SetActive(true);
            }
        }
    }

    /**
     * Called as the user starts going through the final two slides recapping the DNA subunits
     * and bond names. Obviously colours the different subunit types. 
     */
    public void ColourStrandsForFinalSummary()
    {
        foreach (Phosphate phosphate in firstStrand.Phosphates)
        {
            phosphate.SetColourToMat(phosOrange);
        }

        foreach (Phosphate phosphate in secondStrand.Phosphates)
        {
            phosphate.SetColourToMat(phosOrange);
        }

        foreach (Sugar sugar in firstStrand.Sugars)
        {
            sugar.SetColourToMat(sugarBlue);
        }
        
        foreach (Sugar sugar in secondStrand.Sugars)
        {
            sugar.SetColourToMat(sugarBlue);
        }

        foreach (NBase nBase in firstStrand.Bases)
        {
            nBase.SetColourToMat(nBaseGreen);
        }
        
        foreach (NBase nBase in secondStrand.Bases)
        {
            nBase.SetColourToMat(nBaseGreen);
        }
    }

    public void ResetDNA()
    {
        StartCoroutine(ResetDNACoroutine());
    }

    public IEnumerator ResetDNACoroutine()
    {
        // Let go of hands
        if (Player.hands[0].grabbedObject) Player.hands[0].ForceRelease(Player.hands[0].grabbedObject);
        if (Player.hands[1].grabbedObject) Player.hands[1].ForceRelease(Player.hands[1].grabbedObject);

        yield return smashableDna.GetComponent<DNADoubleHelix>().ResetAllAtomsAndBonds();

        // Bat exists
        if (Bat.instance)
        {
            // Bat is not grabbed
            Transform batTf = Bat.instance.batTransform;
            if (!batTf.parent || batTf.parent == batsParent.transform)
            {
                // Teleport bat to start position
                Bat.instance.ResetToStart();
            }
        }
    }

    /**
     * Sends user back to lab scene. Called by Back to Lab button
     */
    public void BackToLab()
    {
        if (!ChapterSelector.main.loading)
        {
            ChapterSelector.main.StartCoroutine(ChapterSelector.main.LoadChapter(0));
        }
    }

    /**
     * Quits game. Called by Finish button on the final canvas
     */
    public void ExitGame()
    {
        Application.Quit(); //also finish cleanup and logging if needed
    }

    IEnumerator LerpToPos(Transform mover, Vector3 goToPosition, float waitTime, string mode)
    {
        float elapsedTime = 0;
        Vector3 startPos = Vector3.zero;
        if (mode == "pos")
        {
            startPos = mover.position;
        }
        else if (mode == "rot")
        {
            startPos = mover.eulerAngles;
        }
        else if (mode == "scale")
        {
            startPos = mover.localScale;
        }
        while (elapsedTime < waitTime)
        {
            Vector3 lerpedVector = Vector3.Lerp(startPos, goToPosition, (elapsedTime / waitTime));
            if (mode == "pos") {
                mover.position = lerpedVector;
            } else if (mode == "rot")
            {
                mover.eulerAngles = lerpedVector;
            } else if (mode == "scale")
            {
                mover.localScale = lerpedVector;
            }
            elapsedTime += Time.deltaTime;

            // Yield here
            yield return null;
        }
        // Make sure we got there
        if (mode == "pos")
        {
            mover.position = goToPosition;
        }
        else if (mode == "rot")
        {
            mover.eulerAngles = goToPosition;
        }
        else if (mode == "scale")
        {
            mover.localScale = goToPosition;
        }
        yield return null;
    }

    IEnumerator LerpToScaleOut(Transform mover, Vector3 goToScale, float waitTime)
    {
        float elapsedTime = 0;
        Vector3 startScale = mover.localScale;
        while (elapsedTime < waitTime)
        {
            mover.localScale = Vector3.Lerp(startScale, goToScale, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;

            // Yield here
            yield return null;
        }
        // Remove the object
        mover.gameObject.SetActive(false);
        yield return null;
    }
}
