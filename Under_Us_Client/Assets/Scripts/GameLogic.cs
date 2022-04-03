using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
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
    [SerializeField] private GameObject deadPlayerPrefab;
    [SerializeField] Texture[] textureArray;

    private void Awake()
    {
        Singleton = this;
        textureArray = Resources.LoadAll<Texture>("AstronautBackpackColor");
    }

    #region Message
    [MessageHandler((ushort)ServerToClientId.playerDead)]
    private static void DeadNotification(Message message)
    {
        ushort deadPlayerId = message.GetUShort();
        if (Player.list.ContainsKey(deadPlayerId))
        {
            if (Player.list[deadPlayerId].IsLocal)
            {
                GameObject connectUI = GameObject.Find("GameplayScreen");

                //Modify Role attribute in Player
                Player.list[deadPlayerId].GetComponent<Player>().Role = 2;
                //player.gameObject.GetComponentInChildren<Camera>().cullingMask |= 9;
                Player.list[deadPlayerId].transform.GetChild(1).GetComponent<Camera>().cullingMask |= 1 << 9;

                //Display dead notification
                connectUI.transform.GetChild(2).GetComponent<Text>().text = "You are dead!";
                connectUI.transform.GetChild(2).gameObject.SetActive(true);

                connectUI.transform.GetChild(2).GetComponent<Animation>().Play();
                connectUI.transform.GetChild(2).GetComponent<Animation>().wrapMode = WrapMode.Loop;
            }
            else
            {
                Player.list[deadPlayerId].transform.GetChild(2).gameObject.SetActive(true);

                //Change his role
                Player.list[deadPlayerId].Role = 3;

                //Add Ghost layer to all of it's children
                SetLayerRecursively(Player.list[deadPlayerId].gameObject, 9);
            }

            // Instantiate a dead corpse 
            Singleton.deadPlayerPrefab.name = "Tombstone of player " + Player.list[deadPlayerId].Id;
            Singleton.deadPlayerPrefab.transform.position = new Vector3(Player.list[deadPlayerId].gameObject.transform.position.x,
                                                                        Player.list[deadPlayerId].gameObject.transform.position.y - 0.5f,
                                                                        Player.list[deadPlayerId].gameObject.transform.position.z);
            Singleton.deadPlayerPrefab.transform.rotation = Player.list[deadPlayerId].gameObject.transform.rotation;
            Material[] playerMaterials = Singleton.deadPlayerPrefab.transform.GetComponent<Renderer>().sharedMaterials;
            playerMaterials[0].SetTexture("_MainTex", Singleton.textureArray[Player.list[deadPlayerId].oldColor]);

            Instantiate(Singleton.deadPlayerPrefab);
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
