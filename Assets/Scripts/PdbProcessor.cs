using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class PdbProcessor : MonoBehaviour
{
    public List<TextAsset> pdbFiles;
    
    Vector3 rotationToApply = new Vector3(25.46f, -42.73f, 170f); // There's a second rotation, applied after this one, but i couldn't combine them for some reason
    Vector3 transToApply = new Vector3(7.460f, 17.280f, 9.300f); // pos of lowest C3'
    
    
    // Start is called before the first frame update
    void Start()
    {
        ConvertPdbFiles();
    }

    private void ConvertPdbFiles()
    {
        int currentBase = 0;
        int baseCount = 0;
        int currentPhosAtom = 0;
        int currentSugarAtom = 0;
        int currentBaseAtom = 0;

        string nextO3pdb = null;
        string nextO3text = null;
        
        StreamWriter phosPdb = null;
        StreamWriter sugarPdb = null;
        StreamWriter basePdb = null;
        StreamWriter phosWriter = null;
        StreamWriter sugarWriter = null;
        StreamWriter baseWriter = null;
        foreach (var pdbFile in pdbFiles)
        {
            string[] lines = Regex.Split(pdbFile.text, "\n|\r|\r\n");
            foreach (string line in lines)
            {
                string[] elements = Regex.Split(line, "[ ]+");
                if (!elements[0].Equals("ATOM")) continue;

                int thisBase = Int32.Parse(elements[4]);
                if (thisBase != currentBase)
                {
                    currentBase = thisBase;
                    
                    if (phosPdb != null)
                    {
                        phosPdb.Close();
                        sugarPdb.Close();
                        basePdb.Close();
                        phosWriter.Close();
                        sugarWriter.Close();
                        baseWriter.Close();
                    }

                    string baseName = baseCount + "_" + NBase.GetBaseName(NBase.DetectBase(elements[3].Substring(1)));

                    // string guid = AssetDatabase.CreateFolder("Assets/Data/Generated", baseName);
                    string folderPath = "Assets/Data/Generated/";

                    phosPdb = new StreamWriter(File.Open(folderPath + baseName + "_phosphatePDB.txt", FileMode.OpenOrCreate,
                        FileAccess.ReadWrite));
                    sugarPdb = new StreamWriter(File.Open(folderPath + baseName + "_sugarPDB.txt", FileMode.OpenOrCreate,
                        FileAccess.ReadWrite));
                    basePdb = new StreamWriter(File.Open(folderPath + baseName + "_basePDB.txt", FileMode.OpenOrCreate,
                        FileAccess.ReadWrite));
                    phosWriter = new StreamWriter(File.Open(folderPath + baseName + "_phosphate.txt", FileMode.OpenOrCreate,
                        FileAccess.ReadWrite));
                    sugarWriter = new StreamWriter(File.Open(folderPath + baseName + "_sugar.txt", FileMode.OpenOrCreate,
                        FileAccess.ReadWrite));
                    baseWriter = new StreamWriter(File.Open(folderPath + baseName + "_base.txt", FileMode.OpenOrCreate,
                        FileAccess.ReadWrite));

                    currentPhosAtom = 0;
                    currentSugarAtom = 0;
                    currentBaseAtom = 0;
                    baseCount++;
                }
                //ATOM     29  P    DG     2      22.410  31.290  21.480  1.00  0.00  
                //1   N9  -0.02124    0.15650 -0.09359

                Vector3 position =
                    new Vector3(float.Parse(elements[5]), float.Parse(elements[6]), float.Parse(elements[7]));
                position -= transToApply;
                position /= 20f;
                position = Quaternion.Euler(rotationToApply) * position;
                position = Quaternion.Euler(new Vector3(-20.9f, 0.0f, -138.549f)) * position;

                string name = elements[2];

                string pdbLine = elements[0] + " " + elements[1] + " " + elements[2] + " " + elements[3] + " " +
                                 elements[4] + " " + position.x + " " + position.y + " " + position.z + " " +
                                 elements[8] + " " + elements[9];

                string myLinePreAtom = " " + elements[2] + " " + position.x + " " + position.y + " " + position.z;

                if (name.Contains("P"))
                {
                    phosPdb.WriteLine(pdbLine);
                    phosWriter.WriteLine(currentPhosAtom + myLinePreAtom);
                    currentPhosAtom++;
                } 
                else if (name.Equals("O5'"))
                {
                    phosPdb.WriteLine(pdbLine);
                    phosWriter.WriteLine(currentPhosAtom + myLinePreAtom);
                    currentPhosAtom++;
                    
                    phosPdb.WriteLine(nextO3pdb);
                    phosWriter.WriteLine(nextO3text);
                }
                else if (name.Equals("O3'"))
                {
                    nextO3pdb = pdbLine;
                    nextO3text = "4" + myLinePreAtom;
                }
                else if (name.Contains("'"))
                {
                    sugarPdb.WriteLine(pdbLine);
                    sugarWriter.WriteLine(currentSugarAtom + myLinePreAtom);
                    currentSugarAtom++;
                }
                else
                {
                    basePdb.WriteLine(pdbLine);
                    baseWriter.WriteLine(currentBaseAtom + myLinePreAtom);
                    currentBaseAtom++;
                }
            }
        }
        
        phosPdb.Close();
        sugarPdb.Close();
        basePdb.Close();
        phosWriter.Close();
        sugarWriter.Close();
        baseWriter.Close();
    }

    static void WriteString()
    {
        string path = "Assets/Resources/test.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine("Test");
        writer.Close();

        //Re-import the file to update the reference in the editor
        //AssetDatabase.ImportAsset(path); 
        // TextAsset asset = Resources.Load("test");

        //Print the text from the file
        // Debug.Log(asset.text);
    }

    static void ReadString()
    {
        string path = "Assets/Resources/test.txt";

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path); 
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }
}
