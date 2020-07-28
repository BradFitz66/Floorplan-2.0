using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace alexism.Floorplan.Core.editor
{
    public class floorplanMenuItem : MonoBehaviour
    {

        [MenuItem("Tools/Add Floorplan Handle")]
        static void addFloorplanHandle()
        {
            GameObject floorplanHandleGameObject = new GameObject("New Floorplan Handle");
            floorplanHandleGameObject.AddComponent<floorplan>();
        }
    }
}