using System.Collections;
using System.Collections.Generic;
using MoleculeTypes;
using UnityEngine;

public class MoleculeBehaviourManager : MonoBehaviour
{
    public Molecule managedMol;

    // Maximum grabbed-atom change-in-position before the rest of the molecule follows the atom
    float grabbedAtomVelocityLimit = 1;
    // Maximum grabbed-atom change-in-rotation before the rest of the molecule follows the atom
    float grabbedAtomAngVelLimit = 15;
    // Maximum force before applying "explosion-fix"
    float bondMaxForce = 10000;
    
    // Centre position - where the molecules should return to
    public Vector3 centre = new Vector3(0, 1.25f, 0);
    // Max distance for atoms from centre before molecules return
    float maxCentreDistance = 1f;
    // Recenter force to apply
    float recenterForce = 15;
    // Recenter force distance power
    float recenterForcePower = 3;
    // Editor tool
    public bool recentreMolecule = true;

    // Maximum time until bat is allowed to apply force again from initial hit
    float batForceMaxTime = 0.25f;
    // Current timer until bat is allowed to apply force again
    float batForceTimer;

    private void Start()
    {
        managedMol.behaviourManager = this;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!enabled) return;

        if (CheckIfMoleculeIsAlreadyExplodingAndReset() || OverallSpeed() > 0)
        {
            // Record last postions and rotations of atoms
            foreach (Atom a in managedMol.atoms)
            {
                a.RecordLastPosAndRot();
            }

            // Update bond positions and lengths first
            foreach (Bond bond in managedMol.bonds)
            {
                bond.UpdatePositionAndScale();
            }
        }
        
        // And if the molecule is too far for user to reach, push it back
        if (recentreMolecule) CheckIfTooFarFromUser();

