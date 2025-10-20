using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField]
    private Button singlePlayerButton;
    [SerializeField]
    private Button multiPlayerButton;
    [SerializeField]
    private Button exitButton;


    private void Awake()
    {
        singlePlayerButton.onClick.AddListener(OnSinglePlayerButton);
        multiPlayerButton.onClick.AddListener(OnMultiPlayerButton);
        exitButton.onClick.AddListener(OnExitButton);
    }


    private void OnSinglePlayerButton()
    {
        SceneLoader.LoadHostScene();
    }

    private void OnMultiPlayerButton()
    {
        SceneLoader.LoadClientScene();
    }

    private void OnExitButton()
    {
        Application.Quit();
    }
}
