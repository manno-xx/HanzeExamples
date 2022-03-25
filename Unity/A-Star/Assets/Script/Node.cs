using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class MouseOverEvent : UnityEvent<Node>
{
}

public class Node : MonoBehaviour
{
    [HideInInspector]
    public MouseOverEvent mouseOver;

    #region private members
    
    private int _terrain;

    private float _g;
    private float _h;
    private float _f;
    
    private Text _gText;
    private Text _fText;
    
    private Color _originalColor;
    
    #endregion

    #region properties
    
    public int X { get; set; }
    public int Z { get; set; }
    public int Terrain
    {
        get => _terrain;
        set
        {
            _terrain = Math.Max(0, Math.Min(255, value));
            var c = SetDefaultColor();
            SetNodeColor(c);
        }
    }
    
    /// <summary>
    /// The cost to get from start to this node
    /// For debug purposes displayed in a UI.Text 
    /// </summary>
    public float G
    {
        get => _g;
        set
        {
            _g = value;
            _gText.text = _g.ToString("0.00");
        }
    }

    public float H
    {
        get => _h;
        set
        {
            _h = value;
            _f = _g + _h;
            _fText.text = _h.ToString("0.00");
        }
    }

    /// <summary>
    /// The Estimated total cost
    /// (Actual cost from start to here and estimated cost from here to goal)
    /// </summary>
    public float F
    {
        get => _f;
        private set
        {
            _f = value;
            _fText.text = _f.ToString("0.00");
        }
    }
    
    #endregion

    #region public members

    /// <summary>
    /// The node that precedes this node in the path
    /// </summary>
    public Node previousNode;

    public List<Node> neighbours = new List<Node>();

    // reference to the map this node is a part of
    public SimpleMap Map { get; set; }
    
    #endregion
    

    /// <summary>
    /// Initialises the node
    /// </summary>
    private void Awake()
    {
        _originalColor = GetComponent<Renderer>().material.color;
        _gText = transform.Find("Canvas/g Text").GetComponent<Text>();
        _fText = transform.Find("Canvas/f Text").GetComponent<Text>();
        SetBaseValues();
        mouseOver = new MouseOverEvent();
    }
    
    /// <summary>
    /// Sort of reset of this node
    /// </summary>
    public void SetBaseValues()
    {
        G = 0;
        F = 0;
    }
    
    /// <summary>
    /// Create a list of neighbours of this node.
    /// Neighbour space depends on setting of the map
    /// </summary>
    public void CollectNeighbours()
    {
        if (Map.UseManhattanDistance)
        {
            GetVonNeumannNeighbourhood();
        }
        else
        {
            GetMooreNeighbourhood();
        }
    }

    /// <summary>
    /// Create a list of neighbours of this node
    /// Neighbour nodes here are those in More neighbourhood (the 8 nodes surrounding this one
    /// https://en.wikipedia.org/wiki/Moore_neighborhood
    /// </summary>
    private void GetMooreNeighbourhood()
    {
        int x1 = Math.Max(0, X-1);
        int x2 = Math.Min(Map.MapWidth-1, X+1);
        int z1 = Math.Max(0, Z-1);
        int z2 = Math.Min(Map.MapHeight-1, Z+1);

        for (int x = x1; x <= x2; x++)
        {
            for (int z = z1; z <= z2; z++)
            {
                Node node = Map.GetNodeAt(x, z);
                if (node != this)
                {
                    neighbours.Add(node);
                }
            }
        }
    }

    /// <summary>
    /// Create a list of neighbours of this node
    /// Neighbour nodes here are those in von Neumann neighbourhood (no diagonal travel)
    /// See: https://en.wikipedia.org/wiki/Von_Neumann_neighborhood
    /// 
    /// a bit crude maybe
    /// </summary>
    private void GetVonNeumannNeighbourhood()
    {
        neighbours = new List<Node>();

        if (X > 0)
        {
            neighbours.Add(Map.GetNodeAt(X - 1, Z));
        }

        if (X < (Map.MapWidth - 1))
        {
            neighbours.Add(Map.GetNodeAt(X + 1, Z));
        }

        if (Z > 0)
        {
            neighbours.Add(Map.GetNodeAt(X, Z - 1));
        }

        if (Z < (Map.MapHeight - 1))
        {
            neighbours.Add(Map.GetNodeAt(X, Z + 1));
        }
    }

    /// <summary>
    /// Highlight the neighbour nodes. Just for checks
    /// </summary>
    private void OnMouseEnter()
    {
        mouseOver?.Invoke(this);
    }
    
    /// <summary>
    /// When you click a node:
    /// 
    /// With Left Control: Becomes start node
    /// With Left Shift: Becomes goal node
    /// Otherwise toggles the traversability
    /// </summary>
    private void OnMouseDown()
    {
        Debug.Log($"Clicked on {this}");

        if(Input.GetKey(KeyCode.LeftControl))
        {
            Map.SetStart(this);
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            Map.SetGoal(this);
        }
        else
        {
            if (Terrain == 0)
                Terrain = 255;
            else
            {
                Terrain = 0;
            }
        }
    }

    /// <summary>
    /// Reset the nodes highlighted by MouseEnter 
    /// </summary>
    private void OnMouseExit()
    {
        mouseOver?.Invoke(null);
    }
    
    /// <summary>
    /// Updates the standard color after the terrain has been updated
    /// </summary>
    /// <returns></returns>
    private Color SetDefaultColor()
    {
        float cv = (float) (255 - _terrain) / 255;
        Color c = new Color(cv, cv, cv);
        _originalColor = c;
        return c;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    public void SetNodeColor(Color c)
    {
        GetComponent<Renderer>().material.color = c;
    }

    /// <summary>
    /// Reset the color to the original
    /// </summary>
    public void ResetColor()
    {
        GetComponent<Renderer>().material.color = _originalColor;
    }

    /// <summary>
    /// Show in the Console what _I_ want to see, not [Object object] or whatever
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"x: {X.ToString()}, z: {Z.ToString()}, terrain: {_terrain.ToString()}";
    }
}
