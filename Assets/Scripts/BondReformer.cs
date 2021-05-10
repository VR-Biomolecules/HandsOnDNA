using System.Collections;
using System.Collections.Generic;
using MoleculeTypes;
using UnityEngine;

public class BondReformer : MonoBehaviour
{
    public GameObject bondPf;

    [Header("Auto-filled")]

    public Atom atom;
    public Atom atomToConnectTo;

    public Vector3 planeForward;
    public Vector3 planeUp;

    bool canReform;
    public AudioSource onBondReform;
    public bool letReformedBondsBreak = true;

    public static void Initialise(BondReformer target, Atom newAtom, Atom newConAtom, Vector3 child1Pos, Vector3 child2Pos)
    {
        target.atom = newAtom;
        target.transform.parent = newAtom.transform;

        target.atomToConnectTo = newConAtom;

        target.planeForward = Vector3.Lerp(child1Pos, child2Pos, 0.5f) - target.atomToConnectTo.transform.position;
        target.planeUp = Vector3.Cross(child1Pos - target.atomToConnectTo.transform.position, child2Pos - target.atomToConnectTo.transform.position);

        target.transform.position = target.atomToConnectTo.transform.position;
    }

    public void CanReform()
    {
        canReform = true;
        // print("CanReform");
    }

    public void ResetToOriginal(bool breakable)
    {
        // Create new bond
        CreateNewBond(atomToConnectTo, breakable);

        // Remove Bondableness
        atom.bcd.Disable();
        atomToConnectTo.bcd.Disable();

        // Disable Reformer
        gameObject.SetActive(false);
        canReform = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!gameObject.activeSelf || !canReform) return;

        Atom otherAtom = other.GetComponent<Atom>() ?? null;

