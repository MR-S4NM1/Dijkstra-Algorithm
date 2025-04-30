using UnityEngine;

namespace Mr_Sanmi.AI_Agents
{
    public class Agent : MonoBehaviour
    {
        #region References

        [SerializeField] protected FiniteStateMachine _fsm;

        #endregion

        #region Knobs

        #endregion

        #region RuntimeVariables

        #endregion

        #region UnityMethods

        private void OnDrawGizmos()
        {
            //if(_fsm == null)
            //{
            //    _fsm = GetComponent<FiniteStateMachine>();
            //}
        }

        void Start()
        {

        }

        void Update()
        {

        }

        #endregion

        #region GettersAndSettters


        #endregion
    }
}