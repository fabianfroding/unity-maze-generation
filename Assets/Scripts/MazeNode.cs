using UnityEngine;

public enum ENodeState { AVAILABLE, CURRENT, COMPLETED }

public class MazeNode : MonoBehaviour
{
    [SerializeField] GameObject[] walls;
    [SerializeField] MeshRenderer floor;        // Used to show the state of the node.
    
    [SerializeField] public Vector2 mazePos;
    [SerializeField] public bool isVisited;

    public void SetMazePos(Vector2 pos)
    {
        this.mazePos = pos;
    }

    public void SetState(ENodeState nodeState)
    {
        switch (nodeState)
        {
            case ENodeState.AVAILABLE:
                floor.material.color = Color.white;
                break;
            case ENodeState.CURRENT:
                floor.material.color = Color.blue;
                break;
            case ENodeState.COMPLETED:
                floor.material.color = Color.green;
                break;
        }
    }

    public void SetWallActive(int wallIdx, bool active) => walls[wallIdx].gameObject.SetActive(active);
}
