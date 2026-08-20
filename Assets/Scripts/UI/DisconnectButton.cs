using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisconnectButton : MonoBehaviour
{
    [SerializeField]
    private Button button;

    private void Awake()
    {
        button.onClick.AddListener(OnDisconnect);    
    }

    private void OnDisconnect()
    {
        if (NetworkRepository.Current.IsServer)
        {
            SceneLoader.LoadMainMenuScene();
            return;
        }

        NetworkBus.OnLocalClientDisconnected?.Invoke();
    }
}
