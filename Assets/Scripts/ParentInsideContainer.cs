using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentInsideContainer : MonoBehaviour
{
    public Transform environment;
    Transform container;
    bool inContainer;
    Vector3 posOffset;
    Quaternion rotOffset;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("container"))
        {
            container = other.transform;
            posOffset = container.position - transform.position;
            rotOffset = container.rotation * Quaternion.Inverse(transform.rotation);
            inContainer = true;
            Debug.Log("ENTER");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("container"))
        {
            container = environment;
            inContainer = false;
        }
    }
    private void FixedUpdate()
    {
        if (inContainer)
        {
            transform.position = container.position + (posOffset);
            transform.rotation = container.rotation * rotOffset;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "container")
        {
            transform.parent = other.transform;
        } else
        {
            transform.parent = environment;
        }
    }
}
