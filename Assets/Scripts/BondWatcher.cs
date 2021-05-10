using System.Collections;
using System.Collections.Generic;
using MoleculeTypes;
using UnityEngine;

public class BondWatcher : MonoBehaviour
{
    public CanvasGroup DiscoveryPanel;
    public MultitargetBoxDetectorManager boxManager;
    public DialogUI toContinue;
    
    public CanvasGroup FivePrimeText;
    public CanvasGroup GlycoText;
    public CanvasGroup ThreePrimeText;

    public AudioSource FivePrimeAudio;
    public AudioSource GlycoAudio;
    public AudioSource ThreePrimeAudio;
    public CellAudioManager cellAudioManager;

    private CanvasGroup currentyDisplaying;
    private AudioSource currentlyPlaying;
    
    private bool fivePrimeShown = false; 
    private bool glycoShown = false; 
    private bool threePrimeShown = false;

    private bool buildingFirstNucleotide = true;
    private bool madeFivePrime;
    private bool madeThreePrime;
    private bool madeGlyco;

    // Set to true once the user is just reordering the bases
    private bool checkBaseOrders;
    public bool baseOrderCorrect;
    
    public CanvasGroup OrderWrongText;
    public AudioSource OrderWrongAudio;
    
    public CanvasGroup DirectionWrongText;
    public AudioSource DirectionWrongAudio;

    public void FivePrimeFormed()
    {
        // Counting towards final bond formed. Needs to happen before nucleotide
        if (AllBondsFormed())
        {
            FinalBondFormed();
            return;
        }
        
        // If tip hasn't been displayed, do it
        if (!fivePrimeShown)
        {
            if (DiscoveryPanel.alpha < 0.1) DiscoveryPanel.alpha = 1;
            fivePrimeShown = true;
            StartCoroutine(DisplayTip(FivePrimeText, FivePrimeAudio));
        }

        // Check where we are in current nucleotide build
        if (buildingFirstNucleotide)
        {
            madeFivePrime = true;
            if (madeFivePrime && madeGlyco)
            {
                madeFivePrime = false;
                madeGlyco = false;
                buildingFirstNucleotide = false;
                StartCoroutine(ResumeUIAfterAudio());
            }
        }
        else 
        {
            madeFivePrime = true;
            if (madeFivePrime && madeGlyco && madeThreePrime)
            {
                boxManager.DisplayNextBackboneMols();
                madeFivePrime = false;
                madeGlyco = false;
                madeThreePrime = false;
            }
        }
    }

    public void GlycoFormed()
    {
        if (checkBaseOrders) // We're in the final exercise and just the NBases can be reordered
        {
            CheckForCorrectBaseOrder();
            return;
        }
        
        if (AllBondsFormed())
        {
            FinalBondFormed();
            return;
        }
        
        if (!glycoShown)
        {
            if (DiscoveryPanel.alpha < 0.1) DiscoveryPanel.alpha = 1;
            glycoShown = true;
            StartCoroutine(DisplayTip(GlycoText, GlycoAudio));
        }
        
        // Check where we are in current nucleotide build
        if (buildingFirstNucleotide)
        {
            madeGlyco = true;
            if (madeFivePrime && madeGlyco)
            {
                madeFivePrime = false;
                madeGlyco = false;
                buildingFirstNucleotide = false;
                StartCoroutine(ResumeUIAfterAudio());
            }
        }
        else 
        {
            madeGlyco = true;
            if (madeFivePrime && madeGlyco && madeThreePrime)
            {
                boxManager.DisplayNextBackboneMols();
                madeFivePrime = false;
                madeGlyco = false;
                madeThreePrime = false;
            }
        }
    }

