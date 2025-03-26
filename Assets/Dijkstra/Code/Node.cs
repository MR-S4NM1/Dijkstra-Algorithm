using System.Collections.Generic;
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
        #region InternalData

        [SerializeField] public NodeStates nodeState;
        [SerializeField] protected List<Connection> connections;

        #endregion
    }
}
