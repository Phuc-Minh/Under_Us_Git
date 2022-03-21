using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meeting : MonoBehaviour
{
    public static bool startColddown;
    private static float MeetingDuration = 10;
    bool meetingInProgress;
    public static Dictionary<ushort, int> playerVote = new Dictionary<ushort, int>();

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


        // Display eject animation


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
