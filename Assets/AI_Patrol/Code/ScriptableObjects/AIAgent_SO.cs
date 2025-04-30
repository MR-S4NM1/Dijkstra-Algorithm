using System.Collections.Generic;
using UnityEngine;

namespace Mr_Sanmi.AI_Agents
{
    #region Enums

    public enum StateMechanics
    {
        STOP,
        MOVE
    }

    #endregion

    #region Structs

    [System.Serializable] //Convertion to bytes which can be saved in the HDD.
    public struct MovingBehaviours
    {
        public StateMechanics stateMechanic;
        public float movSpeed;
        public float durationTime;
        [SerializeField] public Vector3 destinyDirection;
        [SerializeField] public Vector3 destinyRotation;
    }

    [System.Serializable]
    public struct SpawnParameters
    {
        [SerializeField] public Vector3 position;
        [SerializeField] public Vector3 rotation;
    }

    #endregion

    [CreateAssetMenu(fileName = "NPCAgent_SO", menuName = "Scriptable Objects/NPCAgent_SO")] 
    public class AIAgent_SO : ScriptableObject
    {
        //Patrol
        [SerializeField] public List<MovingBehaviours> movingBehaviours;

        //Spawn Transformation
        [SerializeField] public SpawnParameters spawnParameters;
    }
}