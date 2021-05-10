using MoleculeTypes;
using UnityEngine;

public class BoxDetector : MonoBehaviour
{
    public BoxDetectorManager Manager;
    public Light pointLight;
    
    private void OnTriggerEnter(Collider other)
    {
        Manager.PossibleTargetCollided(gameObject, other.gameObject, pointLight);
    }
}
