using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
/// using Editor;
using MoleculeTypes;
using UnityEditor;
using UnityEngine;

public class PdbBuilder : MonoBehaviour
{
    public PrefabGenerator pfGenerator;
    public GameObject molPf;
    public List<TextAsset> pdbFiles;

    void Start()
    {
        // string[] pdbs = Directory.GetFiles("Assets/Data/Generated", "*PDB.txt", SearchOption.AllDirectories);
        // foreach(string matFile in pdbs)
        // {
        //     // string assetPath = "Assets" + matFile.Replace(Application.dataPath, "").Replace('\\', '/');
        //     pdbFiles.Add((TextAsset) AssetDatabase.LoadAssetAtPath(matFile, typeof(TextAsset)));
        // }
        GameObject parent = PrefabUtility.InstantiatePrefab(molPf) as GameObject;

        List<GameObject> molsObs = DrawPdbFile(ModelType.BallAndStick);
        foreach (GameObject molsOb in molsObs)
        {
            molsOb.GetComponent<MoleculeBehaviourManager>().recentreMolecule = false;
            molsOb.transform.SetParent(parent.transform);
            // molsOb.transform.Rotate(-90f, 0, 0);
        }
        
        // parent.transform.Rotate(new Vector3(-20.9f, 0.0f, -138.549f));

        // Molecule lastMol = molsObs[0].GetComponent<Molecule>();
        // // for (int i = 1; i < molsObs.Count; i++)
        // for (int i = 1; i < 2; i++)
        // {
        //     if (i % 2 == 0)
        //     {
        //         lastMol = molsObs[i].GetComponent<Molecule>();
        //     }
        //     else
        //     {
        //         Molecule thisMol = molsObs[i].GetComponent<Molecule>();
        //         
        //         Atom targetC3 = null;
        //         Atom targetO3 = null;
        //         Atom targetC2 = null;
        //         foreach (Atom atom in thisMol.atoms)
        //         {
        //             switch (atom.name)
        //             {
        //                 case "C3'":
        //                     targetC3 = atom;
        //                     continue;
        //                 case "O3'":
        //                     targetO3 = atom;
        //                     continue;
        //                 case "C2'":
        //                     targetC2 = atom;
        //                     continue;
        //             }
        //         }
        //         
        //         Atom origC3 = null;
        //         Atom origO3 = null;
        //         Atom origC2 = null;
        //         
        //         foreach (Atom atom in lastMol.atoms)
        //         {
        //             switch (atom.name)
        //             {
        //                 case "C3'":
        //                     origC3 = atom;
        //                     continue;
        //                 case "O3'":
        //                     origO3 = atom;
        //                     continue;
        //                 case "C2'":
        //                     origC2 = atom;
        //                     continue;
        //             }
        //         }
        //         
        //         // calc the lookrotation to make lastmol in the same frame as the prefab - C3' look at O3' with World.Up at clockwise + 44.676 degrees around z from C3'->C2'
        //         // Look first and then do calc in local space so the rotation is in the correct plane
        //         // then calc the tranlsation to opposite, and use Quarternion.LookRotation to work out the rotation needed for the same frame over there. THIS is the key
        //         Vector3 origcC3Pos = origC3.transform.position;
        //         Vector3 targetC3Pos = targetC3.transform.position;
        //         Vector3 translation = targetC3Pos - origcC3Pos;
        //         
        //         
        //         // Debug.Log("Original C3' = " + origcC3Pos);
        //         // Debug.Log("Target C3' = " + targetC3Pos);
        //         // Debug.Log("Difference = " + translation + " with mag: " + translation.magnitude);
        //         
        //         origC3.transform.LookAt(origO3.transform);
        //         // C3->O3 is z. Now we need to calc y
        //
        //         // here is the vector from c3 to c2 in local space for c3
        //         Vector3 c2PosRelative =
        //             origC3.transform.InverseTransformVector(origC2.transform.position - origC3.transform.position);
        //
        //         // Find the angle diff between that vector and up, but projected onto the z = 0 plane relative to C3
        //         float angle = Vector2.Angle(new Vector2(c2PosRelative.x, c2PosRelative.y), Vector2.up);
        //         // then how far we need to rotate around z so c2 is at -44.676 degrees
        //
        //         float neededRotation = 0.0f;
        //         if (i == 3 || i == 5)
        //         {
        //             neededRotation = -(angle - 44.676f);
        //         }
        //         else
        //         {
        //             neededRotation = angle + 44.676f;
        //         }
        //         
        //         origC3.transform.Rotate(0, 0, neededRotation - (2 * 44.676f), Space.Self);
        //         
        //         Vector3 c2PosRelative2 =
        //             origC3.transform.InverseTransformVector(origC2.transform.position - origC3.transform.position);
        //         float angle2 = Vector2.Angle(new Vector2(c2PosRelative2.x, c2PosRelative2.y), Vector2.up);
        //         // Debug.Log("Result angle diff = " + angle2);
        //
        //         //Store some values for later
        //         Quaternion origRotation = origC3.transform.rotation;
        //         Vector3 relTranslationNeeded = origC3.transform.InverseTransformVector(targetC3.transform.position - origC3.transform.position);
        //         
        //         // OK! Now we move to the target space
        //         origC3.transform.position = targetC3.transform.position;
        //
        //         // Look at target O3' first
        //         origC3.transform.LookAt(targetO3.transform);
        //         // Do the same calcs as above for how to rotate into position
        //         Vector3 c2PosRelative3 =
        //             origC3.transform.InverseTransformVector(targetC2.transform.position) - origC3.transform.InverseTransformVector(origC3.transform.position);
        //         float angle3 = Vector2.Angle(new Vector2(c2PosRelative3.x, c2PosRelative3.y), Vector2.up);
        //         float neededRotation2 = -(angle3 + 44.676f);
        //         origC3.transform.Rotate(0, 0, neededRotation2, Space.Self);
        //         
        //         // now check the angle is right
        //         Vector3 c2PosRelative4 =
        //             origC3.transform.InverseTransformVector(targetC2.transform.position - origC3.transform.position);
        //         float angle4 = Vector2.Angle(new Vector2(c2PosRelative4.x, c2PosRelative4.y), Vector2.up);
        //         // Debug.Log("Final Result angle diff = " + angle4);
        //         
        //         //IT'S RIGHT
        //         //SO, we now have the needed translation, and the needed rotation, to make our double strand
        //         Quaternion finalNeededRot = Quaternion.RotateTowards(origRotation, origC3.transform.rotation, 760f);
        //         // Debug.Log(finalNeededRot);
        //         Debug.Log("Rotation needed: " + finalNeededRot.x + " " + finalNeededRot.y + " " + finalNeededRot.z + " " + finalNeededRot.w);
        //         Debug.Log("Translation needed: " + relTranslationNeeded.x + " " + relTranslationNeeded.y + " " + relTranslationNeeded.z);
        //         Debug.Log("Translation distance: " + relTranslationNeeded.magnitude);
        //     }
        // }
    }

    private List<GameObject> DrawPdbFile(ModelType modelType)
    {
        List<GameObject> mols = new List<GameObject>();
        foreach (TextAsset pdbFile in pdbFiles)
        {
            GameObject mol = PrefabUtility.InstantiatePrefab(molPf) as GameObject;
            List<DataReader.AtomInfo> toMake = new List<DataReader.AtomInfo>();

            foreach (DataReader.AtomInfo info in DataReader.ReadCsvCoords(pdbFile, true).atoms)
            {
                // string[] desiredAtoms = { "C3'", "O3'", "C2'"};
                // string name = info.name;
                // if (desiredAtoms.Contains(name))
                // {
                toMake.Add(info);
                // }
            }
            List<Atom> atoms = PrefabGenerator.DrawAtoms(toMake, mol.GetComponent<Molecule>(), modelType);
            foreach (Atom atom in atoms)
            {
                // Vector3 localScale = atom.transform.localScale;
                // atom.transform.localScale = localScale * 20;

                Vector3 position = atom.transform.position;
                // atom.transform.position = position / 20f;
                atom.GetComponent<Collider>().enabled = false;
            }
            
            mols.Add(mol);
        }
        
        return mols;
    }
}