        if (otherAtom && otherAtom.bcd && otherAtom.bcd.isOtherReformTarget && otherAtom.bcd.enabled && Bondable(other.name))
        {
            FormBondWithPartnerMolecule(otherAtom.bcd.bondReformer);
        }
    }

    void FormBondWithPartnerMolecule(BondReformer other)
    {
        // Check if the atoms are being held now, so we know what to vibrate on bond reformation
        List<CustomGrabber> hands = GetHandsToVibrate(atom, other.atom);
        if (onBondReform) onBondReform.Play();

        // Reorientate other chain to fit
        ReorientatePartnerMolecule(other);
        
        // Create new bond
        CreateNewBond(other.atomToConnectTo, letReformedBondsBreak);

        // Tell the molecule
        atom.parentMol.BondReformedInMolecule(atom, other.atomToConnectTo);
        
        // Vibrate hand holding the sugar if there is one
        foreach (CustomGrabber hand in hands)
        {
            Player.Vibrate(0.2f, 1.0f, hand, true);
        }

        // Foreach molecule in chain
        foreach (Molecule m in atom.parentMol.connectedMols)
        {
            foreach (Atom a in m.atoms)
            {
                a.rigid.velocity = Vector3.zero;
                a.rigid.angularVelocity = Vector3.zero;
                a.RecordLastPosAndRot();
                a.RecordLastStableState();
            }
        }

        // Remove Bondableness from both atoms
        atom.bcd.Disable();
        other.atomToConnectTo.bcd.Disable();

        // Disable Reformer
        gameObject.SetActive(false);
        canReform = false;
    }

    private List<CustomGrabber> GetHandsToVibrate(Atom atom1, Atom atom2)
    {
        List<CustomGrabber> toReturn = new List<CustomGrabber>();
        if (atom1.parentMol.isBeingHeld) toReturn.Add(atom1.parentMol.firstHand);
        if (atom2.parentMol.isBeingHeld) toReturn.Add(atom2.parentMol.firstHand);
        return toReturn;
    }


    void ReorientatePartnerMolecule(BondReformer originalWeAreReplacing)
    {
        Atom toBondTo = originalWeAreReplacing.atomToConnectTo;

        // Foreach molecule in chain
        foreach (Molecule m in toBondTo.parentMol.connectedMols)
        {
            foreach(Atom a in m.atoms)
            {
                a.rigid.velocity = Vector3.zero;
                a.rigid.angularVelocity = Vector3.zero;
            }

            // Detach hands from other (non-sugar) molecule
            if (m.firstHand)
            {
                m.firstHand.ForceRelease(m.firstHand.grabbedObject);
                // print("Let go of atom");
            }
            if (m.secondHand) m.secondHand.ForceRelease(m.secondHand.grabbedObject);
        }

        // Unparent the atom, then set all its connected mols as children. This is so we can rotate the atom and have the whole chain move
        toBondTo.transform.parent = null;
        foreach (Molecule mol in toBondTo.parentMol.connectedMols)
        {
            mol.transform.parent = toBondTo.transform;
        }

        // Snap toBondTo atom into place
        Quaternion dif = Quaternion.LookRotation(planeForward, planeUp) * Quaternion.Inverse(Quaternion.LookRotation(originalWeAreReplacing.planeForward, originalWeAreReplacing.planeUp));
        // print(dif); // Check that when reforming with our original partner, the rotational 'dif'erence is 0
        toBondTo.transform.position = transform.position;
        toBondTo.transform.rotation = atom.transform.rotation * dif;

        // Restore parents of all molecules connect to toBondTo
        foreach (Molecule mol in toBondTo.parentMol.connectedMols)
        {
            mol.transform.parent = mol.strand.transform;
        }
        // Restore toBondTo's original parent
        toBondTo.transform.parent = toBondTo.parentMol.transform; // <---- ToDo. For some reason, it acts like this line doesn't happen
    }

    void CreateNewBond(Atom otherAtom, bool breakable)
    {
        Atom fromAtom = atom;
        Atom toAtom = otherAtom;

        // Work out the initial bond position, and draw it
        Vector3 fromPos = fromAtom.transform.position;
        Vector3 toPos = toAtom.transform.position;
        Vector3 originalPosition = Vector3.Lerp(fromPos, toPos, 0.5f);
        float originalLength = (fromPos - toPos).magnitude;

        GameObject bondOb = Instantiate(bondPf);

        bondOb.transform.Translate(originalPosition);
        bondOb.transform.LookAt(toAtom.transform);
        bondOb.transform.Rotate(new Vector3(1.0f, 0.0f, 0.0f), 90);
        bondOb.transform.SetParent(fromAtom.transform);

        // Set the parameters in the relevant Bond and Atom scripts
        Bond bond = bondOb.GetComponent<Bond>();
        bond.fromAtom = fromAtom;
        bond.toAtom = toAtom;
        //bond.order = bondInfo.order; #ToDo
        bond.originalLength = originalLength;
        bond.bondJoint = AddBondJoint(fromAtom.gameObject, toAtom.gameObject, breakable); //todo make this breakable optionally

        fromAtom.bondsOnThisAtom.Add(bond);
        toAtom.bondsOnThisAtom.Add(bond);
        
        //bonds.Add(bond); #ToDo
        atom.parentMol.AddBond(bond);

        // Set correct neighbouring molecules
        Sugar sugar = atom.parentMol.GetComponent<Sugar>();
        switch (atom.transform.name)
        {
            case "C1'":
                NBase nBase = otherAtom.parentMol.GetComponent<NBase>();
                sugar.SetGBond(bond, nBase);
                break;

            case "C5'":
                Phosphate fivePrime = otherAtom.parentMol.GetComponent<Phosphate>();
                sugar.SetPpdeBond(bond, fivePrime, true);
                break;
            case "C3'":
                Phosphate threePrime = otherAtom.parentMol.GetComponent<Phosphate>();
                sugar.SetPpdeBond(bond, threePrime, false);
                break;
        }
    }

    ConfigurableJoint AddBondJoint(GameObject fromAtom, GameObject toAtom, bool breakable) //todo make the remade joint unbreakable
    {
        ConfigurableJoint cj = fromAtom.AddComponent(typeof(ConfigurableJoint)) as ConfigurableJoint;
        cj.connectedBody = toAtom.GetComponent<Rigidbody>();

        var fromPos = fromAtom.transform.position;
        var toPos = toAtom.transform.position;
        cj.anchor = toPos - fromPos; //todo anchor may be wrong way round re: peppy code in sidechain builder in phenylalanine

        Vector3 worldAxis = fromPos - toPos;
        Vector3 localAxis = fromAtom.transform.InverseTransformDirection(worldAxis);
        cj.axis = localAxis;

        cj.autoConfigureConnectedAnchor = true;

        cj.xMotion = ConfigurableJointMotion.Locked;
        cj.yMotion = ConfigurableJointMotion.Locked;
        cj.zMotion = ConfigurableJointMotion.Locked;

        cj.angularXMotion = ConfigurableJointMotion.Locked;
        cj.angularYMotion = ConfigurableJointMotion.Locked;
        cj.angularZMotion = ConfigurableJointMotion.Locked;

        if (breakable) cj.breakForce = 1500;

        fromAtom.GetComponent<Rigidbody>().mass = 1;
        fromAtom.GetComponent<Rigidbody>().drag = 5;
        fromAtom.GetComponent<Rigidbody>().angularDrag = 5;

        toAtom.GetComponent<Rigidbody>().mass = 1;
        toAtom.GetComponent<Rigidbody>().drag = 5;
        toAtom.GetComponent<Rigidbody>().angularDrag = 5;

        return cj;
    }

    bool Bondable(string otherAtomName)
    {
        switch (atom.transform.name)
        {
            case "C1'":
                return otherAtomName.Equals("N1") || otherAtomName.Equals("N9");
            case "C5'":
                return otherAtomName.Equals("O5'");
            case "C3'":
                return otherAtomName.Equals("O3'");
        }
        return false;
    }
}
