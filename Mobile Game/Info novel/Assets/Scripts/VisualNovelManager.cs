using TMPro;
using UnityEngine;
using System.Collections;

public class VisualNovelManager : MonoBehaviour
{
    [System.Serializable]
    public struct Character
    {
        public string Name;
        public Tutoryash TutoryashScript;
    }

    [System.Serializable]
    public struct DialogueLine
    {
        [Tooltip("Индекс персонажа в массиве 'Characters' (0 - Игрок, 1 - Туторяш и т.д.)")]
        public int CharacterIndex;
        [TextArea(3, 10)]
        public string Text;
        [Tooltip("Ключ эмоции Туторяша (например, 'Normal', 'Happy'). Оставьте пустым, если не меняется.")]
        public string TutoryashEmotionKey;
    }

    // --- Публичные поля для Инспектора ---
    [Header("UI Элементы")]
    [Tooltip("Текстовое поле для имени персонажа")]
    [SerializeField] private TMP_Text NameText;
    [Tooltip("Текстовое поле для реплики диалога")]
    [SerializeField] private TMP_Text DialogueText;

    [Header("Настройки Плавного Текста")]
    [Tooltip("Задержка в секундах между появлением символов")]
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("Данные")]
    [Tooltip("Массив всех действующих лиц")]
    [SerializeField] private Character[] Characters;
    [Tooltip("Вся ветка диалога для данной сцены")]
    [SerializeField] private DialogueLine[] Dialogue;

    // --- Внутренние переменные ---
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (Dialogue.Length > 0)
        {
            DisplayDialogueLine(currentLineIndex);
        }
        else
        {
            Debug.LogError("Массив диалогов пуст! Наполните его в Инспекторе.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                CompleteCurrentLine();
            }
            else
            {
                GoToNextLine();
            }
        }
    }

    private void CompleteCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        DialogueText.text = Dialogue[currentLineIndex].Text;
        isTyping = false;
    }

    public void GoToNextLine()
    {
        if (isTyping) return;

        currentLineIndex++;

        if (currentLineIndex < Dialogue.Length)
        {
            DisplayDialogueLine(currentLineIndex);
        }
        else
        {
            Debug.Log("Диалог завершен.");
            NameText.text = "";
            DialogueText.text = "КОНЕЦ";
        }
    }

    private void DisplayDialogueLine(int index)
    {
        DialogueLine line = Dialogue[index];

        Character character = Characters[line.CharacterIndex];

        NameText.text = character.Name;

        if (character.TutoryashScript != null)
        {
            if (!string.IsNullOrEmpty(line.TutoryashEmotionKey))
            {
                character.TutoryashScript.ActivateEmotion(line.TutoryashEmotionKey);
            }
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(line.Text));
    }

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        DialogueText.text = "";

        foreach (char character in fullText)
        {
            DialogueText.text += character;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }
}
