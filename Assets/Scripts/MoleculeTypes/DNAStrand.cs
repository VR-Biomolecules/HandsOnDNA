using System;
using System.Collections;
using System.Collections.Generic;
using MoleculeTypes;
using UnityEngine;

public class DNAStrand : MonoBehaviour
{
    // The backbone units and Bases list must keep the same indices.
    // Phosphates[0] connects at the 5' end of Sugars[0], which is attached to Bases[0]
    public List<Phosphate> Phosphates = new List<Phosphate>();
    public List<Sugar> Sugars = new List<Sugar>();
    public List<NBase> Bases = new List<NBase>();

    public List<Bond> gBonds = new List<Bond>(); 
    public List<Bond> ppdeBonds = new List<Bond>();

    public bool isInGhostmode;
    
    // true if this strand is complementary, meaning the lists above are 3' -> 5'. Only needed for old model - new model has both strands listed 5' -> 3'
    public bool Complementary { get; set; }

    public List<Molecule> GetAllMolecules()
    {
        List<Molecule> toReturn = new List<Molecule>();
        toReturn.AddRange(Phosphates);
        toReturn.AddRange(Sugars);
        toReturn.AddRange(Bases);
        return toReturn;
    }

    public void SetGhostMode(bool fade)
    {
        isInGhostmode = true;

        foreach (Molecule molecule in GetAllMolecules())
        {
            if (molecule.gameObject.activeSelf) molecule.SetGhostMode(fade);
        }

        // // Foreach atom in this strand
        // foreach (Atom a in GetComponentsInChildren<Atom>())
        // {
        //     // Switch all atom's materials to their faded material equivalent
        //     foreach (Renderer r in a.renderers)
        //     {
        //         r.material = a.materialFade;
        //     }
        //     
        //     // Disable colliders and interactability
        //     a.GetComponent<SphereCollider>().enabled = false;
        //     if (a.GetComponent<BondCollisionDetector>()) 
        //     {
        //         a.GetComponent<BondCollisionDetector>().enabled = false;
        //     }
        //     a.rigid.isKinematic = true;
        //
        //     a.GetComponent<CustomGrabbable>().enabled = false;
        // }
        //
        // // Foreach bond in this strand
        // foreach(Bond b in GetComponentsInChildren<Bond>())
        // {
        //     // Switch all bond's materials to their faded material equivilant
        //     foreach (Renderer r in b.renderers)
        //     {
        //         r.material = b.materialFade;
        //     }
        // }
        //
        // //todo disable unecessary joints and moleculebehaviourmanager functionality
        // foreach(MoleculeBehaviourManager mbm in GetComponentsInChildren<MoleculeBehaviourManager>())
        // {
        //     mbm.enabled = false;
        // }
    }

    private void Start()
    {
        if (Sugars.Count > 0) Sugars[0].UpdateConnectedMolStatus();
        // isInGhostmode = false;
    }

    public IEnumerator ResetAtomsAndBonds()
    {
        foreach (Molecule mol in GetAllMolecules())
        {
            mol.gameObject.SetActive(true);
        }
        
        // Find all breakable bonds in breakable strand
        List<Bond> breakableBonds = new List<Bond>(ppdeBonds);
        breakableBonds.AddRange(gBonds);

        // Cahced list of all atoms
        Atom[] allAtoms = GetComponentsInChildren<Atom>();

        // Foreach breakable bond in breakable strand
        foreach (Bond b in breakableBonds)
        {
            // Forceably break
            b.fromAtom.OnJointBreak(b.bondJoint.breakForce);
            Destroy(b.bondJoint);
        }

        // Teleport atoms to correct position 
        // ToDo 
        // Don't move atoms, move molecule. Unparent atoms from a molecule, and set the molecule's position & rotation to atoms[0]. 
        // Reparent atoms back to molecule, and then lerp position & rotation of the molecule back to Vector3.zero / Quaternion.identity.
        foreach(Atom a in allAtoms)
        {
            a.ResetToStart();
            a.rigid.isKinematic = true;
        }

        //yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        yield return new WaitForSeconds(0.1f);

        // Reform all original bonds
        foreach (BondReformer br in GetComponentsInChildren<BondReformer>(true))
        {
            br.ResetToOriginal(true);
        }

        if (isInGhostmode)
        {
            List<Bond> breakableBonds2 = new List<Bond>(ppdeBonds);
            breakableBonds2.AddRange(gBonds);
            foreach (Bond bond in breakableBonds2)
            {
                foreach (Renderer r in bond.renderers)
                {
                    r.material = bond.materialFade;
                }
            }
        }
        
        // Update Connected Molecules Status
        Sugars[0].UpdateConnectedMolStatus();

        yield return new WaitForSeconds(0.1f);

        // Teleport atoms to correct position
        foreach (Atom a in allAtoms)
        {
            a.rigid.isKinematic = false;
        }
    }
}
