using UnityEngine;
using UnityEditor;

namespace Mr_Sanmi.AI_Agents
{
    [CustomEditor(typeof(AgentNPCFactory))]
    public class AgentNPCFactory_Editor : Editor
    {
        AgentNPCFactory agentNPCFactory;

        #region UnityMethods
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if(agentNPCFactory == null)
            {
                agentNPCFactory = (AgentNPCFactory)target; 
            }

            if (GUILayout.Button("Create Agents"))
            {
                agentNPCFactory.DestroyAgents();
                agentNPCFactory.CreateAgents();
            }
            if (GUILayout.Button("Delete Agents"))
            {
                agentNPCFactory.DestroyAgents();
            }
        }

        #endregion
    }

}