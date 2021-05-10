using System;
using System.Collections;
using System.Collections.Generic;
using MoleculeTypes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MultitargetBoxDetectorManager : BoxDetectorManager
{
    public Cell cell;
    [FormerlySerializedAs("LabelAndPictures")] public MolDisplayData[] MoleculeDisplayData;
    public List<MolAndOrientationInBox1> otherPhosphates;
    public List<MolAndOrientationInBox1> otherSugars;
    
    private Dictionary<GameObject, Molecule> CurrentBoxMatches; // What boxes contain which subunits
    private HashSet<Type> molTypesAnswered;
    private HashSet<NBase.BaseType> basesAnswered;
    private HashSet<Molecule> displayMolecules;
    private HashSet<Molecule> molsAnswered;

    private bool ignoreAllCollisions;
    private MultitargetBoxDetector phosphateBox;
    private MultitargetBoxDetector sugarBox;

    public ConnectedMolBehaviourManager connectedMolMan;
    

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        CurrentBoxMatches = new Dictionary<GameObject, Molecule>();
        molTypesAnswered = new HashSet<Type>();
        basesAnswered = new HashSet<NBase.BaseType>();
        displayMolecules = new HashSet<Molecule>();
        molsAnswered = new HashSet<Molecule>();
        
        foreach (MolDisplayData molDisplayData in MoleculeDisplayData)
        {
            displayMolecules.Add(molDisplayData.data.mol);
        }
    }

    // Called when a collider hits the BoxDetector.
    public override void PossibleTargetCollided(GameObject box, GameObject possibleTarget, Light pointLight)
    {
        if (ignoreAllCollisions) return;
        
        Molecule mol = possibleTarget.GetComponentInParent<Molecule>();

        if (mol) 
        {
            //Check if the molecule is being held. If so, release it
            OVRGrabber hand = mol.firstHand;
            if (mol.isBeingHeld) 
            {
                hand.ForceRelease(mol.firstAtomHeld.GetComponent<CustomGrabbable>());
                if (mol.secondHand)
                {
                    mol.secondHand.ForceRelease(mol.secondAtomHeld.GetComponent<CustomGrabbable>());
                }
            }

            // If the molecule is still part of a chain, buzz but don't reposition
            if (!mol.IsDisconnected())
            {
                if (hand) Player.Vibrate(0.2f, 1.0f, hand.GetComponent<CustomGrabber>(), true);
                if (incorrectAudio) incorrectAudio.Play();
                StartCoroutine(ColourLight(pointLight, Color.red, 0.5f));
                Debug.Log($"BoxAnsweredWithConnectMol\t{box.name}\t{mol.name}\t{Time.timeSinceLevelLoad}");
                return;
            }
            
            // If the box doesn't yet have a match and the mol's type doesn't yet have a box, answer it and display mol
            if (!boxesAnswered.Contains(box) && !MolHasBeenAnswered(mol) && MolTypeMatchesBoxType(mol, box))
            {
                //Vibrate the hand, colour the light, and play a sound
                if (hand) Player.Vibrate(1.2f, 0.3f, hand.GetComponent<CustomGrabber>(), true);
                StartCoroutine(ColourLight(pointLight, Color.green, 1.5f));
                if (correctAudio) correctAudio.Play();
                Debug.Log($"BoxAnsweredCorrectly\t{box.name}\t{mol.name}\t{Time.timeSinceLevelLoad}");

                // Get the box to display the molecule and deactivate the rest of them in scene
                box.GetComponent<MultitargetBoxDetector>().DisplayMolecule(GetMoleculeDisplayData(mol.name));
                // cell.DeactivateSceneMolecules(mol);

                CurrentBoxMatches[box] = mol;
                if (mol.GetType() == typeof(Sugar)) sugarBox = box.GetComponent<MultitargetBoxDetector>();
                else if (mol.GetType() == typeof(Phosphate)) phosphateBox = box.GetComponent<MultitargetBoxDetector>();
                
                StartCoroutine(DeactivateMolNextFrame(mol.gameObject));

                // Record the answer and progress the scene if the exercise is finished
                boxesAnswered.Add(box);
                molsAnswered.Add(mol);
                molTypesAnswered.Add(mol.GetType());
                if (mol.GetType() == typeof(NBase)) basesAnswered.Add(((NBase) mol).baseType);
                if (boxesAnswered.Count == displayMolecules.Count)
                {
                    Debug.Log($"AllBoxesAnsweredCorrectly\t{name}\t{Time.timeSinceLevelLoad}");
                    toContinue.Resume();
                    cellAudioManager.PlayNextNarrativeClip();
                }
            }
            else if (displayMolecules.Contains(mol) || molsAnswered.Contains(mol))
            {
                // If it's a collision of the display mol, do nothing
            }
            // Else this box already has an occupant, or this mol lives in a different box already
            else
            {
                mol.transform.position =
                    mol.transform.position + transform.TransformDirection(new Vector3(0, 0, -0.14f));
                if (hand) Player.Vibrate(0.2f, 1.0f, hand.GetComponent<CustomGrabber>(), true);
                StartCoroutine(ColourLight(pointLight, Color.red, 0.5f));
                if (incorrectAudio) incorrectAudio.Play();
                Debug.Log($"BoxAnsweredIncorrectly\t{box.name}\t{mol.name}\t{Time.timeSinceLevelLoad}");
            }
            
        } 
        // The ruler and bat get sent back to their starting position
        else if (possibleTarget.GetComponent<Ruler>())
        {
            possibleTarget.GetComponent<Ruler>().ResetPosition();
        } 
        else if (possibleTarget.GetComponentInChildren<Bat>())
        {
            possibleTarget.GetComponentInChildren<Bat>().ResetToStart();
        }
    }

    private bool MolTypeMatchesBoxType(Molecule mol, GameObject box)
    {
        int boxNumber = (int) char.GetNumericValue(box.name[3]);

        if (boxNumber == 1 || boxNumber == 4)
        {
            // Backbone box. Only Allow phosphates or Sugars
            return mol.GetType() == typeof(Phosphate) || mol.GetType() == typeof(Sugar);
        }
        // NBase boxes
        return mol.GetType() == typeof(NBase);
    }

    private IEnumerator DeactivateMolNextFrame(GameObject molGameObject)
    {
        yield return new WaitForEndOfFrame();
        // yield return new WaitForEndOfFrame();
        molGameObject.SetActive(false);
    }

    private bool MolHasBeenAnswered(Molecule mol)
    {
        if (mol.GetType() == typeof(NBase))
            return molTypesAnswered.Contains(mol.GetType()) && basesAnswered.Contains(((NBase) mol).baseType);
        return molTypesAnswered.Contains(mol.GetType());
    }

    private MolDisplayData GetMoleculeDisplayData(string molName)
    {
        if (molName.Contains("phosphate")) return MoleculeDisplayData[0];
        if (molName.Contains("sugar")) return MoleculeDisplayData[1];
        if (molName.Contains("adenine")) return MoleculeDisplayData[2];
        if (molName.Contains("thymine")) return MoleculeDisplayData[3];
        if (molName.Contains("guanine")) return MoleculeDisplayData[4];
        if (molName.Contains("cytosine")) return MoleculeDisplayData[5];
        return MoleculeDisplayData[0];
    }
    
    [Serializable]
    public class MolDisplayData
    {
        public string label;
        public Sprite image;
        public MolAndOrientationInBox1 data;
    }

    [Serializable]
    public class MolAndOrientationInBox1
    {
        public Molecule mol;
        public Vector3 pos;
        public Vector3 rot;
    }

    public void GiveMoleculesBack()
    {
        ignoreAllCollisions = true;
        
        // Turn display mols back on. 
        foreach (Molecule molecule in displayMolecules)
        {
            foreach (Atom molAtom in molecule.atoms)
            {
                molAtom.rigid.isKinematic = false;
                molAtom.GetComponent<OVRGrabbable>().enabled = true;
                molAtom.rigid.velocity = Vector3.zero;
            }
        }
    }

    public void DisplayNextBackboneMols() // todo play a sound
    {
        StopBondsBreakingInCurrentChain();
        phosphateBox.DisplayMolecule(otherPhosphates[0]);
        sugarBox.DisplayMolecule(otherSugars[0]);
        otherPhosphates.RemoveAt(0);
        otherSugars.RemoveAt(0);
    }

    public void StopBondsBreakingInCurrentChain()
    {
        foreach (Molecule mol in MoleculeDisplayData[0].data.mol.connectedMols)
        {
            foreach (Bond bond in mol.bonds)
            {
                if (bond.fromAtom.bcd) // Detects if it's a breakable bond - it has a reformer detector
                {
                    bond.bondJoint.breakForce = float.PositiveInfinity;
                    connectedMolMan.AddBondToWatch(bond); // This one watches for high bond forces and releases from the hand
                }
            }
        }
    }
}
