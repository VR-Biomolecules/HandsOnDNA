using System;
using System.Collections.Generic;
using MoleculeTypes;


/*
 * Nitrogenous Base class.
 * Holds handle the bond between it and the backbone, and the N that it attaches at.
 */
public class NBase : Molecule
{
    public Atom bondingNitrogen; // Nitrogen that attaches to backbone. N1 if pyrimidine (GC) and N9 if purine (AT)
    public Bond gBond; // The glycosidic bond between this NBase and its Sugar
    public Sugar sugar; // The Sugar unit this base connects to
    public BaseType baseType;

    public void SetBaseType(BaseType baseType)
    {
        this.baseType = baseType;
    }

    public override void IdentifyKeyAtoms(bool smashModel)
    {
        bondingNitrogen = atoms[0];
        baseType = DetectBase(name);
    }

    /**
     * Defer to sugar, or else this mol is alone
     */
    public override void UpdateConnectedMolStatus()
    {
        if (sugar) sugar.UpdateConnectedMolStatus();
        else connectedMols = new List<Molecule> {this};
    }

    public static BaseType DetectBase(string text)
    {
        if (text.Contains(baseNames[0]) || text.Equals("A", StringComparison.Ordinal)) return BaseType.ADENINE;
        if (text.Contains(baseNames[1]) || text.Equals("T", StringComparison.Ordinal)) return BaseType.THYMINE;
        if (text.Contains(baseNames[2]) || text.Equals("G", StringComparison.Ordinal)) return BaseType.GUANINE;
        if (text.Contains(baseNames[3]) || text.Equals("C", StringComparison.Ordinal)) return BaseType.CYTOSINE;
        return BaseType.UNKNOWN;
    }

    public static string GetBaseName(BaseType type)
    {
        switch (type)
        {
            case BaseType.ADENINE:
                return baseNames[0];
            case BaseType.THYMINE:
                return baseNames[1];
            case BaseType.GUANINE:
                return baseNames[2];
            case BaseType.CYTOSINE:
                return baseNames[3];
            default:
                return baseNames[4];
        }
    }

    public static string[] baseNames = {
        "adenine", 
        "thymine", 
        "guanine",
        "cytosine",
        "unknown"
    };

    [Serializable]
    public enum BaseType
    {
        ADENINE,
        THYMINE,
        GUANINE,
        CYTOSINE,
        UNKNOWN
    }
}
