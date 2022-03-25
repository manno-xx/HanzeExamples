using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Quick And Dirty A* (A-star) demo
/// <p>- A blank map is generated. Tiles can be toggled to be traversable or not (like a maze maybe)</p>
/// <p>- The Manhattan Distance checkbox does not alter settings if the project already runs</p>
///   Changing the setting also toggles between Moore and von Neumann neighbourhood
/// <p>- The Heuristic multiplier _can_ be changed at runtime.</p>
///   For its meaning, see https://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html
/// <p>- Space Bar starts the path finding</p>
/// <p>- Left Ctrl sets the start node of the path</p>
/// <p>- Left Shift sets the end node of the path</p>
///   
/// Potential issues in a 'live' environment:
/// <p>- Path values are stored in the nodes' attributes. The map can therefore only maintain _one_ path.</p>
/// <p>- The data structure for the 'open' set is a plain list. With larger maps that may slow the algorithm down unacceptably</p>
/// </summary>
public class SimpleMap : MonoBehaviour
{
    #region Inspector Values

    /// <summary>
    /// The prefab that is the tile. It has a Node component on it
    /// </summary>
    [SerializeField] private Node nodeObject;

    /// <summary>
    /// Width of the map
    /// </summary>
    [Header("Map Properties")] [SerializeField]
    private int mapWidth = 10;

    /// <summary>
    /// The height of the map
    /// </summary>
    [SerializeField] private int mapHeight = 10;

    [Header("UI of the algorithm")] [SerializeField]
    private HUD hud;

    [Header("Algorithm parameters")]
    [Tooltip("Allows to set to what extent the heuristic weighs in on the algorithm")]
    [Range(0, 4)]
    [SerializeField]
    private float heuristicMultiplier = 1;

    [Tooltip(
        "Use Manhattan distance (and von Neumann neighbourhood) or not (if not, Euclidean distance (and Moore neighbourhood) is used")]
    [SerializeField]
    private bool useManhattanDistance = true;

    #endregion

    #region properties

    public int MapWidth => mapWidth;

    public int MapHeight => mapHeight;

    public bool UseManhattanDistance => useManhattanDistance;

    #endregion

    #region private members

    // the start and end node of the path
    private Node _startNode;
    private Node _goalNode;

    // all the nodes in the map
    private List<Node> nodes = new List<Node>();

    private Stopwatch sw;

    #endregion
    
    /// <summary>
    /// Generates the map and sets start and end-node of the path
    /// </summary>
    void Start()
    {
        sw = new Stopwatch();

        GenerateMap();

        _startNode = nodes[0];
        _goalNode = nodes[MapWidth * mapHeight - 1];
    }

