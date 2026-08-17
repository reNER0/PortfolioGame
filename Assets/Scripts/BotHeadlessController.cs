using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class BotHeadlessController : MonoBehaviour
{
    private static BotHeadlessController instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateForBot()
    {
        if (!LaunchFlags.IsBot || instance != null)
            return;

        new GameObject(nameof(BotHeadlessController))
            .AddComponent<BotHeadlessController>();
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        DisableUiInput();
    }

    private void OnDestroy()
    {
        if (instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        instance = null;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DisableUiInput();
    }

    private static void DisableUiInput()
    {
        foreach (var inputModule in FindObjectsOfType<BaseInputModule>(true))
            inputModule.enabled = false;

        foreach (var eventSystem in FindObjectsOfType<EventSystem>(true))
            eventSystem.enabled = false;

        foreach (var raycaster in FindObjectsOfType<GraphicRaycaster>(true))
            raycaster.enabled = false;
    }
}
