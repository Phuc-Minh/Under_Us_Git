using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepOrder : TaskGeneral
{
    private int[] stepTable = new int[] { 1, 2, 3, 4, 5, 6 };
    private int[] stepTablePlayerVersion = new int[] { 0, 0, 0, 0, 0, 0};
    public int turn = 0;
    public ushort player = 0;

    public bool serverTurn = true;

    private void Start()
    {
        switch (gameObject.name)
        {
            case "StepMachine":
                SetId((ushort)TaskId.StepOrder);
                break;
            case "StepMachineMeeting":
                SetId((ushort)TaskId.StepOrderMeeting);
                break;
            case "StepMachineSpeciment":
                SetId((ushort)TaskId.StepOrderSpeciment);
                break;
            default:
                break;
        }

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
        bool PlayerError = false;
        turn = 4;
        while (!PlayerError && turn < stepTable.Length)
        {
            /*============================ Turn 1 ==================================*/
            /*======================================================================*/
            // Server turn : Instruction  phase
            turn++;
            ShuffleArrayAndCleanAnswer();
            //Debug.Log($"Step Table : [{stepTable[0]},{stepTable[1]},{stepTable[2]},{stepTable[3]},{stepTable[4]},{stepTable[5]}]");
            SendStepInstruction(player, turn, "0Repeat message to save it");
            yield return new WaitForSeconds(2);
            for (int i = 0; i < turn; i++)
            {
                SendStepMessage(player, turn, stepTable[i]);
                yield return new WaitForSeconds(1);
            }

            serverTurn = false;
            // Player turn
            SendStepInstruction(player, turn, "0Your turn, you have 3 seconds");
            yield return new WaitForSeconds(3);
            serverTurn = true;

            // Server turn : Verification phase
            int counter = 0;
            while (!PlayerError && counter < turn)
            {
                if (stepTable[counter] != stepTablePlayerVersion[counter])
                {
                    PlayerError = true;
                }

                counter++;
            }

            if (!PlayerError)
            {
                // Good step
                SendStepInstruction(player, turn, "1Message saved!");
                yield return new WaitForSeconds(2);
            }
            else
            {
                // Bad step
                SendStepInstruction(player, turn, "2Message failed to save! Press reset to try again.");
                turn = 0;
                player = 0;
                yield return new WaitForSeconds(2);
            }
        }

        if (!PlayerError && turn == stepTable.Length)
        {
            // Task finish
            SendStepInstruction(player, turn, "1Task finished! Respect+");
            yield return new WaitForSeconds(3);
            SendStepInstruction(player, turn, "0 ");
            turn = 0;
            player = 0;
            SetIsFinished(true);
        }
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

        // If task is already finished
        if (StepTask.GetIsFinished())
        {
            SendStepInstruction(fromClientId, StepTask.turn, "1Task already finished!");
            return;
        }

        // If sending player is not the one who is playing
        if (StepTask.player != 0 && fromClientId != StepTask.player)
        {
            SendStepInstruction(fromClientId, StepTask.turn, "2Someone is already receiving message!");
            return;
        }

        // No one can play during server turn
        bool StartStepTask = message.GetBool();
        if (StepTask.player != 0 && StepTask.serverTurn)
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
