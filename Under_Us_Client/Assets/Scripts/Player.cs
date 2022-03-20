using RiptideNetworking;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //Color
    public static Texture[] textureArray;
    public static Texture[] textureBackpackArray;
    public int oldColor = 10;

    //General info
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }
    /*============== ROLE =============
        1 = Comrade
        2 = Impostor
        3 = Ghost
        4 = Ghost Impostor
    */
    public ushort Role { get; set; }


    //Animation + Movement
    [SerializeField] private PlayerAnimationManager animationManager;
    [SerializeField] private Transform camTransform;

    private string username;

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    private void Move(Vector3 newPosition, Vector3 forward)
    {
        transform.position = newPosition;

        if (!IsLocal)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(forward.x,0,forward.z));
            animationManager.AnimateBasedOnSpeed();
        }
    }

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        Player player;
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = true;
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = false;
        }

        player.name = $"Player {id}";
        player.Id = id;
        player.username = username;
        player.Role = 1;

        list.Add(id, player);
    }

    #region Color
    private void ChangeColor(int color)
    {
        ChangePlayerColor(color, this.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Renderer>().materials);
    }

    private static void ChangePlayerTexture(Material[] playerMaterials, int colorId)
    {
        playerMaterials[1].SetTexture("_MainTex", textureArray[colorId]);
        playerMaterials[2].SetTexture("_MainTex", textureBackpackArray[colorId]);
    }

    public void ChangePlayerColor(int colorCode, Material[] playerMaterials)
    {
        if (textureArray == null)
        {
            textureArray = Resources.LoadAll<Texture>("AstronautColor");
            textureBackpackArray = Resources.LoadAll<Texture>("AstronautBackpackColor");
        }

        switch (colorCode)
        {
            case 1:
                ChangePlayerTexture(playerMaterials, 0);
                break;
            case 2:
                ChangePlayerTexture(playerMaterials, 1);
                break;
            case 3:
                ChangePlayerTexture(playerMaterials, 2);
                break;
            case 4:
                ChangePlayerTexture(playerMaterials, 3);
                break;
            case 5:
                ChangePlayerTexture(playerMaterials, 4);
                break;
            case 6:
                ChangePlayerTexture(playerMaterials, 5);
                break;
            case 7:
                ChangePlayerTexture(playerMaterials, 6);
                break;
            case 8:
                ChangePlayerTexture(playerMaterials, 7);
                break;
            case 9:
                ChangePlayerTexture(playerMaterials, 8);
                break;
            case 10:
                ChangePlayerTexture(playerMaterials, 9);
                break;
            default:
                break;
        }
    }
    #endregion

    #region Statut
    private void SetRole(ushort role)
    {
        if (IsLocal)
        {
            Role = role;
            switch (Role)
            {
                case 1:
                    Debug.Log("Comrade");
                    break;
                case 2:
                    Debug.Log("Impostor");
                    break;
                case 3:
                    Debug.Log("Ghost");
                    break;
                default:
                    break;
            }
            // Annouce
            GameObject annoucementText = GameObject.Find("GameplayScreen");
            if (annoucementText != null)
            {

                if(role == 2)
                    annoucementText.transform.GetChild(2).GetComponent<Text>().text = "You are an impostor";
                else if(role == 1)
                    annoucementText.transform.GetChild(2).GetComponent<Text>().text = "You are a comrade";

                annoucementText.transform.GetChild(2).gameObject.SetActive(true);
                annoucementText.transform.GetChild(2).GetComponent<Animation>().Play();
            }
        }
    }
    #endregion

    #region Messages
    public void SendPlayerChangeColor(int colorId, int oldColor)
    {
        this.oldColor = colorId;

        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.playerChangeColor);
        message.AddUShort(this.Id);
        message.AddInt(colorId);
        message.AddInt(oldColor);


        NetworkManager.Singleton.Client.Send(message);
    }


    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.Move(message.GetVector3(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerChangeColor)]
    private static void PlayerChangeColor(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
        {
            int color = message.GetInt();
            player.ChangeColor(color + 1);
            if(!player.IsLocal)
                player.oldColor = color;
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerRole)]
    private static void RetrieveRole(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.SetRole(message.GetUShort());
    }

    [MessageHandler((ushort)ServerToClientId.playerTeleport)]
    private static void PlayerTeleport(Message message)
    {
        PlayerController.inMeeting = true;
        float meetingDuration = message.GetFloat();

        //Display meeting notification
        GameObject connectUI = GameObject.Find("GameplayScreen");
        connectUI.transform.GetChild(2).GetComponent<Text>().text = "Meeting started";
        connectUI.transform.GetChild(2).gameObject.SetActive(true);
        connectUI.transform.GetChild(2).GetComponent<Animation>().Play();

        //Play meeting timer
        connectUI.transform.GetChild(5).gameObject.SetActive(true);
        connectUI.transform.GetChild(5).GetChild(0).GetComponent<Coldown>().ResetCooldown();
        connectUI.transform.GetChild(5).GetChild(0).GetComponent<Coldown>().StartTimer();

        foreach (Player player in Player.list.Values)
        {
            if(player.Id == message.GetUShort())
            player.transform.position = message.GetVector3();
        }

        Message messageToSend = Message.Create(MessageSendMode.reliable, ClientToServerId.playerInMeeting);
        NetworkManager.Singleton.Client.Send(messageToSend);
    }
    #endregion
}