using UnityEngine;
using UnityEngine.InputSystem;

namespace Mr_Sanmi.AI_Agents
{
    public class PlayersAvatar : Agent
    {
        #region References

        #endregion

        #region UnityMethods
        private void OnDrawGizmos()
        {
            if (_fsm == null)
            {
                _fsm = GetComponent<FiniteStateMachine>();
            }
        }
        void Start()
        {

        }
        void Update()
        {

        }

        private void FixedUpdate()
        {

        }

        #endregion

        #region LocalMethods

        #endregion

        #region PublicMethods


        #endregion

        #region CallbackFunctions

        public void OnMove(InputAction.CallbackContext value)
        {
            if (value.performed) // Update from the input
            {
                _fsm.StateMechanic(StateMechanic.MOVE);
                _fsm._movementDirection.x = value.ReadValue<Vector2>().x;
                _fsm._movementDirection.z = value.ReadValue<Vector2>().y;
            }
            else if (value.canceled) // Release from this input
            {
                _fsm._movementDirection.x = value.ReadValue<Vector2>().x;
                _fsm._movementDirection.z = value.ReadValue<Vector2>().y;

                if (_fsm._movementDirection.magnitude <= 0.1f)
                {
                    _fsm.StateMechanic(StateMechanic.STOP);
                    _fsm._movementDirection = Vector3.zero;
                }
            }
        }

        #endregion

        #region GettersAndSetters


        #endregion

    }
}
