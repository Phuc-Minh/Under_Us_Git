using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class Meeting : MonoBehaviour
{
    private bool voted;

    public void Voted()
    {
        if(!voted)
        {
            string buttonName = EventSystem.current.currentSelectedGameObject.transform.parent.name;

            Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.meetingChoice);

            // Can not vote if player is dead
            foreach (Player player in Player.list.Values)
            {
                // If the button is the same player as player
                if (player.IsLocal && player.Role == 3)
                {
                    UIGameplayManager.AddMessageToAnnouncement("Can't vote if you are dead",true);
                    return;
                }
            }

            if (buttonName.Substring(0, 13) == "PlayerSection")
            {
                // Can not self vote
                if (Player.list[ushort.Parse(buttonName.Substring(13))].IsLocal)
                {
                    UIGameplayManager.AddMessageToAnnouncement("Can't vote for yourself", true);
                    return;
                }

                // Can not vote dead player
                if (Player.list[ushort.Parse(buttonName.Substring(13))].Role == 3)
                {
                    UIGameplayManager.AddMessageToAnnouncement("Can't vote for dead player", true);
                    return;
                }

                message.AddInt(int.Parse(buttonName.Substring(13)));
            }
            else
            {
                message.AddInt(0);
            }

            voted = true;
            NetworkManager.Singleton.Client.Send(message);
        } 
        else
        {
            UIGameplayManager.AddMessageToAnnouncement("You already voted", true);
        }
    }

    public void ResetMeeting()
    {
        voted = false;
    }
}
