using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoleculeTypes;
using UnityEditor;
using UnityEngine;

/**
 * Class that builds nucleotide units and joins them together.
 * Use DrawbackBone() and DrawBase() to draw models at the given offset.
 */
public class ProceduralModelGenerator : MonoBehaviour
{
    public bool makeBallAndStick;
    public bool makeVanDerWaals;
    public int strandLength;

    // Empty molecule pfs
    public GameObject dnaStrandPf;
    public GameObject dnaDoubleHelixPf;
    public GameObject nBasePf;
    
    // Ball and Stick prefabs
    public GameObject bsPhosphatePf;
    public GameObject bsSugarPf;
    public GameObject bsAdeninePf;
    public GameObject bsThyminePf;
    public GameObject bsGuaninePf;
    public GameObject bsCytosinePf;
    
    // van der Waals prefabs
    public GameObject vdwPhosphatePf;
    public GameObject vdwSugarPf;
    public GameObject vdwAdeninePf;
    public GameObject vdwThyminePf;
    public GameObject vdwGuaninePf;
    public GameObject vdwCytosinePf;
    
    // Copied from PrefabGenerator to connect up the Generated model
    public GameObject singleBondPf;
    public GameObject doubleBondPf;
    public GameObject bondColliderPf; // Collider used to detect when bondable atoms come into radius
    public static string bondTag = "bond";
    public bool debugging;

    // Transformation values to get the backbone aligned correctly
    private Vector3 bbTranslation = new Vector3(-3.4166f, 2.4795f, 4.2205f) * 0.05f;          
    private Vector3 bbRotation = new Vector3(-33.819f, -13.004f, 14.485f);

    // Transformation values to get the complementary strand aligned NOTE: The failure of these necessitated making the Smash model
    private Vector3 sugarComplementaryTrans = new Vector3(-0.1751085f, 0.7067963f, 0.04925814f); 
    private Vector3 sugarComplementaryRot = new Vector3(12.605f, 5.1f, -171.907f);
    private Vector3 phosphateComplementaryTrans = new Vector3(-0.161f, 0.750738f, -0.04f);
    private Vector3 phosphateComplementaryRot = new Vector3(10.146f, 44.853f, -133.178f);

    private string bases = "atgc";

    void Start()
    {
        string sequence = GenerateRandomSequence(strandLength);

        if (makeBallAndStick) 
        {
            // Vector3 offset = new Vector3(0,0,0.0f);
            // DNAStrand ballAndStickA = DrawDNAStrand(sequence, offset, Quaternion.Euler(-90f, 0f, 0f), ModelType.BallAndStick, false);
            // ballAndStickA.transform.Rotate(-90, 0, 0, Space.World);
            
            Vector3 offset = new Vector3(0.5f, 1.0f, 0);
            DNADoubleHelix ballAndStick = DrawDNAHelix(sequence, offset, Quaternion.identity, ModelType.BallAndStick, true);
            ballAndStick.GetAllContainedMolecules();

            foreach (Molecule molecule in ballAndStick.GetAllContainedMolecules())
            {
                molecule.GetComponent<MoleculeBehaviourManager>().recentreMolecule = false;
            }
            
            ballAndStick.transform.Rotate(0, 0, -90, Space.World);
        }

        if (makeVanDerWaals)
        {
            DNADoubleHelix vanDerWaals = DrawDNAHelix(sequence, new Vector3(-0.5f, 1.0f, 0), Quaternion.identity, ModelType.VanDerWaals, true);
            vanDerWaals.transform.Rotate(0, 0, -90, Space.World);
            
            foreach (Molecule molecule in vanDerWaals.GetAllContainedMolecules())
            {
                molecule.GetComponent<MoleculeBehaviourManager>().recentreMolecule = false;
            }
        }
    }

    private string GenerateRandomSequence(int length)
    {
        StringBuilder sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append(bases[Random.Range(0, bases.Length)]);
        }

