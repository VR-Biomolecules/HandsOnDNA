using System;
using System.Collections.Generic;
using System.IO;
using MoleculeTypes;
using UnityEditor;
using UnityEngine;

public class PrefabGenerator : MonoBehaviour
{ 
    private static bool scriptInitialised = false;

    // Basic pfs for atoms and bonds
    public static GameObject atomPf;
    public static GameObject singleBondPf;
    public static GameObject doubleBondPf;
    public static GameObject bondColliderPf; // Collider used to detect when bondable atoms come into radius
    public static GameObject reformerPf;
    public static GameObject labellerPf;

    // Empty objects with molecule scripts
    public static GameObject emptySugarPf;
    public static GameObject emptyPhosphatePf;
    public static GameObject emptyNBasePf;
    public static GameObject dnaStrandPf;
    public static GameObject dnaHelixPf;

    // Materials for the atoms
    public static Material carbonMat;
    public static Material hydrogenMat;
    public static Material nitrogenMat;
    public static Material oxygenMat;
    public static Material phosphorusMat;
    public static Material sulfurMat;
    public static Material unknownMat;
    // Faded materials for atoms (when in "ghost mode")
    public static Material carbonMatFade;
    public static Material hydrogenMatFade;
    public static Material nitrogenMatFade;
    public static Material oxygenMatFade;
    public static Material phosphorusMatFade;
    public static Material sulfurMatFade;
    public static Material unknownMatFade;

    public static string atomTag = "atom";
    public static string bondTag = "bond";

    public static bool debugging;

    // van der Waals radii for the VDW representation. Our default game space is 1 unit = 20 Angstroms = 2000 pm.
    // (original coords were 1 unit = 1 Angstrom, then we scaled down by 20)
    // Therefore an H atom with atomic radii 120 pm = 1.2 Angstroms = 0.06 units in game (1.2 / 20)
    private static float vdwRadiusHydrogen = 0.06f;
    private static float vdwRadiusCarbon = 0.085f;
    private static float vdwRadiusNitrogen = 0.0775f;
    private static float vdwRadiusOxygen = 0.076f;
    private static float vdwRadiusPhosphorus = 0.09f;
    private static float atomPfBaseRadius = 0.5f * 0.05f; // The original atom object has radius 0.5 units, scaled down by 20 again

