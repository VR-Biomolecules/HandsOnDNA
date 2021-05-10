using System.Collections.Generic;
using UnityEngine;

namespace MoleculeTypes
{
    /**
     * The Sugar phosphate backbone unit. Keeps handles to its adjoining 5' and 3' phosphates, and the NBase.
     *
     * Everything is based off the C3' atom in the sugar prefab. It sits at 0,0,0 relative to all the other prefab atoms. 
     * Maintain the same translation and rotation on the attached 5' Phosphate and NBase, and everything stays aligned. 
 */
    public class Sugar : Molecule
    {
        public Atom C5Carbon;
        public Bond fivePrimeBond;
        public Phosphate fivePrimePhosphate;

        
        public Atom C3Carbon;
        public Bond threePrimeBond;
        public Phosphate threePrimePhosphate;

        public Atom C1Carbon;
        public Bond GBond;
        public NBase NBase;
        
        public override void IdentifyKeyAtoms(bool smashModel)
        {
            if (smashModel)
            {
                C5Carbon = atoms[0];
                C3Carbon = atoms[8];
                C1Carbon = atoms[6];
            }
            else
            {
                C5Carbon = atoms[0];
                C3Carbon = atoms[3];
                C1Carbon = atoms[5];
            }
        }

        /**
         * Set a Phosphodiester bond for this Sugar. For ease, the Sugar looks after all breakable bonds.
         */
        public void SetPpdeBond(Bond ppdeBond, Phosphate phosphate, bool isFivePrime)
        {
            if (isFivePrime)
            {
                strand.ppdeBonds.Add(ppdeBond);

                fivePrimeBond = ppdeBond;
                fivePrimePhosphate = phosphate;
                phosphate.fivePrimeSugar = this;
                phosphate.fivePrimeBond = ppdeBond;
                //Debug.Log("Phosphate bond is: " + phosphate.fivePrimeBond);
            }
            else
            {
                strand.ppdeBonds.Add(ppdeBond);

                threePrimeBond = ppdeBond;
                threePrimePhosphate = phosphate;
                phosphate.threePrimeSugar = this;
                phosphate.threePrimeBond = ppdeBond;
            }
        }

        // Remove a Phosphodiester bond from this sugar, including setting all the linkages to 
        // null on the relevant molecules, and unparenting the right molecule. 
        private void RemovePpdeBond(bool isFivePrime)
        {
            if (isFivePrime)
            {
                strand.ppdeBonds.Remove(fivePrimeBond);

                fivePrimeBond = null;
                fivePrimePhosphate.fivePrimeSugar = null;
                fivePrimePhosphate.fivePrimeBond = null;
                fivePrimePhosphate = null;
                // transform.parent = null; // this sugar's 5' Phosphate is gone, which was its parent
            }
            else
            {
                strand.ppdeBonds.Remove(threePrimeBond);

                threePrimeBond = null;
                threePrimePhosphate.threePrimeSugar = null;
                threePrimePhosphate.threePrimeBond = null;
                // threePrimePhosphate.transform.parent = null; // This phosphate loses this sugar as a parent
                threePrimePhosphate = null;
            }
        }

        /**
         * Set the Glycosidic bond between this sugar and the given NBase
         * TODO when remaking bonds, this will have to set the parent again
         */
        public void SetGBond(Bond bond, NBase nBase)
        {
            strand.gBonds.Add(bond);

            GBond = bond;
            NBase = nBase;
            nBase.sugar = this;
            nBase.gBond = bond;
        }

        // When a GBond is broken, remove references to the NBase
        public void RemoveGBond()
        {
            strand.gBonds.Remove(GBond);

            GBond = null;
            NBase.sugar = null;
            NBase.gBond = null;
            // NBase.gameObject.transform.parent = null; // The NBase's parent was this sugar
            NBase = null;
        }

        /**
         * Breaks the bond between this sugar and whichever molecule is on the other end.
         * Sets molecule references everywhere relevant to null, and deparents the molecule
         * whose parent-child relationship was through this bond. 
         */
        public void OnBondBreak(Bond brokenBond, Atom triggeredAtom)
        {
            if (triggeredAtom.label.Equals("C1'"))
            {
                RemoveGBond();
            }
            else
            {
                RemovePpdeBond(triggeredAtom.label.Equals("C5'"));
            }
            bonds.Remove(brokenBond);

            UpdateConnectedMolStatus();
        }

        public override void UpdateConnectedMolStatus()
        {
            HashSet<Molecule> allMols = new HashSet<Molecule>();

            if (fivePrimePhosphate != null)
            {
                allMols = FindAllConnectedMolsInDirection(allMols, true);
            } 
            if (threePrimePhosphate != null)
            {
                allMols = FindAllConnectedMolsInDirection(allMols, false);
            }

            List<Molecule> allMolList = new List<Molecule>(allMols);
            
            // To handle when sugar becomes disconnected
            if (allMolList.Count == 0) {
                allMolList.Add(this); 
                if (NBase != null) allMolList.Add(NBase);
            }
            
            foreach (Molecule mol in allMolList)
            {
                mol.connectedMols = allMolList;
            }
        }

        public HashSet<Molecule> FindAllConnectedMolsInDirection(HashSet<Molecule> allMols, bool isFivePrime)
        {
            allMols.Add(this);
            if (NBase != null)
            {
                allMols.Add(NBase);
            }

            if (isFivePrime)
            {
                if (fivePrimePhosphate != null)
                    return fivePrimePhosphate.FindAllConnectedMolsInDirection(allMols, true);
            }
            else
            {
                if (threePrimePhosphate != null)
                {
                    return threePrimePhosphate.FindAllConnectedMolsInDirection(allMols, false);
                }
            }

            return allMols;
        }
    }
}
