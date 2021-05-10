using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultitargetBoxDetector : BoxDetector
{
    public TextMeshProUGUI text;
    public Image image;
    
    // Start is called before the first frame update
    public void DisplayMolecule(MultitargetBoxDetectorManager.MolDisplayData mol)
    {
        text.text = mol.label;
        image.sprite = mol.image;

        Transform molTransform = mol.data.mol.transform;
        molTransform.localPosition = molTransform.localPosition + mol.data.pos + GetBoxAdjustment();
        molTransform.rotation *= Quaternion.Euler(mol.data.rot);

        // Stop the molecules from moving
        foreach (Atom molAtom in mol.data.mol.atoms)
        {
            molAtom.rigid.velocity = Vector3.zero;
            molAtom.rigid.isKinematic = true;
            molAtom.GetComponent<OVRGrabbable>().enabled = false;
        }

        text.gameObject.SetActive(true);
        image.gameObject.SetActive(true);
        mol.data.mol.gameObject.SetActive(true);
    }

    private Vector3 GetBoxAdjustment()
    {
        int boxNumber = (int) char.GetNumericValue(name[3]);
        // The boxes are numbered 1 -> 6, top left to bottom left, then top right to bottom right
        // All coords are for box 1, so if it's above 3 we need to take a step to the right, and if 
        // it's 2,3,5,6 we also need to go down.
        float x = boxNumber >= 4 ? 0.45f : 0; // 4,5,6 are to the right
        float y = ((boxNumber - 1) % 3) * -0.45f + (boxNumber == 1 || boxNumber == 4 ? 0.28f : 0); // Boxes are stacked at 0.45 intervals, except the top two which are higher
        return new Vector3(x, y, -0.05f);
    }

    public void DisplayMolecule(MultitargetBoxDetectorManager.MolAndOrientationInBox1 newMol)
    {
        Transform molTransform = newMol.mol.transform;
        molTransform.localPosition = molTransform.localPosition + newMol.pos + GetBoxAdjustment();
        molTransform.rotation *= Quaternion.Euler(newMol.rot);
        newMol.mol.gameObject.SetActive(true);
    }
}
