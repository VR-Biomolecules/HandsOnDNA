using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetScaleScript : MonoBehaviour
{
    [Header("Object references")]
    Renderer rend1;
    Renderer rend2;
    public GameObject renderer2;

    [Header("Initial values")]
    public float scaleSpeed;
    public float startScale1;
    public float endScale1;
    public float startScale2;
    public float endScale2;
    float scaleY1;
    float scaleY2;
    public float scale2Offset;
    bool timerOn = false;
    float timerValue;

    [Header("Opacity transition")]
    public AnimationCurve alphaUp;
    public AnimationCurve alphaDown;
    public float fadeRate;

    // Start is called before the first frame update
    void Start()
    {
        rend1 = GetComponent<Renderer>();
        rend2 = renderer2.GetComponent<Renderer>();

        scaleY1 = rend1.material.GetTextureScale("_MainTex").y;
        scaleY2 = rend2.material.GetTextureScale("_MainTex").y + 1.33f;

        Debug.Log(scaleY1);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timerOn)
        {
            timerValue += fadeRate;

        }



        if (scaleY1 >= endScale1)
        {
            scaleY1 = startScale1;
            StartCoroutine(OpacityChange(rend2));
        }
        if (scaleY2 >= endScale2)
        {
            scaleY2 = startScale2;

        }
        scaleY1 += Time.deltaTime * scaleSpeed;
        scaleY2 += Time.deltaTime * scaleSpeed + scale2Offset;
        rend1.material.SetTextureScale("_MainTex", new Vector2(1, scaleY1));
        rend2.material.SetTextureScale("_MainTex", new Vector2(1, scaleY2));

        }

        IEnumerator OpacityChange(Renderer fadeInOut)
        {
            while (timerValue < 1)
            {
                    timerOn = true;
                    yield return new WaitForEndOfFrame();
                    fadeInOut.material.SetColor("_Color", new Color(1, 1, 1, alphaUp.Evaluate(timerValue)));
                }

          
       
            timerOn = false;
            timerValue = 0;
        }
    }

