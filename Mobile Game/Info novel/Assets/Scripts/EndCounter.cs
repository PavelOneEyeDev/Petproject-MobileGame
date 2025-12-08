using UnityEngine;

public class EndCounter : MonoBehaviour
{
    // Статический экземпляр синглтона
    public static EndCounter Instance { get; private set; }

    private int badEndingCount = 0;

    public int BadEndingCount
    {
        get { return badEndingCount; }
    }

    private void Awake()
    {
        // Реализация паттерна Синглтон:
        if (Instance == null)
        {
            Instance = this;
            // Сохраняем объект между сменами сцен
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Если экземпляр уже существует, уничтожаем этот новый
            Destroy(gameObject);
        }
    }

    public void IncrementBadEndingCount()
    {
        badEndingCount++;
        Debug.Log($"EndCounter: Счетчик плохих исходов увеличен. Текущее значение: {badEndingCount}");
    }
}
