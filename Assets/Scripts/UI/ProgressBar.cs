using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ProgressBar : MonoBehaviour
{
    [SerializeField] private float progress;
    [SerializeField] public float maximum = 100;

    [SerializeField] private Image mask;
    [SerializeField] private GameObject ProgressBarUI;

    private bool visible = false;
    public bool IsVisible
    { 
        get { return visible; }
        set 
        { 
            visible = value;
            ProgressBarUI.SetActive(value);
        }
    }

    public float Progress
    {
        get { return progress; }
        set
        {
            progress = value;
            mask.fillAmount = progress / maximum;
        }
    }

    private void Awake()
    {
        ProgressBarUI.SetActive(false);
    }
}
