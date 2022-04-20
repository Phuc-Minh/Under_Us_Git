using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskGeneral : MonoBehaviour
{
    public static Dictionary<ushort, bool> listStatusTask = new Dictionary<ushort, bool>();
    public static Dictionary<ushort,TaskGeneral> listTask = new Dictionary<ushort, TaskGeneral>();

    public enum TaskId : ushort
    {
        Electrical = 1,
        ElectricalMeeting,
        ElectricalO2,
        ElectricalSpeciment,
        ElectricalLabo1,
        ElectricalLabo2,
        ElectricalLabo3,
        LavaMeter,
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
        bool[] oldArrayTask = new bool[listStatusTask.Count];
        int i = 0;
        foreach (bool item in listStatusTask.Values)
        {
            oldArrayTask[i] = item;
            i++;
        }

        isFinished = IsFinished;

        //Update to list
        listStatusTask[GetId()] = GetIsFinished();

        //Each time the list is updated check if all the task is finished
        bool[] newArrayTask = new bool[listStatusTask.Count];
        i = 0;
        foreach (bool item in listStatusTask.Values)
        {
            newArrayTask[i] = item;
            i++;
        }

        //If task is edited then send new task list to all player
        if(oldArrayTask != newArrayTask)
        {
            Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.taskList);

            message.AddInt(listStatusTask.Count);

            foreach (TaskId key in listStatusTask.Keys)
            {
                message.AddString(key.ToString());
                message.AddBool(listStatusTask[(ushort) key]);
            }

            NetworkManager.Singleton.Server.SendToAll(message);
        }


        // TODO :: PUT IN A SEPARATE SCRIPT FOR WIN CONDITION
        if(!Array.Exists(newArrayTask, button => button == false))
            Debug.Log("All task is finished, comrade wins");
    }

    private void OnTriggerEnter(Collider collider)
    {
        //Verify if collider is a comrade or an impostor or a ghost
        if (collider.gameObject.layer == 6 || collider.gameObject.layer == 7 || collider.gameObject.layer == 8)
        {
            PlayerEnterTaskZone(collider.transform.GetComponent<Player>().Id);
        }else
        {
            Debug.Log(collider.gameObject.layer);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        //Verify if collider is a comrade or an impostor or a ghost
        if (collider.gameObject.layer == 6 || collider.gameObject.layer == 7 || collider.gameObject.layer == 8)
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
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.interact);
        message.AddBool(false);

        NetworkManager.Singleton.Server.Send(message, toClientId);
    }
    #endregion
}
