using NUnit.Framework.Interfaces;
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
        #region References
        [SerializeField] protected DijkstraParameters _parameters;
        [SerializeField] public GameObject nodeInstance;
        #endregion

        #region RuntimeVariables
        Node actualNode;
        float xOffset;
        float yOffset;
        #endregion

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
            xOffset = (float)_parameters.nodesMatrixSize.x / ((float)_parameters.numberOfNodes.x - 1.0f);
            yOffset = (float)_parameters.nodesMatrixSize.y / ((float)_parameters.numberOfNodes.y - 1.0f);

            for(float i = 0; i < _parameters.numberOfNodes.y; ++i)
            {
                for(float j = 0; j < _parameters.numberOfNodes.x; ++j)
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
                    actualNode._nodeState = NodeStates.HABILITADO;

                    if (hitColliders.Length > 0)
                    {
                        foreach(Collider collider in hitColliders)
                        {
                            if (collider.gameObject.layer == 6)
                            {
                                IconManager.SetIcon(nodeInstance, IconManager.LabelIcon.Red);
                                actualNode._nodeState = NodeStates.DESHABILITADO;
                                break;
                            }
                        }
                    }

                    nodeInstance.transform.SetParent(this.gameObject.transform, true);
                    
                    tempPos += new Vector3(0.0f, 0.0f, xOffset);
                }

                tempPos.z = _parameters.startNode.transform.position.z;
                tempPos += new Vector3(yOffset, 0.0f, 0.0f);
            }

            _parameters.nodes.Add(_parameters.endNode);
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
