using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "State/MapPreviewState", order = 2)]
public class MapPreviewState : State
{
    public override void OnEnter()
    {
        Debug.Log("We are now client and waiting server to start a game!");
    }

    public override void OnUpdate()
    {

    }

    public override void OnExit()
    {
    }
}
