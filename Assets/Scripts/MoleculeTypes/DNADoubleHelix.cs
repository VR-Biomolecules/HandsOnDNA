using System.Collections;
using System.Collections.Generic;
using MoleculeTypes;
using UnityEngine;

public class DNADoubleHelix : MonoBehaviour
{

    public bool spawnRigid;
    public bool spining;
    public bool interactive = true;
    public float spinningMultiplier;

    public DNAStrand strandA; 
    public DNAStrand strandB; 
    
    // Hook to the hands
    public CustomGrabber leftHand;
    public CustomGrabber rightHand;

    // The atoms being held by the hands this frame
    public Atom heldAtomLeft;
    public Atom heldAtomRight;

    bool isRigid = false;
    public enum colorMode { atom, molecule, nBase};
    public colorMode currentColorMode;

    public Material[] moleculeMats;
    public Material[] baseMats;

    public bool isCartoon;

    Vector3 initialPos;
    public Quaternion initialRot;
    public GameObject combinedModel;

    public AudioSource bondBreak;
    public AudioSource bondMake;

    public BondWatcher watcher;

    private Molecule[] allContainedMols;

    public Molecule[] GetAllContainedMolecules()
    {
        return allContainedMols;
    }
    
    public void Start()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;
        leftHand = Player.hands[0];
        rightHand = Player.hands[1];
        if (spawnRigid)
        {
            EnableModelRigid();
        }
        if (!isCartoon)
        {
            //Debug.Log(gameObject.GetComponentsInChildren<Atom>().Length);
            //Debug.Log(gameObject.GetComponentsInChildren<Bond>().Length);
            ColorByMode();
        }

        // Give the molecules the audio source to play on bond popping
        foreach (Molecule molecule in GetComponentsInChildren<Molecule>(true))
        {
            molecule.bondPop = bondBreak;
            molecule.watcher = watcher;
        }
        
        // And give all the reformers the audio source to play on remaking
        foreach (BondReformer bf in GetComponentsInChildren<BondReformer>(true))
        {
            bf.onBondReform = bondMake;
            //todo also deactivate them until ready
        }

