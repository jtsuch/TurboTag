using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{

    public static bool GameIsPaused = false;

    [Header("Parents")]
    public GameObject inGameUI;
    public GameObject pauseMenuUI;

    [Header("Pages")]
    public GameObject generalPage;
    public GameObject gamePage;
    public GameObject cheatsPage;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        inGameUI.SetActive(true);
        generalPage.SetActive(true);
        gamePage.SetActive(false);
        cheatsPage.SetActive(false);
    }
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        pauseMenuUI.SetActive(false);
        inGameUI.SetActive(true);
        GameIsPaused = false;
    }


    void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        pauseMenuUI.SetActive(true);
        inGameUI.SetActive(false);
        GameIsPaused = true;
    }

    public void GeneralTabChange()
    {
        generalPage.SetActive(true);
        gamePage.SetActive(false);
        cheatsPage.SetActive(false);
    }

    public void GameTabChange()
    {
        generalPage.SetActive(false);
        gamePage.SetActive(true);
        cheatsPage.SetActive(false);
    }

    public void CheatsTabChange()
    {
        generalPage.SetActive(false);
        gamePage.SetActive(false);
        cheatsPage.SetActive(true);
    }

}
