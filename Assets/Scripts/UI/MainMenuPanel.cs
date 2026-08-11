using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField]
    private Button hostButton;
    [SerializeField]
    private Button clientButton;
    [SerializeField]
    private Button matchmakingButton;
    [SerializeField]
    private Button exitButton;

    private MatchmakingClient matchmaking;


    private void Start()
    {
        hostButton.onClick.AddListener(OnHostButton);
        clientButton.onClick.AddListener(OnClientButton);
        matchmakingButton.onClick.AddListener(OnMatchmakingButton);
        exitButton.onClick.AddListener(OnExitButton);


        if (!LaunchFlags.IsBot)
        {
            UIBus.OnChatMessage?.Invoke(new ChatMessage()
            {
                sender = "Game",
                text = "Welcome to my portfolio project!"
            });
            return;
        }

        // Find match if Bot
        OnMatchmakingButton();
        UIBus.OnChatMessage?.Invoke(new ChatMessage()
        {
            sender = "Game",
            text = "Started Bot state"
        });
    }


    private void OnHostButton()
    {
        SceneLoader.LoadHostScene();
    }

    private void OnClientButton()
    {
        SceneLoader.LoadClientScene();
    }

    private void OnMatchmakingButton()
    {
        matchmaking = gameObject.AddComponent<MatchmakingClient>();
        matchmaking.Error += OnMatchmakingFail;
        matchmaking.StartMatchmaking();
        SetButtonsState(false);
    }

    private void OnMatchmakingFail(string obj)
    {
        matchmaking.Error -= OnMatchmakingFail;
        Destroy(matchmaking);
        SetButtonsState(true);
    }

    private void SetButtonsState(bool state) 
    {
        hostButton.interactable = state;
        clientButton.interactable = state;
        matchmakingButton.interactable = state;
    }

    private void OnExitButton()
    {
        Application.Quit();
    }
}