using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class QuestionManager : MonoBehaviour
{
    public enum QuestionType
    {
        TextInput,      
        MultipleChoice, 
        CrossOut        
    }

    [Header("Настройки Вопроса")]
    [Tooltip("Выберите тип данного вопроса")]
    public QuestionType Type = QuestionType.TextInput;
    [Tooltip("Максимальное количество баллов за правильный ответ")]
    public int MaxPoints = 1;

    [Header("Общие UI Элементы")]
    public TMP_Text QuestionTextUI;
    [Tooltip("Общий объект для управления видимостью вопроса")]
    public GameObject QuestionPanel;

    private int finalPoints = 0;
    private bool isAnswered = false;

    public int FinalPoints
    {
        get { return finalPoints; }
    }

    public bool IsAnswered
    {
        get { return isAnswered; }
    }

    [Header("--- A) TextInput (Ввод текста) ---")]
    [Tooltip("Поле ввода для ответа игрока")]
    public TMP_InputField TextInputField;
    [Tooltip("Кнопка для отправки ответа")]
    public Button TextInputSubmitButton;
    [Tooltip("Правильный ответ (Регистр будет игнорироваться)")]
    public string CorrectTextAnswer;

    [Header("--- B) MultipleChoice (Выбор) ---")]
    [Tooltip("Массив кнопок для вариантов ответа")]
    public Button[] ChoiceButtons;
    [Tooltip("Индекс правильной кнопки в массиве ChoiceButtons (начиная с 0)")]
    public int CorrectChoiceIndex;

    [Header("--- C) CrossOut (Вычеркивание) ---")]
    [Tooltip("Кнопки/Переключатели, которые игрок должен нажать, чтобы 'вычеркнуть' лишнее")]
    public Button[] PropertyButtons;
    [Tooltip("Индексы кнопок в массиве PropertyButtons, которые **нужно** нажать (лишние свойства)")]
    public List<int> CrossOutIndexes;
    [Tooltip("Кнопка для отправки ответа 'Вычеркивания'")]
    public Button CrossOutSubmitButton;

    private HashSet<int> selectedCrossOutIndices = new HashSet<int>();

    void Awake()
    {
        AssignListeners();
    }

    private void AssignListeners()
    {
        if (TextInputSubmitButton != null)
        {
            TextInputSubmitButton.onClick.AddListener(CheckTextInputAnswer);
        }

        for (int i = 0; i < ChoiceButtons.Length; i++)
        {
            int index = i;
            ChoiceButtons[i].onClick.AddListener(() => CheckMultipleChoiceAnswer(index));
        }

        for (int i = 0; i < PropertyButtons.Length; i++)
        {
            int index = i;
            PropertyButtons[i].onClick.AddListener(() => ToggleCrossOut(index));
        }
        if (CrossOutSubmitButton != null)
        {
            CrossOutSubmitButton.onClick.AddListener(CheckCrossOutAnswer);
        }
    }


    public void DisplayQuestion(string questionText)
    {
        if (isAnswered) return;

        QuestionPanel.SetActive(true);
        QuestionTextUI.text = questionText;

        if (Type == QuestionType.CrossOut)
        {
            selectedCrossOutIndices.Clear();
            foreach (var button in PropertyButtons)
            {
                button.GetComponent<Image>().color = Color.white;
            }
        }
    }

    public void HideQuestion()
    {
        QuestionPanel.SetActive(false);
    }

    public void CheckTextInputAnswer()
    {
        if (isAnswered || Type != QuestionType.TextInput) return;

        string playerAnswer = TextInputField.text.Trim();
        string correctAnswer = CorrectTextAnswer.Trim();

        if (string.Equals(playerAnswer, correctAnswer, System.StringComparison.OrdinalIgnoreCase))
        {
            finalPoints = MaxPoints;
            Debug.Log($"TextInput: Правильно! (+{finalPoints} баллов)");
        }
        else
        {
            finalPoints = 0;
            Debug.Log("TextInput: Неправильно.");
        }

        isAnswered = true;
        OnAnswerSubmitted();
    }

    public void CheckMultipleChoiceAnswer(int selectedIndex)
    {
        if (isAnswered || Type != QuestionType.MultipleChoice) return;

        if (selectedIndex == CorrectChoiceIndex)
        {
            finalPoints = MaxPoints;
            Debug.Log($"MultipleChoice: Правильно! (+{finalPoints} баллов)");
        }
        else
        {
            finalPoints = 0;
            Debug.Log("MultipleChoice: Неправильно.");
        }

        isAnswered = true;
        OnAnswerSubmitted();
    }

    private void ToggleCrossOut(int index)
    {
        if (isAnswered || Type != QuestionType.CrossOut) return;

        if (selectedCrossOutIndices.Contains(index))
        {
            selectedCrossOutIndices.Remove(index);
            PropertyButtons[index].GetComponent<Image>().color = Color.white;
        }
        else
        {
            selectedCrossOutIndices.Add(index);
            PropertyButtons[index].GetComponent<Image>().color = Color.gray; 
        }
        Debug.Log($"CrossOut: Выбрано/Отменено свойство {index}. Текущий выбор: {selectedCrossOutIndices.Count}");
    }

    public void CheckCrossOutAnswer()
    {
        if (isAnswered || Type != QuestionType.CrossOut) return;

        if (selectedCrossOutIndices.Count != CrossOutIndexes.Count)
        {
            finalPoints = 0;
            Debug.Log("CrossOut: Неправильное количество выбранных элементов.");
            isAnswered = true;
            OnAnswerSubmitted();
            return;
        }

        bool allCorrect = true;
        foreach (int requiredIndex in CrossOutIndexes)
        {
            if (!selectedCrossOutIndices.Contains(requiredIndex))
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            finalPoints = MaxPoints;
            Debug.Log($"CrossOut: Правильно! (+{finalPoints} баллов)");
        }
        else
        {
            finalPoints = 0;
            Debug.Log("CrossOut: Выбраны не все лишние свойства.");
        }

        isAnswered = true;
        OnAnswerSubmitted();
    }

    private void OnAnswerSubmitted()
    {
        HideQuestion();
        Debug.Log("Вопрос отвечен. Итоговые баллы: " + finalPoints);
    }
}