        allContainedMols = GetComponentsInChildren<Molecule>();
        AdjustBondForces();
    }

    /*
     * Think about what these should be
     */
    private void AdjustBondForces()
    {
        if (strandA && !strandA.isInGhostmode)
        {
            foreach (Bond bond in strandA.ppdeBonds)
            {
                bond.bondJoint.breakForce = 1500;
            }
            foreach (Bond bond in strandA.gBonds)
            {
                bond.bondJoint.breakForce = 1500;
            }
        }
        if (strandB && !strandB.isInGhostmode)
        {
            foreach (Bond bond in strandB.ppdeBonds)
            {
                bond.bondJoint.breakForce = 1500;
            }
            foreach (Bond bond in strandB.gBonds)
            {
                bond.bondJoint.breakForce = 1500;
            }
        }
    }

    private void FixedUpdate()
    {
        if (spining)
        {
            transform.RotateAround(GetComponent<CapsuleCollider>().center + initialPos, Vector3.up, spinningMultiplier); //todo spir around its own centre if moved?
            //transform.localEulerAngles += new Vector3(0, spinningMultiplier, 0);
        }
        if (!isRigid)
        {
            CheckHandsForAtoms();
        }
        else
        {
            if (interactive)
            {
                GetComponent<OVRGrabbable>().enabled = true;
            }
            else
            {
                GetComponent<OVRGrabbable>().enabled = false;
            }
        }
    }

    public void ResetDNA()
    {
        // Debug.Log("RESET DNA");
        BondRemaker.ReConnectStrandA(strandA.Phosphates, strandA.Sugars, strandA.Bases, ModelType.BallAndStick, true);
        BondRemaker.ReConnectStrandB(strandB.Phosphates, strandB.Sugars, strandB.Bases, ModelType.BallAndStick, true);
    }

    public void EnableModelRigid()
    {
        isRigid = true;
        if (combinedModel != null)
        {
            Destroy(strandA.gameObject);
            Destroy(strandB.gameObject);
            Instantiate(combinedModel, transform);
        }
        if (!isCartoon)
        {
            Rigidbody rigid = gameObject.AddComponent<Rigidbody>();
            rigid.useGravity = false;
            rigid.isKinematic = true;
            rigid.drag = 1000;
            rigid.angularDrag = 1000;
            CapsuleCollider capsuleCol = gameObject.AddComponent<CapsuleCollider>();
            capsuleCol.center = new Vector3(0.09000003f, 0.9573901f, -0.39f);
            capsuleCol.radius = 0.65f;
            capsuleCol.height = 2.328607f;
            capsuleCol.direction = 1;
            CustomGrabbable customGrab = gameObject.AddComponent<CustomGrabbable>();
            customGrab.enabled = interactive;
            customGrab.CustomGrabCollider(capsuleCol);
        }
        
        //Destroy other components that can mess up the rigid model
        if (!isCartoon)
        {
            ArrayList list = new ArrayList();
            list.AddRange(strandA.Bases);
            list.AddRange(strandA.Sugars);
            list.AddRange(strandA.Phosphates);
            list.AddRange(strandB.Bases);
            list.AddRange(strandB.Sugars);
            list.AddRange(strandB.Phosphates);
            foreach (Molecule molecule in list)
            {
                if (molecule != null) {
                    Destroy(molecule.GetComponent<Molecule>());
                }
                MoleculeBehaviourManager moleculeBehaviourManager = molecule.GetComponent<MoleculeBehaviourManager>();
                if (moleculeBehaviourManager != null)
                {
                    Destroy(moleculeBehaviourManager);
                }
                foreach (Atom atom in molecule.atoms)
                {
                    Destroy(atom.GetComponent<Collider>());
                    Destroy(atom.GetComponent<Atom>());
                    Destroy(atom.GetComponent<CustomGrabbable>());
                   
                    atom.GetComponent<Rigidbody>().isKinematic = true;
                    
                }
            }
            
            foreach (Bond bond in gameObject.GetComponentsInChildren<Bond>())
            {
                Destroy(bond);
            }
            foreach (Joint joint in gameObject.GetComponentsInChildren<Joint>())
            {
                Destroy(joint);
            }
        } else if (combinedModel == null)
        {
            Rigidbody rigid = gameObject.AddComponent<Rigidbody>();
            rigid.useGravity = false;
            rigid.isKinematic = true;
            CapsuleCollider capsuleCol = gameObject.AddComponent<CapsuleCollider>();
            capsuleCol.center = new Vector3(-0.4555663f, 5.400001f, 18.9513f);
            capsuleCol.radius = 9.842175f;
            capsuleCol.height = 42.71654f;
            capsuleCol.direction = 2;
            CustomGrabbable customGrab = gameObject.AddComponent<CustomGrabbable>();
            customGrab.enabled = true;
            customGrab.CustomGrabCollider(capsuleCol);
        }
    }

    public void CheckHandsForAtoms()
    {
        Atom lastHeldLeft = heldAtomLeft;
        Atom lastHeldRight = heldAtomRight;

        //Check the hands for atoms. Inform the molecule if it's a new atom being held
        if (leftHand.grabbedObject && leftHand.grabbedObject.GetComponent<Atom>())
        {
            heldAtomLeft = leftHand.grabbedObject.GetComponent<Atom>();
            if (!heldAtomLeft.Equals(lastHeldLeft))
            {
                heldAtomLeft.parentMol.MoleculePickedUp(leftHand, heldAtomLeft);
                leftHand.hand.shouldCollide = false;
            }
        }
        else
        {
            heldAtomLeft = null;
        }

        if (rightHand.grabbedObject && rightHand.grabbedObject.GetComponent<Atom>())
        {
            heldAtomRight = rightHand.grabbedObject.GetComponent<Atom>();
            if (!heldAtomRight.Equals(lastHeldRight))
            {
                heldAtomRight.parentMol.MoleculePickedUp(rightHand, heldAtomRight);
                rightHand.hand.shouldCollide = false;
            }
        }
        else
        {
            heldAtomRight = null;
        }

        // if either hand dropped an atom, let the molecule know
        if (!heldAtomLeft && lastHeldLeft)
        {
            lastHeldLeft.parentMol.MoleculeDropped(leftHand);
            leftHand.hand.shouldCollide = true;
        }

        if (!heldAtomRight && lastHeldRight)
        {
            lastHeldRight.parentMol.MoleculeDropped(rightHand);
            rightHand.hand.shouldCollide = true;
        }
    }

    public void CacheNewStartPosition()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;
    }
    public void ResetToStartPosition()
    {
        transform.position = initialPos;
        transform.rotation = initialRot;
    }

    // Used to reset the breakable helix back to first positions
    public IEnumerator ResetAllAtomsAndBonds()
    {
        if (!strandA.isInGhostmode) yield return strandA.ResetAtomsAndBonds();
        if (!strandB.isInGhostmode) yield return strandB.ResetAtomsAndBonds();
    }
   
    public void ColorByMode ()
    {
        switch (currentColorMode)
        {
            case colorMode.atom:
                break;
            case colorMode.molecule:
                //DO STUFF
                //Strand A
                //Sugar
                foreach (Sugar molecule in strandA.Sugars)
                {
                    foreach (Atom atom in molecule.atoms)
                    {

                        foreach (Transform meshRenderer in atom.transform) {
                           if (meshRenderer.transform.tag != "bond" && meshRenderer.GetComponent<MeshRenderer>() != null)
                            meshRenderer.GetComponent<MeshRenderer>().material = moleculeMats[0];
                        }
                    }
                }
                //Base
                foreach (NBase molecule in strandA.Bases)
                {
                    foreach (Atom atom in molecule.atoms)
                    {

                        foreach (Transform meshRenderer in atom.transform)
                        {
                            if (meshRenderer.transform.tag != "bond" && meshRenderer.GetComponent<MeshRenderer>() != null)
                                meshRenderer.GetComponent<MeshRenderer>().material = moleculeMats[1];
                        }
                    }
                }
                //Phosphate
                foreach (Phosphate molecule in strandA.Phosphates)
                {
                    foreach (Atom atom in molecule.atoms)
                    {

                        foreach (Transform meshRenderer in atom.transform)
                        {
                            if (meshRenderer.transform.tag != "bond" && meshRenderer.GetComponent<MeshRenderer>() != null)
                                meshRenderer.GetComponent<MeshRenderer>().material = moleculeMats[2];
                        }
                    }
                }
                //Strand B
                //Sugar
                foreach (Sugar molecule in strandB.Sugars)
                {
                    foreach (Atom atom in molecule.atoms)
                    {

                        foreach (Transform meshRenderer in atom.transform)
                        {
                            if (meshRenderer.transform.tag != "bond" && meshRenderer.GetComponent<MeshRenderer>() != null)
                                meshRenderer.GetComponent<MeshRenderer>().material = moleculeMats[0];
                        }
                    }
                }
                //Base
                foreach (NBase molecule in strandB.Bases)
                {
                    foreach (Atom atom in molecule.atoms)
                    {

                        foreach (Transform meshRenderer in atom.transform)
                        {
                            if (meshRenderer.transform.tag != "bond" && meshRenderer.GetComponent<MeshRenderer>() != null)
                                meshRenderer.GetComponent<MeshRenderer>().material = moleculeMats[1];
                        }
                    }
                }
                //Phosphate
                foreach (Phosphate molecule in strandB.Phosphates)
                {
                    foreach (Atom atom in molecule.atoms)
                    {

                        foreach (Transform meshRenderer in atom.transform)
                        {
                            if (meshRenderer.transform.tag != "bond" && meshRenderer.GetComponent<MeshRenderer>() != null)
                                meshRenderer.GetComponent<MeshRenderer>().material = moleculeMats[2];
                        }
                    }
                }
                break;
            case colorMode.nBase:
                //Strand A 

                //Sugars
                foreach(Sugar molecule in strandA.Sugars)
                {
                    foreach (Atom atom in molecule.atoms)
                    {

                        foreach (Transform meshRenderer in atom.transform)
                        {
                            if (meshRenderer.transform.tag != "bond" && meshRenderer.GetComponent<MeshRenderer>() != null)
                            {
                                int baseMatIndex = 0;
                                if (molecule.NBase.baseType == NBase.BaseType.ADENINE) baseMatIndex = 0;
                                if (molecule.NBase.baseType == NBase.BaseType.CYTOSINE) baseMatIndex = 1;
                                if (molecule.NBase.baseType == NBase.BaseType.GUANINE) baseMatIndex = 2;
                                if (molecule.NBase.baseType == NBase.BaseType.THYMINE) baseMatIndex = 3;
                                if (molecule.NBase.baseType == NBase.BaseType.UNKNOWN) baseMatIndex = 4;
                                meshRenderer.GetComponent<MeshRenderer>().material = baseMats[baseMatIndex];
                            }
                        }
                    }
                }
                //Bases
                foreach (NBase molecule in strandA.Bases)
                {
                    foreach (Atom atom in molecule.atoms)
                    {

                        foreach (Transform meshRenderer in atom.transform)
                        {
                            if (meshRenderer.transform.tag != "bond" && meshRenderer.GetComponent<MeshRenderer>() != null)
                            {
                                int baseMatIndex = 0;
                                if (molecule.baseType == NBase.BaseType.ADENINE) baseMatIndex = 0;
                                if (molecule.baseType == NBase.BaseType.CYTOSINE) baseMatIndex = 1;
                                if (molecule.baseType == NBase.BaseType.GUANINE) baseMatIndex = 2;
                                if (molecule.baseType == NBase.BaseType.THYMINE) baseMatIndex = 3;
                                if (molecule.baseType == NBase.BaseType.UNKNOWN) baseMatIndex = 4;
                                meshRenderer.GetComponent<MeshRenderer>().material = baseMats[baseMatIndex];
                            }
                        }
                    }
                }
                //Phosphates
                foreach (Phosphate molecule in strandA.Phosphates)
                {
                    foreach (Atom atom in molecule.atoms)
                    {

                        foreach (Transform meshRenderer in atom.transform)
                        {
                            if (meshRenderer.transform.tag != "bond" && meshRenderer.GetComponent<MeshRenderer>() != null)
                            {
                                int baseMatIndex = 0;
                                if (molecule.fivePrimeSugar.GetComponent<Sugar>().NBase.baseType == NBase.BaseType.ADENINE) baseMatIndex = 0;
                                if (molecule.fivePrimeSugar.GetComponent<Sugar>().NBase.baseType == NBase.BaseType.CYTOSINE) baseMatIndex = 1;
                                if (molecule.fivePrimeSugar.GetComponent<Sugar>().NBase.baseType == NBase.BaseType.GUANINE) baseMatIndex = 2;
                                if (molecule.fivePrimeSugar.GetComponent<Sugar>().NBase.baseType == NBase.BaseType.THYMINE) baseMatIndex = 3;
                                if (molecule.fivePrimeSugar.GetComponent<Sugar>().NBase.baseType == NBase.BaseType.UNKNOWN) baseMatIndex = 4;
                                meshRenderer.GetComponent<MeshRenderer>().material = baseMats[baseMatIndex];
                            }
                        }
                    }
                }


                //Strand B

                //Sugars
                foreach (Sugar molecule in strandB.Sugars)
                {
                    foreach (Atom atom in molecule.atoms)
                    {

                        foreach (Transform meshRenderer in atom.transform)
                        {
                            if (meshRenderer.transform.tag != "bond" && meshRenderer.GetComponent<MeshRenderer>() != null)
                            {
                                int baseMatIndex = 0;
                                if (molecule.NBase.baseType == NBase.BaseType.ADENINE) baseMatIndex = 0;
                                if (molecule.NBase.baseType == NBase.BaseType.CYTOSINE) baseMatIndex = 1;
                                if (molecule.NBase.baseType == NBase.BaseType.GUANINE) baseMatIndex = 2;
                                if (molecule.NBase.baseType == NBase.BaseType.THYMINE) baseMatIndex = 3;
                                if (molecule.NBase.baseType == NBase.BaseType.UNKNOWN) baseMatIndex = 4;
                                meshRenderer.GetComponent<MeshRenderer>().material = baseMats[baseMatIndex];
                            }
                        }
                    }
                }
                //Bases
                foreach (NBase molecule in strandB.Bases)
                {
                    foreach (Atom atom in molecule.atoms)
                    {

                        foreach (Transform meshRenderer in atom.transform)
                        {
                            if (meshRenderer.transform.tag != "bond" && meshRenderer.GetComponent<MeshRenderer>() != null)
                            {
                                int baseMatIndex = 0;
                                if (molecule.baseType == NBase.BaseType.ADENINE) baseMatIndex = 0;
                                if (molecule.baseType == NBase.BaseType.CYTOSINE) baseMatIndex = 1;
                                if (molecule.baseType == NBase.BaseType.GUANINE) baseMatIndex = 2;
                                if (molecule.baseType == NBase.BaseType.THYMINE) baseMatIndex = 3;
                                if (molecule.baseType == NBase.BaseType.UNKNOWN) baseMatIndex = 4;
                                meshRenderer.GetComponent<MeshRenderer>().material = baseMats[baseMatIndex];
                            }
                        }
                    }
                }
                //Phosphates
                foreach (Phosphate molecule in strandB.Phosphates)
                {
                    foreach (Atom atom in molecule.atoms)
                    {

                        foreach (Transform meshRenderer in atom.transform)
                        {
                            if (meshRenderer.transform.tag != "bond" && meshRenderer.GetComponent<MeshRenderer>() != null)
                            {
                                int baseMatIndex = 0;
                                if (molecule.fivePrimeSugar.GetComponent<Sugar>().NBase.baseType == NBase.BaseType.ADENINE) baseMatIndex = 0;
                                if (molecule.fivePrimeSugar.GetComponent<Sugar>().NBase.baseType == NBase.BaseType.CYTOSINE) baseMatIndex = 1;
                                if (molecule.fivePrimeSugar.GetComponent<Sugar>().NBase.baseType == NBase.BaseType.GUANINE) baseMatIndex = 2;
                                if (molecule.fivePrimeSugar.GetComponent<Sugar>().NBase.baseType == NBase.BaseType.THYMINE) baseMatIndex = 3;
                                if (molecule.fivePrimeSugar.GetComponent<Sugar>().NBase.baseType == NBase.BaseType.UNKNOWN) baseMatIndex = 4;
                                meshRenderer.GetComponent<MeshRenderer>().material = baseMats[baseMatIndex];
                            }
                        }
                    }
                }
                break;
        }
    }

}
