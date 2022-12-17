using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


public class GrowingTreeMazeManager : MonoBehaviour
{
    private Dictionary<int, Vector2> DIRECTIONS = new Dictionary<int, Vector2>
    {
        { 0, new Vector2(0, 1) }, // up
        { 1, new Vector2(0, -1) }, // down
        { 2, new Vector2(1, 0) }, // left
        { 3, new Vector2(-1, 0) } // right
    }; 
    
    [SerializeField] bool useRandomActiveNode = false;

    [SerializeField] bool useSteps;
    [SerializeField] float stepWaitTime = 0.03f;
    [SerializeField] int steps;
    [SerializeField] int currentSteps;

    [SerializeField] Vector2Int mazeSize;
    [SerializeField] MazeNode nodePrefab;
    
    List<MazeNode> nodes;
    List<MazeNode> completedNodes;

    List<MazeNode> path;
    bool finishedMaze = false;



    private void Start()
    {
        // StartCoroutine(GenerateMaze(mazeSize));
        this.nodes = GenerateGrid(mazeSize);
        this.completedNodes = new List<MazeNode>();
        
        this.path = new List<MazeNode>();

        // Add first node to list randomly
        AddRandomNode();

    }

    private void Update() 
    {
        if (!useSteps || (useSteps && currentSteps < steps))
        {
            
            GenerateMaze();
            currentSteps++;
        }
        StartCoroutine("Wait");
    }

    private void GenerateMaze()
    {
        if (this.path.Count != 0 && !finishedMaze)
        {
            // Choose "active" node to check neighbors
            MazeNode currentNode;

            // Select newest node
            if (useRandomActiveNode)
            {
                currentNode = this.path[UnityEngine.Random.Range(0, this.path.Count)];
            }
            else
            {
                currentNode = path[path.Count - 1];
            }

            // Check every neighbor of node
            List<Vector2> randomizedDirs = ShuffleList(DIRECTIONS);
            MazeNode unvisitedNeighbor = null;

            for (int i = 0; i < randomizedDirs.Count; i++)
            {
                Vector2 neighborPos = currentNode.mazePos + randomizedDirs[i];
                
                // Not neighbor if hit maze wall
                if (neighborPos.x < 0 || // Left
                    neighborPos.x > mazeSize.x - 1|| // Right
                    neighborPos.y < 0 || // Bot
                    neighborPos.y > mazeSize.y - 1 // Top
                    )
                {
                    // print(neighborPos + "   > Hit edge");
                    continue;
                }
                // arr to grid
                // xcol = pos % width
                // y = pos \ width

                // grid to arr
                // (y_row * row length) + xcol
                
                int neighborIdx = (int)((neighborPos.x * mazeSize.y) + neighborPos.y);
                MazeNode neighbor = nodes[neighborIdx];

                // Valid unvisited neighbor
                if (!neighbor.isVisited)
                {
                    // Create path from currentnode to neighbor
                    // Add to path
                    // indicate new neighbor found
                    // break from loop
                    unvisitedNeighbor = neighbor;
                    print("CURRENT - " + currentNode.mazePos + "NEIGHBOR: " + neighbor.mazePos);
                    
                    int dirkey = getDirKey(randomizedDirs[i]);
                    CreatePath(currentNode, neighbor, dirkey);
                    AddNode(neighbor);

                    break;
                }
                else {
                    continue;
                }

            }

            // if no unvisited neighbors found, delete from path list
            if (unvisitedNeighbor == null)
            {
                this.path.Remove(currentNode); 
                this.completedNodes.Add(currentNode);
                SetCompletedNode(currentNode);
            }
        }
        else if (this.path.Count == 0 && !finishedMaze)
        {
            print("Path empty!");
            this.finishedMaze = true;
        }
    }




    private List<MazeNode> GenerateGrid(Vector2Int mazeSize)
    {
        List<MazeNode> nodes = new List<MazeNode>();

        // Generate Grid.
        for (int x = 0; x < mazeSize.x; x++)
        {
            for (int y = 0; y < mazeSize.y; y++)
            {
                Vector3 nodePos = new Vector3(x - (mazeSize.x * 0.5f), 0, y - (mazeSize.y * 0.5f)); // Centers the node around (0,0) instead of bottom corner.
                MazeNode node = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);
                node.SetMazePos(new Vector2(x, y));

                nodes.Add(node);

            }
        }

        return nodes;
    }

    private void AddRandomNode()
    {
        // Set start node to random node.
        MazeNode randNode = this.nodes[UnityEngine.Random.Range(0, this.nodes.Count)];
        AddNode(randNode);
    }

    private void AddNode(MazeNode node)
    {
        this.path.Add(node);
        node.SetState(ENodeState.CURRENT);
        node.isVisited = true;
    }

    private void SetCompletedNode(MazeNode node)
    {
        node.SetState(ENodeState.COMPLETED);
    }

    public List<Vector2> ShuffleList(Dictionary<int, Vector2> dirs)  
    {   
        List<int> dirKeys = new List<int>();
        List<Vector2> dirValues = new List<Vector2>();
        dirKeys = dirs.Keys.ToList();
        dirValues = dirs.Values.ToList();

        for (int i = 0; i < dirValues.Count; i++) {
            Vector2 temp = dirValues[i];
            int randomIndex = UnityEngine.Random.Range(i, dirValues.Count);
            dirValues[i] = dirValues[randomIndex];
            dirValues[randomIndex] = temp;
        }

        return dirValues;
    }

    public void CreatePath(MazeNode currentNode, MazeNode neighbor, int dir)
    {
        switch (dir)
        {
            case 0:
                print("GOING UP");

                neighbor.SetWallActive(3, false);  // Remove bot wall of neighbour node
                currentNode.SetWallActive(2, false); // Remove top wall of current node
                break;
            case 1:
                print("GOING DOWN");

                neighbor.SetWallActive(2, false); // Remove top wall of neighbour node
                currentNode.SetWallActive(3, false); // Remove bot wall of current node
                break;
            case 2:
                print("GOING LEFT");

                neighbor.SetWallActive(0, false); // Remove right wall of neighbour
                currentNode.SetWallActive(1, false); // Remove left wall of current node
                break;
            case 3:
                print("GOING RIGHT");
                neighbor.SetWallActive(1, false); // Remove left wall of neighbour node
                currentNode.SetWallActive(0, false); // Remove right wall of current node
                break;
        }
    }

    public int getDirKey(Vector2 dir)
    {
        if (dir.Equals(new Vector2(0, 1))) { return 0; } // up
        if (dir.Equals(new Vector2(0, -1))) { return 1; } // down
        if (dir.Equals(new Vector2(-1, 0))) { return 2; } // left
        if (dir.Equals(new Vector2(1, 0))) { return 3; } // right

        else { return -1; }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(this.stepWaitTime);

        //Put code after waiting here
    }

}
