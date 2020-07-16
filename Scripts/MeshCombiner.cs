using UnityEngine;
using System.Collections;
using System.Linq;

public class MeshCombiner : MonoBehaviour
{
    public void Awake()
    {
        Combine();
        Simplify();
    }

    private void Simplify()
    {
        MeshCollider col = GetComponent<MeshCollider>();
        Mesh newMesh = CopyMesh();
        print("Before: "+newMesh.triangles.Length);
        newMesh.Weld(.1f,1f);
        col.sharedMesh = newMesh;
        col.Simplify();
        print("After: " + newMesh.triangles.Length);
    }

    Mesh CopyMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Mesh newmesh = new Mesh();
        newmesh.vertices = mesh.vertices;
        newmesh.triangles = mesh.triangles;
        newmesh.uv = mesh.uv;
        newmesh.normals = mesh.normals;
        newmesh.colors = mesh.colors;
        newmesh.tangents = mesh.tangents;
        return newmesh;
    }

    private void Combine()
    {
        Vector3 basePosition = transform.position;
        Quaternion baseRotation = transform.rotation;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();
        MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();

        //Linq solution to make sure we don't combine any of the doors(a door is easily identifiable by it's rigidbody component, so we look for that).
        meshFilters = meshFilters.Where((source, index) => !(source.transform.GetComponent<Rigidbody>() || source.transform.GetComponentInParent<Rigidbody>())).ToArray();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            if (!meshRenderer ||
                !meshFilter.sharedMesh ||
                meshRenderer.sharedMaterials.Length != meshFilter.sharedMesh.subMeshCount)
            {
                continue;
            }

            for (int s = 0; s < meshFilter.sharedMesh.subMeshCount; s++)
            {
                int materialArrayIndex = Contains(materials, meshRenderer.sharedMaterials[s].name);
                if (materialArrayIndex == -1)
                {
                    materials.Add(meshRenderer.sharedMaterials[s]);
                    materialArrayIndex = materials.Count - 1;
                }
                combineInstanceArrays.Add(new ArrayList());

                CombineInstance combineInstance = new CombineInstance();
                combineInstance.transform = meshRenderer.transform.localToWorldMatrix;
                combineInstance.subMeshIndex = s;
                combineInstance.mesh = meshFilter.sharedMesh;
                (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
            }
        }

        // Get / Create mesh filter & renderer
        MeshFilter meshFilterCombine = gameObject.GetComponent<MeshFilter>();
        if (meshFilterCombine == null)
        {
            meshFilterCombine = gameObject.AddComponent<MeshFilter>();
        }
        MeshRenderer meshRendererCombine = gameObject.GetComponent<MeshRenderer>();
        if (meshRendererCombine == null)
        {
            meshRendererCombine = gameObject.AddComponent<MeshRenderer>();
        }

        // Combine by material index into per-material meshes
        // also, Create CombineInstance array for next step
        Mesh[] meshes = new Mesh[materials.Count];
        CombineInstance[] combineInstances = new CombineInstance[materials.Count];

        for (int m = 0; m < materials.Count; m++)
        {
            CombineInstance[] combineInstanceArray = (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
            meshes[m] = new Mesh();
            meshes[m].CombineMeshes(combineInstanceArray, true, true);

            combineInstances[m] = new CombineInstance();
            combineInstances[m].mesh = meshes[m];
            combineInstances[m].subMeshIndex = 0;
        }

        // Combine into one
        meshFilterCombine.sharedMesh = new Mesh();
        meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);

        // Destroy other meshes
        foreach (Mesh oldMesh in meshes)
        {
            oldMesh.Clear();
            DestroyImmediate(oldMesh);
        }

        // Assign materials
        Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
        meshRendererCombine.materials = materialsArray;

        foreach (MeshFilter meshFilter in meshFilters)
        {

            if (!meshFilter || meshFilter.transform.GetComponent<Rigidbody>() || meshFilter.transform.GetComponentInParent<Rigidbody>() || meshFilter.transform.GetComponentInChildren<Rigidbody>())
                continue;
            DestroyImmediate(meshFilter.gameObject.transform.parent.gameObject);
        }
        transform.position = basePosition;
        transform.rotation = baseRotation;
        gameObject.AddComponent<MeshCollider>().sharedMesh = this.GetComponent<MeshFilter>().mesh;
    }

    private int Contains(ArrayList searchList, string searchName)
    {
        for (int i = 0; i < searchList.Count; i++)
        {
            if (((Material)searchList[i]).name == searchName)
            {
                return i;
            }
        }
        return -1;
    }
}