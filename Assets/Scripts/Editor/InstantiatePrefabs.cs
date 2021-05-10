using UnityEngine;
using UnityEditor;

public class InstantiatePrefabs : MonoBehaviour
{

    public GameObject prefab;
    private void Start()
    {
        PrefabUtility.InstantiatePrefab(prefab);
    }


}