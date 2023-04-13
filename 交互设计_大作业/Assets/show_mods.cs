using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class show_mods : MonoBehaviour
{
    public ScrollRect scroll;

    public void Start()
    {
        scroll.gameObject.SetActive(false);
    }

    public void show()
    {
        if(scroll.IsActive())
            scroll.gameObject.SetActive(false);
        else
            scroll.gameObject.SetActive(true); 
        
    }
}
