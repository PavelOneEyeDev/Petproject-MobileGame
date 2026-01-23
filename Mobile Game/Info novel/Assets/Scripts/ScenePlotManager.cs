using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections; 

public class ScenePlotManager : MonoBehaviour
{
    [Header("1. Менеджеры Сюжета")]
    [Tooltip("Основной менеджер, запускающий диалог в начале сцены.")]
    public VisualNovelManager InitialVNManager;

    [Tooltip("Менеджер, управляющий тестом.")]
    public TestFlowManager SceneTestManager;

    [Tooltip("Диалог, если результат теста НЕУДОВЛЕТВОРИТЕЛЬНЫЙ (< 50%).")]
    public VisualNovelManager BadOutcomeVNManager;

    [Tooltip("Диалог, если результат теста УДОВЛЕТВОРИТЕЛЬНЫЙ (>= 50%).")]
    public VisualNovelManager GoodOutcomeVNManager;

    [Header("2. Переход на Следующую Сцену")]
    [Tooltip("Имя следующей сцены, куда нужно перейти после завершения сюжета.")]
    public string NextSceneName;


    void Start()
    {
        SetupSceneManagers();

        if (InitialVNManager != null)
        {

            InitialVNManager.gameObject.SetActive(true);

            StartCoroutine(WaitForVNToFinish(InitialVNManager, StartTestPhase));
        }
        else
        {
            Debug.LogError("InitialVNManager не назначен. Невозможно начать сцену.");
        }
    }

    private void SetupSceneManagers()
    {
        if (InitialVNManager != null) InitialVNManager.gameObject.SetActive(false);
        if (BadOutcomeVNManager != null) BadOutcomeVNManager.gameObject.SetActive(false);
        if (GoodOutcomeVNManager != null) GoodOutcomeVNManager.gameObject.SetActive(false);

        if (SceneTestManager != null) SceneTestManager.TestPanel.SetActive(false);
    }

    IEnumerator WaitForVNToFinish(VisualNovelManager vnManager, System.Action onFinishAction)
    {
        yield return new WaitUntil(() => vnManager.IsDialogEnded);
        vnManager.gameObject.SetActive(false);
        onFinishAction?.Invoke();
    }

    private void StartTestPhase()
    {
        Debug.Log("Фаза: Тест.");
        if (SceneTestManager != null)
        {
            SceneTestManager.StartTest();
            StartCoroutine(WaitForTestToFinish());
        }
        else
        {
            Debug.LogError("SceneTestManager не назначен. Пропускаем тест.");
            StartFinalVNPhase(0, 0);
        }
    }

    IEnumerator WaitForTestToFinish()
    {
        yield return new WaitUntil(() => !SceneTestManager.TestPanel.activeSelf);

        StartFinalVNPhase(SceneTestManager.TotalScore, SceneTestManager.MaxPossibleScore);
    }

    private void StartFinalVNPhase(int score, int maxScore)
    {
        Debug.Log("Фаза: Финальный Диалог.");
        VisualNovelManager finalManager = null;

        if (maxScore >= 0 && score < maxScore * 0.5f)
        {
            Debug.Log($"Счет {score}/{maxScore} < 50%. Запуск Bad Outcome.");
            finalManager = BadOutcomeVNManager;
            EndCounter counter = FindObjectOfType<EndCounter>();
            if (counter != null)
            {
                counter.IncrementBadEndingCount();
            }
        }
        else
        {
            Debug.Log($"Счет {score}/{maxScore} >= 50%. Запуск Good Outcome.");
            finalManager = GoodOutcomeVNManager;
        }

        if (finalManager != null)
        {
            finalManager.gameObject.SetActive(true);
            StartCoroutine(WaitForVNToFinish(finalManager, GoToNextScene));
        }
        else
        {
            Debug.LogWarning("Финальный VisualNovel Manager не назначен. Переход к следующей сцене.");
            GoToNextScene();
        }
    }

    private void GoToNextScene()
    {
        Debug.Log("Фаза: Переход на следующую сцену.");

        if (!string.IsNullOrEmpty(NextSceneName))
        {
            SceneManager.LoadScene(NextSceneName);
        }
        else
        {
            Debug.LogError("Имя следующей сцены не указано! Невозможно перейти.");
        }
    }
}