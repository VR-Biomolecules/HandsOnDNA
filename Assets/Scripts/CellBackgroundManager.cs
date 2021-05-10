using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBackgroundManager : MonoBehaviour
{
    public Gradient colourGradient;
    public float maxTime = 1;
    float elapsedTime;

    private void Update()
    {
        Color currentColor = colourGradient.Evaluate(elapsedTime / maxTime);

        if (RenderSettings.skybox.HasProperty("_Tint"))
            RenderSettings.skybox.SetColor("_Tint", currentColor);
        else if (RenderSettings.skybox.HasProperty("_SkyTint"))
            RenderSettings.skybox.SetColor("_SkyTint", currentColor);

        // Elapse
        elapsedTime = (elapsedTime + Time.deltaTime) % maxTime;
    }

    /*
    void Start()
    {
        StartCoroutine("ColorFade");
    }
    IEnumerator ColorFade()
    {
        int currentCol = 0;
        int nextCol = 1;
        Color currentColor;
        float timer;
        timer = 0;

        

        while (true)
        {

            timer = 0;

            while (timer <= 1)
            {
                currentColor = Color.Lerp(colorFade[currentCol], colorFade[nextCol], timer);

                if (RenderSettings.skybox.HasProperty("_Tint"))
                    RenderSettings.skybox.SetColor("_Tint", currentColor);
                else if (RenderSettings.skybox.HasProperty("_SkyTint"))
                    RenderSettings.skybox.SetColor("_SkyTint", currentColor);
                timer += Time.deltaTime/timeFadeDelay;
                yield return new WaitForEndOfFrame();
            }
            print("NOW");
            if (currentCol < colorFade.Length - 1)
            {
                currentCol++;
            }
            else
            {
                currentCol = 0;
            }
            if (nextCol < colorFade.Length - 1)
            {
                nextCol++;
            }
            else
            {
                nextCol = 0;
            }
            print(currentCol + " " + nextCol);
            
        }
    }
    */
}
