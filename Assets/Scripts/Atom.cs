using System;
using System.Collections;
using System.Collections.Generic;
using MoleculeTypes;
using UnityEngine;

public class Atom : MonoBehaviour
{
    public string label;
    public AtomType type;
    public Molecule parentMol;

    public Renderer[] renderers;
    public Material material;
    public Material materialFade;

    public List<Bond> bondsOnThisAtom;

    public Vector3 startPos;
    public Quaternion startRot;

    // Last position and stable position (to roll back to on explosion)
    public Vector3 lastPosition;
    public Quaternion lastRotaiton;

    internal Vector3 lastStablePosition;
    internal Quaternion lastStableRotation;

    public Rigidbody rigid;
    public BondCollisionDetector bcd;

    private void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        RecordLastPosAndRot();
        RecordLastStableState();
    }

    public void ResetToStart()
    {
        transform.position = startPos;
        transform.rotation = startRot;
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        RecordLastPosAndRot();
        RecordLastStableState();
    }

    public void RecordLastPosAndRot()
    {
        lastPosition = transform.position;
        lastRotaiton = transform.rotation;
    }

    public void RecordLastStableState()
    {
        lastStablePosition = transform.position;
        lastStableRotation = transform.rotation;
    }

    /**
     * When a joint breaks, this method is called on the jointed object.
     * We can use it to detect bonds breaking.
     */
    public void OnJointBreak(float breakForce)
    {
        // Debug.Log("JOINT BROKEN on atom " + label);

        Bond brokenBond = null;
        foreach (Bond bond in bondsOnThisAtom)
        {
            if (bond != null && Math.Abs(bond.bondJoint.breakForce) - Math.Abs(breakForce) < 0.0001) // find the joint that just broke
            {
                CleanUpJointBreak(bond);
                break;
            }
        }
    }

    public void CleanUpJointBreak(Bond bond)
    {
        // All the breakable bonds are tracked by the sugar
        Sugar sugar = parentMol.GetComponent<Sugar>();
        if (sugar != null)
        {
            bond.OnBondBreak();
            sugar.OnBondBreak(bond, this);

            bcd.enabled = true;
            bond.toAtom.bcd.enabled = true;
        }
        else
        {
            Debug.Log("Had a bond break on an atom that is NOT in a sugar! Atom: " + label);
        }

        // Notify the molecules a bond has broken, so they update connected status and vibrate hands
        Molecule mol1 = bond.fromAtom.parentMol;
        Molecule mol2 = bond.toAtom.parentMol;
        mol1.BondBrokenInMolecule(bond.fromAtom, bond.toAtom, true);
        mol2.BondBrokenInMolecule(bond.fromAtom, bond.toAtom, false);
    }

    // internal void BondableAtomClose(Collider other)
    // {
    //     transform.SetParent(null); 
    //     other.gameObject.transform.SetParent(null);
    //     //should only set parents to null if parent is a hand: Player.instance.hands
    //     StartCoroutine(AlignMoleculeAndBond(other.gameObject.GetComponent<Atom>()));
    // }
    //
    // private IEnumerator AlignMoleculeAndBond(Atom otherAtom)
    // {
    //     float moveStep = 0.5f * Time.deltaTime; // try to go for 0.5 seconds to reach target
    //     float rotStep = 180f * Time.deltaTime; // try to go for 180 degrees rotation per sec
    //     bool inPosition = false;
    //     bool inRotation = false;
    //
    //     while (!(inPosition && inRotation))
    //     {
    //         Transform target = otherAtom.parentMol.transform;
    //
    //         if (!inPosition)
    //         {
    //             Vector3.MoveTowards(parentMol.transform.position, target.position, moveStep);
    //             if (Math.Abs(Vector3.Distance(parentMol.transform.position, target.position)) < Mathf.Epsilon) inPosition = true;
    //         }
    //
    //         if (!inRotation)
    //         {
    //             Quaternion.RotateTowards(parentMol.transform.rotation, target.rotation, rotStep);
    //             if (Math.Abs(Quaternion.Dot(parentMol.transform.rotation, target.rotation)) < Mathf.Epsilon) inRotation = true;
    //         }
    //         yield return null;
    //     }
    //     
    //     PrefabGenerator pfGen = (PrefabGenerator) FindObjectOfType(typeof(PrefabGenerator)); //store a ref so this is much faster
    //     pfGen.DrawBond(this, otherAtom, parentMol, true, parentMol.ModelType);
    //     GetComponentInChildren<BondCollisionDetector>().enabled = false;
    // }

    internal static AtomType DetectAtom(string text)
    {
        if (text.Contains("C")) return AtomType.Carbon;
        if (text.Contains("H")) return AtomType.Hydrogen;
        if (text.Contains("N")) return AtomType.Nitrogen;
        if (text.Contains("O")) return AtomType.Oxygen;
        if (text.Contains("S")) return AtomType.Sulfur;
        if (text.Contains("P")) return AtomType.Phosphorus;
        return AtomType.Unknown;
    }
    
    [Serializable]
    public enum AtomType
    {
        Carbon,
        Hydrogen,
        Nitrogen,
        Oxygen,
        Sulfur,
        Phosphorus,
        Unknown
    }
}

