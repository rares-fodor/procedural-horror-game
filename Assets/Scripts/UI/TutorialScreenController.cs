using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialScreenController : MonoBehaviour
{
    [SerializeField] private TMP_Text tutorialMessageText;
    [SerializeField] private GameObject tutorialScreenUI;
    [SerializeField] private Button okButton;

    private void Awake()
    {
        tutorialScreenUI.SetActive(false);

        okButton.onClick.AddListener(() =>
        {
            tutorialScreenUI.SetActive(false);
        });
    }

    public void SetMessage(string message)
    {
        tutorialScreenUI.SetActive(true);
        tutorialMessageText.text = message;
    }

}
