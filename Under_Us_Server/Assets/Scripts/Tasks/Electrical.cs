using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Electrical : TaskGeneral
{
    public static bool[] tableElectric = new bool[10];

    private void Start()
    {
        SetId((ushort) TaskId.electrical);
    }

    private void Update()
    {
        SetIsFinished(!Array.Exists(tableElectric, button => button == false));
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
    private static void PlayerVote(ushort fromClientId, Message message)
    {
        tableElectric[message.GetUShort()] = message.GetBool();
    }
}
