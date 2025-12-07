using UnityEngine;
using UnityEngine.SceneManagement; // Для смены сцен
using System.Collections; // Для корутин, если потребуется

public class ScenePlotManager : MonoBehaviour
{
    // --- 1. Ссылки на менеджеры сцены ---

    [Header("1. Менеджеры Сюжета")]
    [Tooltip("Основной менеджер, запускающий диалог в начале сцены.")]
    public VisualNovelManager InitialVNManager;

    [Tooltip("Менеджер, управляющий тестом.")]
    public TestFlowManager SceneTestManager;

    [Tooltip("Диалог, если результат теста НЕУДОВЛЕТВОРИТЕЛЬНЫЙ (< 50%).")]
    public VisualNovelManager BadOutcomeVNManager;

    [Tooltip("Диалог, если результат теста УДОВЛЕТВОРИТЕЛЬНЫЙ (>= 50%).")]
    public VisualNovelManager GoodOutcomeVNManager;

    // --- 2. Настройки перехода ---

    [Header("2. Переход на Следующую Сцену")]
    [Tooltip("Имя следующей сцены, куда нужно перейти после завершения сюжета.")]
    public string NextSceneName;

    // --- 3. Внутренние переменные ---

    private VisualNovelManager activeVNManager = null; // Текущий активный VN Manager

    // --- 4. Логика Сюжета ---

    void Start()
    {
        // Убедимся, что все элементы изначально неактивны
        SetupSceneManagers();

        // 1. Начинаем сюжет с основного диалога
        if (InitialVNManager != null)
        {
            activeVNManager = InitialVNManager;
            // Инициализируем и запускаем основной диалог
            // (Предполагаем, что Start() в InitialVNManager уже запускает диалог)
            InitialVNManager.gameObject.SetActive(true);

            // Запускаем корутину для отслеживания завершения диалога
            StartCoroutine(WaitForVNToFinish(InitialVNManager, StartTestPhase));
        }
        else
        {
            Debug.LogError("InitialVNManager не назначен. Невозможно начать сцену.");
        }
    }

    /// <summary>
    /// Устанавливает начальное состояние всех менеджеров.
    /// </summary>
    private void SetupSceneManagers()
    {
        // Отключаем все VN менеджеры, кроме того, который запустится
        if (InitialVNManager != null) InitialVNManager.gameObject.SetActive(false);
        if (BadOutcomeVNManager != null) BadOutcomeVNManager.gameObject.SetActive(false);
        if (GoodOutcomeVNManager != null) GoodOutcomeVNManager.gameObject.SetActive(false);

        // Отключаем панель теста
        if (SceneTestManager != null) SceneTestManager.TestPanel.SetActive(false);
    }

    // --- 5. Фазы Сюжета ---

    /// <summary>
    /// Фаза 1: Ожидание завершения Visual Novel.
    /// </summary>
    IEnumerator WaitForVNToFinish(VisualNovelManager vnManager, System.Action onFinishAction)
    {
        // Ждем, пока NameText не станет пустым, а DialogueText не станет "КОНЕЦ" (по логике VN Manager)
        yield return new WaitUntil(() => vnManager.IsDialogEnded);

        // Отключаем завершенный VN Manager
        vnManager.gameObject.SetActive(false);

        // Переход к следующей фазе
        onFinishAction?.Invoke();
    }

    /// <summary>
    /// Фаза 2: Запуск Теста.
    /// </summary>
    private void StartTestPhase()
    {
        Debug.Log("Фаза: Тест.");
        if (SceneTestManager != null)
        {
            // Запускаем тест
            SceneTestManager.StartTest();

            // Ждем завершения теста
            StartCoroutine(WaitForTestToFinish());
        }
        else
        {
            Debug.LogError("SceneTestManager не назначен. Пропускаем тест.");
            StartFinalVNPhase(0, 0); // Пропускаем тест и переходим к финалу
        }
    }

    /// <summary>
    /// Фаза 3: Ожидание завершения Теста.
    /// </summary>
    IEnumerator WaitForTestToFinish()
    {
        yield return new WaitUntil(() => !SceneTestManager.TestPanel.activeSelf);

        StartFinalVNPhase(SceneTestManager.TotalScore, SceneTestManager.MaxPossibleScore);
    }

    private void StartFinalVNPhase(int score, int maxScore)
    {
        Debug.Log("Фаза: Финальный Диалог.");
        VisualNovelManager finalManager = null;

        // Проверка условия (Меньше половины от максимального)
        if (maxScore >= 0 && score < maxScore * 0.5f)
        {
            // Плохой исход
            Debug.Log($"Счет {score}/{maxScore} < 50%. Запуск Bad Outcome.");
            finalManager = BadOutcomeVNManager;
        }
        else
        {
            // Хороший исход
            Debug.Log($"Счет {score}/{maxScore} >= 50%. Запуск Good Outcome.");
            finalManager = GoodOutcomeVNManager;
        }

        if (finalManager != null)
        {
            activeVNManager = finalManager;
            finalManager.gameObject.SetActive(true);

            // Ждем завершения финального диалога
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
            // Загрузка следующей сцены
            SceneManager.LoadScene(NextSceneName);
        }
        else
        {
            Debug.LogError("Имя следующей сцены не указано! Невозможно перейти.");
        }
    }
}