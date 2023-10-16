using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro_Manager : MonoBehaviour
{
    private SceneManager manager;
    public void click_start() {
        SceneManager.LoadScene("Scene1");
    }

    public void click_quit()
    {
        Application.Quit();
    }
}
