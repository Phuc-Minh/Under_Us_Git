using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum ServerToClientId : ushort
{
    sync = 1,
    playerSpawned,
    taskZone,
    startGame,
    playerChangeColor,
    playerInteracKillZone,
    playerRole,
    playerDead,
    playerMovement,
    togglePlayerInteract,
    interact,
    playerTeleport,
    meetingChoice,
    meetingResult,
    ejectResult,
    meetingEnd,
    electricButton,
    taskList,
    securityCamera,
    lavaButton,
    progressBar,
    sabotage,
}

public enum ClientToServerId : ushort
{
    name = 1,
    playerChangeColor,
    impostorSendKillNotif,
    interative,
    playerInMeeting,
    meetingChoice,
    electricButton,
    lavaButton,
    sabotageTask,
    input,
}

public class NetworkManager : MonoBehaviour
{
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
    private ushort _serverTick;
    public ushort ServerTick
    {
        get => _serverTick;
        private set
        {
            _serverTick = value;
            InterpolationTick = (ushort)(value - TicksBetweenPositionUpdates);
        }
    }
    public ushort InterpolationTick { get; private set; }
    private ushort _ticksBetweenPositionUpdates = 2;
    public ushort TicksBetweenPositionUpdates
    {
        get => _ticksBetweenPositionUpdates;
        private set
        {
            _ticksBetweenPositionUpdates = value;
            InterpolationTick = (ushort)(ServerTick - value);
        }
    }

    //[SerializeField] private string ip;
    //[SerializeField] private ushort port;
    [Space(10)]
    [SerializeField] private ushort tickDivergenceTolerance = 1;

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

        ServerTick = 2;
    }

    private void FixedUpdate()
    {
        Client.Tick();
        ServerTick++;
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
    }

    public void Connect(string ip, ushort port)
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
        /*// Send player id
        Message messageToSend = Message.Create(MessageSendMode.unreliable, ClientToServerId.playerSendId);
        messageToSend.AddUShort(Singleton.Client.Id);
        NetworkManager.Singleton.Client.Send(messageToSend);*/

        // Open doors
        GameObject doors = GameObject.Find("Doors");
        if (doors != null)
            doors.GetComponent<Animation>().Play();
    }

    private void SetTick(ushort serverTick)
    {
        if (Mathf.Abs(ServerTick - serverTick) > tickDivergenceTolerance)
        {
            Debug.Log($"Client tick: {ServerTick} -> {serverTick}");
            ServerTick = serverTick;
        }
    }

    [MessageHandler((ushort)ServerToClientId.sync)]
    public static void Sync(Message message)
    {
        Singleton.SetTick(message.GetUShort());
    }
}