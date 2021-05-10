using System.Collections.Generic;
using System.IO;
using MoleculeTypes;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
 
public static class DebugMenu
{
    [MenuItem("Debug/Print Global Position")]
    public static void PrintGlobalPosition()
    {
        if (Selection.activeGameObject != null)
        {
            Debug.Log(Selection.activeGameObject.name + " is at " + Selection.activeGameObject.transform.position.ToString("F5"));
        }
    }
    
    [MenuItem("Debug/Reset All Atoms to start position")]
    public static void ResetAllAtoms()
    {
        Atom[] atoms = GameObject.FindObjectsOfType<Atom>();
        foreach (Atom atom in atoms)
        {
            atom.ResetToStart();
        }
    }
    
    [MenuItem("Debug/Write mol coords to file")]
    // TODO this stops working when I deparent all the mols
    public static void WriteMolCoordsToFile()
    {
        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Molecule>() != null)
        {
            Molecule initialMol = Selection.activeGameObject.GetComponent<Molecule>();
            Molecule finalParent = FindHighestLevelMol(initialMol);

            Molecule[] allMols = finalParent.GetComponentsInChildren<Molecule>();

            foreach (Molecule molecule in allMols)
            {
                string path = "Assets/Data/TestCoords/" + molecule.name + ".txt";

                List<string> bondLines = new List<string>();
                bool inBonds = false;
                foreach (string line in File.ReadLines(path))
                {
                    if (line.StartsWith("# Format:")) inBonds = true;
                    if (inBonds) bondLines.Add(line);
                }
                
                List<string> lines = new List<string>();
                int index = 0;
                foreach (Atom atom in molecule.atoms)
                {
                    Vector3 position = atom.transform.position;
                    lines.Add(index + " " + atom.name + " " + position.x.ToString("F5") + " " + position.y.ToString("F5") + " " + position.z.ToString("F5"));
                    index++;
                }
                
                List<string> allLines = new List<string>(lines);
                allLines.Add("");
                allLines.AddRange(bondLines);

                File.WriteAllLines(path, allLines);
            }
            // Debug.Log(Selection.activeGameObject.name + " is at " + Selection.activeGameObject.transform.position.ToString("F5"));
        }
    }

    private static Molecule FindHighestLevelMol(Molecule initialMol)
    {
        if (initialMol.transform.parent.GetComponent<Molecule>() == null)
        {
            return initialMol;
        }

        return FindHighestLevelMol(initialMol.transform.parent.GetComponent<Molecule>());
    }
}