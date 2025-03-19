using System.Collections.Generic;
using UnityEngine;

namespace MrSanmi.DijkstraAlgorithm
{
    [System.Serializable]
    public struct DijkstraParameters
    {
        // TODO: Create list of nodes and add them icons.
        [SerializeField] public Transform startPosition;
        [SerializeField] public Node startNode;

        [SerializeField] public Transform endPosition;
        [SerializeField] public Node endNode;

        [SerializeField] public Transform pivot;
        [SerializeField] public List<Node> nodes;
        [SerializeField] public GameObject nodePrefab;
        
        [SerializeField] public Vector2 nodesMatrixSize;
        [SerializeField] public Vector2 numberOfNodes;
    }


    public class Dijkstra : MonoBehaviour
    {
        [SerializeField] protected DijkstraParameters _parameters;

        [SerializeField] public GameObject nodeInstance;
        Node actualNode;

        void Start()
        {

        }

        void Update()
        {

        }

        #region EditorButtons
        public void SetIconToThisGameObject()
        {
            IconManager.SetIcon(gameObject, IconManager.LabelIcon.Green);
        }

        public void GenerateNodes()
        {
            DeleteNodes();
            
            Vector3 tempPos = _parameters.startPosition.position;
            float xOffset = _parameters.nodesMatrixSize.x / _parameters.numberOfNodes.x;
            float yOffset = _parameters.nodesMatrixSize.y / _parameters.numberOfNodes.y;

            for(int i = 0; i < _parameters.numberOfNodes.y; ++i)
            {
                for(int j = 0; j < _parameters.numberOfNodes.x; ++j)
                {
                    nodeInstance = Instantiate(_parameters.nodePrefab);
                    actualNode = nodeInstance.GetComponent<Node>();
                    _parameters.nodes.Add(actualNode);
                    nodeInstance.transform.position = tempPos;

                    if (i == 0 && j == 0)
                    {
                        _parameters.startNode = actualNode;
                    }

                    Collider[] hitColliders = Physics.OverlapSphere(actualNode.transform.position, 0.5f);

                    IconManager.SetIcon(nodeInstance, IconManager.LabelIcon.Green);
                    actualNode._nodeState = NodeStates.CONNECTABLE;

                    if (hitColliders.Length > 0)
                    {
                        foreach(Collider collider in hitColliders)
                        {
                            if (collider.gameObject.layer == 6)
                            {
                                IconManager.SetIcon(nodeInstance, IconManager.LabelIcon.Red);
                                actualNode._nodeState = NodeStates.NOT_CONNECTABLE;
                                break;
                            }
                        }
                    }

                    nodeInstance.transform.SetParent(this.gameObject.transform);
                    
                    tempPos += new Vector3(0.0f, 0.0f, xOffset);
                }

                tempPos.z = _parameters.startNode.transform.position.z;
                tempPos += new Vector3(yOffset, 0.0f, 0.0f);
            }
        }

        public void DeleteNodes()
        {
            foreach (Node node in _parameters.nodes)
            {
                DestroyImmediate(node.gameObject);
            }
            _parameters.nodes.Clear();
        }


        #endregion
    }
}
