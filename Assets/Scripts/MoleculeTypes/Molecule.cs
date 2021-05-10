using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoleculeTypes
{ 
    /**
     * Parent class to act as a container for any molecule (collection of atoms joined by bonds).
     * Subclass for specific cases.
     */
    public abstract class Molecule : MonoBehaviour
    {
        public List<Atom> atoms;
        public List<Bond> bonds;
        public ModelType ModelType;
        internal MoleculeBehaviourManager behaviourManager;
        
        public DNAStrand strand;
        public List<Molecule> connectedMols;

        public bool isBeingHeld;
        public bool connectMolHeld;
        public CustomGrabber firstHand;
        public CustomGrabber secondHand;
        public Atom firstAtomHeld;
        public Atom secondAtomHeld;

        public LabelScript labeller;
        public MeshRenderer labelRenderer;
        [FormerlySerializedAs("overrideLabelOn")] public bool alwaysShowLabel;
        public bool showLabelWhileConnected;

        public AudioSource bondPop;
        public BondWatcher watcher; // Watches for bonds reforming to display tips
        
        // Position and Rotation info to check if a molecule is being stretched
        Vector3 originalPosDiff;
        float originalRotDiff;
        float maxPosDiff = 1.5f;
        float maxRotDiff = 60f;
        private bool isInGhostmode;

        public void AddAtom(Atom atom)
        {
            atoms.Add(atom);
        }

        public void AddBond(Bond bond)
        {
            bonds.Add(bond);
        }

        // Prompts the molecule to set key atom parameters. Call on creation after atoms populated.
        public abstract void IdentifyKeyAtoms(bool smashModel);

        /**
         * Called when the molecule is picked up by any hand. Let's the molecule
         * track its picked up status without polling. 
         */
        
        public void MoleculePickedUp(CustomGrabber hand, Atom heldAtom)
        {
            // Debug.Log($"Molecule {name} picked up by {hand.name}", this);
            
            // If the molecule is already being held by at least one hand
            if (isBeingHeld)
            {
                // If this is the first hand picking up a new atom
                if (hand.Equals(firstHand))
                {
                    firstAtomHeld = heldAtom;
                }
                else // If this is the second hand picking up the molecule
                {
                    secondHand = hand;
                    secondAtomHeld = heldAtom;
                    RecordInitialHeldAtomPositions();
                }
            }
            else if (!isBeingHeld)
            {
                isBeingHeld = true;
                firstHand = hand;
                firstAtomHeld = heldAtom;
                labeller.enabled = true;
                labelRenderer.enabled = true;

                StartCoroutine(HeldUpdate());
            }
            UpdateConnectedMolStatus();
            SetConnectMolHeldStatus();
        }

        /**
         * Record original position and rotation differences of held atoms
         */
        private void RecordInitialHeldAtomPositions()
        {
            Transform firstAtomTf = firstAtomHeld.transform;
            Transform secondAtomTf = secondAtomHeld.transform;
            originalPosDiff = firstAtomTf.InverseTransformPoint(secondAtomTf.position);
            originalRotDiff = Quaternion.Angle(firstAtomTf.rotation, secondAtomTf.rotation);
        }

        /**
         * Called when a molecule is dropped by a hand. Handles keeping track
         * of which hand grabbed first and which atoms are being held. 
         */
        public void MoleculeDropped(CustomGrabber hand)
        {
            // Debug.Log($"Molecule {name} dropped by {hand.name}", this);
            if (hand.Equals(firstHand))
            {
                firstHand = secondHand;
                firstAtomHeld = secondAtomHeld;
                secondHand = null;
                secondAtomHeld = null;
            }
            else if (hand.Equals(secondHand))
            {
                secondHand = null;
                secondAtomHeld = null;
            }

            if (!firstHand && !secondHand)
            {
                isBeingHeld = false;
                labeller.enabled = false;
                labelRenderer.enabled = false;
            }
            UpdateConnectedMolStatus();
            SetConnectMolHeldStatus();
        }

        private void SetConnectMolHeldStatus()
        {
            bool held = false;
            foreach (Molecule mol in connectedMols)
            {
                if (mol.isBeingHeld) held = true;
            }
            
            foreach (Molecule connectedMol in connectedMols)
            {
                connectedMol.connectMolHeld = held;
            }
        }

        IEnumerator HeldUpdate()
        {
            while (isBeingHeld)
            {
                // If this molecule is being held by both hands and is close to breaking
                if (secondAtomHeld && MoleculeIsStretched())
                {
                    // Debug.Log($"Detaching {name} from hand {firstHand.name} - Mol stretched and in both hands", this);

                    // Drop the molecule from the first hand and apply haptic feedback
                    firstHand.ForceRelease(firstHand.grabbedObject);
                    // Player.Vibrate(0.5f, 1f, firstHand);
                    MoleculeDropped(firstHand);
                }

                yield return null;
            }
        }
        
        private bool MoleculeIsStretched()
        {
            return Vector3.Distance(originalPosDiff, firstAtomHeld.transform.InverseTransformPoint(secondAtomHeld.transform.position)) > maxPosDiff
                   || Mathf.Abs(Quaternion.Angle(firstAtomHeld.transform.rotation, secondAtomHeld.transform.rotation) - originalRotDiff) > maxRotDiff;
        }

        /**
         * Detects all the molecules in the connected chain, and sets references to them in each mol
         */
        public abstract void UpdateConnectedMolStatus(); //todo when this happens, check if disconnected and if should ghost when disconnected

        public bool IsDisconnected()
        {
            return connectedMols.Count == 1;
        }

        public void BondBrokenInMolecule(Atom from, Atom to, bool makeSound)
        {
            UpdateConnectedMolStatus();
            SetConnectMolHeldStatus();
            Debug.Log(
                $"BondBrokenInMolecule\t{@from.parentMol.name}\t{to.parentMol.name}\t{@from.name}\t{to.name}\t{Time.timeSinceLevelLoad}");
            if (makeSound && bondPop) bondPop.Play();
            
            if (!watcher) return;
            
            if (from.name.Contains("C5") || to.name.Contains("C5"))
            {
                watcher.FivePrimeBroken();
            }
            else if (from.name.Contains("C1") || to.name.Contains("C1"))
            {
                watcher.GlycoBroken();
            }
            else if (from.name.Contains("C3") || to.name.Contains("C3"))
            {
                // only correct if from different mols
                watcher.ThreePrimeBroken(!from.parentMol.name.Substring(0, 2).Equals(to.parentMol.name.Substring(0, 2)));
            }
        }

        /**
         * Called when this molecule has a bond reformed on it. 
         */
        public void BondReformedInMolecule(Atom atom, Atom otherAtom)
        {
            //update knowledge of connected molecules and if we're being held
            UpdateConnectedMolStatus();
            SetConnectMolHeldStatus();
            Debug.Log(
                $"BondReformedInMolecule\t{atom.parentMol.name}\t{otherAtom.parentMol.name}\t{atom.name}\t{otherAtom.name}\t{Time.timeSinceLevelLoad}");
            
            // Update the watcher on the type of bond formed
            if (!watcher) return;
            
            if (atom.name.Contains("C5") || otherAtom.name.Contains("C5"))
            {
                watcher.FivePrimeFormed();
            }
            else if (atom.name.Contains("C1") || otherAtom.name.Contains("C1"))
            {
                watcher.GlycoFormed();
            }
            else if (atom.name.Contains("C3") || otherAtom.name.Contains("C3"))
            {
                // only correct if from different mols, which have different index numbers as first two chars of name
                watcher.ThreePrimeFormed(!atom.parentMol.name.Substring(0, 2).Equals(otherAtom.parentMol.name.Substring(0, 2)));
            }
        }

        /*
         * Set's this molecule to a non-interactive mode with optional fading
         */
        public void SetGhostMode(bool fade)
        {
            isInGhostmode = true;
            
            foreach (Atom a in atoms)
            {
                if (fade)
                {
                    // Switch all atom's materials to their faded material equivalent
                    foreach (Renderer r in a.renderers)
                    {
                        r.material = a.materialFade;
                    }
                }
                
                // Disable colliders and interactability
                a.GetComponent<SphereCollider>().enabled = false;
                if (a.GetComponent<BondCollisionDetector>()) 
                {
                    a.GetComponent<BondCollisionDetector>().enabled = false;
                }
                a.rigid.isKinematic = true;

                a.GetComponent<CustomGrabbable>().enabled = false;
            }

            // Fade the bonds too
            if (fade)
            {
                foreach(Bond b in bonds)
                {
                    // Switch all bond's materials to their faded material equivilant
                    foreach (Renderer r in b.renderers)
                    {
                        r.material = b.materialFade;
                    }
                }
            }

            // disable the molecule behaviour manager
            MoleculeBehaviourManager mbm = GetComponent<MoleculeBehaviourManager>();
            if (mbm) mbm.enabled = false;
        }
        
        public void Unfade()
        {
            foreach (Atom a in atoms)
            {
                foreach (Renderer r in a.renderers)
                {
                    r.material = a.material;
                }
            }

            foreach(Bond b in bonds)
            {
                foreach (Renderer r in b.renderers)
                {
                    r.material = Cell.BondMat;
                }
            }
        }

        private void Update()
        {
            if (!alwaysShowLabel)
            {
                if (isBeingHeld && (showLabelWhileConnected || IsDisconnected()))
                {
                    labeller.enabled = true;
                    labelRenderer.enabled = true;
                }
                else
                {
                    labeller.enabled = false;
                    labelRenderer.enabled = false;
                }
            }
            else
            {
                labeller.enabled = true;
                labelRenderer.enabled = true;
            }
        }
        
        public void SetGreyMode()
        {
            SetColourToMat(Cell.GreyMat);
        }

        public void SetColourToMat(Material mat)
        {
            foreach (Atom a in atoms)
            {
                foreach (Renderer r in a.renderers)
                {
                    r.material = mat;
                }
            }

            foreach (Bond b in bonds)
            {
                foreach (Renderer r in b.renderers)
                {
                    r.material = mat;
                }
            }
        }
    }

    [Serializable]
    public enum ModelType
    {
        BallAndStick,
        VanDerWaals,
        Cartoon
    }
}