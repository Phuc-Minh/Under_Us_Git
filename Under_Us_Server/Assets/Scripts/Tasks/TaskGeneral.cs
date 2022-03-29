using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskGeneral : MonoBehaviour
{
    public enum TaskId : ushort
    {
        electrical = 1,
    }

    private ushort id;
    private bool isFinished;

    public ushort GetId()
    {
        return id;
    }
    public bool GetIsFinished()
    {
        return isFinished;
    }
    public void SetId(ushort Id)
    {
        id = Id;
    }
    public void SetIsFinished(bool IsFinished)
    {
        isFinished = IsFinished;
    }

    private void OnTriggerEnter(Collider collider)
    {
        //Verify if collider is a comrade or an impostor
        if (collider.gameObject.layer == 6 || collider.gameObject.layer == 7)
        {
            PlayerEnterTaskZone(collider.transform.GetComponent<Player>().Id);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        //Verify if collider is a comrade or an impostor
        if (collider.gameObject.layer == 6 || collider.gameObject.layer == 7)
        {
            PlayerLeaveTaskZone(collider.transform.GetComponent<Player>().Id);
        }
    }

    #region Messages
    private void PlayerEnterTaskZone(ushort toClientId)
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.interact);
        message.AddBool(true);
        message.AddUShort(id);

        NetworkManager.Singleton.Server.Send(message, toClientId);
    }

    private void PlayerLeaveTaskZone(ushort toClientId)
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.interact);
        message.AddBool(false);

        NetworkManager.Singleton.Server.Send(message, toClientId);
    }
    #endregion
}
