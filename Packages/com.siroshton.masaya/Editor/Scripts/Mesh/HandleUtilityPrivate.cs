// original code: https://tryfinally.dev/ray-mesh-intersection-in-unity-editor

using UnityEngine;
using static System.Reflection.BindingFlags;

public static class HandleUtilityPrivate
{
    public static RaycastHit? IntersectRayMesh(MeshFilter meshFilter, in Ray ray) 
        => intersectRayMeshFunc(ray, meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix, out var result) ? result : null;

    public static RaycastHit? IntersectRayMesh(SkinnedMeshRenderer skinnedMesh, in Ray ray)
        => intersectRayMeshFunc(ray, skinnedMesh.sharedMesh, skinnedMesh.transform.localToWorldMatrix, out var result) ? result : null;

    public static RaycastHit? IntersectRayMesh(Mesh mesh, in Ray ray, Matrix4x4 meshTransform)
        => intersectRayMeshFunc(ray, mesh, meshTransform, out var result) ? result : null;

    delegate bool IntersectRayMeshDelegate(Ray ray, Mesh mesh, Matrix4x4 matrix, out RaycastHit hit);

    static readonly IntersectRayMeshDelegate intersectRayMeshFunc 
        = (IntersectRayMeshDelegate)typeof(UnityEditor.HandleUtility)
        .GetMethod("IntersectRayMesh", Static | NonPublic)
        .CreateDelegate(typeof(IntersectRayMeshDelegate));

}
