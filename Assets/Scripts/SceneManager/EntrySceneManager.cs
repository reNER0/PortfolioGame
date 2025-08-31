using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EntrySceneManager : MonoBehaviour
{
    // TODO : Add seamless scene loading

    [SerializeField]
    private string serverScene;
    [SerializeField]
    private string clientScene;

    private void Awake()
    {
#if UNITY_SERVER
        SceneManager.LoadScene(serverScene);
#else
        SceneManager.LoadScene(clientScene);
#endif    
    }
}
