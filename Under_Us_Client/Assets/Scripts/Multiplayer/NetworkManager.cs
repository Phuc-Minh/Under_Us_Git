using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum ServerToClientId : ushort
{
    playerSpawned = 1,
    taskZone = 2,
    startGame = 3,
    playerChangeColor,
    playerInteracKillZone,
    playerRole,
    playerDead,
    playerMovement,
    playerInteract,
    playerTeleport,
    meetingEnd,
}

public enum ClientToServerId : ushort
{
    name = 1,
    playerChangeColor,
    playerSendId,
    impostorSendKillNotif,
    interative,
    playerInMeeting,
    input,
}

public class NetworkManager : MonoBehaviour
{
    public static Sprite[] SpriteArray;

    private static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public Client Client { get; private set; }

    [SerializeField] private string ip;
    [SerializeField] private ushort port;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Client = new Client();
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;
    }

    private void FixedUpdate()
    {
        Client.Tick();
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
    }

    public void Connect()
    {
        Client.Connect($"{ip}:{port}");
    }

    private void DidConnect(object sender, EventArgs e)
    {
        UIManager.Singleton.SendName();
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        UIManager.Singleton.BackToMain();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        if (Player.list.TryGetValue(e.Id, out Player player))
            Destroy(player.gameObject);
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
        UIManager.Singleton.BackToMain();
        foreach (Player player in Player.list.Values)
            Destroy(player.gameObject);
    }

    [MessageHandler((ushort)ServerToClientId.startGame)]
    private static void StartGame(Message message)
    {
        // Send player id
        Message messageToSend = Message.Create(MessageSendMode.unreliable, ClientToServerId.playerSendId);
        messageToSend.AddUShort(Singleton.Client.Id);
        NetworkManager.Singleton.Client.Send(messageToSend);

        // Open doors
        GameObject doors = GameObject.Find("Doors");
        if (doors != null)
            doors.GetComponent<Animation>().Play();

        // Set up meeting buttons
        Transform MeetingScreen = GameObject.Find("GameplayScreen").transform.GetChild(4);
        if (SpriteArray == null)
            SpriteArray = Resources.LoadAll<Sprite>("MeetingCells");

        int countPlayer = 0;
        foreach (Player player in Player.list.Values)
        {
            MeetingScreen.GetChild(countPlayer).name = "PlayerSection" + player.Id;
            MeetingScreen.GetChild(countPlayer).transform.GetChild(0).GetComponent<Image>().sprite = SpriteArray[player.GetComponent<Player>().oldColor];
            MeetingScreen.GetChild(countPlayer).gameObject.SetActive(true);

            countPlayer++;
        }
    }
}