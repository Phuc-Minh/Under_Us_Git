using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Meeting : MonoBehaviour
{
    public static bool startColddown;
    private static float MeetingDuration = 10;
    bool meetingInProgress;
    public static Dictionary<ushort, int> playerVote = new Dictionary<ushort, int>();
    public static List<ushort> playerVoted = new List<ushort>();

    public static float GetMeetingDuration()
    {
        return MeetingDuration;
    }

    private void Update()
    {
        if(startColddown)
        {
            if (!meetingInProgress)
                StartCoroutine(MeetingColddown());
        }
    }

    IEnumerator MeetingColddown()
    {
        meetingInProgress = true;

        yield return new WaitForSeconds(0.25f);
        Message messageEButton = Message.Create(MessageSendMode.unreliable, ServerToClientId.togglePlayerInteract);
        messageEButton.AddBool(true);
        NetworkManager.Singleton.Server.SendToAll(messageEButton);

        // Meeting Duration
        yield return new WaitForSeconds(MeetingDuration-0.25f);
        
        // Display vote result
        MeetingResult();
        yield return new WaitForSeconds(4);

        // Display eject animation
        EjectAnimation();
        yield return new WaitForSeconds(9);

        // Reset player speed
        if (Player.list.Count > 0)
        {
            foreach (Player player in Player.list.Values)
            {
                player.GetComponent<PlayerMovement>().ResetMoveSpeed();
            }
        }

        // Display eject result
        // Meeting end notification
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.meetingEnd);
        NetworkManager.Singleton.Server.SendToAll(message);

        meetingInProgress = false;
        startColddown = false;

        // Check Win condition
        int ImpostorCount = 0;
        int ComradeCount = 0;
        foreach (Player player in Player.list.Values)
        {
            if (player.Role == 1)
                ComradeCount++;
            else if (player.Role == 2)
                ImpostorCount++;
        }
        if (ImpostorCount >= ComradeCount)
        {
            WinCondition.ImpostorWins();
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == 6 || collider.gameObject.layer == 7)
        {
            PlayerEnterMeeting(collider.transform.GetComponent<Player>().Id);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer == 6 || collider.gameObject.layer == 7)
        {
            PlayerLeaveMeeting(collider.transform.GetComponent<Player>().Id);
        }
    }

    public static void resetPlayerVote()
    {
        playerVote.Clear();
        playerVoted.Clear();

        playerVote.Add(0, 0);
        foreach (Player player in Player.list.Values)
        {
            playerVote.Add(player.Id, 0);
        }
    }


    #region Messages
    private void PlayerEnterMeeting(ushort toClientId)
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.togglePlayerInteract);
        message.AddBool(true);

        NetworkManager.Singleton.Server.Send(message, toClientId);
    }

    private void PlayerLeaveMeeting(ushort toClientId)
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.togglePlayerInteract);
        message.AddBool(false);

        NetworkManager.Singleton.Server.Send(message, toClientId);
    }

    private void MeetingResult()
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.meetingResult);
        message.AddInt(playerVote.Count);

        // Auto vote if player did not pick their choice
        foreach (Player player in Player.list.Values)
        {
            if (!playerVoted.Contains(player.Id))
                playerVote[0]++;
        }

        // Add all vote to message
        foreach (ushort playerId in playerVote.Keys)
        {
            message.AddUShort(playerId);
            message.AddInt(playerVote[playerId]);
        }

        // Send meeting result to player
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void EjectAnimation()
    {
        ushort maxId = 0;
        int maxCount = 0;
        bool duplicateMaxCount = false;

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.ejectResult);

        // Get the most voted player
        foreach (ushort playerId in playerVote.Keys)
        {
            if (playerVote[playerId] > maxCount)
            {
                maxId = playerId;
                maxCount = playerVote[playerId];
                duplicateMaxCount = false;
            }
            else if (playerVote[playerId] == maxCount)
                duplicateMaxCount = true;
        }

        // Add the most voted player 
        // 9999 if there are two player with the same vote
        if (duplicateMaxCount)
            message.AddUShort(9999);
        else
        {
            message.AddUShort(maxId);

            // Send result if player rejected is an impostor
            if(maxId != 0)
            {
                if (Player.list[maxId].Role == 2)
                    message.AddBool(true);
                else
                    message.AddBool(false);

                // Send message notice rejected player is dead 
                Message messageToSend = Message.Create(MessageSendMode.reliable, ServerToClientId.playerDead);
                // True if player die in meeting, no dead body is displayed
                messageToSend.AddBool(true);
                messageToSend.AddUShort(maxId);

                // Instantiate dead player tombstone
                //instantiateDeadPlayer(maxId);

                //Change the dead player role to Ghost
                Player.list[maxId].Role = 3;

                //Add impostor layer to all of it's children
                Impostor.SetLayerRecursively(Player.list[maxId].gameObject, 8);

                //Notice all player that someone is dead
                NetworkManager.Singleton.Server.SendToAll(messageToSend);
            }
        }

        // Send meeting result to player
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public static void instantiateDeadPlayer(ushort deadPlayerId)
    {
        GameLogic.Singleton.DeadPlayerPrefab.name = "Tombstone";
        GameLogic.Singleton.DeadPlayerPrefab.transform.position = new Vector3(Player.list[deadPlayerId].gameObject.transform.position.x,
                                                                    Player.list[deadPlayerId].gameObject.transform.position.y - 0.5f,
                                                                    Player.list[deadPlayerId].gameObject.transform.position.z);
        GameLogic.Singleton.DeadPlayerPrefab.transform.rotation = Player.list[deadPlayerId].gameObject.transform.rotation;
        Instantiate(GameLogic.Singleton.DeadPlayerPrefab);
    }

    [MessageHandler((ushort)ClientToServerId.meetingChoice)]
    private static void PlayerVote(ushort fromClientId, Message message)
    {
        int vote = message.GetInt();
        if (Player.list.TryGetValue(fromClientId, out Player player))
        {
            if (playerVote.ContainsKey((ushort) vote))
                playerVote[(ushort)vote]++;
            else
                playerVote.Add((ushort)vote, 1);

            // And to voted pool
            playerVoted.Add(fromClientId);

            // Send pick to all player to display "I voted" image
            Message messageToSend = Message.Create(MessageSendMode.reliable, ServerToClientId.meetingChoice);
            messageToSend.AddUShort(fromClientId);

            NetworkManager.Singleton.Server.SendToAll(messageToSend);
        }
        else
        {
            Debug.Log("Anonymous vote : " + vote);
        }

    }
    #endregion
}
