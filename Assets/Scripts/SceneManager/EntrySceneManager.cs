using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EntrySceneManager : MonoBehaviour
{
    private void Awake()
    {
#if UNITY_SERVER
        SceneLoader.LoadServerScene();
#else
        SceneLoader.LoadMainMenuScene();
#endif    
    }
}
