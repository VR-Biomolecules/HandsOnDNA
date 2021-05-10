using System.Collections.Generic;
using UnityEngine;

namespace MoleculeTypes
{
    // 5' end of the DNA molecule wants the extra phosphate, with 3' end hanging (as OH? Confirm)
    // Therefore in DNAStrand list, phosphaste[0] connects to 5' end of sugar[0]
    public class Phosphate : Molecule
    {
        public Atom fivePrimeOxygen;
        public Bond fivePrimeBond;
        public Sugar fivePrimeSugar;

        public Atom threePrimeOxygen;
        public Bond threePrimeBond;
        public Sugar threePrimeSugar;

        public override void IdentifyKeyAtoms(bool smashModel)
        {
            fivePrimeOxygen = atoms[3];
            if (atoms.Count > 4)
            {
                threePrimeOxygen = atoms[4];   
            }
        }

        /**
         * Defer to closest sugar, or this mol is by itself
         */
        public override void UpdateConnectedMolStatus()
        {
            if (fivePrimeSugar != null) fivePrimeSugar.UpdateConnectedMolStatus();
            else if (threePrimeSugar != null) threePrimeSugar.UpdateConnectedMolStatus();
            else connectedMols = new List<Molecule> {this};
        }

        public HashSet<Molecule> FindAllConnectedMolsInDirection(HashSet<Molecule> allMols, bool isFivePrime)
        {
            allMols.Add(this);
            
            if (isFivePrime) //coming from 5' direction from perspective of the sugar, so this is 5' phos and the next mol we need is its 3' sugar
            {
                if (threePrimeSugar != null)
                    return threePrimeSugar.FindAllConnectedMolsInDirection(allMols, true);
            }
            else
            {
                if (fivePrimeSugar != null)
                {
                    return fivePrimeSugar.FindAllConnectedMolsInDirection(allMols, false);
                }
            }

            return allMols;
        }
    }
}
