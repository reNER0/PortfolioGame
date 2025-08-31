using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SharedSceneManager : MonoBehaviour
{
    // TODO : Add seamless scene loading

    [SerializeField]
    private string serverScene;
    [SerializeField]
    private string clientScene;

    private void Awake()
    {
#if UNITY_SERVER
        SceneManager.LoadScene(serverScene, LoadSceneMode.Additive);
#else
        SceneManager.LoadScene(clientScene, LoadSceneMode.Additive);
#endif    
    }
}
