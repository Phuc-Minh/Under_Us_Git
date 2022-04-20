using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;

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
}

public enum ClientToServerId : ushort
{
    name = 1,
    playerChangeColor,
    playerSendId,
    impostorSendKillNotif,
    interative,
    playerInMeeting,
    meetingChoice,
    electricButton,
    lavaButton,
    input,
}

public class NetworkManager : MonoBehaviour
{
    //Attach network manager to gameObject and access that in code
    private static NetworkManager _singleton;

    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            //Ensure that there is only one instance of network manager
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public Server Server { get; private set; }
    public ushort CurrentTick { get; private set; } = 0;

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClientCount;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        //Limit to 60 FPS
        Application.targetFrameRate = 60;

        //Allow riptide log to be seen on Unity log
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server = new Server();
        Server.Start(port, maxClientCount);
        Server.ClientDisconnected += PlayerLeft;
    }

    private void FixedUpdate()
    {
        Server.Tick();

        if (CurrentTick % 200 == 0)
            SendSync();

        CurrentTick++;
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        if (Player.list.TryGetValue(e.Id, out Player player))
        {
            player.GetComponent<PlayerMovement>().ResetMoveSpeed();
            Destroy(player.gameObject);
        }


    }

    private void SendSync()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.sync);
        message.Add(CurrentTick);

        Server.SendToAll(message);
    }
}
