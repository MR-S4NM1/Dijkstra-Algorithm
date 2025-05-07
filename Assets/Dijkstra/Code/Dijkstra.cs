using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Mr_Sanmi.AI_Agents;
using UnityEditor.ShaderGraph.Internal;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

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
        [SerializeField] public Transform layoutPivot;
        [SerializeField] public List<Node> nodes;
        [SerializeField] public GameObject nodePrefab;
        [SerializeField] public List<int> totalNodesIDs;

        [Space]
        [SerializeField] public GameObject connectionPrefab;
        [SerializeField] public List<Connection> connections;

        [Space]
        [SerializeField] public List<Route> allRoutesList;
        [SerializeField] public List<Route> usefulRoutesList;
    }

    [System.Serializable]
    public struct Route
    {
        [SerializeField] public float totalDistance;
        //[SerializeField, HideInInspector] public List<Node> nodesOfThisRoute;
        [SerializeField] public List<int> nodesIDs;
    }

    [System.Serializable]
    public struct AgentData
    {
        [SerializeField] public GameObject agentPrefab;
        [SerializeField] public AIAgent_SO agentSO;
        [SerializeField] public List<Vector3> destinyPositions;
        [SerializeField] public float agentSpeed;
        [SerializeField] public Transform agentInitialPos;
        [SerializeField] public Transform agentFinalPos;
    }

    [System.Serializable]
    public struct FinalRoute
    {
        [SerializeField] public List<Vector3> _wayPoints; 
    }

    public class Dijkstra : MonoBehaviour
    {
        #region References
        [Header("Parameters")]
        [SerializeField] protected DijkstraParameters _parameters;

        [Header("Internal Data")]
        [SerializeField] protected DijkstraInternalData _internalData;

        [Header("Node Instance")]
        [SerializeField, HideInInspector] public GameObject nodeInstance;

        [Header("Final Route")]
        [SerializeField] public FinalRoute _finalRoute;

        [Header("Agent Data")]
        [SerializeField] protected AgentData _agentData;
        #endregion

        #region Knobs

        [SerializeField, HideInInspector] protected LayerMask _layerMask;

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
        //protected Route actualRoute;
        protected float _nearestNodeDistance;
        protected float minDistance;
        protected int index;
        //protected Route _actualRoute;
        //protected Route _usefulRoute;
        protected float _nearestStartNode;


        #endregion

        #region EditorButtons
        public void SetIconToThisGameObject()
        {
            IconManager.SetIcon(gameObject, IconManager.LabelIcon.Green); 
        }

        public void GenerateNodes()
        {
            ClearAll();

            Vector3 tempPos = _internalData.layoutPivot.position;
            _xOffset = (float)_parameters.nodesMatrixSize.x / ((float)_parameters.numberOfNodes.x - 1.0f);
            _yOffset = (float)_parameters.nodesMatrixSize.y / ((float)_parameters.numberOfNodes.y - 1.0f);

            for(float i = 0; i < _parameters.numberOfNodes.y; ++i)
            {
                for(float j = 0; j < _parameters.numberOfNodes.x; ++j)
                {
                    nodeInstance = Instantiate(_internalData.nodePrefab);
                    actualNode = nodeInstance.GetComponent<Node>();
                    _internalData.nodes.Add(actualNode);
                    _internalData.totalNodesIDs.Add(actualNode.InstanceID);
                    nodeInstance.transform.position = tempPos;

                    if(i == 0 && j == 0)
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
                    tempPos += new Vector3(0.0f, 0.0f, _xOffset);
                }

                tempPos.z = _internalData.startNode.transform.position.z;
                tempPos += new Vector3(_yOffset, 0.0f, 0.0f);
            }

            //_internalData.nodes.Add(_internalData.endNode);
            actualNode = null;

            _nearestStartNode = Mathf.Infinity;

            foreach (Node node in _internalData.nodes)
            {
                if (node.nodeState == NodeStates.HABILITADO)
                {
                    if (Vector3.Distance(node.gameObject.transform.position, _internalData.pivot.position) < _nearestStartNode)
                    {
                        _nearestStartNode = Vector3.Distance(node.gameObject.transform.position, _internalData.pivot.position);
                        _internalData.startNode = node;
                        _internalData.startNode.gameObject.tag = "InitialNode";
                    }
                }
            }
        }

        public void ClearAll()
        {
            foreach (Node node in transform.GetChild(0).GetComponentsInChildren<Node>())
            {
                DestroyImmediate(node.gameObject);
            }
            _internalData.nodes.Clear();
            _internalData.totalNodesIDs.Clear();

            foreach (Connection connection in transform.GetChild(1).GetComponentsInChildren<Connection>())
            {
                DestroyImmediate(connection.gameObject);
            }
            _internalData.connections.Clear();

            _internalData.allRoutesList.Clear();
            _internalData.usefulRoutesList.Clear();

            if(_finalRoute._wayPoints != null) _finalRoute._wayPoints.Clear();

            //Agent Data
            _agentData.destinyPositions.Clear();
            _agentData.agentPrefab = null;
            _agentData.agentSO.movingBehaviours.Clear();
            _agentData.agentSO.spawnParameters.rotation = Vector3.zero;
            _agentData.agentSO.spawnParameters.position = Vector3.zero;
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

            foreach (Node node in _internalData.nodes){
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

                                                        if (((_internalData.nodes[j].transform.position - node.transform.position).normalized ==
                                                            Vector3.forward) || ((_internalData.nodes[j].transform.position - node.transform.position).normalized ==
                                                            -Vector3.forward))
                                                        {
                                                            _actualConnection.connectionType = ConnectionDirection.HORIZONTAL;
                                                        }
                                                        else if (((_internalData.nodes[j].transform.position - node.transform.position).normalized ==
                                                            Vector3.right) || ((_internalData.nodes[j].transform.position - node.transform.position).normalized ==
                                                            Vector3.left))
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

            _nearestNodeDistance = Mathf.Infinity;

            foreach (Node node in _internalData.nodes){
                if (node.nodeState == NodeStates.HABILITADO){
                    if (Vector3.Distance(node.gameObject.transform.position, _internalData.endPosition.position) < _nearestNodeDistance){
                        _nearestNodeDistance = Vector3.Distance(node.gameObject.transform.position, _internalData.endPosition.position);
                        _internalData.endNode = node;
                    }
                }
            }

            _internalData.endNode.gameObject.tag = "FinalNode";
        }

        public void ReduceNodes()
        {
            foreach(Node node in _internalData.nodes)
            {
                if ((node.nodeState == NodeStates.HABILITADO && node.gameObject.activeInHierarchy) 
                    && !node.CompareTag("FinalNode"))
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
                            }
                            else
                            {
                                _actualConnection.NodeA = node.Connections[0].NodeA;
                            }

                            if (node.Connections[1].IsNodeA(node))
                            {
                                _actualConnection.NodeB = node.Connections[1].NodeB;
                            }
                            else
                            {
                                _actualConnection.NodeB = node.Connections[1].NodeA;
                            }

                            _actualConnection.DistanceBetweenNodes = (_actualConnection.NodeB.transform.position -
                                _actualConnection.NodeA.transform.position).magnitude;

                            _actualConnection.gameObject.transform.SetParent(this.gameObject.transform.GetChild(1), true);

                            _actualConnection.connectionType = node.Connections[0].connectionType;

                            node.Connections[0].gameObject.SetActive(false);
                            node.Connections[1].gameObject.SetActive(false);

                            _internalData.connections.Remove(node.Connections[0]); 
                            _internalData.connections.Remove(node.Connections[1]);

                            _actualConnection.NodeA.Connections.Remove(node.Connections[0]); 
                            _actualConnection.NodeB.Connections.Remove(node.Connections[1]);

                             
                            _internalData.connections.Add(_actualConnection);

                            _actualConnection.NodeA.Connections.Add(_actualConnection);
                            _actualConnection.NodeB.Connections.Add(_actualConnection);
                            
                            node.nodeState = NodeStates.DESHABILITADO;
                            node.gameObject.SetActive(false); 
                        }
                        _actualConnection.NodeA.Connections.RemoveAll(item => item == null);
                        _actualConnection.NodeB.Connections.RemoveAll(item => item == null);
                    }
                    else if(node.Connections.Count == 8)
                    {
                        foreach (Connection connectionA in node.Connections)
                        {
                            if (connectionA != null && connectionA.gameObject.activeInHierarchy)
                            {
                                _actualConnectionA = connectionA;
                                for (int j = 0; j < node.Connections.Count; ++j)
                                {
                                    if (node.Connections[j] != null && node.Connections[j].gameObject.activeInHierarchy)
                                    {
                                        _actualConnectionB = node.Connections[j];

                                        if (_actualConnectionA != _actualConnectionB)
                                        {
                                            if (_actualConnectionA.connectionType == _actualConnectionB.connectionType)
                                            {
                                                //Debug.Log($"{_actualConnectionA.gameObject.name} ({_actualConnectionA.gameObject.transform.position}) - " +
                                                //    $"{_actualConnectionB.gameObject.name} - ({_actualConnectionB.gameObject.transform.position})");
                                                _tempConnection = Instantiate(_internalData.connectionPrefab);
                                                _actualConnection = _tempConnection.GetComponent<Connection>();

                                                if (_actualConnectionA.IsNodeA(node))
                                                {
                                                    _actualConnection.NodeA = _actualConnectionA.NodeB;
                                                }
                                                else
                                                {
                                                    _actualConnection.NodeA = _actualConnectionA.NodeA;
                                                }

                                                if (_actualConnectionB.IsNodeA(node))
                                                {
                                                    _actualConnection.NodeB = _actualConnectionB.NodeB;
                                                }
                                                else
                                                {
                                                    _actualConnection.NodeB = _actualConnectionB.NodeA;
                                                }

                                                _actualConnection.DistanceBetweenNodes = (_actualConnection.NodeB.transform.position -
                                                    _actualConnection.NodeA.transform.position).magnitude;

                                                _actualConnection.gameObject.transform.SetParent(this.gameObject.transform.GetChild(1), true);

                                                _actualConnection.connectionType = _actualConnectionA.connectionType;

                                                _actualConnection.NodeA.Connections.Remove(_actualConnectionA);
                                                _actualConnection.NodeA.Connections.Remove(_actualConnectionB);

                                                _actualConnection.NodeB.Connections.Remove(_actualConnectionA);
                                                _actualConnection.NodeB.Connections.Remove(_actualConnectionB);

                                                _actualConnectionA.gameObject.SetActive(false);
                                                _actualConnectionB.gameObject.SetActive(false);

                                                _internalData.connections.Remove(_actualConnectionA);
                                                _internalData.connections.Remove(_actualConnectionB);

                                                _internalData.connections.Add(_actualConnection);

                                                _actualConnection.NodeA.Connections.Add(_actualConnection);
                                                _actualConnection.NodeB.Connections.Add(_actualConnection);
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

        protected void RecursivitySearch(Route p_previousRoute, /*Node p_node,*/ int p_nodeID, float distance)
        {
            #region FirstSolution
            //if (!p_previousRoute.nodesOfThisRoute.Contains(p_node))
            //{
            //    if (p_node == _internalData.endNode)
            //    {
            //        Route usefulRoute = new Route()
            //        {
            //            nodesOfThisRoute = new List<Node>(p_previousRoute.nodesOfThisRoute),
            //            totalDistance = (p_previousRoute.totalDistance + distance)
            //        };
            //        usefulRoute.nodesOfThisRoute.Add(_internalData.endNode);

            //        _internalData.allRoutesList.Add(usefulRoute);
            //        _internalData.usefulRoutesList.Add(usefulRoute);
            //        return; //Recursitivity breaker
            //    }

            //    //Debug.Log($"Ahhhh - {p_node == null} - {p_node.gameObject.name}");

            //    if (p_previousRoute.nodesOfThisRoute.Contains(p_node))
            //    {
            //        return; //Recursitivity breaker
            //    }

            //    //The node survived both conditions, so it is a candidate for a truncated route

            //    Route actualRoute = new Route()
            //    {
            //        nodesOfThisRoute = new List<Node>(p_previousRoute.nodesOfThisRoute),
            //        totalDistance = p_previousRoute.totalDistance + distance
            //    };

            //    actualRoute.nodesOfThisRoute.Add(p_node);
            //    _internalData.allRoutesList.Add(actualRoute);

            //    foreach (Connection connection in p_node.Connections)
            //    {
            //        if ((connection != null) && (connection.OtherNode(p_node) != null))
            //        {
            //            RecursivitySearch(actualRoute, connection.OtherNode(p_node), connection.DistanceBetweenNodes);
            //        }
            //    }
            //}
            #endregion

            #region SecondSolution
            if (p_nodeID == _internalData.endNode.InstanceID)
            {
                Route usefulRoute = new Route()
                {
                    //nodesOfThisRoute = new List<Node>(p_previousRoute.nodesOfThisRoute),
                    nodesIDs = new List<int>(p_previousRoute.nodesIDs),
                    totalDistance = (p_previousRoute.totalDistance + distance)
                };
                usefulRoute.nodesIDs.Add(_internalData.endNode.InstanceID);

                _internalData.allRoutesList.Add(usefulRoute);
                _internalData.usefulRoutesList.Add(usefulRoute);
                return; //Recursitivity breaker
            }

            if (p_previousRoute.nodesIDs.Contains(p_nodeID)) return;

            Route actualRoute = new Route()
            {
                nodesIDs = new List<int>(p_previousRoute.nodesIDs),
                totalDistance = p_previousRoute.totalDistance + distance
            };

            actualRoute.nodesIDs.Add(p_nodeID);
            _internalData.allRoutesList.Add(actualRoute);

            foreach (Connection connection in GetNode(p_nodeID).Connections)
            {
                if ((connection != null) && (connection.OtherNode(GetNode(p_nodeID)) != null))
                {
                    RecursivitySearch(actualRoute, connection.OtherNodeID(GetNode(p_nodeID)),
                        connection.DistanceBetweenNodes); 
                }
            }
            #endregion
        }

        public void SearchAllTheRoutes()
        {
            if(_internalData.allRoutesList.Count == 0)
            {
                //Route initialRoute = new Route()
                //{
                //    nodesOfThisRoute = new List<Node>(),
                //    totalDistance = 0.0f
                //};
                //initialRoute.nodesOfThisRoute.Add(_internalData.startNode);

                Route initialRoute = new Route()
                {
                    nodesIDs = new List<int>(),
                    totalDistance = 0.0f
                };
                initialRoute.nodesIDs.Add(_internalData.startNode.InstanceID);

                _internalData.allRoutesList.Add(initialRoute);

                foreach(Connection connection in _internalData.startNode.Connections)
                {
                    RecursivitySearch(initialRoute, connection.OtherNodeID(_internalData.startNode), 0f);
                    //RecursivitySearch(initialRoute, connection.OtherNode(_internalData.startNode), 0f);
                }

                //Debug.Log($"Number of Routes: { _internalData.allRoutesList.Count }");
                //Debug.Log($"Number of Useful Routes: {_internalData.usefulRoutesList.Count}");
            }
        }

        public void LookForTheBestRoute()
        {
            minDistance = Mathf.Infinity;
            index = 0;

            for (int i = 0; i < _internalData.usefulRoutesList.Count; i++)
            {
                if (_internalData.usefulRoutesList[i].totalDistance < minDistance)
                {
                    minDistance = _internalData.usefulRoutesList[i].totalDistance;
                    index = _internalData.usefulRoutesList.IndexOf(_internalData.usefulRoutesList[i]);
                }
            }

            //Debug.Log($"The shortest distance is: {minDistance} - It has the index: {index}");

            _finalRoute = new FinalRoute(){ _wayPoints = new List<Vector3>() };

            for (int i = 0; i < _internalData.usefulRoutesList[index].nodesIDs.Count; i++){
                _finalRoute._wayPoints.Add(GetNode(_internalData.usefulRoutesList[index].nodesIDs[i]).gameObject.transform.position);
            }

            _finalRoute._wayPoints.Add(_internalData.endPosition.position);
        }

        public void PrepareAgent()
        {
            _agentData.agentPrefab = GameObject.FindWithTag("Agent");
            _agentData.agentInitialPos = GameObject.FindWithTag("InitialPosition").transform;
            _agentData.agentFinalPos = GameObject.FindWithTag("FinalPosition").transform;

            AgentData agentData = new AgentData()
            {
                agentInitialPos = _agentData.agentInitialPos,
                agentFinalPos = _agentData.agentFinalPos,
                agentPrefab = _agentData.agentPrefab,
                agentSO = _agentData.agentSO,
                destinyPositions = new List<Vector3>(_finalRoute._wayPoints)
            };

            _agentData.agentSO.spawnParameters.position = _agentData.agentInitialPos.position;
            _agentData.agentSO.spawnParameters.rotation = _agentData.agentInitialPos.eulerAngles;

            _agentData.agentInitialPos.position = _agentData.agentSO.spawnParameters.position;
            _agentData.agentPrefab.transform.eulerAngles = _agentData.agentSO.spawnParameters.rotation; 

            _agentData.agentPrefab.transform.position = _agentData.agentInitialPos.position;


            for (int i = 0; i < agentData.destinyPositions.Count; ++i)
            {
                MovingBehaviours behaviour = new MovingBehaviours()
                {
                    stateMechanic = StateMechanics.MOVE,
                    destinyDirection = agentData.destinyPositions[i],
                    movSpeed = _agentData.agentSpeed,
                };
                agentData.agentSO.movingBehaviours.Add(behaviour);
            }

            MovingBehaviours finalBehaviour = new MovingBehaviours()
            {
                stateMechanic = StateMechanics.STOP,
                destinyDirection = agentData.agentPrefab.transform.position,
                movSpeed = 0.0f
            };

            agentData.agentSO.movingBehaviours.Add(finalBehaviour);

            _agentData = agentData;
        }

        #endregion

        #region LocalMethods

        #endregion

        #region GettersAndSetters

        public Node GetNode(int index)
        {
            return ((GameObject)EditorUtility.InstanceIDToObject(index)).GetComponent<Node>();
        }
        #endregion
    }
}
