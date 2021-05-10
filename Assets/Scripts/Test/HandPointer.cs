using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Oculus;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class HandPointer : MonoBehaviour
{
    LineRenderer line;
    RaycastHit hit;

    [SerializeField] GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    [SerializeField] EventSystem m_EventSystem;
    [SerializeField] RectTransform canvasRect;

    float axisPress = 0.5f;
    public OVRInput.RawAxis1D inputs;

    void Start()
    {
        line = GetComponentInChildren<LineRenderer>(true);
    }

    void Update()
    {
        if (OVRInput.Get(inputs) >= axisPress && hit.transform.parent.GetComponent<Button>())
        {
            hit.transform.parent.GetComponent<Button>().onClick.Invoke();
        }

        /*
        // Raycast forward hit something
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1000) && hit.transform.gameObject.layer == 5)
        {
            // Enable line
            if (!line.gameObject.activeInHierarchy)
            {
                line.gameObject.SetActive(true);
            }
            // Set end position
            line.SetPosition(1, line.transform.InverseTransformPoint(hit.point));
        }
        // Raycast hit nothing but line is enabled
        else if (line.gameObject.activeInHierarchy)
        {
            // Disable line
            line.gameObject.SetActive(false);
        }
        */

        //Set up the new Pointer Event
        m_PointerEventData = new PointerEventData(m_EventSystem);
        //Set the Pointer Event Position to that of the game object
        m_PointerEventData.position = transform.localPosition;

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        m_Raycaster.Raycast(m_PointerEventData, results);

        if (results.Count > 0) Debug.Log("Hit " + results[0].gameObject.name);
    }
}