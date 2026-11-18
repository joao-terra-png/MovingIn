using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class NextLevel : MonoBehaviour
{

    Scene Current;
    [SerializeField]string NextSceneName;
    void Start()
    {
        Current = SceneManager.GetActiveScene();
    }
    public void GoToNext()
    {
        SceneManager.UnloadSceneAsync(Current);
        SceneManager.LoadScene(NextSceneName);
    }

}
