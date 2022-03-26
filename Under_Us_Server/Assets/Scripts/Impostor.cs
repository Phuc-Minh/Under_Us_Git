using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impostor : MonoBehaviour
{
    private void Update()
    {
        //Remove kill button if no other player is present
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, 1.5f);
        Collider closestTarget = GetClosestEnemyCollider(this.transform.position, hitColliders);

        if (closestTarget == null)
            PlayerLeaveKillZone();
    }

    static Collider GetClosestEnemyCollider(Vector3 position, Collider[] enemyColliders)
    {
        float bestDistance = 99999.0f;
        Collider bestCollider = null;

        foreach (Collider target in enemyColliders)
        {
            if (target.gameObject.layer == 6 && target.gameObject.name.StartsWith("Player"))
            {
                float distance = Vector3.Distance(position, target.transform.position);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCollider = target;
                }
            }
        }

        return bestCollider;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == 6)
        {
            PlayerEnterKillZone();
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer == 6)
        {
            PlayerLeaveKillZone();
        }
    }

    public static void SetLayerRecursively(GameObject gameObject, int newLayer)
    {
        gameObject.layer = newLayer;

        foreach (Transform child in gameObject.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    #region Messages
    private void PlayerEnterKillZone()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerInteracKillZone);
        message.AddBool(true);

        NetworkManager.Singleton.Server.Send(message, this.GetComponentInParent<Player>().Id);
    }

    private void PlayerLeaveKillZone()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerInteracKillZone);
        message.AddBool(false);

        NetworkManager.Singleton.Server.Send(message, this.GetComponentInParent<Player>().Id);
    }

    [MessageHandler((ushort)ClientToServerId.impostorSendKillNotif)]
    private static void KillTheClosestPlayer(ushort fromClientId, Message message)
    {
        Vector3 position = message.GetVector3();

        Collider[] hitColliders = Physics.OverlapSphere(position, 1.5f);
        Collider closestTarget = GetClosestEnemyCollider(position, hitColliders);

        if (closestTarget != null)
        {
            Debug.Log("Id Dead Player :" + closestTarget.gameObject.GetComponent<Player>().Id);

            Message messageToSend = Message.Create(MessageSendMode.reliable, ServerToClientId.playerDead);
            messageToSend.AddUShort(closestTarget.gameObject.GetComponent<Player>().Id);

            //Change the dead player role to Ghost
            closestTarget.gameObject.GetComponent<Player>().Role = 3;
            //Add impostor layer to all of it's children
            SetLayerRecursively(closestTarget.gameObject, 8);

            //Notice all player that someone is dead
            NetworkManager.Singleton.Server.SendToAll(messageToSend);
        }
    }
    #endregion
}