        // Countdown bat force timer
        if (batForceTimer != 0) { batForceTimer = Mathf.Max(0, batForceTimer - Time.fixedDeltaTime); }
    }
    
    /**
     * First checks if there is one grabbed atom and it is being pulled or rotated too quickly, then undoes that movement.
     *
     * Then checks each bond to see if the 
     */
    private bool CheckIfMoleculeIsAlreadyExplodingAndReset()
    {
        bool reset = false; //todo doesnt actually prompt a reset i dont think
        bool stable = true;
        Atom grabbedAtom = null;

        // If molecule if being held by a single hand
        if (managedMol.isBeingHeld && !managedMol.secondHand)
        {
            grabbedAtom = managedMol.firstAtomHeld;

            Vector3 posDiff = grabbedAtom.transform.position - grabbedAtom.lastPosition;
            Quaternion rotDiff = grabbedAtom.transform.rotation * Quaternion.Inverse(grabbedAtom.lastRotaiton);

            // Exceeded positional speed
            if (posDiff.magnitude > grabbedAtomVelocityLimit * Time.deltaTime)
            {
                // Debug.Log("Moving too quickly. Undoing difference", this);
                foreach (Atom a in managedMol.atoms)
                {
                    if (a != grabbedAtom)
                    {
                        a.transform.position += posDiff;
                    }
                }

                stable = false;
            }
            // Exceeded rotational speed
            if (Quaternion.Angle(grabbedAtom.transform.rotation, grabbedAtom.lastRotaiton) > grabbedAtomAngVelLimit)
            {
                // Debug.Log("Rotating too quickly. Undoing difference", this);
                foreach (Atom a in managedMol.atoms)
                {
                    if (a != grabbedAtom)
                    {
                        a.transform.position = RotatePointAroundPivot(a.transform.position, grabbedAtom.transform.position, rotDiff);
                    }
                }

                if (stable) stable = false;
            }
        }

        foreach (Bond bond in managedMol.bonds)
        {
            if (bond && bond.bondJoint && float.IsPositiveInfinity(bond.bondJoint.breakForce))
            {
                // Molecule was stable, but this bond is not
                if (stable && bond.bondJoint.currentForce.magnitude > 50)
                {
                    // Set molecule to unstable
                    stable = false;
                }

                // If the bond exceeds our force limit
                if (bond.bondJoint.currentForce.magnitude > bondMaxForce)
                {
                    Vector3 stableOffset = (grabbedAtom) ? grabbedAtom.transform.position - grabbedAtom.lastStablePosition : Vector3.zero;

                    // Debug.Log($"Detaching {managedMol.name} from hand {managedMol.firstHand.name} - A bond is too stretched", this);
                    DetachObjectFromHand(managedMol.firstHand);

                    // Reset entire molecule to last stable state
                    foreach (Atom a in managedMol.atoms) // todo this should end up relative to the atom still held in a hand, if secondHeldAtom isn't Null. But it doesnt seem to matter
                    {
                        a.transform.position = a.lastStablePosition + stableOffset;
                        a.transform.rotation = a.lastStableRotation;
                        a.rigid.velocity *= 0;
                        a.rigid.angularVelocity *= 0;
                    }

                    print("Exploded");

                    reset = true;

                    break;
                }
            }
        }
         
        // Molecule was stable, record state
        if (stable)
        {
            // Record stable state for whole molecule
            foreach (Atom a in managedMol.atoms)
            {
                a.RecordLastStableState();
            }
        }

        return reset;
    }
    void DetachObjectFromHand(CustomGrabber hand)
    {
        // Detach object
        if (hand && hand.grabbedObject)
        {
            hand.ForceRelease(hand.grabbedObject);
            // Player.Vibrate(0.5f, 1, hand);
        }
    }

    void PushTowardsUser()
    {
        Transform targetAtom = managedMol.atoms[0].transform;

        // Target Atom is lower than centre
        if (targetAtom.position.y < centre.y)
        {
            float distance = Vector3.Distance(new Vector3(centre.x, targetAtom.position.y, centre.z), targetAtom.position) - maxCentreDistance;
            Vector3 force = (new Vector3(centre.x, targetAtom.position.y, centre.z) - targetAtom.position).normalized * Mathf.Pow(1 + distance, recenterForcePower) * recenterForce * Time.smoothDeltaTime;

            // Foreach atom in molecule
            foreach (Atom atom in managedMol.atoms)
            {
                // Push towards centre (ignoring y axis)
                atom.GetComponent<Rigidbody>().AddForce(force);
            }
        }
        // Atom is above center
        else
        {
            float distance = Vector3.Distance(centre, targetAtom.position) - maxCentreDistance;
            Vector3 force = (centre - targetAtom.position).normalized * Mathf.Pow(1 + distance, recenterForcePower) * recenterForce * Time.smoothDeltaTime;

            // Foreach atom in molecule
            foreach (Atom atom in managedMol.atoms)
            {
                // Push towards centre
                atom.GetComponent<Rigidbody>().AddForce(force);
            }
        }
    }

    void CheckIfTooFarFromUser()
    {
        // Foreach molecule
        foreach (Molecule m in managedMol.connectedMols)
        {
            // Atom is in range
            if (m.behaviourManager.AtomInRange())
            {
                return;
            }
        }

        // Atom was not in range, move entire chain towards player
        // Foreach molecule
        foreach (Molecule m in managedMol.connectedMols)
        {
            // Atom is in range
            m.behaviourManager.PushTowardsUser();
        }
        
    }

    bool AtomInRange()
    {
        Transform targetAtom = managedMol.atoms[0].transform;

        // Target Atom is lower than centre
        if (targetAtom.position.y < centre.y)
        {
            return Vector2.Distance(new Vector2(targetAtom.position.x, targetAtom.position.z), new Vector2(centre.x, centre.z)) < maxCentreDistance;
        }
        // Atom is in range of centre
        else
        {
            return Vector3.Distance(targetAtom.position, centre) < maxCentreDistance;
        }
    }

    // internal void NewChainStatus()
    // {
    //     // Is at the top of the chain
    //     bool isTopOfChain = (!transform.parent || !transform.parent.GetComponent<Molecule>());
    //     // Update parent chain status
    //     if (!isTopOfChain)
    //     {
    //         transform.parent.GetComponent<MoleculeBehaviourManager>().NewChainStatus();
    //     }
    //     else // Cache array of all chain members
    //     {
    //         // Get all behaviours in chain (including self)
    //         entireChain = GetComponentsInChildren<MoleculeBehaviourManager>();
    //         // Set topOfChain for entire chain
    //         foreach(MoleculeBehaviourManager m in entireChain)
    //         {
    //             m.topOfChain = this;
    //             if (m != this) m.entireChain = new MoleculeBehaviourManager[0];
    //         }
    //     }
    // }

    // internal MoleculeBehaviourManager LastInChain()
    // {
    //     return (entireChain.Length > 0) ? entireChain[entireChain.Length-1] : null;
    // }

    public void ApplyBatForce(Vector3 force)
    {
        // Bat is allowed to apply force
        if (batForceTimer == 0)
        {
            
            // Foreach atom
            foreach (Atom a in managedMol.atoms)
            {
                a.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
            }

            // Restart timer
            batForceTimer = batForceMaxTime;
        }
    }

    public float OverallSpeed()
    {
        float totalSpeed = 0;

        foreach(Atom a in managedMol.atoms)
        {
            totalSpeed += a.rigid.velocity.magnitude;
        }

        /*
        if (Input.GetKey("i"))
        {
            print(totalSpeed);
        }
        */

        return totalSpeed;
    }

    static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        return rotation * (point - pivot) + pivot;
    }
}
