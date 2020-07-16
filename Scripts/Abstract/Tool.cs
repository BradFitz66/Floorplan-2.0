using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;
using alexism.Floorplan.Core.Enums;
using alexism.Floorplan.Core;
using alexism.Floorplan.Core.Abstract;
using System.Runtime.InteropServices;

//Implement strategy pattern for drawing tools 

namespace alexism.Floorplan.Core.Abstract
{
    public abstract class Tool
    {
        public abstract void MouseDown(Vector3 mousePos);
        public abstract void MouseDrag(Vector3 mousePos);
        public abstract void MouseUp(Vector3 mousePos, TileTypes tileType, GameObject tile,floorplan script, Material[] mat, List<int> selected);

        public abstract void RenderPreview();
    }
}
//Some classes that inherit Tool. I should probably put these into their own separate files.

public class RectangleFilledStrat : alexism.Floorplan.Core.Abstract.Tool
{
    float width;
    float height;
    Vector3 mouseStart;
    Vector3 mouseEnd;

    public void Render(GameObject tile, floorplan script, Material[] mats)
    {
        GameObject gO = new GameObject("Floor");
        gO.transform.parent = GameObject.Find("New Floorplan Geometry").transform;
        Vector3 topLeft = new Vector3(Mathf.Max(mouseStart.x, mouseEnd.x), mouseStart.y, Mathf.Max(mouseStart.z, mouseEnd.z));
        for (int y = 0; y < Mathf.Abs(height); y += (int)script.tileSize)
        {
            for (int x = 0; x < Mathf.Abs(width); x += (int)script.tileSize)
            {
                GameObject floor = script.createInstance(tile, (topLeft - new Vector3(x, 0, y + script.tileSize)), Quaternion.identity);
                floor.GetComponent<Renderer>().materials = mats;
                floor.transform.parent.parent = gO.transform;
            }
        }
        Undo.RegisterCreatedObjectUndo(gO,"Undo floor creation");
    }

    public override void MouseDown(Vector3 mousePos)
    {
        mouseStart = mousePos;
    }

    public override void MouseDrag(Vector3 mousePos)
    {
        width = -(mouseEnd.x - mouseStart.x);
        height = -(mouseEnd.z - mouseStart.z);
        mouseEnd = mousePos;
    }

    public override void MouseUp(Vector3 mousePos, TileTypes tileType, GameObject tile, floorplan script, Material[] mat, List<int> selected)
    {
        Vector3[] points = new Vector3[]
        {
            mouseStart,
            mouseEnd,
            mouseStart-(new Vector3(width,0,0)),
            mouseEnd+(new Vector3(width,0,0))
        };
        List<Material> mats = new List<Material>();
        selected.RemoveAll(o => o == -1);
        for (int i = 0; i < selected.Count; i++)
        {
            mats.Add(mat[selected[i]]);
        }
        //Should we even do this (manage which tiletypes a tool can create) inside tools? Should this be handled by something else?
        switch (tileType){ 
            case TileTypes.Wall:
                Debug.LogWarning("Trying to draw type Wall with incorrect tool");
                break;
            case TileTypes.Floor:
                Render(tile,script,mats.ToArray());
                break;
            case TileTypes.Pillar:
                Debug.LogWarning("Trying to draw type Pillar with incorrect tool");
                break;
        }

        mouseStart = Vector3.zero;
        mouseEnd = Vector3.zero;
    }

    public override void RenderPreview()
    {
        Handles.color = Color.red;
        Handles.DrawWireCube(mouseStart - new Vector3(width / 2, 0, height / 2), new Vector3(width, 2, height));
    }
}


