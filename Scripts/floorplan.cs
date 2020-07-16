using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using alexism.Floorplan.Core.ScriptableObjects;
using alexism.Floorplan.Core.Components;
using alexism.Floorplan.Core.Enums;
using System.Linq;

namespace alexism.Floorplan.Core
{
    [ExecuteInEditMode]
    public class floorplan : MonoBehaviour
    {

        public Bounds bounds;
        public GameObject geometry;

        private void Awake()
        {
            geometry = GameObject.Find("New Floorplan Geometry");
            RecalculateBounds();
        }

        bool toolActive;
        [SerializeField]
        public floorplanTileset tileset;

        public Material[] wallMaterials=new Material[100];

        public List<int> selected = new List<int>(4) { -1,-1,-1,-1};

        [Space(15)]
        Vector3 lastHandlePosition;
        Vector3 snapLastHandlePosition;
        Vector3 handlePosition;
        Vector3 lastTileDelta;
        Vector3 tileDelta;
        GameObject geometryRoot;
        [HideInInspector]
        public float tileSize = 2f;
        Color gizmoColor = Color.red;

        public GameObject[] getTilesFromType(TileTypes type)
        {
            switch (type)
            {
                case TileTypes.Wall:
                    return tileset.wallTiles;
                case TileTypes.Pillar:
                    return tileset.pillarTiles;
                case TileTypes.Floor:
                    return tileset.floorTiles;
            }
            return null;
        }

        

        [ContextMenu("Recalculate Bounds")]
        public void RecalculateBounds()
        {
            MeshFilter this_mf = GetComponent<MeshFilter>();
            if (this_mf == null)
            {
                bounds = new Bounds(transform.position, Vector3.zero);
            }
            else
            {
                bounds = this_mf.sharedMesh.bounds;
            }

            MeshFilter[] mfs = geometry.GetComponentsInChildren<MeshFilter>();

            foreach (MeshFilter mf in mfs)
            {
                Vector3 pos = mf.transform.position;
                Bounds child_bounds = mf.sharedMesh.bounds;
                child_bounds.center += pos;
                bounds.Encapsulate(child_bounds);
            }
        }

        public TileTypes getTypeFromTile(GameObject tile)
        {
            TileTypes type = TileTypes.None;
            if (tileset.wallTiles.ToList().Find(x => x == tile))
                type = TileTypes.Wall;
            if (tileset.floorTiles.ToList().Find(x => x == tile))
                type = TileTypes.Floor;
            if (tileset.pillarTiles.ToList().Find(x => x == tile))
                type = TileTypes.Pillar;
            return type;
        }


        void OnEnable()
        {
            print("Selected: " + selected.Count);
            snapLastHandlePosition = transform.position;
        }

        void Start()
        {
            snapLastHandlePosition = transform.position;
            geometryRoot = GameObject.Find("New Floorplan Geometry");
        }

        void Update()
        {
        }

        private void OnDrawGizmos()
        {
        }

        public GameObject createInstance(GameObject instanceType, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(instanceType) as GameObject;
            instance.transform.position = spawnPosition;
            instance.transform.rotation = spawnRotation;
            instance.transform.parent = geometryRoot.transform;
            instance.GetComponent<floorplanComponent>().tileset = tileset;
            instance.name = instanceType.name;
            return instance.transform.GetChild(0).gameObject;
        }
    }
}