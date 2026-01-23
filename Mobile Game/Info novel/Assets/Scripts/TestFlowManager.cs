using UnityEngine;

public class TestFlowManager : MonoBehaviour
{
    [Header("Общие Настройки Теста")]
    [Tooltip("Главная панель UI, содержащая все вопросы. Включается/выключается для начала/конца теста.")]
    public GameObject TestPanel;

    [Tooltip("Массив всех объектов вопросов (с прикрепленными QuestionManager)")]
    public QuestionManager[] Questions;
    public string[] QuestionTexts;

    private int currentQuestionIndex = 0;
    private int totalScore = 0;
    private int maxPossibleScore = 0;

    public int TotalScore
    {
        get { return totalScore; }
    }
    public int MaxPossibleScore
    {
        get { return maxPossibleScore; }
    }

    public void StartTest()
    {
        if (Questions == null || Questions.Length == 0)
        {
            Debug.LogError("В массиве Questions нет вопросов!");
            return;
        }

        currentQuestionIndex = 0;
        totalScore = 0;

        CalculateMaxScore();

        TestPanel.SetActive(true);

        LoadQuestion(currentQuestionIndex);
        Debug.Log("Тест запущен.");
    }


    private void LoadQuestion(int index)
    {
        DeactivateAllQuestions();

        QuestionManager currentQuestion = Questions[index];

        currentQuestion.QuestionPanel.SetActive(true);

        currentQuestion.DisplayQuestion(QuestionTexts[index]);
    }

    void Update()
    {
        if (!TestPanel.activeSelf) return;
        if (currentQuestionIndex >= Questions.Length) return; 

        QuestionManager currentQuestion = Questions[currentQuestionIndex];

        if (currentQuestion.IsAnswered)
        {
            ReceiveAnswerResult(currentQuestion);
        }
    }

    public void ReceiveAnswerResult(QuestionManager answeredQuestion)
    {
        totalScore += answeredQuestion.FinalPoints;
        Debug.Log($"Счет: +{answeredQuestion.FinalPoints}. Общий счет: {totalScore}");

        answeredQuestion.HideQuestion();

        currentQuestionIndex++;

        if (currentQuestionIndex < Questions.Length)
        {
            LoadQuestion(currentQuestionIndex);
        }
        else
        {
            FinishTest();
        }
    }

    private void DeactivateAllQuestions()
    {
        foreach (QuestionManager qm in Questions)
        {
            if (qm != null && qm.QuestionPanel != null)
            {
                qm.QuestionPanel.SetActive(false);
            }
        }
    }

    private void CalculateMaxScore()
    {
        maxPossibleScore = 0;
        foreach (var qm in Questions)
        {
            maxPossibleScore += qm.MaxPoints;
        }
    }


    private void FinishTest()
    {
        TestPanel.SetActive(false);
        Debug.Log("Тест завершен. Финальный счет: " + totalScore);
    }
}