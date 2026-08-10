using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIBus
{
    public static Action<ChatMessage> OnChatMessage;
}
