using UnityEngine;

public class Tutoryash : MonoBehaviour
{
    [System.Serializable]
    public struct Emotion
    {
        public string Key;
        public GameObject GoEmotion;
    }
    [SerializeField] private Emotion[] tutoryashEmotes;

    private void Start()
    {
        DeactivateAllEmotions();
    }

    public void ActivateEmotion(string key)
    {
        foreach(Emotion emo in tutoryashEmotes)
        {
            if (emo.GoEmotion != null)
            {
                if (emo.Key == key)
                {
                    emo.GoEmotion.SetActive(true);
                }
                else
                {
                    emo.GoEmotion.SetActive(false);
                }
            }
        }
    }

    public void DeactivateAllEmotions()
    {
        foreach (Emotion emo in tutoryashEmotes)
        {
            if (emo.GoEmotion != null)
            {
                emo.GoEmotion.SetActive(false);
            }
        }
    }
}
