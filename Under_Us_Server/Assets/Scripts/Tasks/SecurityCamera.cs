using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        //Verify if collider is a comrade or an impostor
        if (collider.gameObject.layer == 6 || collider.gameObject.layer == 7 || collider.gameObject.layer == 8)
        {
            PlayerEnterLeaveSecurity(collider.transform.GetComponent<Player>().Id,true);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        //Verify if collider is a comrade or an impostor
        if (collider.gameObject.layer == 6 || collider.gameObject.layer == 7 || collider.gameObject.layer == 8)
        {
            PlayerEnterLeaveSecurity(collider.transform.GetComponent<Player>().Id,false);
        }
    }

    private void PlayerEnterLeaveSecurity(ushort toClientId,bool enterLeave)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.securityCamera);
        message.AddBool(enterLeave);

        NetworkManager.Singleton.Server.Send(message, toClientId);
    }
}
