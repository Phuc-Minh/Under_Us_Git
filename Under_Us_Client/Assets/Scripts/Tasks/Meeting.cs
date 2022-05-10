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
                    Debug.Log("Can not vote if you are dead");
                    return;
                }
            }

            if (buttonName.Substring(0, 13) == "PlayerSection")
            {
                // Can not self vote
                if (Player.list[ushort.Parse(buttonName.Substring(13))].IsLocal)
                    return;

                // Can not vote dead player
                if (Player.list[ushort.Parse(buttonName.Substring(13))].Role == 3)
                    return;

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
            Debug.Log("Already voted");
        }
    }

    public void ResetMeeting()
    {
        voted = false;
    }
}
