using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    [SerializeField] LayerMask ghostMask;

    //Attach network manager to gameObject and access that in code
    private static GameLogic _singleton;

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

    public GameObject LocalPlayerPrefab => localPlayerPrefab;
    public GameObject PlayerPrefab => playerPrefab;

    [Header("Prefabs")]
    [SerializeField] private GameObject localPlayerPrefab;
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        Singleton = this;
    }

    #region Message
    [MessageHandler((ushort)ServerToClientId.playerDead)]
    private static void DeadNotification(Message message)
    {
        ushort deadPlayerId = message.GetUShort();
        foreach (Player player in Player.list.Values)
        { 
            if (player.IsLocal && player.Id == deadPlayerId)
            {
                GameObject connectUI = GameObject.Find("GameplayScreen");

                //Modify Role attribute in Player
                Player.list[player.Id].GetComponent<Player>().Role = 2;
                //player.gameObject.GetComponentInChildren<Camera>().cullingMask |= 9;
                player.transform.GetChild(1).GetComponent<Camera>().cullingMask |= 1 << 9;

                //Display dead notification
                connectUI.transform.GetChild(2).GetComponent<Text>().text = "You are dead!";
                connectUI.transform.GetChild(2).gameObject.SetActive(true);

                connectUI.transform.GetChild(2).GetComponent<Animation>().Play();
                connectUI.transform.GetChild(2).GetComponent<Animation>().wrapMode = WrapMode.Loop;
            }
            else if(player.Id == deadPlayerId)
            {
                player.transform.GetChild(2).gameObject.SetActive(true);

                //Change his role
                Player.list[player.Id].Role = 3;

                //Add Ghost layer to all of it's children
                SetLayerRecursively(Player.list[player.Id].gameObject, 9);
            }
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
    #endregion
}
