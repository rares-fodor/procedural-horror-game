using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject UIContainer;
    [SerializeField] private TMP_Text dialogueName;
    [SerializeField] private TMP_Text dialogueBody;

    private Dialogue dialogue;
    private bool active = false;
    private int sentenceIndex = 0;

    private void Awake()
    {
        GameController.DialogueStarted.AddListener(OnDialogueStarted);
    }

    private void Update()
    {
        UIContainer.SetActive(active);
        if (Input.GetKeyDown(Consts.SKIP_DIALOGUE_LINE_KEY))
        {
            NextSentence();
        }
    }

    void OnDialogueStarted(string name, Dialogue dialogue)
    {
        active = !active;
        if (active == true)
        {
            this.dialogue = dialogue;
            dialogueName.text = name;
            sentenceIndex = 0;
            NextSentence();
        }
    }

    void NextSentence()
    {
        if (sentenceIndex > dialogue.sentences.Length - 1)
        {
            active = false;
            GameController.DialogueFinished.Invoke();
        }
        else
            dialogueBody.text = dialogue.sentences[sentenceIndex++];
    }
}
