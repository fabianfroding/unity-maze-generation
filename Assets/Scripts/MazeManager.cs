using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EMAZEGEN_METHOD { RECURSIVE_BACKTRACKER, GROWING_TREE }

public class MazeManager : MonoBehaviour
{
    [SerializeField] public EMAZEGEN_METHOD mazeGenerationMethod = EMAZEGEN_METHOD.RECURSIVE_BACKTRACKER;
    [SerializeField] MazeNode nodePrefab;
    [SerializeField] Vector2Int mazeSize;
    [SerializeField] float generationTick = 0.05f;
    // [SerializeField] bool generationTick = 0.05f;

    private GrowingTreeMazeManager growingTreeManager;


    #region Unity Callback Functions
    private void Start()
    {
        this.growingTreeManager = GetComponent<GrowingTreeMazeManager>();

        switch (mazeGenerationMethod)
        {
            case EMAZEGEN_METHOD.RECURSIVE_BACKTRACKER:
                StartCoroutine(GenerateMaze(mazeSize));
                break;
            case EMAZEGEN_METHOD.GROWING_TREE:
                growingTreeManager.StartGTMaze();
                StartCoroutine(growingTreeManager.IEnumGenerateMaze());
                break;
        }
    }
    #endregion


    private IEnumerator GenerateMaze(Vector2Int mazeSize)
    {
        List<MazeNode> nodes = new List<MazeNode>();

        // Generate node.
        for (int x = 0; x < mazeSize.x; x++)
        {
            for (int y = 0; y < mazeSize.y; y++)
            {
                Vector3 nodePos = new Vector3(x - (mazeSize.x * 0.5f), 0, y - (mazeSize.y * 0.5f)); // Centers the node around (0,0) instead of bottom corner.
                MazeNode node = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);
                nodes.Add(node);

                yield return null;
            }
        }

        List<MazeNode> path = new List<MazeNode>();
        List<MazeNode> completedNodes = new List<MazeNode>();

        // Set start node to random node.
        path.Add(nodes[UnityEngine.Random.Range(0, nodes.Count)]);
        path[0].SetState(ENodeState.CURRENT);

        while(completedNodes.Count < nodes.Count)
        {
            // Check neighbour nodes.
            List<int> possibleNextNodes = new List<int>();
            List<int> possibleDirs = new List<int>();

            // Get pos of node in list of generated nodes, to check adjacent nodes.
            int currentNodeIdx = nodes.IndexOf(path[path.Count - 1]);
            int currentNodeX = currentNodeIdx / mazeSize.y;
            int currentNodeY = currentNodeIdx % mazeSize.y;

            // Check if it is on the right wall, if so there is none next to it so it shouldn't go through the check.
            if (currentNodeX < mazeSize.x - 1)
            {
                // Check node to right of current node.
                // Checking plus size.y since we are iterating through the y-axis.
                if (!completedNodes.Contains(nodes[currentNodeIdx + mazeSize.y]) &&
                    !path.Contains(nodes[currentNodeIdx + mazeSize.y]))
                {
                    // Node is available. Continue...
                    possibleDirs.Add(1); // 1 = Pos X. 2 = Neg X. 3 = Pos Z. 4 = Neg Z.
                    possibleNextNodes.Add(currentNodeIdx + mazeSize.y);
                }
            }

            // Check left wall.
            if (currentNodeX > 0)
            {
                // Check node to left of current node.
                if (!completedNodes.Contains(nodes[currentNodeIdx - mazeSize.y]) &&
                    !path.Contains(nodes[currentNodeIdx - mazeSize.y]))
                {
                    possibleDirs.Add(2);
                    possibleNextNodes.Add(currentNodeIdx - mazeSize.y);
                }
            }

            // Check top wall.
            if (currentNodeY < mazeSize.y - 1)
            {
                // Check node above current node.
                if (!completedNodes.Contains(nodes[currentNodeIdx + 1]) &&
                    !path.Contains(nodes[currentNodeIdx + 1]))
                {
                    possibleDirs.Add(3);
                    possibleNextNodes.Add(currentNodeIdx + 1);
                }
            }

            // Check bottom wall
            if (currentNodeY > 0)
            {
                // Check node below current node.
                if (!completedNodes.Contains(nodes[currentNodeIdx - 1]) &&
                    !path.Contains(nodes[currentNodeIdx - 1]))
                {
                    possibleDirs.Add(4);
                    possibleNextNodes.Add(currentNodeIdx - 1);
                }
            }

            // Choose the next node.
            // Check if there is a possible direction to move in.
            if (possibleDirs.Count > 0)
            {
                int chosenDir = UnityEngine.Random.Range(0, possibleDirs.Count);
                MazeNode chosenNode = nodes[possibleNextNodes[chosenDir]];

                switch (possibleDirs[chosenDir])
                {
                    case 1:
                        chosenNode.SetWallActive(1, false); // Remove left wall of neighbour node
                        path[path.Count - 1].SetWallActive(0, false); // Remove right wall of current node
                        break;
                    case 2:
                        chosenNode.SetWallActive(0, false); // Remove right wall of neighbour
                        path[path.Count - 1].SetWallActive(1, false); // Remove left wall of current node
                        break;
                    case 3:
                        chosenNode.SetWallActive(3, false);  // Remove bot wall of neighbour node
                        path[path.Count - 1].SetWallActive(2, false); // Remove top wall of current node
                        break;
                    case 4:
                        chosenNode.SetWallActive(2, false); // Remove top wall of neighbour node
                        path[path.Count - 1].SetWallActive(3, false); // Remove bot wall of current node
                        break;
                }

                path.Add(chosenNode);
                chosenNode.SetState(ENodeState.CURRENT);
            }
            else
            {
                // Backtrack if there are no nodes left. Go back to a node which had possible dirs.
                completedNodes.Add(path[path.Count - 1]);
                path[path.Count - 1].SetState(ENodeState.COMPLETED);
                path.RemoveAt(path.Count - 1);
            }

            yield return new WaitForSeconds(generationTick);
        }
    }
}
