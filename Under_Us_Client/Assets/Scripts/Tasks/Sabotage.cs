using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Sabotage : MonoBehaviour
{
    public void SabotageTask()
    {
        // Verify if local player is an impostor
        if (Player.isImpostor())
        {
            Transform buttonName = EventSystem.current.currentSelectedGameObject.transform;

            Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.sabotageTask);
            message.AddUShort(ushort.Parse(buttonName.name.Substring(buttonName.name.IndexOf('_') + 1)));

            NetworkManager.Singleton.Client.Send(message);
        }
    }
}
