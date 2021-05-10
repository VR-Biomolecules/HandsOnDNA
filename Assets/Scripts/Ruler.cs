using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Ruler : MonoBehaviour
{
    public static Ruler instance;

    public Transform[] rulers;

    public float[] offsets;

    public AnimationCurve statesCurve;

    public List<float> pauseTimes;

    public Text[] rulerTexts;
    string[] units = new string[] { "10 cm", "1 cm", "1 mm", "100 \u03BCm", "10 \u03BCm", "1 \u03BCm", "100 nm", "10 nm", "1 nm", "1 Å"};

    public bool elapse;
    public float time;

    [Space]

    public Vector3 respawnDetectPoint;
    public float respawnMinRange;
    public float respawnAfterTime;
    Vector3 respawnPosition;
    Quaternion respawnRotation;
    float elapsedRespawnTime;

    private void Awake()
    {
        instance = this;

        respawnPosition = transform.position;
        respawnRotation = transform.rotation;
    }

    void Update()
    {
        // Elapse Time
        if (elapse)
        {
            time += Time.deltaTime;
        }

        // Check for pause
        if (pauseTimes.Count >= 1 && time > pauseTimes[0])
        {
            Pause();
            pauseTimes.RemoveAt(0);
        }

        // Update scale
        for (int i = 0; i < 3; i++)
        {
            float stateTime = statesCurve.Evaluate(time);
            int unitIndex = i + ((int)((stateTime+(3-i-1)) / 3) * 3);

            // Set scale
            rulers[i].localScale = new Vector3(1,1,1) * ScaleByState(stateTime, offsets[i]);

            // Set text
            if (unitIndex < units.Length)
            {
                if (rulerTexts[i].text != "" + units[unitIndex])
                {
                    rulerTexts[i].text = "" + units[unitIndex];
                }
            }
            else if (rulerTexts[i].text != "")
            {
                rulerTexts[i].text = "";
            }
        }

        // Set middle child
        for (int i = 0; i < 3; i++)
        {
            if ((rulers[i].localScale.x < rulers[(i + 1) % 3].localScale.x && rulers[i].localScale.x > rulers[(i + 2) % 3].localScale.x) ||
                (rulers[i].localScale.x > rulers[(i + 1) % 3].localScale.x && rulers[i].localScale.x < rulers[(i + 2) % 3].localScale.x))
            {
                rulers[i].transform.SetAsLastSibling();
            }
        }
        // Set smallest child
        for (int i = 0; i < 3; i++)
        {
            if (rulers[i].localScale.x < rulers[(i + 1) % 3].localScale.x && rulers[i].localScale.x < rulers[(i + 2) % 3].localScale.x)
            {
                rulers[i].transform.SetAsLastSibling();
            }
        }

        /// Respawning
        if (time == 0)
        {
            // Out of range and not grabbed
            if (Vector3.Distance(transform.position, respawnDetectPoint) > respawnMinRange && !transform.parent)
            {
                // Elapse time
                elapsedRespawnTime += Time.deltaTime;

                // Enough time has passed to respawn it
                if (elapsedRespawnTime > respawnAfterTime)
                {
                    transform.position = respawnPosition;
                    transform.rotation = respawnRotation;
                    elapsedRespawnTime = 0;
                }
            }
            // In range, don't elapse respawn time
            else if (elapsedRespawnTime > 0)
            {
                elapsedRespawnTime = 0;
            }
        }
    }

    public void Play()
    {
        elapse = true;
    }

    public void Pause()
    {
        elapse = false;
    }

    float ScaleByState(float stateTime, float offset)
    {
        float newTime = (stateTime / 2 + offset) % 1.5f;

        return Mathf.Pow(11.111111f, 2 * newTime - 2);
    }

    public int InHand()
    {
        for (int i = 0; i < 2; i++)
        {
            if (Player.hands[i].grabbedObject && Player.hands[i].grabbedObject.GetComponent<Ruler>())
            {
                return i;
            }
        }

        return -1;
    }

    public void CacheNewInitialPos()
    {
        respawnPosition = transform.position;
        respawnRotation = transform.rotation;
    }

    public void ResetPosition()
    {
        transform.position = respawnPosition;
        transform.rotation = respawnRotation;
    }
}
