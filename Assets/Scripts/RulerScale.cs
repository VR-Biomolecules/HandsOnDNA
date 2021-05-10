using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RulerScale : MonoBehaviour
{
    public GameObject[] rulers;
    int currentRuler = 0;
    public float maxScaleX = 5.38f;
    public Vector3 smallScale;
    public Vector3 smallPosition;
    public float scaleFactor = 1.05f;
    public GameObject startingRuler;
    //public Text zoomText;
    public bool shrink = true;
    string[] units = new string[] {"10 Cm", "1 Cm", "1 Mm", "100 um", "10 um", "1 um", "100 nm", "10 nm", "1 nm", "1 Å", "10pm " };
    int zoomLevel;
    int zoomStage;
    float scaleLevel = 1f;
    public float maxShrinkTime;
    float timer;
    
    public AnimationCurve shrinkCurve;

    Vector3[] originScale;
    float[] timers;
    float[] scaleTimers;
    float estimateMaxTime = 0;

    bool pausing;


    private void Start()
    {
        scaleTimers = new float[rulers.Length];
        timers = new float[rulers.Length];
        originScale = new Vector3[rulers.Length];
        currentRuler = 0;
        zoomLevel = 0;
        for (int i = 0; i < rulers.Length; i++)
        {
            rulers[i].GetComponentInChildren<Text>().text = units[i];
            originScale[i] = rulers[i].transform.localScale;
            timers[i] = 0;
        }
        //zoomText.text = units[zoomLevel];
        PauseShrink();
    }





    private void FixedUpdate()
    {
        if (timer < maxShrinkTime && shrink)
            {
                timer += 1;
            }
        for (int i = 0; i < rulers.Length; i++) {
            if (timers[i] < maxShrinkTime && shrink)
            {
                timers[i] += 1;
                
                //scaleFactor = shrinkCurve.Evaluate(timers[i]/maxShrinkTime);
            } 
            scaleTimers[i] = shrinkCurve.Evaluate(timers[i]);
        }
        
        
    



        int nextIndex = (currentRuler + 1) % rulers.Length;
        if (rulers[nextIndex].transform.localScale.x >= maxScaleX)
        {
           float differenceInScale = rulers[nextIndex].transform.localScale.x - maxScaleX;
            if (estimateMaxTime == 0) {
                estimateMaxTime = timer * (units.Length-1);
            }
            if (zoomLevel+2 >= units.Length)
            {
                shrink = false;
            }
            float timeOverflow = 0;
                
            if (rulers[nextIndex].transform.localScale.x > maxScaleX) {
                timeOverflow = Mathf.Log((differenceInScale/maxScaleX) + 1, scaleFactor);
                //timeOverflow = RequiredTime(Mathf.Log((differenceInScale/maxScaleX) + 1, scaleFactor)) + timers[nextIndex];
                //timers[(nextIndex+1)%rulers.Length] += timeOverflow;
                Debug.Log("Scale Difference: " + (differenceInScale) + ", Time overflow: " + timeOverflow + ", Scaled time: " + shrinkCurve.Evaluate(timeOverflow));
            }


            scaleLevel *= 0.1f;
            //Debug.Log(scaleLevel);
            rulers[currentRuler].transform.localScale = smallScale;

            rulers[currentRuler].transform.SetAsLastSibling();
            rulers[currentRuler].GetComponentInChildren<Text>().text = units[(zoomLevel+rulers.Length)%units.Length];
            timers[currentRuler] = timeOverflow;
            
            originScale[currentRuler] = rulers[currentRuler].transform.localScale;//*Mathf.Pow(scaleFactor,shrinkCurve.Evaluate(timeOverflow));

            

            currentRuler = nextIndex;
            zoomLevel = zoomLevel + 1;
             if (zoomLevel == 3 || zoomLevel == 10) {
                 PauseShrink();
             
            } else {
                PlayShrink();
            }

            
          
            
        }



        for (int i = 0; i < rulers.Length; i++)
        {
            if (shrink) { 
                rulers[i].transform.localScale = new Vector3(originScale[i].x*Mathf.Pow(scaleFactor,shrinkCurve.Evaluate(timers[i])), originScale[i].y*Mathf.Pow(scaleFactor,shrinkCurve.Evaluate(timers[i])), rulers[i].transform.localScale.z);
                
            }
        }



        
        //startingRuler.transform.localScale = new Vector3(startingRuler.transform.localScale.x * scaleFactor, startingRuler.transform.localScale.y * scaleFactor, startingRuler.transform.localScale.z);

        
    }
    public float RequiredTime(float value) {
        float t = 0;
        while (t < shrinkCurve[shrinkCurve.length -1].time) {
            t++;
            if (shrinkCurve.Evaluate(t) == value) {
                return t;
            }
        }
        return 0f;
    }

    public void PauseShrink() {
         if (!pausing) {
                zoomStage ++;
                pausing = true;
                shrink = false;
             }
    }

    public void PlayShrink() {
        pausing = false;
        shrink = true;
    }
}
