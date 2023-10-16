using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.EventSystems;
public class Intro_Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image background;
    private Color hover_color;
    private Color original_color;

    private void Start()
    {
        TryGetComponent(out background);
        original_color = background.color;
        hover_color = new Color(original_color.r, original_color.g, original_color.b, 0.3f);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        //do stuff
        background.color = hover_color;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //do stuff
        background.color = original_color;

    }
}
