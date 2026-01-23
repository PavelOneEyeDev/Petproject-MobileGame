using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndingsManager : MonoBehaviour
{
    [Header("1. Менеджеры Финала")]
    [Tooltip("Основной менеджер, запускающий диалог перед концовкой.")]
    public VisualNovelManager FinalIntroVNManager;

    [Header("2. Менеджеры Концовок")]
    [Tooltip("Концовка при EndCounter == 0 (Хороший исход)")]
    public VisualNovelManager GoodEndingVNManager;

    [Tooltip("Концовка при EndCounter == 1 (Нейтральный исход)")]
    public VisualNovelManager NeutralEndingVNManager;

    [Tooltip("Концовка при EndCounter > 1 (Плохой исход)")]
    public VisualNovelManager BadEndingVNManager;

    [SerializeField] private string firstSceneName;

    void Start()
    {
        SetupEndingManagers();

        if (FinalIntroVNManager != null)
        {
            FinalIntroVNManager.gameObject.SetActive(true);
            StartCoroutine(WaitForVNToFinish(FinalIntroVNManager, DetermineAndStartEnding));
        }
        else
        {
            Debug.LogError("FinalIntroVNManager не назначен. Невозможно начать финал.");
            DetermineAndStartEnding();
        }
    }

    private void SetupEndingManagers()
    {
        if (FinalIntroVNManager != null) FinalIntroVNManager.gameObject.SetActive(false);
        if (GoodEndingVNManager != null) GoodEndingVNManager.gameObject.SetActive(false);
        if (NeutralEndingVNManager != null) NeutralEndingVNManager.gameObject.SetActive(false);
        if (BadEndingVNManager != null) BadEndingVNManager.gameObject.SetActive(false);
    }

    IEnumerator WaitForVNToFinish(VisualNovelManager vnManager, System.Action onFinishAction)
    {
        yield return new WaitUntil(() => vnManager.IsDialogEnded);

        vnManager.gameObject.SetActive(false);
        onFinishAction?.Invoke();
    }

    private void DetermineAndStartEnding()
    {
        EndCounter counter = FindObjectOfType<EndCounter>();

        if (counter == null)
        {
            Debug.LogError("EndCounter не найден на сцене! Невозможно определить концовку.");
            return;
        }

        int count = counter.BadEndingCount;
        VisualNovelManager finalVN = null;

        if (count == 0)
        {
            Debug.Log("Счетчик = 0. Запуск ХОРОШЕЙ концовки.");
            finalVN = GoodEndingVNManager;
        }
        else if (count == 1)
        {
            Debug.Log("Счетчик = 1. Запуск НЕЙТРАЛЬНОЙ концовки.");
            finalVN = NeutralEndingVNManager;
        }
        else
        {
            Debug.Log($"Счетчик = {count}. Запуск ПЛОХОЙ концовки.");
            finalVN = BadEndingVNManager;
        }

        finalVN.gameObject.SetActive(true);
        if (finalVN == BadEndingVNManager) 
        {
            StartCoroutine(WaitForVNToFinish(finalVN, GoToFirstScene));
        }
        else
        {
            StartCoroutine(WaitForVNToFinish(finalVN, ExitGame));
        }
    }

    private void ExitGame()
    {
        Application.Quit(); 
    }

    private void GoToFirstScene()
    {
        if (!string.IsNullOrEmpty(firstSceneName))
        {
            EndCounter counter = FindObjectOfType<EndCounter>();
            if (counter != null)
            {
                counter.ResetCounter();
            }
            SceneManager.LoadScene(firstSceneName);
        }
        else
        {
            Debug.LogError("Имя следующей сцены не указано! Невозможно перейти.");
        }
    }
}
