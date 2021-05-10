using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxDetectorManager : MonoBehaviour
{
    [Serializable]
    public struct BoxAndTarget
    {
        public GameObject box;
        public GameObject target;
    }

    public BoxAndTarget[] boxAndTargetPairs;
    public DialogUI toContinue;

    public AudioSource correctAudio;
    public AudioSource incorrectAudio;
    public CellAudioManager cellAudioManager;

    // private int pairsFound;
    protected Dictionary<GameObject, GameObject> boxToTargetMatchings;
    protected Dictionary<GameObject, Vector3> targetOrigPositions;

    protected HashSet<GameObject> boxesAnswered;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        boxToTargetMatchings = new Dictionary<GameObject, GameObject>();
        targetOrigPositions = new Dictionary<GameObject, Vector3>();
        boxesAnswered = new HashSet<GameObject>();
        // pairsFound = 0;
        
        foreach (BoxAndTarget pair in boxAndTargetPairs)
        {
            boxToTargetMatchings[pair.box] = pair.target;
            targetOrigPositions[pair.target] = pair.target.transform.position;
        }
    }

    public virtual void PossibleTargetCollided(GameObject box, GameObject possibleTarget, Light pointLight)
    {
        CustomGrabbable grabbable = possibleTarget.GetComponent<CustomGrabbable>();
        if (grabbable)
        {
            OVRGrabber hand = grabbable.grabbedBy;
            if (hand)
            {
                hand.ForceRelease(grabbable);
            }
            // If it's the right object
            if (boxToTargetMatchings.ContainsKey(box) && boxToTargetMatchings[box] == possibleTarget)
            {
                possibleTarget.transform.position = box.transform.position;
                if (possibleTarget.GetComponent<DNADoubleHelix>())
                {
                    possibleTarget.transform.position += new Vector3(0, -0.548f, 0.172f);
                    if (possibleTarget.GetComponent<DNADoubleHelix>().isCartoon) possibleTarget.transform.position += new Vector3(0, -0.11f, 0);
                    possibleTarget.transform.rotation = possibleTarget.GetComponent<DNADoubleHelix>().initialRot;
                }
                
                if (boxesAnswered.Contains(box)) {
                    return; //the colliders sometimes hit for multiple frames. This stops it
                }
                
                possibleTarget.GetComponent<OVRGrabbable>().enabled = false;
                if (possibleTarget.GetComponent<Rigidbody>()) possibleTarget.GetComponent<Rigidbody>().isKinematic = true;
                
                // Vibrate the hand and play correct sound
                if (hand) Player.Vibrate(1.2f, 0.3f, hand.GetComponent<CustomGrabber>(), true);
                if (correctAudio) correctAudio.Play();
                Debug.Log($"BoxAnsweredCorrectly\t{box.name}\t{possibleTarget.name}\t{Time.timeSinceLevelLoad}");
                
                StartCoroutine(ColourLight(pointLight, Color.green, 1.5f));
                
                // Record the answer and progress the scene if the exercise is finished
                boxesAnswered.Add(box);
                if (boxesAnswered.Count == boxToTargetMatchings.Count)
                {
                    Debug.Log($"AllBoxesAnsweredCorrectly\t{name}\t{Time.timeSinceLevelLoad}");
                    toContinue.Resume();
                    cellAudioManager.PlayNextNarrativeClip();
                }
            }
            else
            {
                // if (targetOrigPositions.ContainsKey(possibleTarget))
                if (possibleTarget.GetComponent<DNADoubleHelix>())
                {
                    possibleTarget.GetComponent<DNADoubleHelix>().ResetToStartPosition();
                } else if (possibleTarget.GetComponent<Ruler>())
                {
                    possibleTarget.GetComponent<Ruler>().ResetPosition();
                }
                else
                {
                    possibleTarget.transform.position += transform.TransformDirection(new Vector3(0, 0, -0.12f));
                }
                if (hand) Player.Vibrate(0.2f, 1.0f, hand.GetComponent<CustomGrabber>(), true);
                if (incorrectAudio) incorrectAudio.Play();
                StartCoroutine(ColourLight(pointLight, Color.red, 0.5f));
                Debug.Log($"BoxAnsweredIncorrectly\t{box.name}\t{possibleTarget.name}\t{Time.timeSinceLevelLoad}");
            }
            
        }
    }

    protected IEnumerator ColourLight(Light light, Color colour, float time)
    {
        light.color = colour;
        yield return new WaitForSeconds(time);
        light.color = Color.white;
    }

    public void AnswerAllBoxes()
    {
        foreach (var matching in boxToTargetMatchings)
        {
            PossibleTargetCollided(matching.Key, matching.Value, matching.Key.GetComponent<BoxDetector>().pointLight);
        }
    }
}
