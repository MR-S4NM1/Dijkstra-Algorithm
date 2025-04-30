using UnityEngine;
using System.Collections.Generic;

namespace Mr_Sanmi.AI_Agents
{
    public class AgentNPCFactory : MonoBehaviour
    {
        #region Variables

        [Header("Parameters")]
        [SerializeField] protected GameObject agentPrefab;
        [SerializeField] protected AIAgent_SO[] agentsScriptableObjects;

        [Header("Runtime Variables")]
        [SerializeField] protected List<GameObject> agentInstancesGameObject;

        #endregion

        #region RuntimeVariables

        GameObject agentInstanceGameObject;

        #endregion

        #region UnityMethods
        #endregion

        #region PublicMethods

        public void CreateAgents()
        {
            foreach(AIAgent_SO agent in agentsScriptableObjects)
            {
                // Generate the instance of a new Enemy NPC, baser on the prefab.
                agentInstanceGameObject = Instantiate(agentPrefab);

                // According to the data from the Scriptable Object,
                // We set the position and rotation of the Enemy.
                agentInstanceGameObject.transform.position = agent.spawnParameters.position;
                agentInstanceGameObject.transform.rotation = Quaternion.Euler(agent.spawnParameters.rotation);

                // To have a better structure of the scene,
                // every enemy will be adopted by this game object.
                agentInstanceGameObject.transform.parent = this.gameObject.transform;

                //Patrol Behaviour data.
                agentInstanceGameObject.GetComponent<NPC_AIAgent>().agentNPC_SO = agent;

                // Add the enemy instance for a future deletion of this enemy.
                agentInstancesGameObject.Add(agentInstanceGameObject);
            }
        }

        public void DestroyAgents()
        {
            for(int i = agentInstancesGameObject.Count - 1; i >= 0; i--)
            {
                agentInstanceGameObject = agentInstancesGameObject[i];
                agentInstancesGameObject.Remove(agentInstanceGameObject);
                DestroyImmediate(agentInstanceGameObject);
            }
            agentInstancesGameObject.Clear();
        }

        #endregion

    }

}