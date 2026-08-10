using TMPro;
using UnityEngine;

public class ChatElement : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;

    public ChatElement Init(ChatMessage chatMessage)
    {
        gameObject.SetActive(true);

        text.text = $"{chatMessage.sender}: {chatMessage.text}";

        return this;
    }
}