        return sb.ToString();
    }

    private GameObject ScaleBy(GameObject GO, float scale)
    {
        Vector3 localScale = GO.transform.localScale;
        GO.transform.localScale = localScale * scale;
        return GO;
    }

    public DNADoubleHelix DrawDNAHelix(string sequence, Vector3 offset, Quaternion rotOffset, ModelType type, bool makeComplementaryToo)
    {
        GameObject dnaOb = Instantiate(dnaStrandPf);
        dnaOb.transform.Translate(offset);
        dnaOb.transform.rotation *= rotOffset;
        
        List<GameObject> bbPhosphates = new List<GameObject>();
        List<GameObject> bbSugars = new List<GameObject>();
        List<GameObject> nBases = new List<GameObject>();
        
        List<Phosphate> bbPhosphatesScripts = new List<Phosphate>();
        List<Sugar> bbSugarsScripts = new List<Sugar>();
        List<NBase> nBaseScripts = new List<NBase>();

        GameObject compDnaOb = Instantiate(dnaStrandPf);
        compDnaOb.transform.Translate(offset);
        compDnaOb.transform.rotation *= rotOffset;

        List<GameObject> bbPhosphatesComp = new List<GameObject>();
        List<GameObject> bbSugarsComp = new List<GameObject>();
        List<GameObject> nBasesComp = new List<GameObject>();
        
        List<Phosphate> bbPhosphatesCompScripts = new List<Phosphate>();
        List<Sugar> bbSugarsCompScripts = new List<Sugar>();
        List<NBase> nBasesCompScripts = new List<NBase>();
        
        string seq = sequence.ToLower();

        // Order of prefabs: 0 - Phosphate, 1 - Sugar, 2 - Adenine, 3 - Thymine, 4 - Guanine, 5 - Cytosine
        GameObject[] prefabs = SelectPrefabs(type);

        for (int i = 0; i < seq.Length; i++)
        {
            GameObject bbPhosphate; // 5' to the sugar on the next line
            GameObject bbSugar; 
            
            GameObject bbPhosphateComp = null; // 5' to the sugar on the next line
            GameObject bbSugarComp = null; 
            if (i == 0) // Making the first base for both strands
            {
                // Make the first phosphate and sugar at the offset (contained in dnaOb)
                bbPhosphate = Instantiate(prefabs[0], dnaOb.transform);
                bbSugar = Instantiate(prefabs[1], bbPhosphate.transform);

                if (makeComplementaryToo)
                {
                    // Create complementary sugar at same position as current sugar, then translate and rotate it into place
                    bbSugarComp = Instantiate(prefabs[1], bbSugar.transform);
                    bbSugarComp.transform.Translate(sugarComplementaryTrans, Space.Self);
                    bbSugarComp.transform.rotation *= Quaternion.Euler(sugarComplementaryRot);
                    bbSugarComp.transform.SetParent(compDnaOb.transform, true);

                    // Create comp phosphate at same position as current sugar, then translate and rotate too
                    bbPhosphateComp = Instantiate(prefabs[0], bbSugar.transform);
                    bbPhosphateComp.transform.Translate(phosphateComplementaryTrans, Space.Self);
                    bbPhosphateComp.transform.rotation *= Quaternion.Euler(phosphateComplementaryRot);
                    bbPhosphateComp.transform.SetParent(bbSugarComp.transform, true);
                }
            }
            else // it's not the the first base. 
            {
                // Positioning based off the sugar molecule for consistency.
                // Prefabs treat 0,0,0 as the same point - the C3' on the sugar of the 5'phosphate-Sugar-NBase complex
                Sugar lastSugar = bbSugars[i - 1].GetComponent<Sugar>();
                Transform lastSugarTransform = bbSugars[i - 1].transform;
                
                // Create the 5' phosphate and sugar at the position of the last unit, then translate and rotate
                // them to the next position
                bbPhosphate = Instantiate(prefabs[0], lastSugarTransform);
                bbSugar = Instantiate(prefabs[1], lastSugarTransform);
                
                bbPhosphate.transform.Translate(bbTranslation, Space.Self);
                bbPhosphate.transform.Rotate(bbRotation, Space.Self);

                bbSugar.transform.Translate(bbTranslation, Space.Self);
                bbSugar.transform.Rotate(bbRotation, Space.Self);
                bbSugar.transform.SetParent(bbPhosphate.transform, true);

                // Add the 3' phosphodiester bond between the newly made phosphate and the last sugar
                Phosphate phos = bbPhosphate.GetComponent<Phosphate>();
                Bond ppdeThree = DrawBond(lastSugar.C3Carbon, 
                    phos.threePrimeOxygen, lastSugar, type == ModelType.BallAndStick, type);

                lastSugar.SetPpdeBond(ppdeThree, phos, false);

                if (makeComplementaryToo)
                {
                    // Create the complementary sugar and phosphate, then apply the complementary transformations
                    // to get them in the right position
                    bbPhosphateComp = Instantiate(prefabs[0], bbPhosphate.transform);
                    bbPhosphateComp.transform.Translate(phosphateComplementaryTrans, Space.Self);
                    bbPhosphateComp.transform.Rotate(phosphateComplementaryRot, Space.Self);

                    bbSugarComp = Instantiate(prefabs[1], bbPhosphate.transform);
                    bbSugarComp.transform.Translate(sugarComplementaryTrans, Space.Self);
                    bbSugarComp.transform.Rotate(sugarComplementaryRot, Space.Self);
                    
                    bbPhosphateComp.transform.SetParent(bbSugarComp.transform);
                    bbSugarComp.transform.SetParent(bbPhosphatesComp[i-1].transform); 

                    // Add the 3' Phosphodiester bond between the newly made sugar and the last phosphate
                    Sugar sugarComp = bbSugarComp.GetComponent<Sugar>();
                    Phosphate lastPhosphateComp = bbPhosphatesComp[i-1].GetComponent<Phosphate>();
                    Bond ppdeBondComp = DrawBond(sugarComp.C3Carbon, 
                        lastPhosphateComp.threePrimeOxygen, sugarComp, type == ModelType.BallAndStick, type);
                    sugarComp.SetPpdeBond(ppdeBondComp, lastPhosphateComp, false);
                }
            }

            Phosphate phosphate = bbPhosphate.GetComponent<Phosphate>();
            Sugar sugar = bbSugar.GetComponent<Sugar>();

            // Add the 5' phosphodiester bond between the new phosphate and sugar
            Bond ppdeBond = DrawBond(sugar.C5Carbon, 
                phosphate.fivePrimeOxygen, sugar, type == ModelType.BallAndStick, type);
            sugar.SetPpdeBond(ppdeBond, phosphate, true);

            // Instantiate the Nitrogenous Base to attach to the sugar
            GameObject nBaseOb = Instantiate(GetBasePf(seq, i, prefabs, false), bbSugar.transform);
            // nBaseOb.transform.SetParent(bbSugar.transform);

            // Add the Glycosidic bond between the sugar and the nitrogenous base
            NBase nBase = nBaseOb.GetComponent<NBase>();
            Bond bondOb = DrawBond(sugar.C1Carbon, nBase.bondingNitrogen, sugar, type == ModelType.BallAndStick, type);
            sugar.SetGBond(bondOb, nBase);
            
            bbPhosphates.Add(bbPhosphate);
            bbSugars.Add(bbSugar);
            nBases.Add(nBaseOb);
            
            bbPhosphatesScripts.Add(phosphate);
            bbSugarsScripts.Add(sugar);
            nBaseScripts.Add(nBase);

            if (!makeComplementaryToo) continue;
            
            Phosphate compPhosphate = bbPhosphateComp.GetComponent<Phosphate>();
            Sugar compSugar = bbSugarComp.GetComponent<Sugar>();

            // Add the 5' phosphodiester bond between the new phosphate and sugar
            Bond ppdeBondCompFive = DrawBond(compSugar.C5Carbon, 
                compPhosphate.fivePrimeOxygen, compSugar, type == ModelType.BallAndStick, type);
            compSugar.SetPpdeBond(ppdeBondCompFive, compPhosphate, true);

            // Instantiate the Nitrogenous Base to attach to the sugar
            GameObject compNBaseOb = Instantiate(GetBasePf(seq, i, prefabs, true), bbSugarComp.transform);
            // compNBaseOb.transform.SetParent(bbSugarComp.transform);

            // Add the Glycosidic bond between the sugar and the nitrogenous base
            NBase compNBase = compNBaseOb.GetComponent<NBase>();
            Bond compBondOb = DrawBond(compSugar.C1Carbon, compNBase.bondingNitrogen, compSugar, type == ModelType.BallAndStick, type);
            compSugar.SetGBond(compBondOb, compNBase);

            bbPhosphatesComp.Add(bbPhosphateComp);
            bbSugarsComp.Add(bbSugarComp);
            nBasesComp.Add(compNBaseOb);
            
            bbPhosphatesCompScripts.Add(compPhosphate);
            bbSugarsCompScripts.Add(compSugar);
            nBasesCompScripts.Add(compNBase);
        }

        GameObject helixOb = Instantiate(dnaDoubleHelixPf);
        helixOb.transform.Translate(offset);
        helixOb.transform.rotation *= rotOffset;
        DNADoubleHelix helix = helixOb.GetComponent<DNADoubleHelix>();

        DNAStrand dna = dnaOb.GetComponent<DNAStrand>();
        dna.Phosphates = bbPhosphatesScripts;
        dna.Sugars = bbSugarsScripts;
        dna.Bases = nBaseScripts;
        
        dnaOb.transform.SetParent(helixOb.transform, true);
        helix.strandA = dna;

        if (makeComplementaryToo)
        {
            DNAStrand compDna = compDnaOb.GetComponent<DNAStrand>();
            compDna.Phosphates = bbPhosphatesCompScripts;
            compDna.Sugars = bbSugarsCompScripts;
            compDna.Bases = nBasesCompScripts;
            compDna.Complementary = true;
            
            compDnaOb.transform.SetParent(helixOb.transform, true);
            helix.strandB = compDna;

            if (type == ModelType.VanDerWaals)
            {
                foreach (Transform trans in compDnaOb.GetComponentsInChildren<Transform>(true))
                {
                    trans.gameObject.layer = 13; // This layer won't collide with itself but will with everything else
                }
            }
            
        }

        // todo terminal phosphates and oxygens
        return helix;
    }

    private GameObject[] SelectPrefabs(ModelType type)
    {
        if (type == ModelType.BallAndStick)
        {
            return new [] {bsPhosphatePf, bsSugarPf, bsAdeninePf, bsThyminePf, bsGuaninePf, bsCytosinePf};
        }
        
        if (type == ModelType.VanDerWaals)
        {
            return new [] {vdwPhosphatePf, vdwSugarPf, vdwAdeninePf, vdwThyminePf, vdwGuaninePf, vdwCytosinePf};
        }
        return new GameObject[6];
    }

    private GameObject GetBasePf(string seq, int i, GameObject[] prefabs, bool complementary)
    {
        GameObject basePf;
        switch (seq[i])
        {
            case 'a':
                basePf = complementary ? prefabs[3] : prefabs[2];
                break;
            case 't':
                basePf = complementary ? prefabs[2] : prefabs[3];
                break;
            case 'g':
                basePf = complementary ? prefabs[5] : prefabs[4];
                break;
            case 'c':
                basePf = complementary ? prefabs[4] : prefabs[5];
                break;
            default:
                basePf = nBasePf;
                Debug.Log("Did not recognise base type " + seq + " at index " + i + ".");
                return basePf;
        }

        return basePf;
    }
    
    private string ReverseComplement(string sequence)
    {
        string revComp = "";
        foreach (char c in sequence.Reverse())
        {
            char comp;
            switch (c)
            {
                case 'a':
                    comp = 't';
                    break;
                case 't':
                    comp = 'a';
                    break;
                case 'g':
                    comp = 'c';
                    break;
                case 'c':
                    comp = 'g';
                    break;
                default:
                    comp = 'x';
                    break;
            }
            revComp += comp;
        }

        return revComp;
    }
    
    //////////////////////////////////////////////////////////////////////////////////////////////////////

    /// Below copied from PrefabGenerator and edited to not use PrefabUtility so it can run in builds/////
    /// //////////////////////////////////////////////////////////////////////////////////////////////////
    
    /**
     * Draw a single bond between two atoms, and add it to the given molecule's bond list
     */
    internal Bond DrawBond(Atom fromAtom, Atom toAtom, Molecule containingMol, bool breakable, ModelType modelType)
    {
        DataReader.BondInfo bondInfo = new DataReader.BondInfo(1, 0, 1);
        // DataReader.BondInfo bondInfo = new DataReader.BondInfo(1, 1, 2); // TODO Needed for the old prefabs. build a switch
        bondInfo.breakable = breakable;
        return DrawBonds(new List<DataReader.BondInfo>() {bondInfo},
            new List<Atom>() {fromAtom, toAtom}, containingMol, false, modelType)[0];
    }

    /**
     * Draws a list of bonds based on bondInfo. Expects the atoms in atoms to be indexed as the
     * numbers in the bondinfos - 1 (the bonds were deduced from atoms numbered from 1).
     *
     * Also adds configurable joints between bonded atoms
     *
     * containingMol = The Molecule object this bond will be a part of
     */
    private List<Bond> DrawBonds(List<DataReader.BondInfo> bondInfos, List<Atom> atoms,
        Molecule containingMol, bool addBondsByAtomName, ModelType modelType)
    {
        List<Bond> bonds = new List<Bond>();
        foreach (DataReader.BondInfo bondInfo in bondInfos)
        {
            Atom fromAtom = null; 
            Atom toAtom = null; 
            if (addBondsByAtomName)
            {
                foreach (Atom atom in atoms)
                {
                    if (atom.label.Equals(bondInfo.fromName))
                    {
                        fromAtom = atom;
                    }
                    else if (atom.label.Equals(bondInfo.toName))
                    {
                        toAtom = atom;
                    }
                }
            }
            else
            {
                fromAtom = atoms[bondInfo.@from];
                toAtom = atoms[bondInfo.@to];
                
                // TODO I need this for the old prefabs...
                // fromAtom = atoms[bondInfo.@from - 1];
                // toAtom = atoms[bondInfo.@to - 1];
            }

            if (fromAtom == null || toAtom == null)
            {
                if (debugging) Debug.Log("Could not find from or to atom for bond from " + 
                                         bondInfo.fromName + " to " + bondInfo.toName);
                continue;
            }

            // Work out the initial bond position, and draw it
            Vector3 fromPos = fromAtom.transform.position;
            Vector3 toPos = toAtom.transform.position;
            Vector3 originalPosition = Vector3.Lerp(fromPos, toPos, 0.5f);
            float originalLength = (fromPos - toPos).magnitude;
            float scale = originalLength / 0.1f; // The bondPf is 0.1 units long. This scales it to the distance between atom centres
            if (debugging) Debug.Log("Drawing bond from " + fromAtom.name + " to " + toAtom.name + " and scaling by " + scale);

            // GameObject bondOb = Instantiate(bondInfo.order == 1 ? singleBondPf : doubleBondPf, originalPosition, Quaternion.identity);
            GameObject bondOb = Instantiate(bondInfo.order == 1 ? singleBondPf : doubleBondPf);
            bondOb.tag = bondTag;
            
            bondOb.transform.Translate(originalPosition);
            bondOb.transform.LookAt(toAtom.transform);
            bondOb.transform.Rotate(new Vector3(1.0f, 0.0f, 0.0f), 90);
            bondOb.transform.SetParent(fromAtom.transform);

            // float finalScale = relBondScale > 0.0001f ? relBondScale : 0.0001f; //Protection against 0 scale
            ScaleBy(bondOb, 1.0f, scale, 1.0f);

            // Set the parameters in the relevant Bond and Atom scripts
            Bond bond = bondOb.GetComponent<Bond>();
            bond.fromAtom = fromAtom;
            bond.toAtom = toAtom;
            bond.order = bondInfo.order;
            bond.originalLength = originalLength;
            bond.bondJoint = AddBondJoint(fromAtom.gameObject, toAtom.gameObject, bondInfo.breakable);
            
            fromAtom.bondsOnThisAtom.Add(bond);
            toAtom.bondsOnThisAtom.Add(bond);

            if (bondInfo.breakable)
            {
                // Give fromAtom and toAtom BondCollisionDetector
                foreach(Transform t in new Transform[2] { fromAtom.transform, toAtom.transform })
                {
                    GameObject bc = Instantiate(bondColliderPf, t);
                    bc.transform.localScale = fromAtom.transform.localScale * 20f; // for some reason the 20x scale down of our atoms needs to be accounted for separately here
                    bc.GetComponent<BondCollisionDetector>().enabled = false; // Since we just made the bond, it doesn't need to be checking for collisions yet
                    bc.GetComponent<BondCollisionDetector>().atom = t.GetComponent<Atom>(); 
                }
            }

            // We want all the bond connections but no visible objects when in van der Waals mode
            if (modelType == ModelType.VanDerWaals)
            {
                // Need to cycle because double bonds have renderers on their children
                foreach (Renderer rend in bondOb.GetComponentsInChildren<Renderer>())
                {
                    rend.enabled = false;
                }
            }

            // Add bond to return lists
            bonds.Add(bond);
            containingMol.AddBond(bond);
        }

        return bonds;
    }

    /**
     * Add a configurable joint between the two bonded atoms.
     */
    private ConfigurableJoint AddBondJoint(GameObject fromAtom, GameObject toAtom, bool breakable)
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

        // cj.angularXMotion = ConfigurableJointMotion.Free;
        // cj.angularXDrive = new JointDrive
        // {
        //     positionSpring = 100f, //10000.0f, // 40.0f,//20.0f
        //     positionDamper = 1,
        //     maximumForce = 100f, //10000.0f, //40.0f // 10.0f
        // };
        // cj.targetRotation = Quaternion.Euler(180 + 0, 0, 0); // default to 0 phi
        if (breakable)
        {
            switch(fromAtom.name)
            {
                case "C5'":
                    cj.breakForce = 500f;
                    break;
                case "C1'":
                    cj.breakForce = 550f;
                    break;
                case "C3'":
                    cj.breakForce = 600f;
                    break;
                default:
                    print("Breakable bond on unexpected atom: " + fromAtom.name);
                    break;
            }
            //cj.breakForce = 500f;
        }

        fromAtom.GetComponent<Rigidbody>().mass = 1;
        fromAtom.GetComponent<Rigidbody>().drag = 5;
        fromAtom.GetComponent<Rigidbody>().angularDrag = 5;

        toAtom.GetComponent<Rigidbody>().mass = 1;
        toAtom.GetComponent<Rigidbody>().drag = 5;
        toAtom.GetComponent<Rigidbody>().angularDrag = 5;

        return cj;
    }
    

    private GameObject ScaleBy(GameObject GO, float xScale, float yScale, float zScale)
    {
        Vector3 localScale = GO.transform.localScale;
        GO.transform.localScale = new Vector3(localScale.x * xScale, localScale.y * yScale, localScale.z * zScale);
        return GO;
    }
}




