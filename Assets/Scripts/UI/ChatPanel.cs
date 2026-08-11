using TMPro;
using UnityEngine;

public class ChatPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private ChatElement chatElement;

    private void Awake()
    {
        UIBus.OnChatMessage += OnMessage;
        inputField.onSubmit.AddListener(OnInputField);
    }

    private void OnMessage(ChatMessage chatMessage) 
    {
        Instantiate(chatElement, chatElement.transform.parent)
            .Init(chatMessage)
            .transform.SetSiblingIndex(0);
    }

    private void OnInputField(string text) 
    {
        inputField.text = string.Empty;

        var messageCmd = new ChatMessageCmd(text);

        NetworkBus.OnCommandSendToServer?.Invoke(messageCmd);
    }

    private void OnDestroy()
    {
        UIBus.OnChatMessage -= OnMessage;
    }
}
