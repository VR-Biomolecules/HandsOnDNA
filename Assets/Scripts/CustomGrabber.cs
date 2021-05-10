using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus;
using OVRTouchSample;
using UnityEngine.SceneManagement;

public class CustomGrabber : OVRGrabber
{
    [Header("Custom")]

    public Transform pcOffset;
    public Transform snapToObjectOffset;
    public Hand hand;

    CustomGrabbable closestGrabbable;
    Collider closestGrabbableCollider = null;
    float closestMagSq;

    Transform originalParent;

    bool locked;
    private double safetyDistance = 0.15; // if a potential grabbable is outside this radius, ignore it

    private void FixedUpdate()
    {
        if (m_grabbedObj) return;

        // Find closest grabbable
        if (m_grabCandidates.Count > 0)
        {
            CustomGrabbable prevClosest = closestGrabbable;

            closestMagSq = float.MaxValue;
            closestGrabbable = null;
            closestGrabbableCollider = null;

            // Iterate grab candidates and find the closest grabbable candidate
            foreach (CustomGrabbable grabbable in m_grabCandidates.Keys)
            {
                if (!grabbable || !grabbable.gameObject.activeSelf || (grabbable.GetComponent<Atom>() && Vector3.Distance(grabbable.transform.position, transform.position) > safetyDistance))
                {
                    m_grabCandidates.Clear();
                    break;
                }

                bool canGrab = grabbable.enabled && !(grabbable.isGrabbed && !grabbable.allowOffhandGrab);
                if (!canGrab)
                {
                    continue;
                }

                for (int j = 0; j < grabbable.grabPoints.Length; ++j)
                {
                    Collider grabbableCollider = grabbable.grabPoints[j];
                    // Store the closest grabbable
                    Vector3 closestPointOnBounds = grabbableCollider.ClosestPointOnBounds(m_gripTransform.position);
                    float grabbableMagSq = (m_gripTransform.position - closestPointOnBounds).sqrMagnitude;
                    if (grabbableMagSq < closestMagSq)
                    {
                        closestMagSq = grabbableMagSq;
                        closestGrabbable = grabbable;
                        closestGrabbableCollider = grabbableCollider;
                    }
                }
            }

            // Update glow
            if (prevClosest != closestGrabbable)
            {
                if (prevClosest) prevClosest.GrabberChange(-1);
                if (closestGrabbable && closestGrabbable.gameObject.activeSelf) closestGrabbable.GrabberChange(1); //todo sometimes this NPE's when i inactivate a molecule
            }
        }
        else if (closestGrabbable)
        {
            if (closestGrabbable.gameObject.activeSelf) closestGrabbable.GrabberChange(-1);

            closestGrabbable = null;
            closestGrabbableCollider = null;
        }
    }

