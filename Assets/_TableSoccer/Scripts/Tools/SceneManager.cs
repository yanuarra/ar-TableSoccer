using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YRA
{
    public class SceneManager : Singleton<SceneManager>
    {
        private int _backSceneIndex;
        private string _sceneName;

        public void OpenScene(int sceneIndex = -1, string sceneName = null)
        {
            _backSceneIndex = sceneIndex;
            if (sceneName != null)
            {
                _sceneName = sceneName;
            }
            StartCoroutine(OpenSceneCoroutine());
        }

        public void RestartLevel()
        {
            OpenScene(GetCurrentActiveScene().buildIndex);
        }

        public Scene GetCurrentActiveScene()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        }

        private IEnumerator OpenSceneCoroutine()
        {
            yield return new WaitForEndOfFrame();
            if (_backSceneIndex != -1)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(_backSceneIndex);
            }else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(_sceneName);
            }
        }
    }
}
