using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class load : MonoBehaviour
{
    private int i = 0;
    public Slider slider;
    private void FixedUpdate()
    {
        if (i < 50)
            i++;
        else
        {
            i = 0;
            slider.value += 0.2f;
        }

        if(slider.value >= 1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
