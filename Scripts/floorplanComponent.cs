using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using alexism.Floorplan.Core.ScriptableObjects;
using alexism.Floorplan.Core.Enums;

namespace alexism.Floorplan.Core.Components
{
    [ExecuteInEditMode]
    [SelectionBase]

    public class floorplanComponent : MonoBehaviour
    {
        public floorplanTileset tileset;
        public List<GameObject> stuff;
        Vector3 offset;
        public TileTypes tileType=TileTypes.Wall;
        void Start() {
            if (EditorApplication.isPlaying)
                return;
            Renderer r = transform.GetChild(0).GetComponent<Renderer>();
            offset = r.bounds.center - new Vector3(0, r.bounds.size.y / 2.4f);
            stuff = new List<GameObject>();
#if UNITY_EDITOR
            if (Physics.CheckSphere(offset, .1f) && tileType==TileTypes.Floor)
            {

                Collider[] overlaps = Physics.OverlapSphere(offset, .1f);
                foreach (Collider overlap in overlaps)
                {
                    if (overlap.transform.root == transform.root && !isChild(overlap.transform, transform))
                    {
                        print("Destroyed overlap: " + overlap.transform.name);
                        DestroyImmediate(overlap.transform.parent.gameObject);
                    }
                }
            }
#endif
        }

        private void OnEnable()
        {
            //if (EditorApplication.isPlaying)
            //    return;
            //if (Physics.CheckSphere(offset, .1f))
            //{

            //    Collider[] overlaps = Physics.OverlapSphere(offset, .1f);
            //    foreach (Collider overlap in overlaps)
            //    {
            //        if (overlap.transform.root == transform.root && !isChild(overlap.transform, transform))
            //        {
            //            print("Destroyed overlap: " + overlap.transform.name);
            //            overlap.transform.parent.position += new Vector3(0, 10, 0);
            //        }
            //    }
            //}
        }

        bool isChild(Transform c,Transform p)
        {
            bool t = false;
            foreach(Transform child in p)
            {
                if (child == c)
                {
                    t = true;
                    break;
                }
                else
                {
                    if (child.childCount > 0)
                    {
                        t=isChild(c, child);
                    }
                }
            }
            return t;
        }

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

        public TileTypes getTypeFromTile(GameObject tile)
        {
            TileTypes type=TileTypes.None;
            if(tileset.wallTiles.ToList().Find(x => x==tile))
                type = TileTypes.Wall;
            if(tileset.floorTiles.ToList().Find(x => x==tile))
                type = TileTypes.Floor;
            if(tileset.pillarTiles.ToList().Find(x => x==tile))
                type = TileTypes.Pillar;
            return type;
        }

        public void ChangeComponentType(GameObject newType)
        {

            //    GameObject newInstance = GameObject.Instantiate (newType, transform.position, transform.rotation);
            GameObject newInstance = PrefabUtility.InstantiatePrefab(newType) as GameObject;
            newInstance.transform.position = this.transform.position;
            newInstance.transform.rotation = this.transform.rotation;
            newInstance.transform.localScale = this.transform.localScale;
            newInstance.transform.parent = this.transform.parent;
            newInstance.GetComponent<floorplanComponent>().tileset = tileset;
            Renderer newRenderer = newInstance.transform.GetChild(0).GetComponent<Renderer>();
            Renderer oldRenderer = transform.GetChild(0).GetComponent<Renderer>();
            if (newRenderer.sharedMaterials.Length > 0)
            {
                newInstance.transform.GetChild(0).GetComponent<Renderer>().materials = transform.GetChild(0).GetComponent<Renderer>().sharedMaterials;
            }
            else
            {
                newInstance.transform.GetChild(0).GetComponent<Renderer>().material = transform.GetChild(0).GetComponent<Renderer>().sharedMaterial;
            }
            GameObject.DestroyImmediate(this.gameObject);

        }
        private void OnDrawGizmos()
        {
        }
    }
}