    protected override void GrabBegin()
    {
        if (!closestGrabbable || !closestGrabbable.gameObject.activeSelf || 
            (closestGrabbable.grabbedBy && closestGrabbable.grabbedBy.GetComponent<CustomGrabber>().locked)) return;

        // Disable grab volumes to prevent overlaps
        GrabVolumeEnable(false);

        if (closestGrabbable != null)
        {
            Atom atom = closestGrabbable.GetComponent<Atom>();
            if (atom)
            {
                Debug.Log($"GrabBegin\t{gameObject.name}\t{closestGrabbable.name}\t{atom.parentMol.name}\t{Time.timeSinceLevelLoad}");
            }
            else
            {
                Debug.Log($"GrabBegin\t{gameObject.name}\t{closestGrabbable.name}\t{Time.timeSinceLevelLoad}");
            }
            
            if (closestGrabbable.isGrabbed)
            {
                closestGrabbable.grabbedBy.OffhandGrabbed(closestGrabbable);
            }

            m_grabbedObj = closestGrabbable;
            m_grabbedObj.GrabBegin(this, closestGrabbableCollider);

            m_lastPos = transform.position;
            m_lastRot = transform.rotation;

            // Set up offsets for grabbed object desired position relative to hand.
            if (m_grabbedObj.snapPosition)
            {
                //m_grabbedObjectPosOff = m_gripTransform.position;
                //m_grabbedObjectPosOff = m_gripTransform.localPosition;
                m_grabbedObjectPosOff = transform.InverseTransformPoint(m_gripTransform.position);

                if (m_grabbedObj.snapOffset)
                {
                    Vector3 snapOffset = m_grabbedObj.snapOffset.localPosition;
                    if (m_controller == OVRInput.Controller.LTouch) snapOffset.x = -snapOffset.x;
                    m_grabbedObjectPosOff += snapOffset;
                }
            }
            else
            {
                Vector3 relPos = m_grabbedObj.transform.position - transform.position;
                relPos = Quaternion.Inverse(transform.rotation) * relPos;
                m_grabbedObjectPosOff = relPos;
            }

            if (m_grabbedObj.snapOrientation)
            {
                //m_grabbedObjectRotOff = m_gripTransform.rotation;
                //m_grabbedObjectRotOff = m_gripTransform.localRotation;
                m_grabbedObjectRotOff = Quaternion.LookRotation(transform.InverseTransformDirection(m_gripTransform.forward), transform.InverseTransformDirection(m_gripTransform.up));

                if (m_grabbedObj.snapOffset)
                {
                    m_grabbedObjectRotOff = m_grabbedObj.snapOffset.localRotation * m_grabbedObjectRotOff;
                }
            }
            else
            {
                Quaternion relOri = Quaternion.Inverse(transform.rotation) * m_grabbedObj.transform.rotation;
                m_grabbedObjectRotOff = relOri;
            }

            // NOTE: force teleport on grab, to avoid high-speed travel to dest which hits a lot of other objects at high
            // speed and sends them flying. The grabbed object may still teleport inside of other objects, but fixing that
            // is beyond the scope of this demo.
            MoveGrabbedObject(m_lastPos, m_lastRot, true);

            // NOTE: This is to get around having to setup collision layers, but in your own project you might
            // choose to remove this line in favor of your own collision layer setup.
            SetPlayerIgnoreCollision(m_grabbedObj.gameObject, true);

            if (m_parentHeldObject)
            {
                originalParent = m_grabbedObj.transform.parent;
                m_grabbedObj.transform.parent = transform;
            }

            if (closestGrabbable.snapHandToThis)
            {
                snapToObjectOffset.position = closestGrabbable.transform.position;
                if (m_controller == OVRInput.Controller.RTouch) snapToObjectOffset.localPosition += closestGrabbable.snapHandOffset;
                else snapToObjectOffset.localPosition += new Vector3(-closestGrabbable.snapHandOffset.x, closestGrabbable.snapHandOffset.y, closestGrabbable.snapHandOffset.z);
            }
        }
    }

    protected override void GrabEnd()
    {
        if (!GetLocked())
        {
            if (m_grabbedObj != null)
            {
                Atom atom = m_grabbedObj.GetComponent<Atom>();
                if (atom)
                {
                    Debug.Log(
                        $"GrabEnd\t{gameObject.name}\t{m_grabbedObj.name}\t{atom.parentMol.name}\t{Time.timeSinceLevelLoad}");
                }
                else
                {
                    Debug.Log(
                        $"GrabEnd\t{gameObject.name}\t{m_grabbedObj.name}\t{Time.timeSinceLevelLoad}");
                }
            }
            base.GrabEnd();
            snapToObjectOffset.localPosition = Vector3.zero;
        }
    }

    protected override void GrabbableRelease(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        m_grabbedObj.GrabEnd(linearVelocity, angularVelocity);
        if (m_parentHeldObject)
        {
            m_grabbedObj.transform.parent = originalParent;

            if (!originalParent) SceneManager.MoveGameObjectToScene(m_grabbedObj.gameObject, SceneManager.GetActiveScene());
        }
        m_grabbedObj = null;
    }

    public override void OnTriggerEnter(Collider otherCollider)
    {
        // Get the grab trigger
        CustomGrabbable grabbable = otherCollider.GetComponent<CustomGrabbable>() ?? otherCollider.GetComponentInParent<CustomGrabbable>();
        if (!grabbable) return;

        // Add the grabbable
        int refCount = 0;
        m_grabCandidates.TryGetValue(grabbable, out refCount);
        m_grabCandidates[grabbable] = refCount + 1;
    }

    public override void OnTriggerExit(Collider otherCollider)
    {
        CustomGrabbable grabbable = otherCollider.GetComponent<CustomGrabbable>() ?? otherCollider.GetComponentInParent<CustomGrabbable>();
        if (!grabbable) return;

        // Remove the grabbable
        int refCount = 0;
        bool found = m_grabCandidates.TryGetValue(grabbable, out refCount);
        if (!found)
        {
            return;
        }

        if (refCount > 1)
        {
            m_grabCandidates[grabbable] = refCount - 1;
        }
        else
        {
            m_grabCandidates.Remove(grabbable);
        }
    }

    internal void SetLocked(bool value)
    {
        locked = value;

        if (!locked && m_prevFlex <= grabEnd)
        {
            GrabEnd();
        }
    }

    internal bool GetLocked()
    {
        return locked;
    }
}
