using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCondition : MonoBehaviour
{
    public static void ImpostorWins()
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.endGame);

        //Impostor have more or equal number as comrade
        message.AddString("Impostor wins");
        message.AddInt(1);
        foreach (Player player in Player.list.Values)
        {
            if (player.Role == 2 || player.Role == 4)
                message.AddUShort(player.Id);
        }

        NetworkManager.Singleton.Server.SendToAll(message);

        GameLogic.gameInProcess = false;

        RemoveAllPlayerSpeed();
    }

    public static void ComradeWins(bool winByTask)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.endGame);

        // ALL TASK FINISHED
        int comradePlayer = 0;

        if(winByTask)
            message.AddString("All task is finished, comrade wins");
        else
            message.AddString("All Impostor die, comrade wins");

        foreach (Player player in Player.list.Values)
        {
            if (player.Role == 1 || player.Role == 3)
                comradePlayer++;
        }
        message.AddInt(comradePlayer);
        foreach (Player player in Player.list.Values)
        {
            if (player.Role == 1 || player.Role == 3)
                message.AddUShort(player.Id);
        }

        NetworkManager.Singleton.Server.SendToAll(message);

        GameLogic.gameInProcess = false;

        RemoveAllPlayerSpeed();
    }

    public static void RemoveAllPlayerSpeed()
    {
        foreach (Player player in Player.list.Values)
            player.GetComponent<PlayerMovement>().RemoveMoveSpeed();
    }
}
