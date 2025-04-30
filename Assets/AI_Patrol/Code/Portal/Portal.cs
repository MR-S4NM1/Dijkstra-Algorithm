using UnityEngine;

namespace Mr_Sanmi.AI_Agents
{
    public class Portal : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameReferee.instance.ChangeToVictoryScene();
            }
        }
    }

}