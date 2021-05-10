using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tubular;
using Curve;

public class RNAMovement : MonoBehaviour
{

    CurveTester curveTester;
    List<Vector3> curvePointsStart = new List<Vector3>();
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    Curve.Curve curve;
    CartoonGenerator cartoonGenerator;
    bool needToUpdate = false;
    public List<GameObject> cylinders = new List<GameObject>();

    public GameObject cylinderPrefab;
    float texScale;
    float currentBasesnum;
    // 0 to 1, the % of the curve where it becomes invisible
    float progressVisible;
    float startProgressVisible = 999;


    [Header("Animation properties")]
    public Transform creator;
    Vector3 creatorOrigin;
    public float scaleFactor;
    public AnimationCurve bendCurve;
    [Tooltip("How many points should move with the cube")]
    public int endPointRange;

    [Header("Tubular Settings")]
    public TubularSettings tubularSettings;






    private void Start()
    {
        creatorOrigin = creator.position;
        cartoonGenerator = GetComponent<CartoonGenerator>();
        curveTester = GetComponent<CurveTester>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        foreach (Vector3 curvePoint in curveTester.Points)
        {
            curvePointsStart.Add(curvePoint - creator.position);
        }
        curve = curveTester.Build();
        UpdateMesh();
        StartCoroutine(LateStart());
    }


    IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("MATERIAL");
        
    }



    private void Update()
    {

        float incrementSize;
        incrementSize = 0.001f;
        for (int i = Mathf.FloorToInt(1 / incrementSize) - 1; i > -1; i--)
        {
            //Debug.Log(curve.GetPointAt(i * incrementSize).y + transform.position.y);
            if (curve.GetPointAt(i * incrementSize).y + transform.position.y >= meshRenderer.sharedMaterials[0].GetVector("_ClippingCentre").y)
            {
                progressVisible = i * incrementSize;
                break;
            }
        }
        if (startProgressVisible == 999)
        {
            startProgressVisible = progressVisible;
        }

            //Texture scaling and offsetting
        float cHeight = creator.position.y;
        float offset1 = 0; // Bigger one
        float offset2 = 0.3314f-1;
        float cHeightMax = 50;
            //float offsetGradient = 
            float texOffset = 1- progressVisible;// (cHeight * (offset2 - offset1) - (offset1 * cHeightMax)) / cHeightMax;
            //(cHeight * (offset2 - offset1)) / cHeightMax;
        meshRenderer.sharedMaterial.SetTextureOffset("_MainTex", new Vector2(0, texOffset));
        texScale = 1;// -1*(cHeight * (0.5f) - (1 * cHeightMax)) / cHeightMax;//(creator.position.y / 62.5f) - 1;
        meshRenderer.sharedMaterial.SetTextureScale("_MainTex", new Vector2(1, texScale));




        
        




        for (int i = 0; i < curveTester.Points.Count; i++)
        {
            Vector3 newPos = curvePointsStart[i] + creator.position;

            //if (i == curveTester.Points.Count - 1)
            //{
            //    if (curveTester.Points[i] != newPos)
            //    {
            //        curveTester.Points[i] = curvePointsStart[i] + creator.position;
            //    }
            //}
            //else
            //{
            //    curveTester.Points[i] = new Vector3(0, bendCurve.Evaluate(creator.position.y), curveTester.Points[i].z);
            //    //curveTester.Points[i] = curvePointsStart[i] + creator.position * (1 / ((curveTester.Points.Count - (float)i) * scaleFactor));
            //    //curveTester.Points[i] = new Vector3(0, Mathf.Clamp(curveTester.Points[i].y, -999, (curvePointsStart[i].y + creatorOrigin.y)), curveTester.Points[i].z);
            //}
            if (curveTester.Points[i] != newPos)
            {
                needToUpdate = true;
                curveTester.Points[i] = curvePointsStart[i] + creator.position;
            }
            
            
            
        }
        if (needToUpdate)
        {
            UpdateMesh();
            needToUpdate = false;
        }
        
    }





    void UpdateMesh()
    {
        meshFilter.sharedMesh = Tubular.Tubular.Build(curveTester.Build(), tubularSettings.segments, tubularSettings.radius, (int)tubularSettings.radialSegments, tubularSettings.closed);
         currentBasesnum = texScale * (cartoonGenerator.dnaSpline.phPerIteration - 1) * cartoonGenerator.dnaSpline.dnaSegments;
        if (cylinders.Count < currentBasesnum)
        {
            for (int i = 0; i < currentBasesnum - cylinders.Count-1; i++)
            {
                cylinders.Add(Instantiate(cylinderPrefab, this.transform));
            }
            
        }

        UpdateCylinders();


    }





    void UpdateCylinders()
    {
        Vector3 start = new Vector3();
        float length = .4f;
        float width = .1f;
        Vector3 up = Vector3.up;
        float cylOffset = (1f / (currentBasesnum * 2));
        for (int i = 0; i < cylinders.Count; i++)
        {
            //Debug.Log("Here " + ((float)i / (phPerIteration * dnaSegments)));
            float curveP = cylOffset + (float)i / (currentBasesnum) + (startProgressVisible-progressVisible);
            if (curveP > cylOffset + (float)i / (currentBasesnum))
            {
                curveP = cylOffset + (float)i / (currentBasesnum);
            }
            //Debug.Log(curve.GetPointAt(((float)i/((phPerIteration * dnaSegments)))) + " here too");
            up = Vector3.Cross(Vector3.left, curve.GetTangentAt(curveP) );
            cylinders[i].transform.up = up;

            start = curve.GetPointAt(curveP);
            up = Vector3.Cross(curve.GetTangentAt(cylOffset + (float)i / (currentBasesnum)), Vector3.right);
            var scale = new Vector3(width, length, width);
            var position = start + new Vector3(0, length, 0).magnitude*up;

           // var cylinder = Instantiate(cylinderPrefab, position, Quaternion.identity, this.transform);
            cylinders[i].transform.localPosition = (position);
            cylinders[i].transform.localScale = scale;
        }
        
    }

}





[System.Serializable]
public class TubularSettings
{
    public int segments;
    public float radius;
    public float radialSegments;
    public bool closed;
}
