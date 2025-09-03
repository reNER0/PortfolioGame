using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField]
    private string sharedScene;
    [SerializeField]
    private string serverScene;
    [SerializeField]
    private string clientScene;

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
        SceneManager.LoadScene(sharedScene);
        SceneManager.LoadScene(serverScene, LoadSceneMode.Additive);
    }

    private void OnMultiPlayerButton()
    {
        SceneManager.LoadScene(sharedScene);
        SceneManager.LoadScene(clientScene, LoadSceneMode.Additive);
    }

    private void OnExitButton()
    {
        Application.Quit();
    }
}
