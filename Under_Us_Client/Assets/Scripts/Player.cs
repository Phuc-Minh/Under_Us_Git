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
    public static Sprite[] SpriteArray;
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
    [SerializeField] public Transform camTransform;
    [SerializeField] private Interpolator interpolator;

    private string username;

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public string GetName()
    {
        return username;
    }

    private void Move(ushort tick, Vector3 newPosition, Vector3 forward)
    {
        interpolator.NewUpdate(tick, newPosition);

        if (!IsLocal)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(forward.x, 0, forward.z));
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

            GameObject soundManager = GameObject.Find("SoundManager");
            soundManager.transform.GetChild(0).gameObject.SetActive(true);
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

    public static bool isImpostor()
    {
        foreach (Player player in Player.list.Values)
        {
            if (player.IsLocal && player.Role == 2)
                return true;
        }

        return false;
    }
    #region Color
    private void ChangeColor(int color)
    {
        ChangePlayerColor(color, this.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Renderer>().materials);
        string colorName = "";
        if (textureArray != null)
            colorName = " to " + textureArray[color - 1].name.Substring(textureArray[color - 1].name.IndexOf('_') + 1);
        UIGameplayManager.AddMessageToAnnouncement($"{this.GetName()} changed their color{colorName}",false);
    }

    public static void ChangePlayerTexture(Material[] playerMaterials, int colorId)
    {
        playerMaterials[1].SetTexture("_MainTex", textureArray[colorId]);
        playerMaterials[1].SetTexture("_EmissionMap", textureArray[colorId]);
        playerMaterials[2].SetTexture("_MainTex", textureBackpackArray[colorId]);
        playerMaterials[2].SetTexture("_EmissionMap", textureBackpackArray[colorId]);
    }

    public static void ChangePlayerColor(int colorCode, Material[] playerMaterials)
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
            // Edit player stat
            string roleText = "Role Distributed";
            Role = role;
            switch (Role)
            {
                case 1:
                    roleText = "You're a Comrade";
                    break;
                case 2:
                    roleText = "You're an Impostor";
                    break;
                case 3:
                    roleText = "You're now a Ghost";
                    break;
                default:
                    break;
            }
            UIGameplayManager.AddMessageToAnnouncement(roleText,false);

            // Role Distribute Stage
            GameObject roleAnimation = GameObject.Find("RoleAnimation");
            if (roleAnimation != null)
            {
                // Edit Stage
                roleAnimation.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
                roleAnimation.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);

                // Change player color in stage 
                Material[] playerMaterials = roleAnimation.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<Renderer>().sharedMaterials;
                ChangePlayerTexture(playerMaterials, this.oldColor);

                // Activate everything in Role Animation Object
                roleAnimation.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = roleText;
                roleAnimation.transform.GetChild(0).gameObject.SetActive(true);
                roleAnimation.transform.GetChild(1).gameObject.SetActive(true);
                roleAnimation.transform.GetChild(2).gameObject.SetActive(true);
                roleAnimation.transform.GetChild(3).gameObject.SetActive(true);
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
            player.Move(message.GetUShort(), message.GetVector3(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerChangeColor)]
    private static void PlayerChangeColor(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
        {
            int color = message.GetInt();
            player.ChangeColor(color + 1);
            if (!player.IsLocal)
                player.oldColor = color;
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerRole)]
    private static void RetrieveRole(Message message)
    {
        UIGameplayManager.CleanScreen();

        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.SetRole(message.GetUShort());
    }

    [MessageHandler((ushort)ServerToClientId.playerTeleport)]
    private static void PlayerTeleport(Message message)
    {
        UIGameplayManager.CleanScreen();
        GameObject connectUI = GameObject.Find("GameplayScreen");

        PlayerController.inMeeting = true;
        int meetingMode = message.GetInt();
        float meetingDuration = message.GetFloat();

        //Set up meeting screen
        Transform MeetingScreen = connectUI.transform.GetChild(4);
        if (SpriteArray == null)
            SpriteArray = Resources.LoadAll<Sprite>("MeetingCells");

        int countPlayer = 0;
        foreach (Player player in Player.list.Values)
        {
            MeetingScreen.GetChild(countPlayer).name = "PlayerSection" + player.Id;
            MeetingScreen.GetChild(countPlayer).transform.GetChild(0).GetComponent<Image>().sprite = SpriteArray[player.GetComponent<Player>().oldColor];
            MeetingScreen.GetChild(countPlayer).gameObject.SetActive(true);
            MeetingScreen.GetChild(countPlayer).gameObject.SetActive(true);
            MeetingScreen.GetChild(countPlayer).transform.GetChild(2).GetComponent<Text>().text = player.GetName();
            if(player.Role == 3)
                MeetingScreen.GetChild(countPlayer).transform.GetChild(3).gameObject.SetActive(true);
            
            countPlayer++;
        }

        //Display meeting notification
        if (meetingMode == 0)
            connectUI.transform.GetChild(2).GetComponent<Text>().text = "Dead Player Reported";
        else
            connectUI.transform.GetChild(2).GetComponent<Text>().text = "Meeting started";

        connectUI.transform.GetChild(2).gameObject.SetActive(true);
        connectUI.transform.GetChild(2).GetComponent<Animation>().Play("AppearRightNow");

        //Play meeting timer
        connectUI.transform.GetChild(5).gameObject.SetActive(true);
        connectUI.transform.GetChild(5).GetChild(0).GetComponent<Coldown>().ResetCooldown();
        connectUI.transform.GetChild(5).GetChild(0).GetComponent<Coldown>().StartTimer();

        //Reset Meeting
        for (int i = 0; i <= 7; i++)
        {
            //Remove I voted and vote count
            MeetingScreen.GetChild(i).GetChild(0).GetChild(0).gameObject.SetActive(false);
            MeetingScreen.GetChild(i).GetChild(1).gameObject.SetActive(false);
        }
        MeetingScreen.GetChild(8).gameObject.SetActive(true);
        MeetingScreen.GetComponent<Meeting>().ResetMeeting();

        foreach (Player player in Player.list.Values)
        {
            if (player.Id == message.GetUShort())
                player.transform.position = message.GetVector3();
        }

        Message messageToSend = Message.Create(MessageSendMode.reliable, ClientToServerId.playerInMeeting);
        NetworkManager.Singleton.Client.Send(messageToSend);
    }
    #endregion
}