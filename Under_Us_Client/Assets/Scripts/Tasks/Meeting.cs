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
            Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.meetingChoice);

            string buttonName = EventSystem.current.currentSelectedGameObject.transform.parent.name;

            if (buttonName.Substring(0, 13) == "PlayerSection")
            {
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
