using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public static class SceneLoader
{
    // TODO : Remove hardcode
    private static string sharedScene = "SharedScene";
    private static string serverScene = "ServerScene";
    private static string hostScene = "HostScene";
    private static string clientScene = "ClientScene";
    private static string mainMenuScene = "MainMenuScene";


    public static void LoadServerScene()
    {
        SceneManager.LoadScene(sharedScene);
        SceneManager.LoadScene(serverScene, LoadSceneMode.Additive);
    }

    public static void LoadHostScene()
    {
        SceneManager.LoadScene(sharedScene);
        SceneManager.LoadScene(hostScene, LoadSceneMode.Additive);
    }

    public static void LoadClientScene()
    {
        SceneManager.LoadScene(sharedScene);
        SceneManager.LoadScene(clientScene, LoadSceneMode.Additive);
    }

    public static void LoadMainMenuScene()
    {
        SceneManager.LoadScene(mainMenuScene);
    }
}