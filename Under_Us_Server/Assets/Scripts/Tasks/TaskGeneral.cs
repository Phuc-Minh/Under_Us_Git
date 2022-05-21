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

    public static string GetName(ushort idTask)
    {
        string taskName = "";
        foreach (TaskId key in listTask.Keys)
        {
            if ((ushort)key == idTask)
            {
                taskName = key.ToString();
                break;
            }
        }

        return taskName;
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
        float finishedTask = 0;
        foreach (bool item in listStatusTask.Values)
        {
            newArrayTask[i] = item;
            if (item)
                finishedTask++;

            i++;
        }

        //If task is edited then send new task list to all player
        if (oldArrayTask != newArrayTask)
            SendNewTaskList(finishedTask / listStatusTask.Count);

        // TODO :: PUT IN A SEPARATE SCRIPT FOR WIN CONDITION
        // TODO :: Also check when all impostor is dead

        if (GameLogic.gameInProcess)
        {
            if (!Array.Exists(newArrayTask, button => button == false))
            {
                WinCondition.ComradeWins(true);
                return;
            }
        }
        
    }

    public static void SendNewTaskList(float FinishedTaskPercentage)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.taskList);

        //Add the total number of task
        message.AddInt(listStatusTask.Count);
        //Add the number of unfinished task
        message.AddFloat(FinishedTaskPercentage);

        foreach (TaskId key in listStatusTask.Keys)
        {
            message.AddString(key.ToString());
            message.AddBool(listStatusTask[(ushort)key]);
        }

        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void OnTriggerEnter(Collider collider)
    {
        //Verify if collider is a comrade or an impostor or a ghost
        if (collider.gameObject.layer == 6 
            || collider.gameObject.layer == 7 
            /*|| collider.gameObject.layer == 8*/)
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
        if (collider.gameObject.layer == 6 
            || collider.gameObject.layer == 7 
            /*|| collider.gameObject.layer == 8*/)
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

    [MessageHandler((ushort)ClientToServerId.sabotageTask)]
    private static void Sabotage(ushort fromClientId, Message message)
    {

        // 0 Error
        // 1 Light
        // 2 Door
        ushort sabotageType = 0;

        ushort idSabotageTask = message.GetUShort();

        if (!Electrical.sabotageOnColddown)
        {
            // Sabotage Electrical Task
            // There are 8 electrical task that Impostor can sabotage so sabotageType will always be 1 for them
            if (idSabotageTask > 0 && idSabotageTask < 8)
            {
                sabotageType = 1;

                if (listTask[idSabotageTask].TryGetComponent(out Electrical electrical))
                    electrical.isSabotaged = true;

                // Put sabotage on colddown
                Electrical.sabotageTimeSpan = Time.time + Electrical.sabotageColddown;
                Electrical.sabotageOnColddown = true;

                for (int i = 0; i < 10; i++)
                {
                    bool status = UnityEngine.Random.Range(0, 2) > 0.5f ? true : false;

                    // Only edit if status random is false
                    // Only turns button off
                    if (!status)
                        Electrical.EditElectricalButton(idSabotageTask, i, status);
                }
            }

            Message messageToSend = Message.Create(MessageSendMode.reliable, ServerToClientId.sabotage);
            // Sabotage On or Off
            messageToSend.AddBool(true);
            messageToSend.AddUShort(sabotageType);
            // If sabotage is light, also add light name to distingue
            if (sabotageType == 1 && listTask.ContainsKey(idSabotageTask))
                messageToSend.AddString(TaskGeneral.GetName(idSabotageTask));

            NetworkManager.Singleton.Server.SendToAll(messageToSend);
        }
        else
        {
            // Notice impostor that they can't sabotage
            NetworkManager.AnnounceToClient(fromClientId, $"You have to wait {Mathf.FloorToInt(Electrical.sabotageTimeSpan - Time.time)} sec to sabotage", true);
        }
        
    }
    #endregion
}
