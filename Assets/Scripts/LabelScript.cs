using MoleculeTypes;
using TMPro;
using UnityEngine;

/**
 * Adds a hanging label over a molecule or gameobject. Make it a child of the central most atom.
 * If the gameobject is not a molecule, make sure to override the label using labelOverride
 */
public class LabelScript : MonoBehaviour
{
    public TextMeshPro labelComp;
    public string labelText;
    public Molecule mol;
    public Transform player;

    public string labelOverride;
    public float xOffsetOverride = 0;
    public float yOffsetOverride = 0;
    public float zOffsetOverride = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        if (transform.localScale.x > 2)
        {
            transform.localScale = Vector3.one;
        }

        if (labelOverride == "")
        {
            mol = transform.parent.GetComponent<Atom>().parentMol;
            labelText = GetLabelText(mol.name);
        }
        else
        {
            labelText = labelOverride;
        }

        labelComp = GetComponent<TextMeshPro>();
        labelComp.text = labelText;

        player = Player.instance.transform.GetChild(0).GetChild(4);
    }

    private string GetLabelText(string molName)
    {
        if (molName.Contains("phosphate")) return "Phosphate";
        if (molName.Contains("sugar")) return "Deoxyribose\nSugar";
        if (molName.Contains("base"))
        {
            if (molName.Contains("adenine")) return "Adenine (A)";
            if (molName.Contains("guanine")) return "Guanine (G)";
            if (molName.Contains("thymine")) return "Thymine (T)";
            if (molName.Contains("cytosine")) return "Cytosine (C)";
        }
        return "Unknown";
    }

    void Update()
    {
        transform.position = transform.parent.position + new Vector3(xOffsetOverride < 0 ? xOffsetOverride : 0, yOffsetOverride > 0 ? yOffsetOverride : 0.15f, zOffsetOverride < 0 ? zOffsetOverride : 0);
        transform.rotation = Quaternion.LookRotation(transform.position - player.position);
    }
}
