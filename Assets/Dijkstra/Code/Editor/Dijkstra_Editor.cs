using UnityEditor;
using UnityEngine;

namespace MrSanmi.DijkstraAlgorithm
{
    [CustomEditor(typeof(Dijkstra))]
    public class Dijkstra_Editor : Editor
    {
        [SerializeField] protected Dijkstra _dijkstra;
        public override void OnInspectorGUI()
        {
            if (_dijkstra == null)
            {
                _dijkstra = (Dijkstra)target;
            }

            DrawDefaultInspector();

            if(GUILayout.Button("Probe Nodes"))
            {
                _dijkstra.GenerateNodes();
            }
            if (GUILayout.Button("Clear All"))
            {
                _dijkstra.ClearAll();
            }
            if (GUILayout.Button("Generate Graph"))
            {
                _dijkstra.GenerateGraph();
            }
            if(GUILayout.Button("Reduce Nodes"))
            {
                _dijkstra.ReduceNodes();
            }

        }
    }

}