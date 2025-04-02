using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

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
        protected Connection _actualConnection2;
        protected float xOffset;
        protected float yOffset;
        protected float distanceThreshold;
        protected GameObject tempConnection;
        protected GameObject tempConnection2;
        protected RaycastHit _currentHit;
        protected RaycastHit _currentHit2;
        protected bool _containsNode;
        protected int horizontalNehighbours;
        protected int verticalNeighbours;

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
            xOffset = (float)_parameters.nodesMatrixSize.x / ((float)_parameters.numberOfNodes.x - 1.0f);
            yOffset = (float)_parameters.nodesMatrixSize.y / ((float)_parameters.numberOfNodes.y - 1.0f);

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
                    tempPos += new Vector3(0.0f, 0.0f, xOffset);
                }

                tempPos.z = _internalData.startNode.transform.position.z;
                tempPos += new Vector3(yOffset, 0.0f, 0.0f);
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
            xOffset = (float)_parameters.nodesMatrixSize.x / ((float)_parameters.numberOfNodes.x - 1.0f);
            yOffset = (float)_parameters.nodesMatrixSize.y / ((float)_parameters.numberOfNodes.y - 1.0f);

            distanceThreshold = Mathf.Sqrt(
                    Mathf.Pow(xOffset, 2.0f) + 
                    Mathf.Pow(yOffset, 2.0f)
                );

            for (int i = 0; i < _internalData.nodes.Count; ++i)
            {
                if (_internalData.nodes[i].nodeState == NodeStates.HABILITADO)
                {
                    actualNode = _internalData.nodes[i];
                    for(int j = 0; j < _internalData.nodes.Count; ++j)
                    {
                        if ((actualNode != _internalData.nodes[j]) && 
                            (_internalData.nodes[j].nodeState == NodeStates.HABILITADO))
                        {
                            if (Vector3.Distance(actualNode.transform.position,
                                _internalData.nodes[j].transform.position) <= distanceThreshold)
                            {
                                //TODO: Validar si ya existía conexión entre ellos.
                                if(_internalData.connections.Count > 0)
                                {
                                    _containsNode = false;
                                    foreach(Connection connection in _internalData.connections)
                                    {
                                        if(connection.ContainsNode(actualNode) && !connection.ContainsNode(_internalData.nodes[j]))
                                        {
                                            _containsNode = false;
                                        }
                                        else if(!connection.ContainsNode(actualNode) && !connection.ContainsNode(_internalData.nodes[j]))
                                        {
                                            _containsNode = false;
                                        }
                                        else if (connection.ContainsNode(actualNode) && connection.ContainsNode(_internalData.nodes[j]))
                                        {
                                            _containsNode = true;
                                            break;
                                        }
                                    }
                                    if (!_containsNode)
                                    {
                                        if (Physics.Linecast(actualNode.transform.position, _internalData.nodes[j].transform.position,
                                            out _currentHit))
                                        {
                                            if (_currentHit.collider.gameObject.layer != LayerMask.NameToLayer("Obstacle"))
                                            {
                                                if (Physics.Linecast(_internalData.nodes[j].transform.position, actualNode.transform.position,
                                                    out _currentHit2))
                                                {
                                                    if (_currentHit2.collider.gameObject.layer != LayerMask.NameToLayer("Obstacle"))
                                                    {
                                                        tempConnection = Instantiate(_internalData.connectionPrefab);
                                                        _actualConnection = tempConnection.GetComponent<Connection>();
                                                        _actualConnection.NodeA = actualNode;
                                                        _actualConnection.NodeB = _internalData.nodes[j];
                                                        _actualConnection.DistanceBetweenNodes = (_actualConnection.NodeB.transform.position -
                                                            _actualConnection.NodeA.transform.position).magnitude;
                                                        _actualConnection.gameObject.transform.SetParent(this.gameObject.transform.GetChild(1), true);
                                                        actualNode.Connections.Add(_actualConnection);
                                                        _internalData.nodes[j].Connections.Add(_actualConnection);
                                                        _internalData.connections.Add(_actualConnection);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (Physics.Linecast(actualNode.transform.position, _internalData.nodes[j].transform.position,
                                    out _currentHit))
                                    {
                                        if (_currentHit.collider.gameObject.layer != LayerMask.NameToLayer("Obstacle"))
                                        {
                                            if (Physics.Linecast(_internalData.nodes[j].transform.position, actualNode.transform.position,
                                                out _currentHit2))
                                            {
                                                if (_currentHit.collider.gameObject.layer != LayerMask.NameToLayer("Obstacle"))
                                                {
                                                    tempConnection = Instantiate(_internalData.connectionPrefab);
                                                    _actualConnection = tempConnection.GetComponent<Connection>();
                                                    _actualConnection.NodeA = actualNode;
                                                    _actualConnection.NodeB = _internalData.nodes[j];
                                                    _actualConnection.DistanceBetweenNodes = (_actualConnection.NodeB.transform.position -
                                                        _actualConnection.NodeA.transform.position).magnitude;
                                                    _actualConnection.gameObject.transform.SetParent(this.gameObject.transform.GetChild(1), true);

                                                    actualNode.Connections.Add(_actualConnection);
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
                horizontalNehighbours = 0;
                verticalNeighbours = 0;
                foreach(Connection connection in node.Connections)
                {
                    //if(node)
                }
            }
        }

        #endregion
    }
}
