using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepOrder : TaskGeneral
{
    private int[] stepTable = new int[] { 1, 2, 3, 4, 5, 6 };
    private int[] stepTablePlayerVersion = new int[] { 0, 0, 0, 0, 0, 0};
    private int turn = 0;
    private ushort player = 0;

    public bool serverTurn = true;

    private void Start()
    {
        SetId((ushort)TaskId.StepOrder);
        listStatusTask.Add(GetId(), GetIsFinished());
        listTask.Add(GetId(), this);
    }

    private void Update()
    {
        // If no one is playing, don't check 
        if (player != 0)
        {
            if (turn == 0)
                StartCoroutine(PlayStepOrder());
        }
    }
    IEnumerator PlayStepOrder()
    {
        /*============================ Turn 1 ==================================*/
        /*======================================================================*/
        // Server turn : Instruction  phase
        turn = 1;
        serverTurn = true;
        ShuffleArrayAndCleanAnswer();
        SendStepInstruction(player, turn, "0Repeat message to save it");
        yield return new WaitForSeconds(2);
        SendStepMessage(player, turn, stepTable[turn - 1]);
        yield return new WaitForSeconds(1);
        serverTurn = false;

        // Player turn
        SendStepInstruction(player, turn, "0Your turn, you have 2 seconds");
        yield return new WaitForSeconds(2);

        // Server turn : Verification phase
        if (stepTable[0] == stepTablePlayerVersion[0]) {
            // Good step
            SendStepInstruction(player, turn, "1Message saved!");
            yield return new WaitForSeconds(2);
        }
        else {
            // Bad step
            SendStepInstruction(player, turn, "2Message failed to save! Press reset to try again.");
            yield return new WaitForSeconds(2);
        }

        //FORCE END
        turn = 0;
        player = 0;
        ShuffleArrayAndCleanAnswer();
    }

    private void ShuffleArrayAndCleanAnswer()
    {
        int lastIndex = stepTable.Length-1;

        while(lastIndex > 0) {
            int randomIndex = UnityEngine.Random.Range(0, lastIndex+1);
            int tempStep = stepTable[lastIndex];
            stepTable[lastIndex] = stepTable[randomIndex];
            stepTable[randomIndex] = tempStep;

            lastIndex--;
        }

        for (int i = 0; i < stepTablePlayerVersion.Length; i++)
            stepTablePlayerVersion[i] = 0;
    }

    [MessageHandler((ushort)ClientToServerId.stepButton)]
    private static void StepButton(ushort fromClientId, Message message)
    {
        // TODO : RESET IF PLAYING PLAYER GET OUT OF TASK ZONE
        StepOrder StepTask = TaskGeneral.listTask[message.GetUShort()].GetComponent<StepOrder>();

        // If sending player is not the one who is playing
        if (StepTask.player != 0 && fromClientId != StepTask.player)
            return;
        // Only start or reset instruction is allowed to be played during server turn
        bool StartStepTask = message.GetBool();
        if (!StartStepTask && StepTask.serverTurn)
            return;

        // Start or reset step task 
        if (StartStepTask)
        {
            StepTask.player = fromClientId;
        }
        else
        {
            for (int i = 0; i < StepTask.stepTablePlayerVersion.Length; i++)
            {
                if (StepTask.stepTablePlayerVersion[i] == 0) 
                {
                    StepTask.stepTablePlayerVersion[i] = message.GetUShort();
                    break;
                }
            }
        }
    }

    private static void SendStepMessage(ushort idPlayer, int turn, int step)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.stepButton);
        message.AddInt(turn);
        message.AddInt(step);

        NetworkManager.Singleton.Server.Send(message, idPlayer);
    }

    private static void SendStepInstruction(ushort idPlayer, int turn, string instruction)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.stepButtonInformation);
        message.AddInt(turn);
        message.AddString(instruction);

        NetworkManager.Singleton.Server.Send(message, idPlayer);
    }
}
