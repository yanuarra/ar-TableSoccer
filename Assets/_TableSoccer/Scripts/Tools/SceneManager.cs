using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YRA
{
    public class SceneManager : Singleton<SceneManager>
    {
        private int _backSceneIndex;

        public void OpenScene(int sceneIndex)
        {
            _backSceneIndex = sceneIndex;
            StartCoroutine(OpenSceneCoroutine(sceneIndex));
        }

        public void RestartLevel()
        {
            OpenScene(GetCurrentActiveScene().buildIndex);
        }

        public Scene GetCurrentActiveScene()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        }

        private IEnumerator OpenSceneCoroutine(int sceneIndex)
        {
            yield return new WaitForEndOfFrame();
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
        }
    }
}
