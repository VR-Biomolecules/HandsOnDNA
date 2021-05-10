using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
public class CartoonGenerator : MonoBehaviour
{
    int imageHeight;
    public GameObject[] dnaRenderers;
    public DNASpline dnaSpline;
    public Color[] baseColors;
    Vector3 tempScale;
    Quaternion tempRotation;
    Vector3 tempPos;
    public bool doubleStranded;


    public int[] bases;


    private void Awake()
    {
        tempScale = transform.localScale;
        tempPos = transform.position;
        Debug.Log(transform.position + " is the pos of the cartoon model.");
        transform.position = Vector3.zero;
        Debug.Log(transform.position + " is the pos of the cartoon model.");
        tempRotation = transform.rotation;
        transform.rotation = Quaternion.identity;
        transform.localScale = new Vector3(1, 1, 1);
    }
    IEnumerator Start()
    {
       
        
        
       imageHeight = dnaSpline.dnaSegments * 12;
        
        Texture2D texture = GenerateTexture();
        
        //First strand
        dnaRenderers[0].GetComponent<MeshRenderer>().material.mainTexture =  texture;
        if (doubleStranded)
        {
            
            dnaRenderers[1].GetComponent<MeshRenderer>().material.mainTexture = GenerateSecondTexture(texture);
        } else if (dnaRenderers.Length >= 2)
        {
            dnaRenderers[1].GetComponent<MeshRenderer>().enabled = false;
        }
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        transform.position = tempPos;
        transform.rotation = tempRotation;
        transform.localScale = tempScale;
    }
    
    Texture2D GenerateTexture()
    {
        Texture2D result = new Texture2D(1, imageHeight);

        for (int i = 0; i < dnaSpline.dnaSegments; i++)
        {
            for (int j = 0; j < bases.Length; j++)
            {
                
                result.SetPixel(1, j + 12*i, baseColors[bases[j] - 1]);
            }
        }
        //result.SetPixel(1, 0, Color.black);
        result.filterMode = FilterMode.Point;
        
        result.Apply(false, false);
        //File.WriteAllBytes("Assets/CartoonA.png", result.EncodeToPNG());
        return result;
    }

    Texture2D GenerateSecondTexture(Texture2D reference)
    {
        Texture2D result = new Texture2D(1, imageHeight);

        for (int i = 0; i < imageHeight; i++)
        {
            for (int j = 0; j < baseColors.Length; j++)
            {
                if (baseColors[j] == reference.GetPixel(1, i))
                {
                    result.SetPixel(1, i, baseColors[(j + 2) % 4]);
                }
            }
            

        }
        result.filterMode = FilterMode.Point;
        result.Apply();
        //File.WriteAllBytes("Assets/CartoonB.png", result.EncodeToPNG());
        return result;
    }

    
}
