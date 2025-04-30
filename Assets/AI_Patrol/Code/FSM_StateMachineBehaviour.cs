using UnityEngine;

namespace Mr_Sanmi.AI_Agents
{
    public class FSM_StateMachineBehaviour : StateMachineBehaviour
    {
        public States state;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.gameObject.GetComponent<FiniteStateMachine>().EnteredState(state);
        }
    }
}