public class RectangleStrat : alexism.Floorplan.Core.Abstract.Tool
{
    float width;
    float height;
    Vector3 mouseStart;
    Vector3 mouseEnd;
    GameObject depthText;
    GameObject widthText;
    bool replace;
    void Render(floorplan script, GameObject tile,Material[] mats)
    {
        GameObject gO = new GameObject("Walls");
        gO.transform.parent = GameObject.Find("New Floorplan Geometry").transform;
        Vector3 topLeft = new Vector3(Mathf.Max(mouseStart.x, mouseEnd.x), mouseStart.y, Mathf.Max(mouseStart.z, mouseEnd.z));
        Vector3 bottomLeft = new Vector3(Mathf.Min(mouseStart.x, mouseEnd.x), mouseStart.y, Mathf.Min(mouseStart.z, mouseEnd.z));

        for (int x = 0; x < Mathf.Abs(width); x += (int)script.tileSize)
        {
            Vector3 offset = topLeft - new Vector3(x+script.tileSize/2, -.5f, 0);
            Vector3 offset2 = bottomLeft - new Vector3(-x-script.tileSize/2, -.5f, 0);

            GameObject wall = null;
            if (!Physics.CheckSphere(offset, .1f))
            {
                wall = script.createInstance(tile, (topLeft - new Vector3(x + script.tileSize, 0, 0)), Quaternion.LookRotation(Vector3.right, Vector3.up));
                wall.GetComponent<Renderer>().materials = mats;
                wall.transform.parent.parent = gO.transform;
            }
            if (!Physics.CheckSphere(offset2, .1f))
            {
                wall = script.createInstance(tile, (bottomLeft - new Vector3(-x - script.tileSize, 0, 0)), Quaternion.LookRotation(-Vector3.right, Vector3.up));
                wall.GetComponent<Renderer>().materials = mats;
                wall.transform.parent.parent = gO.transform;
            }

        }
        for (int z = 0; z < Mathf.Abs(height); z += (int)script.tileSize)
        {
            Vector3 offset = topLeft - new Vector3(0, -.5f, z+script.tileSize/2);
            Vector3 offset2 = bottomLeft - new Vector3(0,-.5f, -z-script.tileSize/2);
            GameObject wall = null;
            if (!Physics.CheckSphere(offset, .1f))
            {
                wall = script.createInstance(tile, (topLeft - new Vector3(0, 0, z)), Quaternion.LookRotation(-Vector3.forward, Vector3.up));
                wall.GetComponent<Renderer>().materials = mats;
                wall.transform.parent.parent = gO.transform;

            }

            if (!Physics.CheckSphere(offset2, .1f))
            {
                wall = script.createInstance(tile, (bottomLeft - new Vector3(0, 0, -z)), Quaternion.identity);
                wall.GetComponent<Renderer>().materials = mats;
                wall.transform.parent.parent = gO.transform;
            }
        }
        Undo.RegisterCreatedObjectUndo(gO, "Undo wall creation");
    }

    public override void RenderPreview()
    {

        Handles.color = Color.red;
        Handles.DrawWireCube(mouseStart - new Vector3(width / 2, 0, height / 2), new Vector3(width, 2, height));
    }

    public override void MouseDown(Vector3 mousePos)
    {
        mouseStart = mousePos;
    }
    public override void MouseDrag(Vector3 mousePos)
    {
        width = -(mouseEnd.x - mouseStart.x);
        height = -(mouseEnd.z - mouseStart.z);
        mouseEnd = mousePos;
        if (depthText == null)
        {
            //Initialize text. I could probably just use Handles.Label but this gives me more control over the look for text imo (and gives me way more awful code :D!)
            depthText = new GameObject();
            TextMeshPro dText = depthText.AddComponent<TextMeshPro>();
            dText.enableAutoSizing = true;
            depthText.transform.rotation = Quaternion.LookRotation(-Vector3.up, -Vector3.right);
            dText.color = Color.black;
            dText.fontSizeMax = 8;
            widthText = new GameObject();
            TextMeshPro wText = widthText.AddComponent<TextMeshPro>();
            wText.enableAutoSizing = true;
            widthText.transform.rotation = Quaternion.LookRotation(-Vector3.up, Vector3.up);
            widthText.transform.localScale = new Vector3(1, 1, -1);
            wText.color = Color.black;
            wText.fontSizeMax = 8;
        }
        else
        {
            TextMeshPro dText = depthText.GetComponent<TextMeshPro>();
            depthText.GetComponent<RectTransform>().position = mouseStart - new Vector3(.5f, -1, (height / 2) + dText.textBounds.center.x);
            depthText.GetComponent<TextMeshPro>().text = "Depth: "+Mathf.Abs((height / 2)).ToString();
            TextMeshPro wText = widthText.GetComponent<TextMeshPro>();
            
            widthText.GetComponent<RectTransform>().position = mouseStart - new Vector3((width/2)+wText.textBounds.center.x, -1, -.5f);
            widthText.GetComponent<TextMeshPro>().text = "Width: "+Mathf.Abs((width / 2)).ToString();

        }
    }
    public override void MouseUp(Vector3 mousePos, TileTypes tileType,GameObject tile,floorplan script, Material[] mat,List<int> selected)
    {
        //Get the 4 corners of the rectangle
        Vector3[] points = new Vector3[]
        {
            mouseStart,
            mouseEnd,
            mouseStart-(new Vector3(width,0,0)),
            mouseEnd+(new Vector3(width,0,0))
        };
        List<Material> mats = new List<Material>();
        selected.RemoveAll(o => o == -1);
        Debug.Log(selected.Count);
        for(int i=0; i<selected.Count; i++)
        {
            mats.Add(mat[selected[i]]);
        }
        switch (tileType)
        {
            case TileTypes.Wall:
                Render(script, tile,mats.ToArray());
                break;
            case TileTypes.Floor:
                Debug.LogWarning("Trying to draw type Floor with incorrect tool");
                break;
            case TileTypes.Pillar:
                Debug.LogWarning("Pillar is not supported by this tool yet");
                break;
        }
        Editor.DestroyImmediate(depthText);
        Editor.DestroyImmediate(widthText);
        mouseStart = Vector3.zero;
        mouseEnd = Vector3.zero;
    }
}