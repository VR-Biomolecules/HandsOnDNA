using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Rigidbody rigid;

    public Quaternion QDiff;
    public Vector3 VDiff;

    public float multiplier;

    public Transform target;

    void FixedUpdate()
    {
        QDiff = target.rotation * Quaternion.Inverse(transform.rotation);
        VDiff = QDiff.eulerAngles;

        Vector3 trueVDiff = new Vector3(A(VDiff.x), A(VDiff.y), A(VDiff.z));

        rigid.angularVelocity = trueVDiff * multiplier;
    }

    float A(float a)
    {
        if (a > 180)
        {
            a -= 360;
        }

        return a;
    }
}
