using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReportDeadPlayer : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        //Verify if collider is a comrade or an impostor
        if (collider.gameObject.layer == 6 || collider.gameObject.layer == 7)
        {
            PlayerEnterTombStone(collider.transform.GetComponent<Player>().Id);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        //Verify if collider is a comrade or an impostor
        if (collider.gameObject.layer == 6 || collider.gameObject.layer == 7)
        {
            PlayerLeaveTombStone(collider.transform.GetComponent<Player>().Id);
        }
    }

    #region Messages
    private void PlayerEnterTombStone(ushort toClientId)
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.togglePlayerInteract);
        message.AddBool(true);

        NetworkManager.Singleton.Server.Send(message, toClientId);
    }

    private void PlayerLeaveTombStone(ushort toClientId)
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.togglePlayerInteract);
        message.AddBool(false);

        NetworkManager.Singleton.Server.Send(message, toClientId);
    }
    #endregion
}