    private static void InitialiseScript() //todo need to make this happen before the menu actions are called. TURN this into setup script to find assets. MAKE ALL STATIC
    {
        atomPf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("AtomPf", new[] {"Assets/Prefabs"})[0]));
        singleBondPf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("SingleBondPf", new[] {"Assets/Prefabs"})[0]));
        doubleBondPf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("DoubleBondPf", new[] {"Assets/Prefabs"})[0]));
        bondColliderPf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("BondColliderPf", new[] {"Assets/Prefabs"})[0]));
        reformerPf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("BondReformerPf", new[] {"Assets/Prefabs"})[0]));
        labellerPf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("LabellerPf", new[] {"Assets/Prefabs"})[0]));

        // Empty objects with molecule scripts
        emptySugarPf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("EmptySugarPf", new[] {"Assets/Prefabs/EmptyMolecules"})[0]));
        emptyPhosphatePf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("EmptyPhosphatePf", new[] {"Assets/Prefabs/EmptyMolecules"})[0]));
        emptyNBasePf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("EmptyNBasePf", new[] {"Assets/Prefabs/EmptyMolecules"})[0]));
        dnaStrandPf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("DNAStrandPf", new[] {"Assets/Prefabs/EmptyMolecules"})[0]));
        dnaHelixPf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("DNADoubleHelixPf", new[] {"Assets/Prefabs/EmptyMolecules"})[0]));
    
        // Materials for the atoms
        carbonMat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("CarbonMat", new[] {"Assets/Materials/Atoms"})[0]));
        hydrogenMat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("HydrogenMat", new[] {"Assets/Materials/Atoms"})[0]));
        nitrogenMat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("NitrogenMat", new[] { "Assets/Materials/Atoms" })[0]));
        oxygenMat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("OxygenMat", new[] { "Assets/Materials/Atoms" })[0]));
        phosphorusMat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("PhosphorusMat", new[] { "Assets/Materials/Atoms" })[0]));
        sulfurMat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("SulfurMat", new[] { "Assets/Materials/Atoms" })[0]));
        unknownMat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("UnknownMat", new[] { "Assets/Materials/Atoms" })[0]));

        carbonMatFade = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("CarbonFade", new[] { "Assets/Materials/Atoms/Fade" })[0]));
        hydrogenMatFade = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("HydrogenFade", new[] { "Assets/Materials/Atoms/Fade" })[0]));
        nitrogenMatFade = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("NitrogenFade", new[] { "Assets/Materials/Atoms/Fade" })[0]));
        oxygenMatFade = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("OxygenFade", new[] { "Assets/Materials/Atoms/Fade" })[0]));
        phosphorusMatFade = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("PhosphorusFade", new[] { "Assets/Materials/Atoms/Fade" })[0]));
        sulfurMatFade = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("SulfurFade", new[] { "Assets/Materials/Atoms/Fade" })[0]));
        unknownMatFade = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("UnknownFade", new[] { "Assets/Materials/Atoms/Fade" })[0]));
    }

    [MenuItem("Prefab Maker/Make Helix Portion")]
    static void MakeHelixPortion()
    {
        if (!scriptInitialised) InitialiseScript();

        List<Phosphate> smashPhosphates = GenerateSmashModelPhosphates(ModelType.BallAndStick);
        List<Sugar> smashSugars = GenerateSmashModelSugars(ModelType.BallAndStick);
        List<NBase> smashNBases = GenerateSmashModelNBases(ModelType.BallAndStick);

        List<Phosphate> phosphates = new List<Phosphate>();
        List<Sugar> sugars = new List<Sugar>();
        List<NBase> nBases = new List<NBase>();

        for (int i = 19; i > 15; i--)
        {
            phosphates.Add(smashPhosphates[i]);
            sugars.Add(smashSugars[i]);
            nBases.Add(smashNBases[i]);
            
            smashPhosphates.RemoveAt(i);
            smashSugars.RemoveAt(i);
            smashNBases.RemoveAt(i);
        }

        foreach (Phosphate phosphate in smashPhosphates)
        {
            DestroyImmediate(phosphate.gameObject);
        }

        foreach (Sugar sugar in smashSugars)
        {
            DestroyImmediate(sugar.gameObject);
        }

        foreach (NBase smashNBase in smashNBases)
        {
            DestroyImmediate(smashNBase.gameObject);
        }
        
        phosphates.Reverse();
        sugars.Reverse();
        nBases.Reverse();

        GameObject strand =
            ConnectStrand(phosphates, sugars, nBases, 0, 4, ModelType.BallAndStick, true);
        
        GameObject helix = PrefabUtility.InstantiatePrefab(dnaHelixPf) as GameObject;
        strand.transform.SetParent(helix.transform);
        
        DNADoubleHelix helixScript = helix.GetComponent<DNADoubleHelix>();
        DNAStrand strandScript = strand.GetComponent<DNAStrand>();
        helixScript.strandA = strandScript;
        
        // Find all breakable bonds in breakable strand
        List<Bond> breakableBonds = new List<Bond>();
        foreach (Sugar sugar in sugars)
        {
            if (sugar.fivePrimeBond != null) breakableBonds.Add(sugar.fivePrimeBond);
            if (sugar.threePrimeBond != null) breakableBonds.Add(sugar.threePrimeBond);
            if (sugar.GBond != null) breakableBonds.Add(sugar.GBond);
        }
        
        // Foreach breakable bond in breakable strand
        foreach (Bond b in breakableBonds)
        {
            // Forceably break
            // b.bondJoint.brea
            b.fromAtom.CleanUpJointBreak(b);
            b.OnBondBreak();
        }
    }
    
    [MenuItem("Prefab Maker/Make Complementary Helix Portion")]
    static void MakeComplementaryHelixPortion()
    {
        if (!scriptInitialised) InitialiseScript();

        List<Phosphate> smashPhosphates = GenerateSmashModelPhosphates(ModelType.BallAndStick);
        List<Sugar> smashSugars = GenerateSmashModelSugars(ModelType.BallAndStick);
        List<NBase> smashNBases = GenerateSmashModelNBases(ModelType.BallAndStick);

        List<Phosphate> phosphates = new List<Phosphate>();
        List<Sugar> sugars = new List<Sugar>();
        List<NBase> nBases = new List<NBase>();

        for (int i = 19; i > 15; i--)
        {
            phosphates.Add(smashPhosphates[i]);
            sugars.Add(smashSugars[i]);
            nBases.Add(smashNBases[i]);
            
            smashPhosphates.RemoveAt(i);
            smashSugars.RemoveAt(i);
            smashNBases.RemoveAt(i);
        }

        phosphates.Reverse();
        sugars.Reverse();
        nBases.Reverse();

        GameObject strand = ConnectStrand(phosphates, sugars, nBases, 0, 4, ModelType.BallAndStick, true);
        
        GameObject strandA = ConnectStrand(smashPhosphates, smashSugars, smashNBases, 0, 12, ModelType.BallAndStick, true);
        GameObject strandB = ConnectStrand(smashPhosphates, smashSugars, smashNBases, 12, 16, ModelType.BallAndStick, true);
        GameObject strandC = ConnectStrand(smashPhosphates, smashSugars, smashNBases, 16, 20, ModelType.BallAndStick, true);
        
        GameObject helix = PrefabUtility.InstantiatePrefab(dnaHelixPf) as GameObject;
        strand.transform.SetParent(helix.transform);
        
        DNADoubleHelix helixScript = helix.GetComponent<DNADoubleHelix>();
        helixScript.strandA = strand.GetComponent<DNAStrand>();
        
        GameObject helix2 = PrefabUtility.InstantiatePrefab(dnaHelixPf) as GameObject;
        strandA.transform.SetParent(helix2.transform);
        strandB.transform.SetParent(helix2.transform);
        strandC.transform.SetParent(helix2.transform);

        DNADoubleHelix helixScript2 = helix2.GetComponent<DNADoubleHelix>();
        helixScript2.strandA = strandA.GetComponent<DNAStrand>();
        helixScript2.strandB = strandB.GetComponent<DNAStrand>();

        strandA.GetComponent<DNAStrand>().SetGhostMode(true);
        strandB.GetComponent<DNAStrand>().SetGhostMode(true);
        strandC.GetComponent<DNAStrand>().SetGhostMode(true);
    }

    [MenuItem("Prefab Maker/Test Ball and Stick")]
    static void TestBallAndStick()
    {
        if (!scriptInitialised) InitialiseScript();
        
        // Make the Smashable prefabs from base coords and place them in the scene at 0,0,0, then link them
        List<Phosphate> smashPhosphates = GenerateSmashModelPhosphates(ModelType.BallAndStick);
        List<Sugar> smashSugars = GenerateSmashModelSugars(ModelType.BallAndStick);
        List<NBase> smashNBases = GenerateSmashModelNBases(ModelType.BallAndStick);
        
        GameObject strandA = ConnectStrandA(smashPhosphates, smashSugars, smashNBases, ModelType.BallAndStick, true);
        GameObject strandB = ConnectStrandB(smashPhosphates, smashSugars, smashNBases, ModelType.BallAndStick, true);
        
        GameObject helix = PrefabUtility.InstantiatePrefab(dnaHelixPf) as GameObject;
        strandA.transform.SetParent(helix.transform);
        strandB.transform.SetParent(helix.transform);
        
        DNADoubleHelix helixScript = helix.GetComponent<DNADoubleHelix>();
        helixScript.strandA = strandA.GetComponent<DNAStrand>();
        helixScript.strandB = strandB.GetComponent<DNAStrand>();
    }
    
    [MenuItem("Prefab Maker/Test van der Waals")]
    static void TestVanDerWaals()
    {
        if (!scriptInitialised) InitialiseScript();
        
        // Make the Smashable prefabs from base coords and place them in the scene at 0,0,0, then link them
        List<Phosphate> smashPhosphates = GenerateSmashModelPhosphates(ModelType.VanDerWaals);
        List<Sugar> smashSugars = GenerateSmashModelSugars(ModelType.VanDerWaals);
        List<NBase> smashNBases = GenerateSmashModelNBases(ModelType.VanDerWaals);
        
        GameObject strandA = ConnectStrandA(smashPhosphates, smashSugars, smashNBases, ModelType.VanDerWaals, false);
        GameObject strandB = ConnectStrandB(smashPhosphates, smashSugars, smashNBases, ModelType.VanDerWaals, false);
        
        GameObject helix = PrefabUtility.InstantiatePrefab(dnaHelixPf) as GameObject;
        strandA.transform.SetParent(helix.transform);
        strandB.transform.SetParent(helix.transform);
        
        DNADoubleHelix helixScript = helix.GetComponent<DNADoubleHelix>();
        helixScript.strandA = strandA.GetComponent<DNAStrand>();
        helixScript.strandB = strandB.GetComponent<DNAStrand>();
        
        AddToVdwAtomsLayer(helixScript);
    }
    
    public static void AddToVdwAtomsLayer(DNADoubleHelix helix)
    {
        foreach (Transform trans in helix.strandA.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = 12; // Add to VDW layer 1
        }
        
        foreach (Transform trans in helix.strandB.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = 13; // Add to VDW layer 2
        }
    }

    [MenuItem("Prefab Maker/Save Ball And Stick")]
    static void SaveBallAndStick()
    {
        if (!scriptInitialised) InitialiseScript();
        
        string prefix = "Assets/Prefabs/DNAModel/BallAndStick/";
        string suffix = "_bas.prefab";
        
        // Make the whole Smashable model, link it and save it to Assets
        List<Phosphate> smashPhosphates = GenerateSmashModelPhosphates(ModelType.BallAndStick);
        List<Sugar> smashSugars = GenerateSmashModelSugars(ModelType.BallAndStick);
        List<NBase> smashNBases = GenerateSmashModelNBases(ModelType.BallAndStick);
        
        foreach (Phosphate phosphate in smashPhosphates)
        {
            GameObject phosphateGameObject = phosphate.gameObject;
            PrefabUtility.SaveAsPrefabAsset(phosphateGameObject, prefix + "Phosphates/" + phosphateGameObject.name + suffix);
        }

        foreach (Sugar sugar in smashSugars)
        {
            GameObject sugarGameObject = sugar.gameObject;
            PrefabUtility.SaveAsPrefabAsset(sugarGameObject, prefix + "Sugars/" + sugarGameObject.name + suffix);
        }

        foreach (NBase nBase in smashNBases)
        {
            GameObject nbaseGameObject = nBase.gameObject;
            PrefabUtility.SaveAsPrefabAsset(nbaseGameObject, prefix + "NBases/" + nbaseGameObject.name + suffix);
        }
        
        GameObject strandA = ConnectStrandA(smashPhosphates, smashSugars, smashNBases, ModelType.BallAndStick, true);
        GameObject strandB = ConnectStrandB(smashPhosphates, smashSugars, smashNBases, ModelType.BallAndStick, true);
        
        GameObject helix = PrefabUtility.InstantiatePrefab(dnaHelixPf) as GameObject;
        strandA.transform.SetParent(helix.transform);
        strandB.transform.SetParent(helix.transform);
        
        DNADoubleHelix helixScript = helix.GetComponent<DNADoubleHelix>();
        helixScript.strandA = strandA.GetComponent<DNAStrand>();
        helixScript.strandB = strandB.GetComponent<DNAStrand>();

        // string prefix = $"Assets/Prefabs/SmashModel/{(testing ? "Test/" : "Molecules/")}";
        // string suffix = $"{(testing ? "_test" : "")}.prefab";
        
        GameObject strandAOb = strandA.gameObject;
        PrefabUtility.SaveAsPrefabAsset(strandAOb, prefix + "strandA" + suffix);
            
        GameObject strandBOb = strandB.gameObject;
        PrefabUtility.SaveAsPrefabAsset(strandBOb, prefix + "strandB" + suffix);
            
        GameObject helixOb = helix.gameObject;
        PrefabUtility.SaveAsPrefabAsset(helixOb, prefix + "DNAHelix" + suffix);
    }
    
    [MenuItem("Prefab Maker/Save van der Waals")]
    static void SaveVanDerWaals()
    {
        if (!scriptInitialised) InitialiseScript();
        
        string prefix = "Assets/Prefabs/DNAModel/vanderWaals/";
        string suffix = "_vdw.prefab";
        
        // Make the whole Smashable model, link it and save it to Assets
        List<Phosphate> smashPhosphates = GenerateSmashModelPhosphates(ModelType.VanDerWaals);
        List<Sugar> smashSugars = GenerateSmashModelSugars(ModelType.VanDerWaals);
        List<NBase> smashNBases = GenerateSmashModelNBases(ModelType.VanDerWaals);
        
        foreach (Phosphate phosphate in smashPhosphates)
        {
            GameObject phosphateGameObject = phosphate.gameObject;
            PrefabUtility.SaveAsPrefabAsset(phosphateGameObject, prefix + "Phosphates/" + phosphateGameObject.name + suffix);
        }

        foreach (Sugar sugar in smashSugars)
        {
            GameObject sugarGameObject = sugar.gameObject;
            PrefabUtility.SaveAsPrefabAsset(sugarGameObject, prefix + "Sugars/" + sugarGameObject.name + suffix);
        }

        foreach (NBase nBase in smashNBases)
        {
            GameObject nbaseGameObject = nBase.gameObject;
            PrefabUtility.SaveAsPrefabAsset(nbaseGameObject, prefix + "NBases/" + nbaseGameObject.name + suffix);
        }
        
        GameObject strandA = ConnectStrandA(smashPhosphates, smashSugars, smashNBases, ModelType.VanDerWaals, false);
        GameObject strandB = ConnectStrandB(smashPhosphates, smashSugars, smashNBases, ModelType.VanDerWaals, false);
        
        GameObject helix = PrefabUtility.InstantiatePrefab(dnaHelixPf) as GameObject;
        strandA.transform.SetParent(helix.transform);
        strandB.transform.SetParent(helix.transform);
        
        DNADoubleHelix helixScript = helix.GetComponent<DNADoubleHelix>();
        helixScript.strandA = strandA.GetComponent<DNAStrand>();
        helixScript.strandB = strandB.GetComponent<DNAStrand>();

        AddToVdwAtomsLayer(helixScript);

        GameObject strandAOb = strandA.gameObject;
        PrefabUtility.SaveAsPrefabAsset(strandAOb, prefix + "strandA" + suffix);
            
        GameObject strandBOb = strandB.gameObject;
        PrefabUtility.SaveAsPrefabAsset(strandBOb, prefix + "strandB" + suffix);
            
        GameObject helixOb = helix.gameObject;
        PrefabUtility.SaveAsPrefabAsset(helixOb, prefix + "DNAHelix" + suffix);
    }
    
    [MenuItem("Prefab Maker/Make Phosphates")]
    static void OnlyPhosphates()
    {
        if (!scriptInitialised) InitialiseScript();
        List<Phosphate> phosphates = GenerateSmashModelPhosphates(ModelType.BallAndStick);
    }
    
    [MenuItem("Prefab Maker/Make Sugars")]
    static void OnlySugars()
    {
        if (!scriptInitialised) InitialiseScript();
        List<Sugar> sugars = GenerateSmashModelSugars(ModelType.BallAndStick);
    }
    
    [MenuItem("Prefab Maker/Make NBases")]
    static void OnlyNBases()
    {
        if (!scriptInitialised) InitialiseScript();
        List<NBase> nBases = GenerateSmashModelNBases(ModelType.BallAndStick);
    }

    private static GameObject ConnectStrandA(List<Phosphate> smashPhosphates, List<Sugar> smashSugars,
        List<NBase> smashNBases, ModelType type, bool breakable)
    {
        return ConnectStrand(smashPhosphates, smashSugars, smashNBases, 0, 12, type, breakable);
    }

    private static GameObject ConnectStrandB(List<Phosphate> smashPhosphates, List<Sugar> smashSugars,
        List<NBase> smashNBases, ModelType type, bool breakable)
    {
        return ConnectStrand(smashPhosphates, smashSugars, smashNBases, 12, 24, type, breakable);
    }

    private static GameObject ConnectStrand(List<Phosphate> smashPhosphates, List<Sugar> smashSugars,
        List<NBase> smashNBases, int start, int end, ModelType type, bool breakable)
    {
        GameObject strand = PrefabUtility.InstantiatePrefab(dnaStrandPf) as GameObject;
        DNAStrand strandScript = strand.GetComponent<DNAStrand>();

        for (int i = start; i < end; i++)
        {
            Phosphate phos = smashPhosphates[i];
            Sugar sugar = smashSugars[i];
            NBase nBase = smashNBases[i];
            
            strandScript.Phosphates.Add(phos);
            strandScript.Sugars.Add(sugar);
            strandScript.Bases.Add(nBase);
            phos.strand = strandScript;
            sugar.strand = strandScript;
            nBase.strand = strandScript;

            // Add the Glycosidic bond between the sugar and the nitrogenous base
            Bond gBond = DrawBond(sugar.C1Carbon, nBase.bondingNitrogen, sugar, breakable, type);
            sugar.SetGBond(gBond, nBase);
            //strandScript.gBonds.Add(gBond);


            if (i > start)
            {
                // Add the 3' phosphodiester bond between last sugar and this phosphate
                Sugar lastSugar = smashSugars[i - 1];
                Bond ppdeThree = DrawBond(lastSugar.C3Carbon,
                    phos.threePrimeOxygen, lastSugar, breakable, type);

                lastSugar.SetPpdeBond(ppdeThree, phos, false);
                //strandScript.ppdeBonds.Add(ppdeThree);
                
                // phos.transform.SetParent(lastSugar.transform);
            }
            

            // Add the 5' phosphodiester bond between the sugar and phosphate
            Bond ppdeFive = DrawBond(sugar.C5Carbon,
                phos.fivePrimeOxygen, sugar, breakable, type);

            sugar.SetPpdeBond(ppdeFive, phos, true);
            //strandScript.ppdeBonds.Add(ppdeFive);

            // Set the parents
            phos.transform.SetParent(strand.transform);
            sugar.transform.SetParent(strand.transform);
            nBase.transform.SetParent(strand.transform);
        }
        
        smashSugars[0].UpdateConnectedMolStatus();

        return strand;
    }

    public static List<Phosphate> GenerateSmashModelPhosphates(ModelType type)
    {
        List<Phosphate> toReturn = new List<Phosphate>();
        foreach (DataReader.MoleculeInfo phosInfo in GetSmashModelPhosphateInfos())
        {
            GameObject baseOb = PrefabUtility.InstantiatePrefab(emptyPhosphatePf) as GameObject;
            baseOb.name = phosInfo.name;
            Phosphate phosphate = baseOb.GetComponent<Phosphate>();
            phosphate.ModelType = type;
            InstantiateAtomsAndBonds(phosInfo, phosphate, type);
            phosphate.IdentifyKeyAtoms(true);
            phosphate.labeller = AddLabeller(phosphate.atoms[0]);
            phosphate.labelRenderer = phosphate.labeller.GetComponent<MeshRenderer>();
            toReturn.Add(phosphate);
        }

        return toReturn;
    }

    public static List<Sugar> GenerateSmashModelSugars(ModelType type)
    {
        List<Sugar> toReturn = new List<Sugar>();
        foreach (DataReader.MoleculeInfo sugarInfo in GetSmashModelSugarInfos())
        {
            GameObject baseOb = PrefabUtility.InstantiatePrefab(emptySugarPf) as GameObject;
            baseOb.name = sugarInfo.name;
            Sugar sugar = baseOb.GetComponent<Sugar>();
            sugar.ModelType = type;
            InstantiateAtomsAndBonds(sugarInfo, sugar, type);
            sugar.IdentifyKeyAtoms(true);
            sugar.labeller = AddLabeller(sugar.atoms[3]);
            sugar.labelRenderer = sugar.labeller.GetComponent<MeshRenderer>();
            toReturn.Add(sugar);
        }

        return toReturn;
    }

    public static List<NBase> GenerateSmashModelNBases(ModelType type)
    {
        List<NBase> toReturn = new List<NBase>();
        foreach (DataReader.MoleculeInfo nbaseInfo in GetSmashModelNBaseInfos())
        {
            GameObject baseOb = PrefabUtility.InstantiatePrefab(emptyNBasePf) as GameObject;
            baseOb.name = nbaseInfo.name;
            NBase nBase = baseOb.GetComponent<NBase>();
            nBase.ModelType = type;
            InstantiateAtomsAndBonds(nbaseInfo, nBase, type);
            nBase.IdentifyKeyAtoms(true);
            nBase.labeller = AddLabeller(nBase.atoms[GetLabelAtom(nBase.baseType)]);
            nBase.labelRenderer = nBase.labeller.GetComponent<MeshRenderer>();
            toReturn.Add(nBase);
        }

        return toReturn;
    }

    private static int GetLabelAtom(NBase.BaseType type)
    {
        switch (type)
        {
            case NBase.BaseType.ADENINE:
            case NBase.BaseType.GUANINE:
                return 4;
            case NBase.BaseType.THYMINE:
                return 8;
            case NBase.BaseType.CYTOSINE:
                return 5;
        }

        return 0;
    }

    /**
 * Given a full molInfo and a base molecule, draws all of the atoms and bonds represented
 * by it. 
 */
    private static void InstantiateAtomsAndBonds(DataReader.MoleculeInfo molInfo, Molecule mol, ModelType modelType)
    {
        List<Atom> atoms = DrawAtoms(molInfo.atoms, mol, modelType);
        DrawBonds(molInfo.bonds, atoms, mol, false, modelType);
    }

    /**
 * Draws a list of atoms based on atomInfo and a parent molecule's position
 */
    public static List<Atom> DrawAtoms(List<DataReader.AtomInfo> atomInfos, Molecule mol, ModelType modelType)
    {
        List<Atom> atoms = new List<Atom>();
        foreach (DataReader.AtomInfo atomInfo in atomInfos)
        {
            // if (modelType == ModelType.VanDerWaals && atomInfo.atomType == Atom.AtomType.Hydrogen)
            // {
            //     atoms.Add(null);
            //     continue;
            // }
            // GameObject atom = Instantiate(atomPf, atomInfo.coords, Quaternion.identity, mol.transform);
            GameObject atom = PrefabUtility.InstantiatePrefab(atomPf, mol.transform) as GameObject;
            Atom atomScript = atom.GetComponent<Atom>();

            ScaleBy(atom, GetAtomScale(atomInfo.atomType, modelType));
            ApplyAtomMaterial(atomInfo.atomType, atomScript);

            atom.transform.Translate(mol.transform.position + atomInfo.coords);
            atom.name = atomInfo.name;
            atom.tag = atomTag;
            atomScript.label = atomInfo.name;
            atomScript.type = atomInfo.atomType;
            atomScript.parentMol = mol;

            mol.AddAtom(atomScript);
            atoms.Add(atomScript);
        }

        return atoms;
    }

    private static float GetAtomScale(Atom.AtomType atomType, ModelType modelType)
    {
        if (modelType == ModelType.BallAndStick) return atomType != Atom.AtomType.Hydrogen ? 0.8f : 0.6f;
        if (modelType == ModelType.VanDerWaals)
        {
            switch (atomType)
            {
                case Atom.AtomType.Hydrogen:
                    return vdwRadiusHydrogen / atomPfBaseRadius;
                case Atom.AtomType.Carbon:
                    return vdwRadiusCarbon / atomPfBaseRadius;
                case Atom.AtomType.Nitrogen:
                    return vdwRadiusNitrogen / atomPfBaseRadius;
                case Atom.AtomType.Oxygen:
                    return vdwRadiusOxygen / atomPfBaseRadius;
                case Atom.AtomType.Phosphorus:
                    return vdwRadiusPhosphorus / atomPfBaseRadius;
            }
        }

        return 1.0f;
    }

    /**
 * Draw a single bond between two atoms, and add it to the given molecule's bond list
 */
    internal static Bond DrawBond(Atom fromAtom, Atom toAtom, Molecule containingMol, bool breakable, ModelType modelType)
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

            // GameObject bondOb = Instantiate(bondInfo.order == 1 ? singleBondPf : doubleBondPf, originalPosition, Quaternion.identity);
            GameObject bondOb = PrefabUtility.InstantiatePrefab(bondInfo.order == 1 ? singleBondPf : doubleBondPf) as GameObject;
        
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
                foreach(Atom a in new Atom[2] { fromAtom, toAtom })
                {
                    a.bcd = (PrefabUtility.InstantiatePrefab(bondColliderPf, a.transform) as GameObject).GetComponent<BondCollisionDetector>();
                    if (modelType == ModelType.BallAndStick)
                    {
                        a.bcd.transform.localScale = fromAtom.transform.localScale * 20f; // for some reason the 20x scale down of our atoms needs to be accounted for separately here
                    }
                    a.bcd.enabled = false; // Since we just made the bond, it doesn't need to be checking for collisions yet
                    a.bcd.atom = a; 
                }


                // Give fromAtom reformer
                fromAtom.bcd.bondReformer = (PrefabUtility.InstantiatePrefab(reformerPf) as GameObject).GetComponent<BondReformer>();
                int atom1Index = 0;
                int atom2Index = 1;
                if (toAtom.parentMol.GetComponent<NBase>())
                {
                    switch(toAtom.parentMol.GetComponent<NBase>().baseType)
                    {
                        case NBase.BaseType.ADENINE:
                            atom1Index = 1;
                            atom2Index = 13;
                            break;
                        case NBase.BaseType.CYTOSINE:
                            atom1Index = 1;
                            atom2Index = 10;
                            break;
                        case NBase.BaseType.GUANINE:
                            atom1Index = 1;
                            atom2Index = 14;
                            break;
                        case NBase.BaseType.THYMINE:
                            atom1Index = 1;
                            atom2Index = 12;
                            break;
                    }
                }

                BondReformer.Initialise(fromAtom.bcd.bondReformer, fromAtom, toAtom, 
                    toAtom.parentMol.atoms[atom1Index].transform.position, toAtom.parentMol.atoms[atom2Index].transform.position);

                // Give reference to reformer on toAtom
                toAtom.bcd.bondReformer = fromAtom.bcd.bondReformer;
                toAtom.bcd.isOtherReformTarget = true;
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
     * Adds a gameobject to an atom that will label a molecule when held.
     */
    private static LabelScript AddLabeller(Atom toLabel)
    {
        GameObject labeller = PrefabUtility.InstantiatePrefab(labellerPf) as GameObject;
        Transform parent = toLabel.transform;
        labeller.transform.localScale = Vector3.one;
        labeller.transform.position = parent.position;
        labeller.transform.SetParent(parent);
        return labeller.GetComponent<LabelScript>();
    }

    /**
 * Sets the atom-specific material and tag on the atom GameObject
 */
    internal static void ApplyAtomMaterial(Atom.AtomType type, Atom atom)
    {
        Material material;
        Material materialFade;

        switch (type)
        {
            case Atom.AtomType.Carbon:
                material = carbonMat;
                materialFade = carbonMatFade;
                break;
            case Atom.AtomType.Hydrogen:
                material = hydrogenMat;
                materialFade = hydrogenMatFade;
                break;
            case Atom.AtomType.Nitrogen:
                material = nitrogenMat;
                materialFade = nitrogenMatFade;
                break;
            case Atom.AtomType.Oxygen:
                material = oxygenMat;
                materialFade = oxygenMatFade;
                break;
            case Atom.AtomType.Phosphorus:
                material = phosphorusMat;
                materialFade = phosphorusMatFade;
                break;
            case Atom.AtomType.Sulfur:
                material = sulfurMat;
                materialFade = sulfurMatFade;
                break;
            default:
                material = unknownMat;
                materialFade = unknownMatFade;
                break;
        }

        atom.material = material;
        atom.materialFade = materialFade;
        foreach(Renderer r in atom.renderers)
        {
            r.material = material;
        }
    }

    /**
 * Add a configurable joint between the two bonded atoms.
 */
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
            // switch(fromAtom.name)
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


    private static GameObject ScaleBy(GameObject GO, float xScale, float yScale, float zScale)
    {
        Vector3 localScale = GO.transform.localScale;
        GO.transform.localScale = new Vector3(localScale.x * xScale, localScale.y * yScale, localScale.z * zScale);
        return GO;
    }

    private static GameObject ScaleBy(GameObject GO, float scale)
    {
        return ScaleBy(GO, scale, scale, scale);
    }

    // GameObject threePrimeO1 = null;
    // GameObject threePrimeO2 = null;
    /**
 * Writes the names and 3D coords of all child objects into the steam writer.
 * Used to print all atom coords from a transformed prefab into a new data file
 */
    internal void PrintAtomCoords(GameObject ob, StreamWriter writer)
    {
        Debug.Log("Printing coords for atoms in pf: " + ob.name);
        for (int i = 0; i < ob.transform.childCount; i++)
        {
            Transform child = ob.transform.GetChild(i);
            string message = child.name + "\t" + child.position.ToString("F5");
            if (writer != null)
            {
                writer.WriteLine(message);
            }
            else
            {
                Debug.Log(message);
            }
            if (child.childCount > 0) PrintAtomCoords(child.gameObject, writer);
        }
    }
    
    public static List<DataReader.MoleculeInfo> GetSmashModelPhosphateInfos()
    {
        List<DataReader.MoleculeInfo> molInfos = new List<DataReader.MoleculeInfo>();
        string[] phosphateGUIDS = AssetDatabase.FindAssets("phosphate", new[] {"Assets/Data/SmashModelCoordinates/Phosphates"});
        foreach (string guid in phosphateGUIDS)
        {
            molInfos.Add(DataReader.ReadCsvCoords(AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guid)), false));
        }

        return molInfos;
    }

    public static List<DataReader.MoleculeInfo> GetSmashModelSugarInfos()
    {
        List<DataReader.MoleculeInfo> molInfos = new List<DataReader.MoleculeInfo>();
        string[] sugarGUIDS = AssetDatabase.FindAssets("sugar", new[] {"Assets/Data/SmashModelCoordinates/Sugars"});
        foreach (string guid in sugarGUIDS)
        {
            molInfos.Add(DataReader.ReadCsvCoords(AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guid)), false));
        }

        return molInfos;
    }

    public static List<DataReader.MoleculeInfo> GetSmashModelNBaseInfos()
    {
        List<DataReader.MoleculeInfo> molInfos = new List<DataReader.MoleculeInfo>();
        string[] baseGUIDS = AssetDatabase.FindAssets("base", new[] {"Assets/Data/SmashModelCoordinates/Bases"});
        foreach (string guid in baseGUIDS)
        {
            molInfos.Add(DataReader.ReadCsvCoords(AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guid)), false));
        }

        return molInfos;
    }
    
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /**
     * I began to implement these before the holidays, then realised I probably won't use the old model any more.
     * I need to make static methods for reading the datafiles in the DataReader. This was from the old prefabgenerator
     * that had an instanced datareader.
     */
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    
    [MenuItem("Prefab Maker/Test Generated Model (BaS)")]
    static void TestBallAndStickOld()
    {
        if (!scriptInitialised) InitialiseScript();
        MakeAndSaveGeneratedModel(ModelType.BallAndStick, true, false);
    }

    [MenuItem("Prefab Maker/Save Generated Model (BaS)")]
    static void SaveBallAndStickOld()
    {
        if (!scriptInitialised) InitialiseScript();
        MakeAndSaveGeneratedModel(ModelType.BallAndStick, false, true);

    }

    [MenuItem("Prefab Maker/Test Generated Model (VDW)")]
    static void TestVanDerWaalsOld()
    {
        if (!scriptInitialised) InitialiseScript();
        MakeAndSaveGeneratedModel(ModelType.VanDerWaals, true, false);

    }

    [MenuItem("Prefab Maker/Save Generated Model (VDW)")]
    static void SaveVanDerWaalsOld()
    {
        if (!scriptInitialised) InitialiseScript();
        MakeAndSaveGeneratedModel(ModelType.VanDerWaals, false, false);
    }
    
    public static void MakeAndSaveGeneratedModel(ModelType toGenerate, bool testing, bool saveToAssets)
    {
        // Make the old style, constant transformation prefabs (with messed up second strand)

        // if (saveToAssets)
            // {
            //     string prefix = $"Assets/Prefabs/GeneratingModel/{(testing ? "Test/" : "Molecules/")}{GetStrandTypePrefix(toGenerate)}";
            //     string suffix = $"{(testing ? "_test" : "")}.prefab";
            //
            //     Phosphate phosphateOb = GeneratePhosphatePf(Vector3.zero, toGenerate);
            //     PrefabUtility.SaveAsPrefabAsset(phosphateOb.gameObject, $"{prefix}PhosphatePf{suffix}");
            //     Destroy(phosphateOb);
            //
            //     Sugar sugarOb = GenerateSugarPf(Vector3.zero, toGenerate);
            //     PrefabUtility.SaveAsPrefabAsset(sugarOb.gameObject, $"{prefix}SugarPf{suffix}");
            //     Destroy(sugarOb);
            //
            //     NBase adenineOb = GenerateBasePf(NBase.BaseType.ADENINE, Vector3.zero, null, toGenerate);
            //     PrefabUtility.SaveAsPrefabAsset(adenineOb.gameObject, $"{prefix}AdeninePf{suffix}");
            //     Destroy(adenineOb);
            //
            //     NBase cytosineOb = GenerateBasePf(NBase.BaseType.CYTOSINE, Vector3.zero, null, toGenerate);
            //     PrefabUtility.SaveAsPrefabAsset(cytosineOb.gameObject, $"{prefix}CytosinePf{suffix}");
            //     Destroy(cytosineOb);
            //
            //     NBase guanineOb = GenerateBasePf(NBase.BaseType.GUANINE, Vector3.zero, null, toGenerate);
            //     PrefabUtility.SaveAsPrefabAsset(guanineOb.gameObject, $"{prefix}GuaninePf{suffix}");
            //     Destroy(guanineOb);
            //
            //     NBase thymineOb = GenerateBasePf(NBase.BaseType.THYMINE, Vector3.zero, null, toGenerate);
            //     PrefabUtility.SaveAsPrefabAsset(thymineOb.gameObject, $"{prefix}ThyminePf{suffix}");
            //     Destroy(thymineOb);
            // }
            // else
            // {
            //     Phosphate phosphateOb = GeneratePhosphatePf(Vector3.zero, toGenerate);
            //     Sugar sugarOb = GenerateSugarPf(Vector3.zero, toGenerate);
            //     NBase adenineOb = GenerateBasePf(NBase.BaseType.ADENINE, Vector3.forward, null, toGenerate);
            //     NBase cytosineOb = GenerateBasePf(NBase.BaseType.CYTOSINE, Vector3.forward * 2, null, toGenerate);
            //     NBase guanineOb = GenerateBasePf(NBase.BaseType.GUANINE, Vector3.forward * 3, null, toGenerate);
            //     NBase thymineOb = GenerateBasePf(NBase.BaseType.THYMINE, Vector3.forward * 4, null, toGenerate);
            // }
    }
    
        /**
 * Draws a single nitrogenous base from original data positions. No prefabs 
 */
    // public NBase GenerateBasePf(NBase.BaseType baseType, Vector3 offset, Sugar attachedSugar, ModelType modelType)
    // {
    //     DataReader.MoleculeInfo molInfo;
    //     string baseName;
    //     switch (baseType)
    //     {
    //         case NBase.BaseType.ADENINE:
    //             molInfo = dataReader.GetAdenineInfo();
    //             baseName = NBase.baseNames[0];
    //             break;
    //         case NBase.BaseType.CYTOSINE:
    //             molInfo = dataReader.GetCytosineInfo();
    //             baseName = NBase.baseNames[1];
    //             break;
    //         case NBase.BaseType.GUANINE:
    //             molInfo = dataReader.GetGuanineInfo();
    //             baseName = NBase.baseNames[2];
    //             break;
    //         case NBase.BaseType.THYMINE:
    //             molInfo = dataReader.GetThymineInfo();
    //             baseName = NBase.baseNames[3];
    //             break;
    //         default:
    //             throw new ArgumentOutOfRangeException(nameof(baseType), baseType, null);
    //     }
    //
    //     GameObject baseOb;
    //
    //     baseOb = PrefabUtility.InstantiatePrefab(emptyNBasePf) as GameObject;
    //     baseOb.transform.Translate(attachedSugar == null ? offset : attachedSugar.transform.position);
    //     baseOb.name = baseName;
    //
    //     NBase mol = baseOb.GetComponent<NBase>();
    //     mol.ModelType = modelType;
    //     mol.SetBaseType(baseType);
    //
    //     InstantiateAtomsAndBonds(molInfo, mol, modelType);
    //     mol.IdentifyKeyAtoms(false);
    //
    //     if (modelType == ModelType.VanDerWaals)
    //     {
    //         AddToVdwAtomsLayer(baseOb); // VDW atoms layer, which don't collide with each other.
    //     }
    //
    //     return mol;
    // }
    //
    // public Phosphate GeneratePhosphatePf(Vector3 offset, ModelType modelType)
    // {
    //     GameObject baseOb = PrefabUtility.InstantiatePrefab(emptyPhosphatePf) as GameObject;
    //     baseOb.transform.position = offset;
    //     baseOb.name = "Phosphate";
    //     DataReader.MoleculeInfo phosphateInfo = dataReader.GetPhosphateInfo();
    //     Phosphate phosphate = baseOb.GetComponent<Phosphate>();
    //     phosphate.ModelType = modelType;
    //     InstantiateAtomsAndBonds(phosphateInfo, phosphate, modelType);
    //     phosphate.IdentifyKeyAtoms(false);
    //
    //     if (modelType == ModelType.VanDerWaals)
    //     {
    //         AddToVdwAtomsLayer(baseOb); // VDW atoms layer, which don't collide with each other.
    //     }
    //
    //     return phosphate;
    // }
    //
    // public Sugar GenerateSugarPf(Vector3 offset, ModelType modelType)
    // {
    //     GameObject baseOb = PrefabUtility.InstantiatePrefab(emptySugarPf) as GameObject;
    //     baseOb.transform.position = offset;
    //     baseOb.name = "Deoxyribose Sugar";
    //     DataReader.MoleculeInfo backboneInfo = dataReader.GetSugarInfo();
    //     Sugar sugar = baseOb.GetComponent<Sugar>();
    //     sugar.ModelType = modelType;
    //     InstantiateAtomsAndBonds(backboneInfo, sugar, modelType);
    //     sugar.IdentifyKeyAtoms(false);
    //
    //     if (modelType == ModelType.VanDerWaals)
    //     {
    //         AddToVdwAtomsLayer(baseOb); // VDW atoms layer, which don't collide with each other.
    //     }
    //
    //     return sugar;
    // }
    //

}

