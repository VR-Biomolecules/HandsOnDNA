using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BondVisualiser : MonoBehaviour
{
    // List of bonds
    public List<BondDetails> bonds = new List<BondDetails>();

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        foreach (BondDetails bond in bonds)
        {
            // Set position between parent and target
            bond.bond.position = Vector3.Lerp(bond.parent.position, bond.target.position, 0.5f);

            // Set rotation
            bond.bond.LookAt(bond.target);
            bond.bond.RotateAround(bond.bond.position, bond.bond.right, 90);

            // Set scale
            Vector3 bondScale = bond.bond.localScale;
            bondScale.y = (Vector3.Distance(bond.parent.position, bond.target.position) / bond.parent.localScale.x / 2);
            bond.bond.localScale = bondScale;
        }
    }
}

[System.Serializable]
public class BondDetails
{
    // Bond object
    public Transform bond;
    // Parent of the bond object
    /*[HideInInspector]*/ public Transform parent;
    // The target object the bond is meant to be connected to
    public Transform target;

    /*
    public BondDetails(Transform newBond, Transform newTarget)
    {
        bond = newBond;
        parent = bond.parent;
        target = newTarget;
    }
    */
}
