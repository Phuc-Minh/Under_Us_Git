using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameMaster : MonoBehaviour
{
    public GameObject ImpostorPrefab => impostorPrefab;

    [Header("Prefabs")]
    [SerializeField] private GameObject impostorPrefab;

    public void StartGame()
    {
        GameLogic.tablePlayerId.Clear();

        TaskGeneral.SendNewTaskList(0);

        GameObject doors = GameObject.Find("Doors");
        if (doors != null)
        {
            doors.GetComponent<Animation>().Play();
        }

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.startGame);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public void DistributeRole()
    {
        int pickImpostor = Random.Range(0, GameLogic.tablePlayerId.Count);

        foreach (ushort playerId in GameLogic.tablePlayerId)
        {
            Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerRole);
            message.AddUShort(playerId);


            if (playerId == GameLogic.tablePlayerId[pickImpostor])
            {
                // Distribute impostor
                message.AddUShort(2);
                //Remove all Impostor object
                foreach (Transform child in Player.list[playerId].transform)
                {
                    if (child.name.StartsWith("Impostor"))
                        Destroy(child.gameObject);
                }
                Instantiate(impostorPrefab, Player.list[playerId].transform);

                //Modify Role attribute in Player
                Player.list[playerId].GetComponent<Player>().Role = 2;

                //Add impostor layer to all of it's children
                Player.list[playerId].gameObject.layer = 7;
                SetLayerRecursively(Player.list[playerId].gameObject, 7);

                Player.list[playerId].gameObject.transform.GetChild(2).gameObject.AddComponent<Impostor>();
            }
            else
            {
                //Modify Role attribute in Player
                Player.list[playerId].GetComponent<Player>().Role = 1;
                
                //Modifier layer
                SetLayerRecursively(Player.list[playerId].gameObject, 6);

                //Remove all Impostor object
                foreach (Transform child in Player.list[playerId].transform)
                {
                    if (child.name.StartsWith("Impostor"))
                        Destroy(child.gameObject);
                }

                // Distribute comrade
                message.AddUShort(1);
            }

            NetworkManager.Singleton.Server.Send(message, playerId);
        }
    }

    private static void SetLayerRecursively(GameObject gameObject, int newLayer)
    {
        gameObject.layer = newLayer;

        foreach (Transform child in gameObject.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    #region Meeting

    #endregion

    #region Message
    [MessageHandler((ushort)ClientToServerId.interative)]
    private static void PlayerInteractive(ushort fromClientId, Message message)
    {
        Collider[] Colliders = Physics.OverlapSphere(Player.list[fromClientId].transform.position, 2f);
        foreach (Collider Collider in Colliders)
        {
            // Check if collider is an interable object
            if (Collider.gameObject.layer == 11)
            {
                // Meeting Call
                if (Collider.gameObject.name == "MeetingButton" || Collider.gameObject.name == "Tombstone(Clone)")
                {
                    GameObject meetingTable = GameObject.Find("MeetingSeat");
                    if (meetingTable != null)
                    {
                        int i = 0;
                        PlayerMovement.enterMetting = true;

                        Message messageToSend = Message.Create(MessageSendMode.reliable, ServerToClientId.playerTeleport);

                        if (Collider.gameObject.name == "Tombstone")
                            messageToSend.AddInt(0);
                        else
                            messageToSend.AddInt(1);

                        messageToSend.AddFloat(Meeting.GetMeetingDuration());
                        foreach (Player player in Player.list.Values)
                        {
                            player.transform.position = new Vector3(meetingTable.transform.GetChild(i).position.x, 1, meetingTable.transform.GetChild(i).position.z);

                            messageToSend.AddUShort(player.Id);
                            messageToSend.AddVector3(player.transform.position);

                            i++;
                        }

                        NetworkManager.Singleton.Server.SendToAll(messageToSend);
                    }
                    break;
                }
                else
                {
                    // If it is not a meeting call, check it TaskGeneral component for IdTask
                    try
                    {
                        /*ushort TaskId = Collider.gameObject.GetComponent<TaskGeneral>().GetId();

                        Message messageToSend = Message.Create(MessageSendMode.reliable, ServerToClientId.interact);
                        messageToSend.AddUShort(TaskId);

                        NetworkManager.Singleton.Server.Send(messageToSend, fromClientId);*/
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log(e.Message);
                        Debug.Log(Collider.gameObject.name);

                    }
                }
            }
        }
    }

    [MessageHandler((ushort)ClientToServerId.playerInMeeting)]
    private static void RemoveSpeed(ushort fromClientId, Message message)
    {
        foreach (Player player in Player.list.Values)
        {
            player.GetComponent<PlayerMovement>().RemoveMoveSpeed();
        }

        PlayerMovement.enterMetting = false;

        Meeting.startColddown = true;
        Meeting.resetPlayerVote();
    }

    #endregion
}
