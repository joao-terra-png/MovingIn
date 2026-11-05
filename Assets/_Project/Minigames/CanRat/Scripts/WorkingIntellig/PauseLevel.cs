using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PauseLevel : MonoBehaviour
{
    // Start is called before the first frame update
    bool IsPaused;
    public UnityEvent PauseGame = new UnityEvent();
    [SerializeField]UnityEvent ResumeGame = new UnityEvent();   //Inscrito pelo inspector
    //Jogo inicializa pausado
    void Start()
    {
        IsPaused = true;
        Time.timeScale = 0f;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            SetPause(); 
    }

    public void SetPause()
    {
        IsPaused = !IsPaused;
        if(IsPaused)
            PauseGame.Invoke();
        else
            ResumeGame.Invoke();
        Time.timeScale = IsPaused ? 0f : 1f;
    }
}