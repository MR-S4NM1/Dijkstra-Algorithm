using UnityEngine;

namespace MrSanmi.DijkstraAlgorithm
{
    public enum NodeStates
    {
        HABILITADO,
        DESHABILITADO
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
