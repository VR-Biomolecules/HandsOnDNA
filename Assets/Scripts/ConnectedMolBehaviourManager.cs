using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedMolBehaviourManager : MonoBehaviour
{
    private List<Bond> toWatch;
    
    // Start is called before the first frame update
    void Start()
    {
        toWatch = new List<Bond>();
    }

    public void AddBondToWatch(Bond bond)
    {
        if (!toWatch.Contains(bond))
        {
            toWatch.Add(bond);
        }
    }

    void FixedUpdate()
    {
        foreach (Bond bond in toWatch)
        {
            if (bond && bond.bondJoint && bond.bondJoint.currentForce.magnitude > 5000)
            {
                Player.hands[0].ForceRelease(Player.hands[0].grabbedObject);
                Player.hands[1].ForceRelease(Player.hands[1].grabbedObject);
            }
        }
    }
}
