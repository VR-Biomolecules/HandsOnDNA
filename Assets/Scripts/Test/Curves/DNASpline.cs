using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tubular;
using Curve;
using System.IO;
using System.Globalization;

public class DNASpline : MonoBehaviour
{

    public DNASpline secondSpline;
    public int tubularSegments = 20;
    public float radius = 0.1f;
    public float radialSegments = 6;
    public bool closed = false;
    public DataReader dataReader;
    public List<Vector3> points;
    CurveTester curveTester;
    public List<Vector3> iterativePoints;
    public CartoonGenerator cartoonGenerator;
    public int dnaSegments;
    public string pathToPoints;
    // Start is called before the first frame update
    string output;

    public bool printXYZ;
    public bool secondStrand;
    public int phPerIteration = 10;
    public List<Vector3> cylinderPositions;

    public GameObject cylinderPrefab;
    public Vector3 firstPos;
    //Step for each segment
    Vector3 offset;
    public Vector3[] cylinderPos;
    void CreateCylinderBetweenPoints(Vector3 start, Vector3 end, float width, int posInDNA)
    {
        var offsetCyl = end - start;
        var scale = new Vector3(width, offsetCyl.magnitude / (cartoonGenerator.doubleStranded ? 2.0f : 4.0f), width);
        var position = start + (offsetCyl / (cartoonGenerator.doubleStranded ? 2.0f : 4.0f));

        var cylinder = Instantiate(cylinderPrefab, position, Quaternion.identity, this.transform);
        //cylinder.transform.localPosition = position;
        cylinder.transform.up = offsetCyl;
        cylinder.transform.localScale = scale;
        cylinder.GetComponent<MeshRenderer>().materials[1].color = cartoonGenerator.baseColors[cartoonGenerator.bases[posInDNA%12]-1];
        cylinder.GetComponent<MeshRenderer>().materials[0].color = cartoonGenerator.baseColors[(cartoonGenerator.bases[posInDNA % 12] -1 + 2) % 4];
    }
    void PrintPoints()
    {
        string path = "Assets/Resources/test.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, false);
        output = "";
        for (int i = 0; i < (phPerIteration-1); i++) {
            writer.WriteLine(curveTester.Points[i].ToString());
        }
        writer.WriteLine("Done");   
        writer.Close();

    }

    List<Vector3> ReadPoints(string path)
    {
        string line;
        StreamReader reader = new StreamReader(path);
        List<Vector3> result = new List<Vector3>();
        while (!(line = reader.ReadLine()).Equals("Done"))
        {
            line = line.Substring(1,line.Length - 2);
            string[] values = line.Split(',');
            //Debug.Log(values[0] + " " + values[1] + " " + values[2]);
            Vector3 currentPoint = new Vector3(float.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat), float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat));
            
            result.Add(currentPoint);
           
        }
        return result;
    }

    IEnumerator Start()
    {
        tubularSegments *= dnaSegments;
        //GetComponent<MeshRenderer>().sharedMaterial.SetTextureScale("_MainTex", new Vector2(1, 20 * dnaSegments));
        curveTester = GetComponent<CurveTester>();
        points = ReadPoints(pathToPoints);//dataReader.GetPhosphorusCoordsFromPDB();////
        
        


        for (int j = 0; j < points.Count; j++)
        {
            if (!secondStrand)
            {
                if (j > points.Count - phPerIteration)
                {
                    iterativePoints.Add(points[j]);
                }
                
            }
            else
            {

                if (j < phPerIteration - 1)
                {
                    iterativePoints.Add(points[j]);
                    //Debug.Log(firstSpline.iterativePoints[0]);
                }
            }
        }
        if (!secondStrand)
        {
            secondSpline.firstPos = iterativePoints[0];
            firstPos = iterativePoints[0];
        }
        //Debug.Log(firstPos);
        for (int i = 0; i < dnaSegments; i++)
        {
            //34.529
            offset = (new Vector3(0, 0, 38.8f * i)) ; //new Vector3(0, 0, 25.55f * i);
            //
            for (int j = 0; j < iterativePoints.Count; j++)
            {


                //Debug.Log(firstSpline.iterativePoints.ToArray());
                curveTester.Points.Add(iterativePoints[j] + offset - firstPos );
                
            }
        }

        //Debug.Log(points[0] + " " + points[1]);
        Curve.Curve curve = curveTester.Build();
        GetComponent<MeshFilter>().sharedMesh = Tubular.Tubular.Build(curve, tubularSegments, radius, (int)radialSegments, closed);
        float cylOffset = (1f/((phPerIteration-1)*dnaSegments*2)); 
        for (int i = 0; i < ((phPerIteration - Mathf.Ceil(cylOffset)) *dnaSegments) ; i++)
        {
            //Debug.Log("Here " + ((float)i / (phPerIteration * dnaSegments)));

            //Debug.Log(curve.GetPointAt(((float)i/((phPerIteration * dnaSegments)))) + " here too");
            cylinderPositions.Add(curve.GetPointAt(cylOffset + (float)i/ ((phPerIteration-1) * dnaSegments)));

        }
        //cylinderPositions.Add(curve.GetPointAt(0.5f));
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < cylinderPositions.Count; i++)
        {

            if (!secondStrand)
            {
                Debug.Log("Cylinder point: " + cylinderPositions[i] + ", " + secondSpline.cylinderPositions[i]);
                CreateCylinderBetweenPoints(cylinderPositions[i], (secondSpline.cylinderPositions[i]) + secondSpline.gameObject.transform.position, 0.8f, i);
            }
        }


    }
    private void Update()
    {
        if (printXYZ)
        {
            GetComponent<MeshFilter>().sharedMesh = Tubular.Tubular.Build(curveTester.Build(), tubularSegments, radius, (int)radialSegments, closed);
        }
        if (printXYZ && Input.GetKeyDown(KeyCode.S)) {
            PrintPoints();
            Debug.Log("(what a) SAVE");
            
        }
    }
}
