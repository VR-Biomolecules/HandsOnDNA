using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomOutline : Outline
{
    protected override void Awake()
    {
        //    SkinnedMeshRenderer[] skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        //    for (int i = 0; i < skinnedMeshRenderer.Length; i++)
        //     {
        //        if (skinnedMeshRenderer[i].sharedMesh.subMeshCount > 1)
        //         {
        //            skinnedMeshRenderer[i].sharedMesh.subMeshCount = skinnedMeshRenderer[i].sharedMesh.subMeshCount + 1;
        //            skinnedMeshRenderer[i].sharedMesh.SetTriangles(skinnedMeshRenderer[i].sharedMesh.triangles, skinnedMeshRenderer[i].sharedMesh.subMeshCount - 1);
        //        }
        //    }

        //     MeshFilter[] meshFilter = GetComponentsInChildren <MeshFilter > ();
        //    for (int i = 0; i < meshFilter.Length; i++)
        //    {
        //        if (meshFilter[i].sharedMesh.subMeshCount > 1)
        //         {
        //       // meshFilter[i].sharedMesh.subMeshCount = meshFilter[i].sharedMesh.subMeshCount + 1;
        //        meshFilter[i].sharedMesh.SetTriangles(meshFilter[i].sharedMesh.triangles, meshFilter[i].sharedMesh.subMeshCount - 1);
        //    }
        //}
        //foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        //{
        //    if (skinnedMeshRenderer.sharedMesh.subMeshCount > 1)
        //    {
        //        skinnedMeshRenderer.sharedMesh.subMeshCount = skinnedMeshRenderer.sharedMesh.subMeshCount + 1;
        //        skinnedMeshRenderer.sharedMesh.SetTriangles(skinnedMeshRenderer.sharedMesh.triangles, skinnedMeshRenderer.sharedMesh.subMeshCount - 1);
        //    }

        //}

        //foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        //{
        //    if (meshFilter.sharedMesh.subMeshCount > 1)
        //    {
        //        //meshFilter.sharedMesh.subMeshCount = meshFilter.sharedMesh.subMeshCount + 1;
        //        meshFilter.sharedMesh.SetTriangles(meshFilter.sharedMesh.triangles, meshFilter.sharedMesh.subMeshCount - 1);
        //    }
        //}
        base.Awake();
    }
}
