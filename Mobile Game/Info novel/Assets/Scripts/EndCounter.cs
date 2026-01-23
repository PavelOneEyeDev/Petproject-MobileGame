using UnityEngine;

public class EndCounter : MonoBehaviour
{
    public static EndCounter Instance { get; private set; }

    private int badEndingCount = 0;

    public int BadEndingCount
    {
        get { return badEndingCount; }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void IncrementBadEndingCount()
    {
        badEndingCount++;
    }

    public void ResetCounter()
    {
        badEndingCount = 0;
    }
}
