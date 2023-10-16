using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character_Info : MonoBehaviour
{
    [SerializeField] private Slider hp_bar;
    [SerializeField] private Slider stamina_bar;
    [SerializeField] private GameObject rage_icon;

    // Start is called before the first frame update
    public void update_hp(float value) {
        hp_bar.value = value;
    }
    public void update_stamina(float value)
    {
        stamina_bar.value = value;
    }

    public void set_bar_width(float width) { 
        hp_bar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        stamina_bar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
    public void set_bar_height(float height)
    {
        hp_bar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        hp_bar.transform.localPosition = new Vector3(0f, height / 3f, 0f);
        stamina_bar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height*0.85f);
        stamina_bar.transform.localPosition = new Vector3(0f,-height*0.85f/3f,0f);
    }

    public void set_hp_bar_color(Color color) {
        hp_bar.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = color;
    }
    public void set_stamina_bar_color(Color color)
    {
        stamina_bar.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = color;
    }
    public void on_rage()
    {
        rage_icon.SetActive(true); ;
    }
    public void off_rage() {
        rage_icon.SetActive(false); ;
    }
}
