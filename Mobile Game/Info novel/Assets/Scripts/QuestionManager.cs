using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestionManager : MonoBehaviour
{
    // --- 1. Общие настройки вопроса ---

    public enum QuestionType
    {
        TextInput,      // Ввод ответа с клавиатуры
        MultipleChoice, // Выбор одного варианта ответа
        CrossOut        // Вычеркивание лишних свойств (Multiple Unselection)
    }

    [Header("Настройки Вопроса")]
    [Tooltip("Выберите тип данного вопроса")]
    public QuestionType Type = QuestionType.TextInput;
    [Tooltip("Максимальное количество баллов за правильный ответ")]
    public int MaxPoints = 1;

    // --- Общие UI Элементы ---
    [Header("Общие UI Элементы")]
    public Text QuestionTextUI;
    [Tooltip("Общий объект для управления видимостью вопроса")]
    public GameObject QuestionPanel;

    // --- 2. Переменные для хранения результата ---
    private int finalPoints = 0;
    private bool isAnswered = false;

    /// <summary>
    /// Геттер для получения заработанных баллов.
    /// </summary>
    public int FinalPoints
    {
        get { return finalPoints; }
    }

    // --- 3. Типы вопросов и их сериализованные поля ---

    // ---------------------------------------------
    // А) Ввод ответа с клавиатуры (TextInput)
    // ---------------------------------------------
    [Header("--- A) TextInput (Ввод текста) ---")]
    [Tooltip("Поле ввода для ответа игрока")]
    public InputField TextInputField;
    [Tooltip("Кнопка для отправки ответа")]
    public Button TextInputSubmitButton;
    [Tooltip("Правильный ответ (Регистр будет игнорироваться)")]
    public string CorrectTextAnswer;

    // ---------------------------------------------
    // Б) Выбор варианта ответа (MultipleChoice)
    // ---------------------------------------------
    [Header("--- B) MultipleChoice (Выбор) ---")]
    [Tooltip("Массив кнопок для вариантов ответа")]
    public Button[] ChoiceButtons;
    [Tooltip("Индекс правильной кнопки в массиве ChoiceButtons (начиная с 0)")]
    public int CorrectChoiceIndex;

    // ---------------------------------------------
    // В) Вычеркивание лишних свойств (CrossOut)
    // ---------------------------------------------
    [Header("--- C) CrossOut (Вычеркивание) ---")]
    [Tooltip("Кнопки/Переключатели, которые игрок должен нажать, чтобы 'вычеркнуть' лишнее")]
    public Button[] PropertyButtons;
    [Tooltip("Индексы кнопок в массиве PropertyButtons, которые **нужно** нажать (лишние свойства)")]
    public List<int> CrossOutIndexes;
    [Tooltip("Кнопка для отправки ответа 'Вычеркивания'")]
    public Button CrossOutSubmitButton;

    // Вспомогательная переменная для отслеживания выбора в CrossOut
    private HashSet<int> selectedCrossOutIndices = new HashSet<int>();

    // --- 4. Методы инициализации ---

    void Awake()
    {
        // Скрываем вопрос по умолчанию. Основной менеджер будет его показывать.
        QuestionPanel.SetActive(false);

        // Назначаем обработчики (Listener'ы) для удобства
        AssignListeners();
    }

    private void AssignListeners()
    {
        // TextInput
        if (TextInputSubmitButton != null)
        {
            TextInputSubmitButton.onClick.AddListener(CheckTextInputAnswer);
        }

        // MultipleChoice
        for (int i = 0; i < ChoiceButtons.Length; i++)
        {
            int index = i;
            ChoiceButtons[i].onClick.AddListener(() => CheckMultipleChoiceAnswer(index));
        }

        // CrossOut
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

    /// <summary>
    /// Отображает вопрос и настраивает необходимые UI элементы.
    /// Этот метод должен вызываться вашим основным скриптом.
    /// </summary>
    public void DisplayQuestion(string questionText)
    {
        if (isAnswered) return;

        QuestionPanel.SetActive(true);
        QuestionTextUI.text = questionText;

        // Скрываем все контейнеры/кнопки, не относящиеся к текущему типу
        // (Рекомендуется, чтобы в Unity вы сами настроили дочерние объекты 
        // так, чтобы они скрывались/показывались вместе с этим скриптом)

        // ВАЖНО: В реальном проекте здесь нужно включать/отключать родительские
        // объекты для каждого типа вопросов. Здесь мы полагаемся, что вы 
        // настроите это в Инспекторе через активацию/деактивацию QuestionPanel 
        // и дочерних контейнеров, связанных с конкретным типом вопроса.

        // Дополнительная инициализация для CrossOut
        if (Type == QuestionType.CrossOut)
        {
            selectedCrossOutIndices.Clear();
            // Сброс визуального состояния кнопок (например, цвета)
            foreach (var button in PropertyButtons)
            {
                // Пример: button.GetComponent<Image>().color = Color.white;
            }
        }
    }

    /// <summary>
    /// Скрывает вопрос.
    /// </summary>
    public void HideQuestion()
    {
        QuestionPanel.SetActive(false);
    }

    // --- 5. Методы проверки ответа ---

    // ---------------------------------------------
    // А) Ввод ответа с клавиатуры (TextInput)
    // ---------------------------------------------
    public void CheckTextInputAnswer()
    {
        if (isAnswered || Type != QuestionType.TextInput) return;

        string playerAnswer = TextInputField.text.Trim();
        string correctAnswer = CorrectTextAnswer.Trim();

        // Сравнение без учета регистра
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

    // ---------------------------------------------
    // Б) Выбор варианта ответа (MultipleChoice)
    // ---------------------------------------------
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

    // ---------------------------------------------
    // В) Вычеркивание лишних свойств (CrossOut)
    // ---------------------------------------------

    /// <summary>
    /// Переключает состояние 'вычеркнутости' для выбранной кнопки.
    /// </summary>
    /// <param name="index">Индекс нажатой кнопки в массиве PropertyButtons.</param>
    private void ToggleCrossOut(int index)
    {
        if (isAnswered || Type != QuestionType.CrossOut) return;

        if (selectedCrossOutIndices.Contains(index))
        {
            selectedCrossOutIndices.Remove(index);
            // Визуальный сброс (например, убрать рамку/цвет)
            // PropertyButtons[index].GetComponent<Image>().color = Color.white;
        }
        else
        {
            selectedCrossOutIndices.Add(index);
            // Визуальное выделение (например, добавить рамку/цвет)
            // PropertyButtons[index].GetComponent<Image>().color = Color.red; 
        }
        Debug.Log($"CrossOut: Выбрано/Отменено свойство {index}. Текущий выбор: {selectedCrossOutIndices.Count}");
    }

    /// <summary>
    /// Проверяет ответ на вычеркивание.
    /// </summary>
    public void CheckCrossOutAnswer()
    {
        if (isAnswered || Type != QuestionType.CrossOut) return;

        // 1. Проверяем, совпадает ли количество выбранных и правильных элементов
        if (selectedCrossOutIndices.Count != CrossOutIndexes.Count)
        {
            finalPoints = 0;
            Debug.Log("CrossOut: Неправильное количество выбранных элементов.");
            isAnswered = true;
            OnAnswerSubmitted();
            return;
        }

        // 2. Проверяем, совпадают ли все выбранные индексы с правильными
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

    // --- 6. Завершение вопроса ---

    /// <summary>
    /// Вызывается после получения ответа на любой тип вопроса.
    /// </summary>
    private void OnAnswerSubmitted()
    {
        // Здесь можно добавить:
        // 1. Визуальный фидбек (правильно/неправильно)
        // 2. Вызов метода в вашем общем TestManager'е (например, NextQuestion()), 
        //    передавая ему полученные баллы (FinalPoints).

        Debug.Log("Вопрос отвечен. Итоговые баллы: " + finalPoints);

        // Пример вызова внешнего менеджера (вам нужно будет реализовать его)
        // TestManager.Instance.ReceiveAnswerResult(this);
    }
}
