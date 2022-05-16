using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    //Attach network manager to gameObject and access that in code
    private static GameLogic _singleton;
    public static bool gameInProcess;


    public static GameLogic Singleton
    {
        get => _singleton;
        private set
        {
            //Ensure that there is only one instance of network manager
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameLogic)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public GameObject PlayerPrefab => playerPrefab;
    public GameObject DeadPlayerPrefab => deadPlayerPrefab;



    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject deadPlayerPrefab;


    public static bool[] tableColor = new bool[10];
    public static List<ushort> tablePlayerId = new List<ushort>();

    private void Awake()
    {
        Singleton = this;
    }
    /*
    #region Message
    [MessageHandler((ushort)ClientToServerId.playerSendId)]
    private static void AddPlayerToList(ushort fromClientId, Message message)
    {
        ushort playerId = message.GetUShort();

        if (!tablePlayerId.Contains(playerId))
            tablePlayerId.Add(playerId);
    }
    #endregion*/
}
