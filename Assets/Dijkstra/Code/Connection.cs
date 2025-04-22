using UnityEngine;
using System.Collections;

namespace MrSanmi.DijkstraAlgorithm
{
    [System.Serializable]
    public struct ConnectionInternalData
    {
        [SerializeField] public Node nodeA;
        [SerializeField] public Node nodeB;
        [SerializeField] public float distanceBetweenNodes;
    }

    [System.Serializable]
    public struct ConnectionDebug
    {
        [SerializeField] public GameObject _debugNodeA;
        [SerializeField] public GameObject _debugNodeB;
        [SerializeField] public GameObject _debugDistanceBetweenNodes;
    }

    public enum ConnectionDirection
    {
        LEFT_DIAGONAL,
        IRREGULAR_DIAGONAL,
        RIGHT_DIAGONAL,
        HORIZONTAL,
        VERTICAL
    }

    public class Connection : MonoBehaviour
    {
        #region InternalData

        [SerializeField] public ConnectionDirection connectionType;
        [SerializeField] protected ConnectionInternalData _internalData;

        #endregion

        #region Debug

        [SerializeField] protected ConnectionDebug _debug;

        #endregion

        #region RuntimeVariables

        protected Vector3 _origin;
        protected Vector3 _directionAndMagnitude;

        #endregion

        private void OnDrawGizmos()
        {
            if (_internalData.nodeA != null && _internalData.nodeB != null)
            {
                // We have a connection between both nodes :P

                _origin = _internalData.nodeA.transform.position;
                _directionAndMagnitude = _internalData.nodeB.transform.position - _origin;
                Debug.DrawRay(_origin, _directionAndMagnitude, Color.blue);

                _internalData.distanceBetweenNodes = _directionAndMagnitude.magnitude;
                transform.position = _origin + _directionAndMagnitude / 2.0f;

                _debug._debugDistanceBetweenNodes.name = $"D: {_internalData.distanceBetweenNodes}";
                _debug._debugNodeA.name = $"NA: {_internalData.nodeA.name}";
                _debug._debugNodeB.name = $"NB: {_internalData.nodeB.name}";
            }
        }

        #region PublicMethods

        public Node OtherNode(Node value)
        {
            if (value == _internalData.nodeA || value == _internalData.nodeB)
            {
                if (value == _internalData.nodeA)
                {
                    return _internalData.nodeB;
                }
                else
                {
                    return _internalData.nodeA;
                }
            }
            Debug.LogError($" {this.name} {gameObject.name} - Node {value.name} is asking for a connection " +
                $"not valid with {_internalData.nodeA.name} - {_internalData.nodeB.name}.", gameObject);
            return null;
        }

        public bool ContainsNode(Node value)
        {
            bool itContainsIt;
            if (value == _internalData.nodeA || value == _internalData.nodeB)
            {
                itContainsIt = true;
            }
            else
            {
                itContainsIt = false;
            }
            return itContainsIt;
        }

        public bool IsNodeA(Node value)
        {
            bool itIsNodeA;
            if (value == _internalData.nodeA)
            {
                itIsNodeA = true;
            }
            else
            {
                itIsNodeA = false;
            }
            return itIsNodeA;
        }

        #endregion

        #region GettersAndSetters

        public Node NodeA
        {
            get { return _internalData.nodeA; }
            set { _internalData.nodeA = value; }
        }

        public Node NodeB
        {
            get { return _internalData.nodeB; }
            set { _internalData.nodeB = value; }
        }

        public float DistanceBetweenNodes
        {
            get { return _internalData.distanceBetweenNodes; }
            set { _internalData.distanceBetweenNodes = value; }
        }

        #endregion
    }
}