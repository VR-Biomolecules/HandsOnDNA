using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DomTest : MonoBehaviour
{
    public bool align;

    public Transform otherSugarAtom;
    public Transform other1;
    public Transform other2;
    public Transform other3;

    public Transform originalSugarAtom;
    public Transform original1;
    public Transform original2;
    public Transform original3;

    Vector3 otherDir1;
    Vector3 otherDir2;
    Vector3 otherDir3;

    Vector3 originalDir1;
    Vector3 originalDir2;
    Vector3 originalDir3;

    Quaternion dif1;
    Quaternion dif2;

    #region Old 1

    /*
    private void Start()
    {
        otherDir1 = other1.transform.position - otherSugarAtom.position;
        otherDir2 = InverseTransformPoint(otherSugarAtom.position, Quaternion.LookRotation(otherDir1), new Vector3(1, 1, 1), other2.position)
                     - InverseTransformPoint(otherSugarAtom.position, Quaternion.LookRotation(otherDir1), new Vector3(1, 1, 1), other1.position);


        originalDir1 = original1.position - originalSugarAtom.position;
        originalDir2 = InverseTransformPoint(originalSugarAtom.position, Quaternion.LookRotation(originalDir1), new Vector3(1, 1, 1), original2.position)
                     - InverseTransformPoint(originalSugarAtom.position, Quaternion.LookRotation(originalDir1), new Vector3(1, 1, 1), original1.position);

        dif1 = Quaternion.FromToRotation(otherDir1.normalized, originalDir1.normalized);

        dif2 = Quaternion.LookRotation(originalDir2.normalized, Vector3.up) * Quaternion.Inverse(Quaternion.LookRotation(otherDir2.normalized, Vector3.up));
        dif2Eular = new Vector3(dif2.eulerAngles.x, dif2.eulerAngles.y, dif2.eulerAngles.z);
    }

    private void Update()
    {
        other1.position = originalSugarAtom.TransformPoint(originalDir1);

        other1.rotation = Quaternion.FromToRotation(otherDir1.normalized, Vector3.forward);

    }

    Vector3 InverseTransformPoint(Vector3 transforPos, Quaternion transformRotation, Vector3 transformScale, Vector3 pos)
    {
        Matrix4x4 matrix = Matrix4x4.TRS(transforPos, transformRotation, transformScale);
        Matrix4x4 inverse = matrix.inverse;
        return inverse.MultiplyPoint3x4(pos);
    }
    */

    #endregion

    #region Old 2
    /*
    private void Start()
    {
        otherDir1 = other1.transform.position - otherSugarAtom.position;
        originalDir1 = original1.position - originalSugarAtom.position;

        dif1 = Quaternion.FromToRotation(otherDir1.normalized, originalDir1.normalized);
    }

    private void Update()
    {
        if (align)
        {
            Quaternion originalRotation = original1.rotation;
            original1.rotation = Quaternion.identity;

            other1.position = original1.position;
            other1.rotation = originalSugarAtom.rotation * dif1;

            otherDir2 = Vector3.Lerp(other2.position, other3.position, 0.5f) - other1.position;
            originalDir2 = Vector3.Lerp(original2.position, original3.position, 0.5f) - original1.position;

            otherDir3 = Vector3.Cross(other2.position - other1.position, other3.position - other1.position);
            originalDir3 = Vector3.Cross(original2.position - original1.position, original3.position - original1.position);

            dif2 = Quaternion.LookRotation(originalDir2, originalDir3) * Quaternion.Inverse(Quaternion.LookRotation(otherDir2, otherDir3));

            other1.position = TransformPoint(originalDir1, originalSugarAtom.position, originalSugarAtom.rotation, Vector3.one);
            other1.rotation = originalSugarAtom.rotation * dif2 * dif1;
            original1.rotation = originalRotation;
        }
    }

    Vector3 TransformPoint(Vector3 point, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Matrix4x4 m = Matrix4x4.TRS(position, rotation, scale);
        return m.MultiplyPoint3x4(point);
    }
    */
    #endregion

    private void Start()
    {
        //otherDir1 = other1.transform.position - otherSugarAtom.position;
        originalDir1 = original1.position - originalSugarAtom.position;

        otherDir2 = Vector3.Lerp(other2.position, other3.position, 0.5f) - other1.position;
        originalDir2 = Vector3.Lerp(original2.position, original3.position, 0.5f) - original1.position;

        otherDir3 = Vector3.Cross(other2.position - other1.position, other3.position - other1.position);
        originalDir3 = Vector3.Cross(original2.position - original1.position, original3.position - original1.position);
    }

    private void Update()
    {
        if (align)
        {
            dif2 = Quaternion.LookRotation(originalDir2, originalDir3) * Quaternion.Inverse(Quaternion.LookRotation(otherDir2, otherDir3));
            other1.position = TransformPoint(originalDir1, originalSugarAtom.position, originalSugarAtom.rotation, Vector3.one); // Replace with transform.position on Reformer
            other1.rotation = originalSugarAtom.rotation * dif2;
        }
    }

    Vector3 TransformPoint(Vector3 point, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Matrix4x4 m = Matrix4x4.TRS(position, rotation, scale);
        return m.MultiplyPoint3x4(point);
    }
}
