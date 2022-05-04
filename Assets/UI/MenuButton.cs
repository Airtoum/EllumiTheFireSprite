using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private UnityEngine.UI.Image image;
    [SerializeField] private Color hover_color;
    [SerializeField] private Color pressed_color;
    private Color normal_color;

    [SerializeField] public string scene_name;
    private bool moused_over;
    
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<UnityEngine.UI.Image>();
        normal_color = image.color;
    }

    public void OnPointerEnter(PointerEventData event_data)
    {
        image.color = hover_color;
        moused_over = true;
    }
    
    public void OnPointerExit(PointerEventData event_data)
    {
        image.color = normal_color;
        moused_over = false;
    }

    private void OnMouseDown()
    {
        if (moused_over) {
            image.color = pressed_color;
        }
    }

    private void OnMouseUpAsButton()
    {
        if (moused_over){
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene_name);
        }
    }
}