    public void ThreePrimeFormed(bool correct)
    {
        if (AllBondsFormed())
        {
            FinalBondFormed();
            return;
        }
        
        // Check where we are in current nucleotide build
        if (!buildingFirstNucleotide)
        {
            madeThreePrime = correct;
            if (madeFivePrime && madeGlyco && madeThreePrime)
            {
                
                boxManager.DisplayNextBackboneMols();
                madeFivePrime = false;
                madeGlyco = false;
                madeThreePrime = false;
            }
        }

        if (!threePrimeShown)
        {
            if (DiscoveryPanel.alpha < 0.1) DiscoveryPanel.alpha = 1;
            threePrimeShown = true;
            StartCoroutine(DisplayTip(ThreePrimeText, ThreePrimeAudio));
        }
    }

    private bool AllBondsFormed()
    {
        return boxManager.MoleculeDisplayData[0].data.mol.connectedMols.Count == 12;
    }

    public void FinalBondFormed()
    {
        StopCurrentTip();
        StartCoroutine(ResumeUIAfterAudio());
    }

    private bool waitingForDisplay = false;

    private IEnumerator DisplayTip(CanvasGroup text, AudioSource audio)
    {
        StopCurrentTip();
        currentyDisplaying = text;
        currentlyPlaying = audio;
        waitingForDisplay = true; // this was necessary to stop narration starting during final tip display
        yield return new WaitForSeconds(0.5f);
        audio.Play();
        waitingForDisplay = false;
        currentyDisplaying.alpha = 1;
    }

    public void StopCurrentTip()
    {
        if (currentyDisplaying)
        {
            // Stop the current tip being displayed
            currentyDisplaying.alpha = 0;
            if (currentlyPlaying && currentlyPlaying.isPlaying) currentlyPlaying.Stop();
            currentyDisplaying = null;
            currentlyPlaying = null;
        }
    }

    private IEnumerator ResumeUIAfterAudio()
    {
        boxManager.StopBondsBreakingInCurrentChain();
        yield return new WaitUntil(() => !waitingForDisplay && (!currentlyPlaying || !currentlyPlaying.isPlaying));
        yield return new WaitForSeconds(1.0f);
        
        toContinue.Resume();
        cellAudioManager.PlayNextNarrativeClip();
    }

    public void FivePrimeBroken()
    {
        madeFivePrime = false;
    }

    public void GlycoBroken()
    {
        baseOrderCorrect = false;
        madeGlyco = false;
    }

    public void ThreePrimeBroken(bool correctBondBroken)
    {
        if (correctBondBroken) madeThreePrime = false;
    }

    public void StartWaitingForCorrectBaseOrder()
    {
        checkBaseOrders = true;
        StopCurrentTip();
        CheckForCorrectBaseOrder();
    }

    /**
     * To fit into the strand, the base order needs to be TACG. This checks the base order and sets a bool
     */
    private void CheckForCorrectBaseOrder()
    {
        Phosphate firstPhos = (Phosphate) boxManager.MoleculeDisplayData[0].data.mol;

        NBase firstBase = firstPhos.fivePrimeSugar.NBase;
        NBase secondBase = firstPhos.fivePrimeSugar.threePrimePhosphate.fivePrimeSugar.NBase;
        NBase thirdBase = firstPhos.fivePrimeSugar.threePrimePhosphate.fivePrimeSugar.threePrimePhosphate.fivePrimeSugar.NBase;
        NBase fourthBase = firstPhos.fivePrimeSugar.threePrimePhosphate.fivePrimeSugar.threePrimePhosphate.fivePrimeSugar
            .threePrimePhosphate.fivePrimeSugar.NBase;
        
        if (firstBase && firstBase.baseType == NBase.BaseType.ADENINE
            && secondBase && secondBase.baseType == NBase.BaseType.THYMINE
            && thirdBase && thirdBase.baseType == NBase.BaseType.GUANINE
            && fourthBase && fourthBase.baseType == NBase.BaseType.CYTOSINE)
        {
            baseOrderCorrect = true;
        }
        else
        {
            baseOrderCorrect = false;
        }
    }

    public void DisplayOrderIsWrongTip()
    {
        StartCoroutine(DisplayTip(OrderWrongText, OrderWrongAudio));
    }

    public void DisplayAntiparallelTip()
    {
        StartCoroutine(DisplayTip(DirectionWrongText, DirectionWrongAudio));
    }
}
