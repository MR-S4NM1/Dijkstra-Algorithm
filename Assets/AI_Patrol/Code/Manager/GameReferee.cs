using Unity.VisualScripting;
using UnityEngine;

namespace Mr_Sanmi.AI_Agents
{
    public class GameReferee : MonoBehaviour
    {
        #region References

        public static GameReferee instance;
        [SerializeField] protected PlayersAvatar _avatar;
        [SerializeField] protected Transform _initialPlayersPos;

        #endregion

        #region UnityMethods

        private void Awake()
        {
            if(instance == null)
            {
                instance = this;
            }
            _avatar = FindAnyObjectByType<PlayersAvatar>();
        }

        #endregion

        #region PublicMethods

        public void ChangeToVictoryScene() 
        {
            SceneChanger.instance.ChangeSceneTo(1);
        }
        
        public void ResetPlayersPosition()
        {
            _avatar.gameObject.transform.position = _initialPlayersPos.position;
        }

        #endregion
    }

}