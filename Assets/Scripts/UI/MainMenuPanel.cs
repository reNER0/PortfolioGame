using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField]
    private Button hostButton;
    [SerializeField]
    private Button clientButton;
    [SerializeField]
    private Button exitButton;


    private void Awake()
    {
        hostButton.onClick.AddListener(OnHostButton);
        clientButton.onClick.AddListener(OnClientButton);
        exitButton.onClick.AddListener(OnExitButton);
    }


    private void OnHostButton()
    {
        SceneLoader.LoadHostScene();
    }

    private void OnClientButton()
    {
        SceneLoader.LoadClientScene();
    }

    private void OnExitButton()
    {
        Application.Quit();
    }
}
