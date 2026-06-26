using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestTask.UI
{
    public class MenuManager : MonoBehaviour
    {
        public void LoadLevel(int scene){
            SceneManager.LoadScene(scene);
        }

        public void LoadLevelByName(string sceneName){
            SceneManager.LoadScene(sceneName);
        }

        public void Quit(){
            Application.Quit();
        }
    }
}

