using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaMeter : TaskGeneral
{
    private static int lavaTemp;
    private static int meterTemp;
    private static bool needCheckLava;

    private void Start()
    {
        SetId((ushort)TaskId.LavaMeter);
        listTask.Add(GetId(), GetIsFinished());

        lavaTemp = Mathf.RoundToInt(Random.Range(-50f, 50f));
        meterTemp = Mathf.RoundToInt(Random.Range(lavaTemp-30,lavaTemp+30));
    }

    private void Update()
    {
        //Only check when someone changes a button
        if (needCheckLava)
        {
            if(meterTemp == lavaTemp)
                SetIsFinished(true);
            else
                SetIsFinished(false);

            needCheckLava = false;
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        //Verify if collider is a comrade or an impostor
        if (collider.gameObject.layer == 6 || collider.gameObject.layer == 7)
        {
            PlayerStayLavaMeter(collider.transform.GetComponent<Player>().Id);
        }
    }

    private void PlayerStayLavaMeter(ushort toClientId)
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.lavaButton);
        message.AddInt(lavaTemp);
        message.AddInt(meterTemp);

        NetworkManager.Singleton.Server.Send(message, toClientId);
    }

    [MessageHandler((ushort)ClientToServerId.lavaButton)]
    private static void AdjustMeter(ushort fromClientId, Message message)
    {
        if (message.GetBool())
            meterTemp++;
        else
            meterTemp--;

        needCheckLava = true;
    }
}
