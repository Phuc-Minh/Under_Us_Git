using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;

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
    meetingChoice,
    meetingResult,
    ejectResult,
    meetingEnd
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
}
