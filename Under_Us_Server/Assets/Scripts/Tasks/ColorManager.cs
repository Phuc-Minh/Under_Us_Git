using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.layer == 6 || collider.gameObject.layer == 7)
        {
            PlayerEnterColor(collider.transform.GetComponent<Player>().Id);
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.layer == 6 || collider.gameObject.layer == 7)
        {
            PlayerEnterColor(collider.transform.GetComponent<Player>().Id);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer == 6 || collider.gameObject.layer == 7)
        {
            PlayerLeaveColor(collider.transform.GetComponent<Player>().Id);
        }
    }

    #region Messages
    private void PlayerEnterColor(ushort toClientId)
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.colorZone);
        message.AddBool(true);
        message.AddBools(GameLogic.tableColor);

        NetworkManager.Singleton.Server.Send(message, toClientId);
    }

    private void PlayerLeaveColor(ushort toClientId)
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.colorZone);
        message.AddBool(false);

        NetworkManager.Singleton.Server.Send(message, toClientId);
    }
    #endregion
}

