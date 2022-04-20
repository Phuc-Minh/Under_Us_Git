using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Electrical : TaskGeneral
{
    private bool[] tableElectric = new bool[10];
    private bool needCheckTable;

    private void Start()
    {
        switch (gameObject.name)
        {
            case "ElectricalBox":
                SetId((ushort)TaskId.Electrical);
                break;
            case "ElectricalMeetingBox":
                SetId((ushort)TaskId.ElectricalMeeting);
                break;
            case "ElectricalO2Box":
                SetId((ushort)TaskId.ElectricalO2);
                break;
            case "ElectricalSpecimentBox":
                SetId((ushort)TaskId.ElectricalSpeciment);
                break;
            case "ElectricalLabo1Box":
                SetId((ushort)TaskId.ElectricalLabo1);
                break;
            case "ElectricalLabo2Box":
                SetId((ushort)TaskId.ElectricalLabo2);
                break;
            case "ElectricalLabo3Box":
                SetId((ushort)TaskId.ElectricalLabo3);
                break;
            default:
                break;
        }
        listStatusTask.Add(GetId(), GetIsFinished());
        listTask.Add(GetId(),this);
    }

    private void Update()
    {
        //Only check when someone changes a button
        if (needCheckTable)
        {
            //Check id array electric contains a false value and update isFinished parameter
            SetIsFinished(!Array.Exists(tableElectric, button => button == false));

            needCheckTable = false;
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        //Verify if collider is a comrade or an impostor
        if (collider.gameObject.layer == 6 || collider.gameObject.layer == 7)
        {
            PlayerStayElectricBox(collider.transform.GetComponent<Player>().Id);
        }
    }

    private void PlayerStayElectricBox(ushort toClientId)
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.electricButton);
        message.AddBools(tableElectric);

        NetworkManager.Singleton.Server.Send(message, toClientId);
    }

    [MessageHandler((ushort)ClientToServerId.electricButton)]
    private static void SwitchElectrical(ushort fromClientId, Message message)
    {
        ushort idTask = message.GetUShort();

        listTask[idTask].gameObject.GetComponent<Electrical>().tableElectric[message.GetUShort()] = message.GetBool();
        listTask[idTask].gameObject.GetComponent<Electrical>().needCheckTable = true;
    }
}
