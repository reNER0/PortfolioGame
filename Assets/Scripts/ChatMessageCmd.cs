using System;
using Assets.Scripts.Network.Commands;
using UnityEngine;

[Serializable]
public class ChatMessageCmd : SerializableClass, ICommand
{
    [SerializeField]
    private string _text;

    public ChatMessageCmd(string text)
    {
        _text = text;
    }

    public void Execute()
    {
        UIBus.OnChatMessage?.Invoke(new ChatMessage() 
        {
            sender = senderId.ToString(), 
            text = _text
        });
    }
}

public struct ChatMessage 
{
    public string sender;
    public string text;
}