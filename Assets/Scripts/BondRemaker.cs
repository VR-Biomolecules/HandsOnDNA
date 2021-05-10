using MoleculeTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BondRemaker : MonoBehaviour
{
    private static bool debugging;
    private static GameObject singleBondPf;
    private static GameObject doubleBondPf;
    private static string bondTag;
    private static GameObject aStrandObj;
    private static GameObject bStrandObj;

    public GameObject singleBondPf_shell;
    public GameObject doubleBondPf_shell;
    public  bool debugging_shell;
    public  string bondTag_shell;
    public GameObject aStrandObj_shell;
    public GameObject bStrandObj_shell;

    static public BondRemaker instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        singleBondPf = singleBondPf_shell;
        doubleBondPf = doubleBondPf_shell;
        debugging = debugging_shell;
        bondTag = bondTag_shell;
        aStrandObj = aStrandObj_shell;
        bStrandObj = bStrandObj_shell;
    }
    internal static Bond DrawBond(Atom fromAtom, Atom toAtom, Molecule containingMol, bool breakable, ModelType modelType)
    {
        DataReader.BondInfo bondInfo = new DataReader.BondInfo(1, 0, 1);
        // DataReader.BondInfo bondInfo = new DataReader.BondInfo(1, 1, 2); // TODO Needed for the old prefabs. build a switch
        bondInfo.breakable = breakable;
        return DrawBonds(new List<DataReader.BondInfo>() { bondInfo },
            new List<Atom>() { fromAtom, toAtom }, containingMol, false, modelType)[0];
    }

    /**
 * Draws a list of bonds based on bondInfo. Expects the atoms in atoms to be indexed as the
 * numbers in the bondinfos - 1 (the bonds were deduced from atoms numbered from 1).
 *
 * Also adds configurable joints between bonded atoms
 *
 * containingMol = The Molecule object this bond will be a part of
 */
    private static List<Bond> DrawBonds(List<DataReader.BondInfo> bondInfos, List<Atom> atoms,
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

             GameObject bondOb = Instantiate(bondInfo.order == 1 ? singleBondPf : doubleBondPf, originalPosition, Quaternion.identity);
            //GameObject bondOb = PrefabUtility.InstantiatePrefab(bondInfo.order == 1 ? singleBondPf : doubleBondPf) as GameObject;
            bondOb.tag = bondTag;

            //bondOb.transform.Translate(originalPosition);
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
                foreach (Transform t in new Transform[2] { fromAtom.transform, toAtom.transform })
                {
                    BondCollisionDetector bcd = t.GetComponent<BondCollisionDetector>();
                    if (bcd != null)
                    {
                        bcd.enabled = false; // Since we just made the bond, it doesn't need to be checking for collisions yet
                        bcd.atom = t.GetComponent<Atom>();
                    }
                   
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
    private static GameObject ScaleBy(GameObject GO, float xScale, float yScale, float zScale)
    {
        Vector3 localScale = GO.transform.localScale;
        GO.transform.localScale = new Vector3(localScale.x * xScale, localScale.y * yScale, localScale.z * zScale);
        return GO;
    }
    private static ConfigurableJoint AddBondJoint(GameObject fromAtom, GameObject toAtom, bool breakable)
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
            // switch (fromAtom.name)
            // {
            //     case "C5'":
            //         // cj.breakForce = 500f;
            //         cj.breakForce = 1000f;
            //         break;
            //     case "C1'":
            //         // cj.breakForce = 550f;
            //         cj.breakForce = 1100f;
            //         break;
            //     case "C3'":
            //         // cj.breakForce = 600f;
            //         cj.breakForce = 1200f;
            //         break;
            //     default:
            //         print("Breakable bond on unexpected atom: " + fromAtom.name);
            //         break;
            // }
            cj.breakForce = 2500f;
        }

        fromAtom.GetComponent<Rigidbody>().mass = 1;
        fromAtom.GetComponent<Rigidbody>().drag = 5;
        fromAtom.GetComponent<Rigidbody>().angularDrag = 5;

        toAtom.GetComponent<Rigidbody>().mass = 1;
        toAtom.GetComponent<Rigidbody>().drag = 5;
        toAtom.GetComponent<Rigidbody>().angularDrag = 5;

        return cj;
    }

    public static GameObject ReConnectStrandA(List<Phosphate> smashPhosphates, List<Sugar> smashSugars,
        List<NBase> smashNBases, ModelType type, bool breakable)
    {
        return ReConnectStrand(smashPhosphates, smashSugars, smashNBases, 0, 12, type, breakable, true);
    }

    public static GameObject ReConnectStrandB(List<Phosphate> smashPhosphates, List<Sugar> smashSugars,
        List<NBase> smashNBases, ModelType type, bool breakable)
    {
        return ReConnectStrand(smashPhosphates, smashSugars, smashNBases, 12, 24, type, breakable, false);
    }

    static IEnumerator ResetKinematic(List<Phosphate> smashPhosphates, List<Sugar> smashSugars,
        List<NBase> smashNBases, int start, int end)
    {
        yield return new WaitForFixedUpdate();
        for (int i = start; i < end; i++)
        {
            Phosphate phos = smashPhosphates[i - start];
            Sugar sugar = smashSugars[i - start];
            NBase nBase = smashNBases[i - start];

            foreach (Atom atom in sugar.atoms)
            {
                atom.rigid.velocity = Vector3.zero;
                atom.rigid.angularVelocity = Vector3.zero;
                atom.rigid.isKinematic = false;
                //atom.rigid.Sleep();
            }
            foreach (Atom atom in phos.atoms)
            {
                atom.rigid.velocity = Vector3.zero;
                atom.rigid.angularVelocity = Vector3.zero;
                atom.rigid.isKinematic = false;
                //atom.rigid.Sleep();
            }
            
            foreach (Atom atom in nBase.atoms)
            {
                atom.rigid.velocity = Vector3.zero;
                atom.rigid.angularVelocity = Vector3.zero;
                atom.rigid.isKinematic = false;
                //atom.rigid.Sleep();
            }

            //instance.StopCoroutine("ResetKinematic");

        }
    }

    public static GameObject ReConnectStrand(List<Phosphate> smashPhosphates, List<Sugar> smashSugars,
        List<NBase> smashNBases, int start, int end, ModelType type, bool breakable, bool aStrand)
    {
        GameObject strand = bStrandObj;
        if (aStrand)
        {
             strand = aStrandObj;
        }

        //Debug.Log("Made it here1" + strand.tag);
        DNAStrand strandScript;
        strandScript = strand.GetComponent<DNAStrand>();
        //strandScript.SetGhostMode();
        //Debug.Log("Made it here");
        for (int i = start; i < end; i++)
        {
            Phosphate phos = smashPhosphates[i-start];
            Sugar sugar = smashSugars[i-start];
            NBase nBase = smashNBases[i-start];

            //strandScript.Phosphates.Add(phos);
            //strandScript.Sugars.Add(sugar);
            //strandScript.Bases.Add(nBase);

            // Set the parents
            sugar.transform.SetParent(phos.transform);
            nBase.transform.SetParent(sugar.transform);
            
            if (i - start > 0) phos.transform.SetParent(smashSugars[i-1-start].transform);

            phos.transform.localPosition = Vector3.zero;
            sugar.transform.localPosition = Vector3.zero;
            nBase.transform.localPosition = Vector3.zero;

            foreach (Atom atom in phos.atoms)
            {
                atom.rigid.isKinematic = true;
                atom.transform.position = atom.startPos;
                atom.transform.localRotation = atom.startRot;
            }
            foreach (Atom atom in sugar.atoms)
            {
                atom.rigid.isKinematic = true;
                atom.transform.position = atom.startPos;
                atom.transform.localRotation = atom.startRot;
                
            }
            foreach (Atom atom in nBase.atoms)
            {
                atom.rigid.isKinematic = true;
                atom.transform.position = atom.startPos;
                atom.transform.localRotation = atom.startRot;
            }

            


            // Add the Glycosidic bond between the sugar and the nitrogenous base
            //Bond gBond = DrawBond(sugar.C1Carbon, nBase.bondingNitrogen, sugar, breakable, type);
            //sugar.SetGBond(gBond, nBase);
            //strandScript.gBonds.Add(gBond);


            //if (i > start)
            //{
            //    // Add the 3' phosphodiester bond between last sugar and this phosphate
            //    Sugar lastSugar = smashSugars[i - 1 - start];
            //    Bond ppdeThree = DrawBond(lastSugar.C3Carbon,
            //        phos.threePrimeOxygen, lastSugar, breakable, type);

            //    lastSugar.SetPpdeBond(ppdeThree, phos, false);
            //    if (phos.threePrimeBond == null)
            //    {
            //        Debug.Log("Another bond reformed - three");
            //        strandScript.ppdeBonds.Add(ppdeThree);
            //    }

            //    phos.transform.SetParent(lastSugar.transform);
            //}
            //else
            //{
            //    phos.transform.SetParent(strand.transform);
            //}

            //// Add the 5' phosphodiester bond between the sugar and phosphate
            //Bond ppdeFive = DrawBond(sugar.C5Carbon,
            //    phos.fivePrimeOxygen, sugar, breakable, type);
            //if (phos.threePrimeBond == null)
            //{
            //    Debug.Log("Another bond reformed - five");
            //    sugar.SetPpdeBond(ppdeFive, phos, true);
            //}
            ////strandScript.ppdeBonds.Add(ppdeFive);

           
        }


        for (int i = start; i < end; i++)
        {
            Sugar sugar = smashSugars[i - start];

            if (sugar.threePrimeBond == null && sugar.threePrimePhosphate != null)
            {
                Debug.Log("Bond reset");
                //Three primed bond
                BondRemaker.DrawBond(sugar.C3Carbon, sugar.threePrimePhosphate.threePrimeOxygen, sugar, true, ModelType.BallAndStick);
                //sugar.threePrimeBond.bondJoint.enabled = true;
            }
            if (sugar.fivePrimeBond == null && sugar.fivePrimePhosphate != null)
            {
                Debug.Log("Bond reset");
                //Five primed bond
                BondRemaker.DrawBond(sugar.C5Carbon, sugar.fivePrimePhosphate.fivePrimeOxygen, sugar, true, ModelType.BallAndStick);
                //sugar.threePrimeBond.bondJoint.enabled = true;
            }
            if (sugar.GBond == null && sugar.NBase != null)
            {
                Debug.Log("Bond reset");
                //NBase bond
                BondRemaker.DrawBond(sugar.C1Carbon, sugar.NBase.bondingNitrogen, sugar, true, ModelType.BallAndStick);
                //sugar.threePrimeBond.bondJoint.enabled = true;
            }
           
        }

        instance.StartCoroutine(ResetKinematic(smashPhosphates, smashSugars, smashNBases, start, end));
        

        return strand;
    }



    

}
