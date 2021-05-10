using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HelixPositionDetector : MonoBehaviour
{
    public GameObject phosSignal;
    public GameObject phosDetector;
    public GameObject sugarSignal;
    public GameObject sugarDetector;
    public BondWatcher orderChecker;
    public Cell cell;

    public Material correctMat;
    public Material wrongMat;

    public AudioSource incorrect;

    public float distanceDelta;
    private float tipTimer = 0;

    // Update is called once per frame
    void Update()
    {
        if (tipTimer > 0)
        {
            tipTimer -= Time.deltaTime;
        }
        
        if (PhosInRange() && SugarInRange())
        {
            if (orderChecker.baseOrderCorrect)
            {
                Debug.Log($"HelixCheckerCorrect\t{Time.timeSinceLevelLoad}");
                phosSignal.SetActive(false);
                sugarSignal.SetActive(false);
                orderChecker.StopCurrentTip();
                cell.FinishHelixPlacement();
            }
            else if (tipTimer < 0.0001f)
            {
                Debug.Log($"HelixCheckerBaseOrderWrong\t{Time.timeSinceLevelLoad}");
                orderChecker.DisplayOrderIsWrongTip();
                incorrect.Play();
                StartCoroutine(ChangeOrbsToWrongColour());
                tipTimer = 4f;
            }
        }
        else if (AtomsInOppositePlace() && tipTimer < 0.0001f)
        {
            Debug.Log($"HelixCheckerOrientationWrong\t{Time.timeSinceLevelLoad}");
            orderChecker.DisplayAntiparallelTip();
            incorrect.Play();
            StartCoroutine(ChangeOrbsToWrongColour());
            tipTimer = 4f;
        }
    }

    private bool AtomsInOppositePlace()
    {
        return Vector3.Distance(sugarSignal.transform.position, phosDetector.transform.position) < distanceDelta &&
               Vector3.Distance(phosSignal.transform.position, sugarDetector.transform.position) < distanceDelta;
    }

    private IEnumerator ChangeOrbsToWrongColour()
    {
        phosDetector.GetComponent<Renderer>().material = wrongMat;
        phosSignal.GetComponent<Renderer>().material = wrongMat;
        sugarDetector.GetComponent<Renderer>().material = wrongMat;
        sugarSignal.GetComponent<Renderer>().material = wrongMat;
        yield return new WaitForSeconds(1.5f);
        phosDetector.GetComponent<Renderer>().material = correctMat;
        phosSignal.GetComponent<Renderer>().material = correctMat;
        sugarDetector.GetComponent<Renderer>().material = correctMat;
        sugarSignal.GetComponent<Renderer>().material = correctMat;
    }

    private bool SugarInRange()
    {
        return Vector3.Distance(sugarSignal.transform.position, sugarDetector.transform.position) < distanceDelta;
    }

    private bool PhosInRange()
    {
        return Vector3.Distance(phosSignal.transform.position, phosDetector.transform.position) < distanceDelta;
    }

    /**
     * Starts displaying the orbs on the user's strand, so they know what to bring together
     */
    public void ActivateSignalOrbs()
    {
        phosSignal.SetActive(true);
        sugarSignal.SetActive(true);
    }
}
