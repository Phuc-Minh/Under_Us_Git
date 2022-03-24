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

        // Meeting Duration
        yield return new WaitForSeconds(MeetingDuration);

        if(Player.list.Count > 0)
        {
            foreach (Player player in Player.list.Values)
            {
                player.GetComponent<PlayerMovement>().ResetMoveSpeed();
            }
        }

        // Display vote result
        MeetingResult();
        yield return new WaitForSeconds(4);

        // Display eject animation
        EjectAnimation();
        yield return new WaitForSeconds(9);

        // Display eject result
        // Meeting end notification
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.meetingEnd);
        NetworkManager.Singleton.Server.SendToAll(message);

        meetingInProgress = false;
        startColddown = false;
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
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerInteract);
        message.AddBool(true);

        NetworkManager.Singleton.Server.Send(message, toClientId);
    }

    private void PlayerLeaveMeeting(ushort toClientId)
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerInteract);
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
            if(Player.list[maxId].Role == 2)
                message.AddBool(true);
            else
                message.AddBool(false);
        }

        // Send meeting result to player
        NetworkManager.Singleton.Server.SendToAll(message);
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
