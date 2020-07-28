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
    [RequireComponent(typeof(Grid))]
    [ExecuteInEditMode]
    
    public class floorplan : MonoBehaviour
    {

        
        public Bounds bounds;
        [HideInInspector]
        public GameObject geometry;

        private void Awake()
        {
            if (!geometry)
                geometry = new GameObject("FloorPlanGeometry");
            //geometry = GameObject.Find("New Floorplan Geometry");
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

        private void OnDrawGizmosSelected()
        {

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
            geometryRoot = geometry;
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
            instance.transform.parent = geometry.transform;
            instance.GetComponent<floorplanComponent>().tileset = tileset;
            instance.name = instanceType.name;
            return instance.transform.GetChild(0).gameObject;
        }
    }
}