using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    private GameObject resume_button;
    // Start is called before the first frame update
    void Start()
    {
        resume_button = gameObject.transform.Find("Resume_Button").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pause();
        }
    }
    public void pause()
    {
        Time.timeScale = 0;
        resume_button.SetActive(true);
    }
    public void resume() {
        resume_button.SetActive(false) ;
        Time.timeScale = 1;
    }
    public void resume_temp()
    {
        Time.timeScale = 1;
    }
}
