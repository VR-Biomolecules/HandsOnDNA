using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus;
using System;
public class CustomGrabbable : OVRGrabbable
{
    [Header("Custom")]

    [Tooltip("Leave blank to automatically find all outlines on gameobject.")]
    public Outline[] outlines;
    public bool toggleOutlineRenderers;
    internal int potentialGrabbers;

    [Space]

    public bool snapHandToThis;
    public Vector3 snapHandOffset;

    public override void Awake()
    {
        if (m_grabPoints == null || m_grabPoints.Length == 0)
        {
            // Get the collider from the grabbable
            Collider collider = this.GetComponent<Collider>();
            if (collider == null)
            {
                throw new ArgumentException("Grabbables cannot have zero grab points and no collider -- please add a grab point or collider.");
            }

            // Create a default grab point
            m_grabPoints = new Collider[1] { collider };
        }

        if (outlines != null && outlines.Length == 0 )
        {
            outlines = GetComponentsInChildren<Outline>();
        }
    }

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        base.GrabBegin(hand, grabPoint);
        Player.Vibrate(0.1f, 0.4f, hand.GetComponent<CustomGrabber>(), true);
        if (outlines != null) { 
            foreach (Outline o in outlines)
            {
                o.enabled = false;
                if (toggleOutlineRenderers) o.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        base.GrabEnd(linearVelocity, angularVelocity);
        // Player.Vibrate(0.2f, 0.5f, grabbedBy.GetComponent<CustomGrabber>());
        if (outlines != null)
        {
            foreach (Outline o in outlines)
            {
                if (toggleOutlineRenderers) o.GetComponent<Renderer>().enabled = true;
                o.enabled = true;
            }
        }
    }

    public void GrabberChange(int change)
    {
        potentialGrabbers += change;

        if (outlines != null && !isGrabbed )
        {
            foreach (Outline o in outlines)
            {
                if (toggleOutlineRenderers && (potentialGrabbers > 0)) o.GetComponent<Renderer>().enabled = true;
                o.enabled = (potentialGrabbers > 0);
                if (toggleOutlineRenderers && (potentialGrabbers == 0)) o.GetComponent<Renderer>().enabled = false;
            }
        }
    }
    virtual public void CustomGrabCollider(Collider collider)
    {
        m_grabPoints = new Collider[1] { collider };
    }
}
