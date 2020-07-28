using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.UI;

public class MeshCombiner : MonoBehaviour
{
    GameObject wallMesh;
    GameObject floorMesh;
    public void Awake()
    {
        
    }

    private void Start()
    {
        wallMesh = new GameObject("WallMesh", typeof(MeshFilter), typeof(Renderer));
        floorMesh = new GameObject("FloorMesh", typeof(MeshFilter), typeof(Renderer));
        print(transform.childCount);
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            Transform child = transform.GetChild(i);
            if (child.name == "Floor")
                child.SetParent(floorMesh.transform, false);
            else if (child.name == "Walls")
                child.SetParent(wallMesh.transform, false);
        }
        wallMesh.transform.parent = transform;
        floorMesh.transform.parent = transform;

        CombineWalls();
        CombineFloor();
        ClearChildren(floorMesh.transform);
        //ClearChildren(wallMesh.transform);
        SplitMesh(floorMesh.GetComponent<MeshFilter>());
        SplitMesh(wallMesh.GetComponent<MeshFilter>());

        for (int i = 0; i < floorMesh.transform.childCount; i++)
        {
            Simplify(floorMesh.transform.GetChild(i).GetComponent<MeshFilter>());
        }
        for (int i = 0; i < wallMesh.transform.childCount; i++)
        {
            Simplify(wallMesh.transform.GetChild(i).GetComponent<MeshFilter>());
        }
    }

    void ClearChildren(Transform t)
    {
        for(int i=0; i<t.childCount; i++)
        {
            Destroy(t.GetChild(i).gameObject);
        }
    }

    private void Simplify(MeshFilter mf)
    {
        if (mf != null)
        {
            mf.mesh.Weld(0.00000328f, 1f);
            mf.mesh.Simplify();
        }
    }

    void SplitMesh(MeshFilter mf)
    {
        for (int i = 0; i < mf.mesh.subMeshCount; i++)
        {
            GameObject gO = new GameObject();
            gO.name = mf.gameObject.name + " submesh " + (i+1).ToString();
            gO.transform.parent = mf.gameObject.transform;
            MeshFilter filter = gO.AddComponent<MeshFilter>();
            filter.mesh=mf.mesh.GetSubmesh(i);
            Renderer r = gO.AddComponent<MeshRenderer>();
            r.material = mf.gameObject.GetComponent<Renderer>().materials[i];
            MeshCollider c = gO.AddComponent<MeshCollider>();
            c.sharedMesh = filter.mesh;
        }
        Destroy(mf.gameObject.GetComponent<Renderer>());
        Destroy(mf.gameObject.GetComponent<MeshCollider>());
    }

    private void CombineWalls()
    {
        Vector3 basePosition = transform.position;
        Quaternion baseRotation = transform.rotation;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;



        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();
        MeshFilter[] meshFilters = wallMesh.GetComponentsInChildren<MeshFilter>();

        //Linq solution to make sure we don't combine any of the doors(a door is easily identifiable by it's rigidbody component, so we look for that).
        meshFilters = meshFilters.Where((source, index) => !(source.transform.name == "FloorMesh" || source.transform.name == "WallMesh" || source.transform.GetComponent<Rigidbody>() || source.transform.GetComponentInParent<Rigidbody>())).ToArray();
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
                if (meshRenderer.sharedMaterials[s]){
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
        }

        // Get / Create mesh filter & renderer
        MeshFilter meshFilterCombine = wallMesh.GetComponent<MeshFilter>();
        if (meshFilterCombine == null)
        {
            meshFilterCombine = wallMesh.AddComponent<MeshFilter>();
        }
        MeshRenderer meshRendererCombine = gameObject.GetComponent<MeshRenderer>();
        if (meshRendererCombine == null)
        {
            meshRendererCombine = wallMesh.AddComponent<MeshRenderer>();
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
        wallMesh.transform.position = basePosition;
        wallMesh.transform.rotation = baseRotation;
        wallMesh.AddComponent<MeshCollider>().sharedMesh = floorMesh.GetComponent<MeshFilter>().sharedMesh;
    }
    private void CombineFloor()
    {
        Vector3 basePosition = transform.position;
        Quaternion baseRotation = transform.rotation;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;



        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();
        MeshFilter[] meshFilters = floorMesh.GetComponentsInChildren<MeshFilter>();

        //Linq solution to make sure we don't combine any of the doors(a door is easily identifiable by it's rigidbody component, so we look for that).
        meshFilters = meshFilters.Where((source, index) => !(source.transform.name=="FloorMesh" || source.transform.name=="WallMesh" || source.transform.GetComponent<Rigidbody>() || source.transform.GetComponentInParent<Rigidbody>())).ToArray();
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
                if (meshRenderer.sharedMaterials[s])
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
        }

        // Get / Create mesh filter & renderer
        MeshFilter meshFilterCombine = floorMesh.GetComponent<MeshFilter>();
        if (meshFilterCombine == null)
        {
            meshFilterCombine = floorMesh.AddComponent<MeshFilter>();
        }
        MeshRenderer meshRendererCombine = gameObject.GetComponent<MeshRenderer>();
        if (meshRendererCombine == null)
        {
            meshRendererCombine = floorMesh.AddComponent<MeshRenderer>();
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
        floorMesh.transform.position = basePosition;
        floorMesh.transform.rotation = baseRotation;
        floorMesh.AddComponent<MeshCollider>().sharedMesh = floorMesh.GetComponent<MeshFilter>().sharedMesh;
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