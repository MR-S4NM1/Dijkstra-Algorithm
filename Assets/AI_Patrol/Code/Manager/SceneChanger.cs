using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mr_Sanmi.AI_Agents
{
    public class SceneChanger : MonoBehaviour
    {
        #region References

        public static SceneChanger instance;

        #endregion

        #region UnityMethods
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }
        #endregion

        #region UnityMethods
        public void ChangeSceneTo(int sceneID)
        {
            SceneManager.LoadScene(sceneID);
        }
        #endregion
    }

}