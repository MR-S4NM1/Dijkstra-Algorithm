using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace MrSanmi.DijkstraAlgorithm
{
    [System.Serializable]
    public struct DijkstraParameters
    {
        [SerializeField] public Vector2 nodesMatrixSize;
        [SerializeField] public Vector2 numberOfNodes;
    }

    [System.Serializable]
    public struct DijkstraInternalData
    {
        [SerializeField] public Transform startPosition;
        [SerializeField] public Node startNode;

        [Space]
        [SerializeField] public Transform endPosition;
        [SerializeField] public Node endNode;

        [Space]
        [SerializeField] public Transform pivot;
        [SerializeField] public List<Node> nodes;
        [SerializeField] public GameObject nodePrefab;

        [Space]
        [SerializeField] public GameObject connectionPrefab;
        [SerializeField] public List<Connection> connections;
    }
    public class Dijkstra : MonoBehaviour
    {
        #region References
        [Header("Parameters")]
        [SerializeField] protected DijkstraParameters _parameters;

        [Header("Internal Data")]
        [SerializeField] protected DijkstraInternalData _internalData;

        [Header("Node Instance")]
        [SerializeField] public GameObject nodeInstance;
        #endregion

        #region Knobs

        [SerializeField] protected LayerMask _layerMask;

        #endregion

        #region RuntimeVariables

        protected Node actualNode;
        protected Connection _actualConnection;
        protected Connection _actualConnectionA;
        protected Connection _actualConnectionB;
        protected float _xOffset;
        protected float _yOffset;
        protected float _distanceThreshold;
        protected GameObject _tempConnection;
        protected RaycastHit _currentHit;
        protected bool _containsNode;

        #endregion

        #region EditorButtons
        public void SetIconToThisGameObject()
        {
            IconManager.SetIcon(gameObject, IconManager.LabelIcon.Green);
        }

        public void GenerateNodes()
        {
            ClearAll();

            Vector3 tempPos = _internalData.startPosition.position;
            _xOffset = (float)_parameters.nodesMatrixSize.x / ((float)_parameters.numberOfNodes.x - 1.0f);
            _yOffset = (float)_parameters.nodesMatrixSize.y / ((float)_parameters.numberOfNodes.y - 1.0f);

            for(float i = 0; i < _parameters.numberOfNodes.y; ++i)
            {
                for(float j = 0; j < _parameters.numberOfNodes.x; ++j)
                {
                    nodeInstance = Instantiate(_internalData.nodePrefab);
                    actualNode = nodeInstance.GetComponent<Node>();
                    _internalData.nodes.Add(actualNode);
                    nodeInstance.transform.position = tempPos;

                    if (i == 0 && j == 0)
                    {
                        _internalData.startNode = actualNode;
                        actualNode.gameObject.tag = "InitialNode";
                    }

                    Collider[] hitColliders = Physics.OverlapSphere(actualNode.transform.position, 0.5f);

                    IconManager.SetIcon(nodeInstance, IconManager.LabelIcon.Green);
                    actualNode.nodeState = NodeStates.HABILITADO;

                    if (hitColliders.Length > 0)
                    {
                        foreach(Collider collider in hitColliders)
                        {
                            if (collider.gameObject.layer == 6)
                            {
                                IconManager.SetIcon(nodeInstance, IconManager.LabelIcon.Red);
                                actualNode.nodeState = NodeStates.DESHABILITADO;
                                break;
                            }
                        }
                    }
                    actualNode.gameObject.name = "Node " + "(" + actualNode.gameObject.transform.position.x.ToString("F2") + ", "
                        + actualNode.gameObject.transform.position.z.ToString("F2") + ")" + '\n' + actualNode.nodeState.ToString();
                    nodeInstance.transform.SetParent(this.gameObject.transform.GetChild(0), true);
                    tempPos += new Vector3(0.0f, 0.0f, _xOffset);
                }

                tempPos.z = _internalData.startNode.transform.position.z;
                tempPos += new Vector3(_yOffset, 0.0f, 0.0f);
            }

            _internalData.nodes.Add(_internalData.endNode);
            actualNode = null;
        }

        public void ClearAll()
        {
            foreach (Node node in transform.GetChild(0).GetComponentsInChildren<Node>())
            {
                if (!node.gameObject.CompareTag("FinalNode"))
                {
                    DestroyImmediate(node.gameObject);
                }
            }
            _internalData.nodes.Clear();

            foreach (Connection connection in transform.GetChild(1).GetComponentsInChildren<Connection>())
            {
                DestroyImmediate(connection.gameObject);
            }
            _internalData.connections.Clear();
        }

        public void GenerateGraph()
        {
            //TODO: CAMBIAR LA HIPOTENUSA POR SI NO ES IGUAL EL ALTO Y LARGO
            _xOffset = (float)_parameters.nodesMatrixSize.x / ((float)_parameters.numberOfNodes.x - 1.0f);
            _yOffset = (float)_parameters.nodesMatrixSize.y / ((float)_parameters.numberOfNodes.y - 1.0f);

            _distanceThreshold = Mathf.Sqrt(
                    Mathf.Pow(_xOffset, 2.0f) + 
                    Mathf.Pow(_yOffset, 2.0f)
                );

            foreach (Node node in _internalData.nodes)
            {
                if (node.nodeState == NodeStates.HABILITADO)
                {
                    for(int j = 0; j < _internalData.nodes.Count; ++j)
                    {
                        if ((node != _internalData.nodes[j]) && 
                            (_internalData.nodes[j].nodeState == NodeStates.HABILITADO))
                        {
                            if (Vector3.Distance(node.transform.position,
                                _internalData.nodes[j].transform.position) <= _distanceThreshold)
                            {
                                //TODO: Validar si ya existía conexión entre ellos.
                                if(_internalData.connections.Count > 0)
                                {
                                    _containsNode = false;
                                    foreach(Connection connection in _internalData.connections)
                                    {
                                        if (connection.ContainsNode(node) && connection.ContainsNode(_internalData.nodes[j]))
                                        {
                                            _containsNode = true;
                                            break;
                                        }
                                    }
                                    if (!_containsNode)
                                    {
                                        if (Physics.Raycast(node.transform.position, _internalData.nodes[j].transform.position - node.transform.position,
                                            out _currentHit, _distanceThreshold))
                                        {
                                            if (_currentHit.collider.gameObject.layer != LayerMask.NameToLayer("Obstacle"))
                                            {
                                                if (Physics.Raycast(_internalData.nodes[j].transform.position, node.transform.position - _internalData.nodes[j].transform.position,
                                                    out _currentHit, _distanceThreshold))
                                                {
                                                    if (_currentHit.collider.gameObject.layer != LayerMask.NameToLayer("Obstacle"))
                                                    {
                                                        _tempConnection = Instantiate(_internalData.connectionPrefab);
                                                        _actualConnection = _tempConnection.GetComponent<Connection>();
                                                        _actualConnection.NodeA = node;
                                                        _actualConnection.NodeB = _internalData.nodes[j];
                                                        _actualConnection.DistanceBetweenNodes = (_actualConnection.NodeB.transform.position -
                                                            _actualConnection.NodeA.transform.position).magnitude;
                                                        _actualConnection.gameObject.transform.SetParent(this.gameObject.transform.GetChild(1), true);
                                                        node.Connections.Add(_actualConnection);
                                                        _internalData.nodes[j].Connections.Add(_actualConnection);
                                                        _internalData.connections.Add(_actualConnection);

                                                        if ((_internalData.nodes[j].transform.position - node.transform.position).normalized ==
                                                            Vector3.forward)
                                                        {
                                                            _actualConnection.connectionType = ConnectionDirection.HORIZONTAL;
                                                        }
                                                        else if ((_internalData.nodes[j].transform.position - node.transform.position).normalized ==
                                                            Vector3.right)
                                                        {
                                                            _actualConnection.connectionType = ConnectionDirection.VERTICAL;
                                                        }
                                                        else if ((Vector3.Dot((_internalData.nodes[j].transform.position - node.transform.position).normalized, Vector3.forward) >= 0.5f) &&
                                                            (Vector3.Dot((_internalData.nodes[j].transform.position - node.transform.position).normalized, Vector3.right) >= 0.5f))
                                                        {
                                                            _actualConnection.connectionType = ConnectionDirection.LEFT_DIAGONAL;
                                                        }
                                                        else if ((Vector3.Dot((_internalData.nodes[j].transform.position - node.transform.position).normalized, Vector3.forward) <= -0.5f) &&
                                                            (Vector3.Dot((_internalData.nodes[j].transform.position - node.transform.position).normalized, Vector3.right) >= 0.5f))
                                                        {
                                                            _actualConnection.connectionType = ConnectionDirection.RIGHT_DIAGONAL;
                                                        }
                                                        else
                                                        {
                                                            _actualConnection.connectionType = ConnectionDirection.IRREGULAR_DIAGONAL;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (Physics.Raycast(node.transform.position, _internalData.nodes[j].transform.position - node.transform.position,
                                            out _currentHit, _distanceThreshold))
                                    {
                                        if (_currentHit.collider.gameObject.layer != LayerMask.NameToLayer("Obstacle"))
                                        {
                                            if (Physics.Raycast(_internalData.nodes[j].transform.position, node.transform.position - _internalData.nodes[j].transform.position,
                                                out _currentHit, _distanceThreshold))
                                            {
                                                if (_currentHit.collider.gameObject.layer != LayerMask.NameToLayer("Obstacle"))
                                                {
                                                    _tempConnection = Instantiate(_internalData.connectionPrefab);
                                                    _actualConnection = _tempConnection.GetComponent<Connection>();
                                                    _actualConnection.NodeA = node;
                                                    _actualConnection.NodeB = _internalData.nodes[j];
                                                    _actualConnection.DistanceBetweenNodes = (_actualConnection.NodeB.transform.position -
                                                        _actualConnection.NodeA.transform.position).magnitude;
                                                    _actualConnection.gameObject.transform.SetParent(this.gameObject.transform.GetChild(1), true);
                                                    node.Connections.Add(_actualConnection);
                                                    _internalData.nodes[j].Connections.Add(_actualConnection);
                                                    _internalData.connections.Add(_actualConnection);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ReduceNodes()
        {
            foreach(Node node in _internalData.nodes)
            {
                if (node.nodeState == NodeStates.HABILITADO)
                {
                    if (node.Connections.Count == 2)
                    {
                        if (node.Connections[0].connectionType == node.Connections[1].connectionType)
                        {
                            _tempConnection = Instantiate(_internalData.connectionPrefab);
                            _actualConnection = _tempConnection.GetComponent<Connection>();

                            if (node.Connections[0].IsNodeA(node))
                            {
                                _actualConnection.NodeA = node.Connections[0].NodeB;
                                _actualConnection.NodeB = node.Connections[1].NodeA;
                            }
                            else
                            {
                                _actualConnection.NodeA = node.Connections[0].NodeA;
                                _actualConnection.NodeB = node.Connections[1].NodeB;
                            }

                            _actualConnection.DistanceBetweenNodes = (_actualConnection.NodeB.transform.position -
                                _actualConnection.NodeA.transform.position).magnitude;

                            _actualConnection.gameObject.transform.SetParent(this.gameObject.transform.GetChild(1), true);

                            #region NewConnectionDirection
                            if ((_actualConnection.NodeB.transform.position - _actualConnection.NodeA.transform.position).normalized ==
                                Vector3.forward)
                            {
                                _actualConnection.connectionType = ConnectionDirection.HORIZONTAL;
                            }
                            else if ((_actualConnection.NodeB.transform.position - _actualConnection.NodeA.transform.position).normalized ==
                                Vector3.right)
                            {
                                _actualConnection.connectionType = ConnectionDirection.VERTICAL;
                            }
                            else if ((Vector3.Dot((_actualConnection.NodeB.transform.position - _actualConnection.NodeA.transform.position).normalized, Vector3.forward) >= 0.5f) &&
                                (Vector3.Dot((_actualConnection.NodeB.transform.position - _actualConnection.NodeA.transform.position).normalized, Vector3.right) >= 0.5f))
                            {
                                _actualConnection.connectionType = ConnectionDirection.LEFT_DIAGONAL;
                            }
                            else if ((Vector3.Dot((_actualConnection.NodeB.transform.position - _actualConnection.NodeA.transform.position).normalized, Vector3.forward) <= -0.5f) &&
                                (Vector3.Dot((_actualConnection.NodeB.transform.position - _actualConnection.NodeA.transform.position).normalized, Vector3.right) >= 0.5f))
                            {
                                _actualConnection.connectionType = ConnectionDirection.RIGHT_DIAGONAL;
                            }
                            else
                            {
                                _actualConnection.connectionType = ConnectionDirection.IRREGULAR_DIAGONAL;
                            }
                            #endregion

                            node.nodeState = NodeStates.DESHABILITADO;
                            node.gameObject.SetActive(false);
                            _internalData.connections.Remove(node.Connections[0]);
                            _internalData.connections.Remove(node.Connections[1]);
                            _internalData.connections.Add(_actualConnection);

                            _actualConnection.NodeA.Connections.Add(_actualConnection);
                            _actualConnection.NodeB.Connections.Add(_actualConnection);

                            DestroyImmediate(node.Connections[1].gameObject);
                            DestroyImmediate(node.Connections[0].gameObject);
                        }
                        _actualConnection.NodeA.Connections.RemoveAll(item => item == null);
                        _actualConnection.NodeB.Connections.RemoveAll(item => item == null);
                    }
                    else if(node.Connections.Count == 8)
                    {
                        foreach (Connection connectionA in node.Connections)
                        {
                            if (connectionA != null)
                            {
                                _actualConnectionA = connectionA;
                                for (int j = 0; j < node.Connections.Count; ++j)
                                {
                                    if (node.Connections[j] != null)
                                    {
                                        _actualConnectionB = node.Connections[j];

                                        if (_actualConnectionA != _actualConnectionB)
                                        {
                                            if ((_actualConnectionA.connectionType == ConnectionDirection.HORIZONTAL) &&
                                                (_actualConnectionB.connectionType == ConnectionDirection.HORIZONTAL))
                                            {

                                                Debug.Log($"{_actualConnectionA.gameObject.name} ({_actualConnectionA.gameObject.transform.position}) - " +
                                                    $"{_actualConnectionB.gameObject.name} - ({_actualConnectionB.gameObject.transform.position})");
                                                _tempConnection = Instantiate(_internalData.connectionPrefab);
                                                _actualConnection = _tempConnection.GetComponent<Connection>();

                                                if (_actualConnectionA.IsNodeA(node))
                                                {
                                                    _actualConnection.NodeA = _actualConnectionA.NodeB;
                                                    _actualConnection.NodeB = _actualConnectionB.NodeA;
                                                }
                                                else
                                                {
                                                    _actualConnection.NodeA = _actualConnectionA.NodeA;
                                                    _actualConnection.NodeB = _actualConnectionB.NodeB;
                                                }

                                                _actualConnection.DistanceBetweenNodes = (_actualConnection.NodeB.transform.position -
                                                    _actualConnection.NodeA.transform.position).magnitude;

                                                _actualConnection.gameObject.transform.SetParent(this.gameObject.transform.GetChild(1), true);

                                                _actualConnection.connectionType = ConnectionDirection.HORIZONTAL;

                                                _internalData.connections.Remove(node.Connections[j]);
                                                _internalData.connections.Add(_actualConnection);

                                                _actualConnection.NodeA.Connections.Add(_actualConnection);
                                                _actualConnection.NodeB.Connections.Add(_actualConnection);

                                                _internalData.connections.Remove(connectionA);
                                                DestroyImmediate(connectionA.gameObject);
                                                DestroyImmediate(node.Connections[j].gameObject);
                                            }
                                            else if ((_actualConnectionA.connectionType == ConnectionDirection.VERTICAL) &&
                                                (_actualConnectionB.connectionType == ConnectionDirection.VERTICAL))
                                            {

                                                Debug.Log($"{_actualConnectionA.gameObject.name} ({_actualConnectionA.gameObject.transform.position}) - " +
                                                    $"{_actualConnectionB.gameObject.name} - ({_actualConnectionB.gameObject.transform.position})");
                                                _tempConnection = Instantiate(_internalData.connectionPrefab);
                                                _actualConnection = _tempConnection.GetComponent<Connection>();

                                                if (_actualConnectionA.IsNodeA(node))
                                                {
                                                    _actualConnection.NodeA = _actualConnectionA.NodeB;
                                                    _actualConnection.NodeB = _actualConnectionB.NodeA;
                                                }
                                                else
                                                {
                                                    _actualConnection.NodeA = _actualConnectionA.NodeA;
                                                    _actualConnection.NodeB = _actualConnectionB.NodeB;
                                                }

                                                _actualConnection.DistanceBetweenNodes = (_actualConnection.NodeB.transform.position -
                                                    _actualConnection.NodeA.transform.position).magnitude;

                                                _actualConnection.gameObject.transform.SetParent(this.gameObject.transform.GetChild(1), true);

                                                _actualConnection.connectionType = ConnectionDirection.VERTICAL;

                                                _internalData.connections.Remove(node.Connections[j]);
                                                _internalData.connections.Add(_actualConnection);

                                                _actualConnection.NodeA.Connections.Add(_actualConnection);
                                                _actualConnection.NodeB.Connections.Add(_actualConnection);

                                                _internalData.connections.Remove(connectionA);
                                                DestroyImmediate(connectionA.gameObject);
                                                DestroyImmediate(node.Connections[j].gameObject);
                                            }
                                            else if ((_actualConnectionA.connectionType == ConnectionDirection.RIGHT_DIAGONAL) &&
                                                (_actualConnectionB.connectionType == ConnectionDirection.RIGHT_DIAGONAL))
                                            {

                                                Debug.Log($"{_actualConnectionA.gameObject.name} ({_actualConnectionA.gameObject.transform.position}) - " +
                                                    $"{_actualConnectionB.gameObject.name} - ({_actualConnectionB.gameObject.transform.position})");
                                                _tempConnection = Instantiate(_internalData.connectionPrefab);
                                                _actualConnection = _tempConnection.GetComponent<Connection>();

                                                if (_actualConnectionA.IsNodeA(node))
                                                {
                                                    _actualConnection.NodeA = _actualConnectionA.NodeB;
                                                    _actualConnection.NodeB = _actualConnectionB.NodeA;
                                                }
                                                else
                                                {
                                                    _actualConnection.NodeA = _actualConnectionA.NodeA;
                                                    _actualConnection.NodeB = _actualConnectionB.NodeB;
                                                }

                                                _actualConnection.DistanceBetweenNodes = (_actualConnection.NodeB.transform.position -
                                                    _actualConnection.NodeA.transform.position).magnitude;

                                                _actualConnection.gameObject.transform.SetParent(this.gameObject.transform.GetChild(1), true);

                                                _actualConnection.connectionType = ConnectionDirection.RIGHT_DIAGONAL;

                                                _internalData.connections.Remove(node.Connections[j]);
                                                _internalData.connections.Add(_actualConnection);

                                                _actualConnection.NodeA.Connections.Add(_actualConnection);
                                                _actualConnection.NodeB.Connections.Add(_actualConnection);

                                                _internalData.connections.Remove(connectionA);
                                                DestroyImmediate(connectionA.gameObject);
                                                DestroyImmediate(node.Connections[j].gameObject);
                                            }
                                            else if ((_actualConnectionA.connectionType == ConnectionDirection.LEFT_DIAGONAL) &&
                                                (_actualConnectionB.connectionType == ConnectionDirection.LEFT_DIAGONAL))
                                            {

                                                Debug.Log($"{_actualConnectionA.gameObject.name} ({_actualConnectionA.gameObject.transform.position}) - " +
                                                    $"{_actualConnectionB.gameObject.name} - ({_actualConnectionB.gameObject.transform.position})");
                                                _tempConnection = Instantiate(_internalData.connectionPrefab);
                                                _actualConnection = _tempConnection.GetComponent<Connection>();

                                                if (_actualConnectionA.IsNodeA(node))
                                                {
                                                    _actualConnection.NodeA = _actualConnectionA.NodeB;
                                                    _actualConnection.NodeB = _actualConnectionB.NodeA;
                                                }
                                                else
                                                {
                                                    _actualConnection.NodeA = _actualConnectionA.NodeA;
                                                    _actualConnection.NodeB = _actualConnectionB.NodeB;
                                                }

                                                _actualConnection.DistanceBetweenNodes = (_actualConnection.NodeB.transform.position -
                                                    _actualConnection.NodeA.transform.position).magnitude;

                                                _actualConnection.gameObject.transform.SetParent(this.gameObject.transform.GetChild(1), true);

                                                _actualConnection.connectionType = ConnectionDirection.LEFT_DIAGONAL;

                                                _internalData.connections.Remove(node.Connections[j]);
                                                _internalData.connections.Add(_actualConnection);

                                                _actualConnection.NodeA.Connections.Add(_actualConnection);
                                                _actualConnection.NodeB.Connections.Add(_actualConnection);

                                                _internalData.connections.Remove(connectionA);
                                                DestroyImmediate(connectionA.gameObject);
                                                DestroyImmediate(node.Connections[j].gameObject);
                                            }
                                            _actualConnection.NodeA.Connections.RemoveAll(item => item == null);
                                            _actualConnection.NodeB.Connections.RemoveAll(item => item == null);
                                        }
                                    }
                                }
                            }
                            node.nodeState = NodeStates.DESHABILITADO;
                            node.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        #endregion

        #region LocalMethods

        #endregion
    }
}
