using System;
using UnityEngine;
using System.Collections.Generic;

/**
 * This script sits on a child object of an Atom that can bond another atom.
 *
 * When drawing a C1, C3 or C5 sugar atom, this detector is added to the carbon to detect if a bond can be formed.
 * This needs to be inactivated when a bond is active
 */
public class BondCollisionDetector : MonoBehaviour
{
    public Atom atom;

    public BondReformer bondReformer;
    public bool isOtherReformTarget;

    [System.Serializable] 
    public struct GlowInfo 
    {
        public Color color;
        public float thickness;

        public float pulseAmplitude;
        public float pulsePeriod;
    }
    public GlowInfo[] glowInfos;
    int glowIndex = 2;

    public SphereCollider sphereCollider;
    public Outline glow;
    public Renderer render;

    // List of the BondCollisionDetectors within range this frame
    List<BondCollisionDetector> bcdsInRangeOfMe = new List<BondCollisionDetector>();
    List<bool> bondables = new List<bool>();

    internal bool grabbed;

    float pulseTime;

    private void OnEnable()
    {
        sphereCollider.enabled = true;
        
        // Check list of bcds still valid
        // for (int i = bcdsInRangeOfMe.Count - 1; i >= 0; i--)
        // {
        //     BondCollisionDetector bcd = bcdsInRangeOfMe[i];
        //     if (Vector3.Distance(bcd.transform.position, transform.position) > 0.15)
        //     {
        //         bcdsInRangeOfMe.RemoveAt(i);
        //         RemoveBondable(bcd);
        //     }
        // }

        if (bondReformer && !isOtherReformTarget) bondReformer.Invoke("CanReform", 1f);
    }
    

    private void FixedUpdate()
    {
        if (!glow)
        {
            return;
        }

        #region #OPTIMISE

        if (atom.parentMol.connectMolHeld)
        // if (atom.parentMol.isBeingHeld)
        {
            if (!grabbed)
            {
                grabbed = true;

                foreach(BondCollisionDetector bcd in bcdsInRangeOfMe)
                {
                    AddBondable(bcd);
                }

                CheckHighlightColour();
            }
        }
        else if (grabbed)
        {
            foreach (BondCollisionDetector bcd in bcdsInRangeOfMe)
            {
                RemoveBondable(bcd, true);
            }

            grabbed = false;

            CheckHighlightColour();
        }

        #endregion

        GlowInfo glowInfo = glowInfos[glowIndex];

        // Material needs to pulse
        if (glowInfo.pulsePeriod != 0 && glow.enabled)
        {
            // Elapse
            pulseTime = (pulseTime + Time.fixedDeltaTime) % glowInfo.pulsePeriod;

            // Set glow thickness
            glow.OutlineWidth = glowInfo.thickness + (glowInfo.pulseAmplitude * Mathf.Sin((pulseTime * 2 * Mathf.PI) / glowInfo.pulsePeriod));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        BondCollisionDetector bcd = other.GetComponent<BondCollisionDetector>();

        if (enabled && bcd && bcd.enabled && bcd.atom && bcd.atom.parentMol != atom.parentMol)
        {
            bcdsInRangeOfMe.Add(bcd);

            if (grabbed)
            {
                AddBondable(bcd);
            }
        }
    }

    void AddBondable(BondCollisionDetector bcd)
    {
        bondables.Add(AtomsShouldBond(transform.parent.name, bcd.transform.parent.name));
        CheckHighlightColour();

        if (!bcd.grabbed)
        {
            bcd.AddBondable(this);
            bcd.CheckHighlightColour();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BondCollisionDetector bcd = other.GetComponent<BondCollisionDetector>();

        if (enabled && bcd && bcd.enabled && bcd.atom && bcd.atom.parentMol != atom.parentMol)
        {
            bcdsInRangeOfMe.Remove(bcd);

            if (grabbed)
            {
                RemoveBondable(bcd, true);
            }
        }
    }

    void RemoveBondable(BondCollisionDetector bcd, bool removeThisFromOtherBcd)
    {
        bondables.Remove(AtomsShouldBond(transform.parent.name, bcd.transform.parent.name));
        CheckHighlightColour();

        if (!bcd.grabbed && removeThisFromOtherBcd)
        {
            bcd.RemoveBondable(this, false);
            bcd.CheckHighlightColour();
        }
    }

    public static bool AtomsShouldBond(string name, string otherAtomName)
    {
        switch (name)
        {
            case "C1'":
                return otherAtomName.Equals("N1") || otherAtomName.Equals("N9");
            case "C5'":
                return otherAtomName.Equals("O5'");
            case "C3'":
                return otherAtomName.Equals("O3'");


            case "N1":
                return otherAtomName.Equals("C1'");
            case "N9":
                return otherAtomName.Equals("C1'");
            case "O5'":
                return otherAtomName.Equals("C5'");
            case "O3'":
                return otherAtomName.Equals("C3'");
        }
        return false;
    }

    void CheckHighlightColour()
    {
        if (!glow)
        {
            return;
        }

        // Enable glow
        if (!glow.enabled && (grabbed || bondables.Count > 0))
        {
            glow.enabled = true;
            render.enabled = true;
        }

        // Has a valid bond to try form
        if (bondables.Contains(true))
        {
            // Set to green
            SetGlowColour(0);

            // Enable reformer
            if (bondReformer && !isOtherReformTarget && !bondReformer.gameObject.activeSelf) bondReformer.gameObject.SetActive(true);
        }
        else
        {
            // Disable reformer
            if (bondReformer && !isOtherReformTarget && bondReformer.gameObject.activeSelf) bondReformer.gameObject.SetActive(false);

            // Has only invalid bonds to try form
            if (bondables.Count > 0)
            {
                // Set to red
                SetGlowColour(1);
            }
            else // Has no bonds to try form
            {
                // Set to yellow
                SetGlowColour(2);

                // Disable glow
                if (!grabbed && glow.enabled)
                {
                    glow.enabled = false;
                    render.enabled = false;
                }
            }
        }
    }

    void SetGlowColour(int newIndex)
    {
        if (glowIndex == newIndex)
        {
            return;
        }

        glowIndex = newIndex;
        GlowInfo info = glowInfos[glowIndex];

        glow.OutlineColor = info.color;
        glow.OutlineWidth = info.thickness;
    }

    public void Disable()
    {
        for (int i = bcdsInRangeOfMe.Count-1; i > -1; i--)
        {
            BondCollisionDetector bcd = bcdsInRangeOfMe[i];

            bcdsInRangeOfMe.Remove(bcd);
            RemoveBondable(bcd, false); //this removes it from my list and also me from its list
        }

        if (bondReformer && !isOtherReformTarget) bondReformer.gameObject.SetActive(false);

        glow.enabled = false;
        render.enabled = false;
        sphereCollider.enabled = false;
        enabled = false;
    }
}
