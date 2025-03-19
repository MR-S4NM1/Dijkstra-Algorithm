using UnityEngine;

namespace MrSanmi.DijkstraAlgorithm
{
    public enum NodeStates
    {
        CONNECTABLE,
        NOT_CONNECTABLE
    }
    public class Node : MonoBehaviour
    {
        [SerializeField] public NodeStates _nodeState;

        void Start()
        {

        }

        void Update()
        {

        }
    }
}
