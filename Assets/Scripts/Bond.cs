using System;
using UnityEngine;

[Serializable]
public class Bond : MonoBehaviour
{
    public Renderer[] renderers;
    public Material materialFade;

    [Header("Auto-filled")]

    public Atom fromAtom;
    public Atom toAtom;
    public ConfigurableJoint bondJoint;
    
    public int order;
    public bool broken;
    public float originalLength;

    private void Awake()
    {
        broken = false;
    }

    /**
     * Called by the containing molecule each FixedUpdate. Not called in individual update method for
     * performance reasons. 
     */
    public void UpdatePositionAndScale()
    {
        if (!broken)
        {
            //Set position
            Transform trans = transform;
            Vector3 fromPos = fromAtom.transform.position;
            Vector3 toPos = toAtom.transform.position;
            // Actually moved
            if (Vector3.Distance(trans.position, Vector3.Lerp(fromPos, toPos, 0.5f)) > 0.001f)
            {
                trans.position = Vector3.Lerp(fromPos, toPos, 0.5f);

                // Set rotation
                trans.LookAt(toPos);
                trans.RotateAround(trans.position, trans.right, 90);

                // Set scale
                Vector3 bondScale = trans.localScale;
                bondScale.y = (fromPos - toPos).magnitude / 0.1f;
                // = Vector3.Distance(bond.parent.position, bond.target.position) / bond.parent.localScale.x / 2;
                if (Mathf.Abs(trans.localScale.y - bondScale.y) > 0.01f)
                {
                    trans.localScale = bondScale;
                }
            }
        }
    }

    public void OnBondBreak()
    {
        broken = true;
        Destroy(gameObject);
    }
}