    /// <summary>
    /// If the space bar is pressed, the path is calculated
    /// If the path is possible, the path is drawn
    /// </summary>
    private void Update()
    {
        // trigger the A* algorithm with the SPACE bar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetMap();

            sw.Restart();
            
            Node pathNode = GeneratePath(_startNode, _goalNode);

            sw.Stop();
            Debug.Log($"Path Found in {sw.ElapsedMilliseconds} ms");
            
            // draw path if it is a possible path
            while (pathNode)
            {
                pathNode.SetNodeColor(Color.magenta);
                pathNode = pathNode.previousNode;
            }
        }
    }

    /// <summary>
    /// Generates the initial map
    /// The map is blank at first. Click tiles to toggle 'traversability'
    /// 
    /// After tiles are created, nodes connect to their neighbours
    /// </summary>
    private void GenerateMap()
    {
        for (int z = 0; z < mapHeight; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Node node = Instantiate(nodeObject, new Vector3(x - (mapWidth / 2), 0, z - (mapHeight / 2)),
                    Quaternion.identity, transform);

                node.X = x;
                node.Z = z;

                node.Terrain = 0;
                node.Map = this;
                node.mouseOver.AddListener(hud.ShowNodeInfo);

                nodes.Add(node);
            }
        }

        // let each node find a reference to its neighbours
        foreach (var node in nodes)
        {
            node.CollectNeighbours();
            Debug.Log($"Node {node} has {node.neighbours.Count} neighbours");
        }
    }

    /// <summary>
    /// Retrieve the node in the map at given x- and z-position
    /// </summary>
    /// <param name="x">the x position (row)</param>
    /// <param name="z">the y position (column)</param>
    /// <returns>The node on the map at that position</returns>
    public Node GetNodeAt(int x, int z)
    {
        return nodes[z * MapWidth + x];
    }

    /// <summary>
    /// Set the startNode
    /// </summary>
    /// <param name="node">The start node to be</param>
    public void SetStart(Node node)
    {
        _startNode = node;
    }
    
    /// <summary>
    /// Set the goal node
    /// </summary>
    /// <param name="node">The goal node to be</param>
    internal void SetGoal(Node node)
    {
        _goalNode = node;
    }

    /// <summary>
    /// Loosely following the algorithm as described in the slides
    ///
    /// There are slight variations in the algorithm
    /// But for sure in the way of traversing the map, calculating distances, managing terrain cost
    ///
    /// </summary>
    /// <param name="start">The start of the path</param>
    /// <param name="goal">The goal of the path</param>
    private Node GeneratePath(Node start, Node goal)
    {
        // the 'candidate' nodes that still can be checked as the next step on the path
        // for now a list. Make a Heap later maybe
        List<Node> open = new List<Node>();

        // The list to keep nodes in that have already been checked
        List<Node> closed = new List<Node>();

        open.Add(start);
        start.G = 0;

        // Continue to look for a path as long as there's still an option open
        while (open.Count > 0)
        {
            // Get the best option from the open list
            Node current = GetBestNode(open);

            // If that best option _is_ the goal: We're there!
            // Return the node (current or goal, potato, potato)
            if (current == goal)
            {
                // if a path is found, the goal is reached
                // the goal node contains a reference to the previous node to the goal
                // and that node in its turn the step before etc.
                return goal;
            }

            // remove the current node from the open list (containing items to still check)
            open.Remove(current);
            // add the current node to the closed list (we are almost done with checking it)
            closed.Add(current);

            // check each and every neighbour of current
            foreach (var neighbour in current.neighbours)
            {
                // if we have already checked this neighbour, skip to the next
                // also: 255 as terrain value is considered in-traversable 
                if (closed.Contains(neighbour) || neighbour.Terrain == 255)
                {
                    continue;
                }

                // calculate the possible cost score of the neighbouring node
                float tentativeG = current.G + ActualCost(current, neighbour);
                // if the node was not seen before OR if it was seen before, but the score is better than a previous recorded score,
                // set the score to the newly calculated score
                if (!open.Contains(neighbour) || tentativeG < neighbour.G)
                {
                    neighbour.G = tentativeG;
                    neighbour.H = Heuristic(neighbour, goal);
                    neighbour.previousNode = current;
                }

                // if the neighbour not in open yet, add it
                if (!open.Contains(neighbour))
                {
                    open.Add(neighbour);
                }
            }
        }

        Debug.Log("no path found");
        return null;
    }

    /// <summary>
    /// Resets the tiles in the map, ready for a new A* calculation
    /// </summary>
    private void ResetMap()
    {
        foreach (var node in nodes)
        {
            node.SetBaseValues();
            node.ResetColor();
        }
    }

    /// <summary>
    /// Get the estimated distance between two nodes.
    /// </summary>
    /// <param name="nodeA"></param>
    /// <param name="nodeB"></param>
    /// <returns></returns>
    private float Heuristic(Node nodeA, Node nodeB)
    {
        float result;
        var a = nodeA.transform.position;
        var b = nodeB.transform.position;
        if (useManhattanDistance)
        {
            result = (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z)) * heuristicMultiplier;
        }
        else
        {
            result = Vector3.Distance(nodeA.transform.position, nodeB.transform.position) * heuristicMultiplier;
        }

        return result;
    }

    /// <summary>
    /// Calculate the cost to move from one cell to another
    /// Purely on distance, either manhattan distance or Euclidean
    /// </summary>
    /// <param name="nodeA">The one cell</param>
    /// <param name="nodeB">The other cell</param>
    /// <returns>The 'travel cost'</returns>
    private float ActualCost(Node nodeA, Node nodeB)
    {
        float result; 
        var a = nodeA.transform.position;
        var b = nodeB.transform.position;
        if (useManhattanDistance)
        {
            result = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
        }
        else
        {
            result = Vector3.Distance(nodeA.transform.position, nodeB.transform.position);
        }

        return result;
    }

    /// <summary>
    /// Somewhat crude way to get the node with the best f value from the list
    /// TODO Optimize this. using a Heap? 
    /// </summary>
    /// <param name="list">The list of candidate nodes</param>
    /// <returns>The node with the lowest f value</returns>
    Node GetBestNode(List<Node> list)
    {
        int best = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].F < list[best].F)
            {
                best = i;
            }
        }

        return list[best];
    }
}