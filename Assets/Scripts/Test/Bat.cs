using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bat : MonoBehaviour
{
    public static Bat instance;

    public Transform batTransform;
    Vector3 startPos;
    Quaternion startRot;

    public float maxVelocity = 1;
    public float angularForceMultipler = 1;
    public float maxAngVelocity = 1;

    public AnimationCurve posMultiplierByDistance;
    public AnimationCurve rotMultiplierByDistance;
    
    CustomGrabber leftHand;
    CustomGrabber rightHand;

    Rigidbody rigid;
    
    void Awake()
    {
        instance = this;

        leftHand = Player.hands[0];
        rightHand = Player.hands[1];

        rigid = GetComponent<Rigidbody>();

        var transform1 = transform;
        startPos = transform1.position;
        startRot = transform1.rotation;
    }

    public void ResetToStart()
    {
        // if (BatHeldInfo().Item1) BatHeldInfo().Item2.ForceRelease(); todo need to release from hand
        var transform1 = transform;
        transform1.position = startPos;
        transform1.rotation = startRot;

        batTransform.position = startPos;
        batTransform.rotation = startRot;
    }

    void FixedUpdate()
    {
        if (BatHeldInfo().Item1) // If bat is being held
        {
            rigid.velocity = (batTransform.position - transform.position) * posMultiplierByDistance.Evaluate(Vector3.Distance(batTransform.position, transform.position));

            Vector3 VDiff = (batTransform.rotation * Quaternion.Inverse(transform.rotation)).eulerAngles;
            Vector3 trueVDiff = new Vector3(Wrap(VDiff.x), Wrap(VDiff.y), Wrap(VDiff.z));

            rigid.angularVelocity = trueVDiff * rotMultiplierByDistance.Evaluate(trueVDiff.magnitude);

            transform.rotation = Quaternion.Lerp(transform.rotation, batTransform.rotation, 0.05f);
        }
        else
        {
             if (transform.position.y < -0.8f)
             {
                 // Debug.Log(transform.position);
                 ResetToStart();
             }
        }
    }

    /**
     * Keep velocity between -180 and 180
     */
    float Wrap(float a)
    {
        if (a > 180)
        {
            a -= 360;
        }

        return a;
    }

    private void OnTriggerEnter(Collider col)
    {
        // Hit Atom
        if (col.transform.GetComponent<Atom>())
        {
            Vector3 force = rigid.velocity * Mathf.Min(1, maxVelocity / rigid.velocity.magnitude);
            force *= Mathf.Max(1, Mathf.Min(1, maxAngVelocity / rigid.angularVelocity.magnitude) * angularForceMultipler);
            col.transform.GetComponent<Atom>().parentMol.GetComponent<MoleculeBehaviourManager>().ApplyBatForce(force);
            
            // Create haptic pulse in the hand holding the bat.
            var (isHeld, hand) = BatHeldInfo();
            if (isHeld)
            {
                Player.Vibrate(0.2f, 1.0f, hand, true);
            }
        }
    }

    private (bool, CustomGrabber) BatHeldInfo()
    {
        Transform batParent = batTransform.parent;

        if (batParent)
        {
            if (batParent.Equals(leftHand.transform))
            {
                return (true, leftHand);
            }
            if (batParent.Equals(rightHand.transform))
            {
                return (true, rightHand);
            }
        }

        return (false, null);
    }
}
