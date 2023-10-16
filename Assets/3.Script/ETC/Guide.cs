using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Guide : MonoBehaviour
{
    private bool is_showing = false;
    [SerializeField] private Text guide_button_text;
    [SerializeField] private GameObject guide_text_ob;
    // Start is called before the first frame update
    void Start()
    {
        is_showing = false;
    }

    public void click_button() {
        if (is_showing)
        {
            close_guide();
        }
        else {
            show_guide();
        }
    }
    public void show_guide() {
        is_showing = true;
        guide_text_ob.SetActive(is_showing);
        guide_button_text.text = "x";
    }

    public void close_guide()
    {
        is_showing = false;
        guide_text_ob.SetActive(is_showing);
        guide_button_text.text = "?";
    }
}
