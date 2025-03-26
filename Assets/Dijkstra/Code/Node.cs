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

        #region GettersAndSetters

        public List<Connection> Connections
        {
            get { return connections; }
            set { connections = value; }
        }

        #endregion
    }
}
