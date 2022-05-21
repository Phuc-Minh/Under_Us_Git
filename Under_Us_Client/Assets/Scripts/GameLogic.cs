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
        bool dieInMeeting = message.GetBool();
        ushort deadPlayerId = message.GetUShort();
        if (Player.list.ContainsKey(deadPlayerId))
        {
            if (Player.list[deadPlayerId].IsLocal)
            {
                UIGameplayManager.CleanScreen();
                GameObject connectUI = GameObject.Find("GameplayScreen");

                // Modify Role attribute in Player
                Player.list[deadPlayerId].GetComponent<Player>().Role = 3;
                // player.gameObject.GetComponentInChildren<Camera>().cullingMask |= 9;
                // Player can now see ghost
                Player.list[deadPlayerId].transform.GetChild(1).GetComponent<Camera>().cullingMask |= 1 << 9;

                //Display dead notification
                UIGameplayManager.AddMessageToAnnouncement("You are dead!", false);
                connectUI.transform.GetChild(2).GetComponent<Text>().text = "You are dead!";
                connectUI.transform.GetChild(2).gameObject.SetActive(true);

                connectUI.transform.GetChild(2).GetComponent<Animation>().Play();
            }
            else
            {
                Player.list[deadPlayerId].transform.GetChild(2).gameObject.SetActive(true);

                //Change his role
                Player.list[deadPlayerId].Role = 3;

                //Add Ghost layer to all of it's children
                SetLayerRecursively(Player.list[deadPlayerId].gameObject, 9);
            }

            if (!dieInMeeting)
            {
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
    }

    private static void SetLayerRecursively(GameObject gameObject, int newLayer)
    {
        gameObject.layer = newLayer;

        foreach (Transform child in gameObject.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    [MessageHandler((ushort)ServerToClientId.endGame)]
    private static void EndGame(Message message)
    {
        // Role Distribute Stage
        GameObject roleAnimation = GameObject.Find("RoleAnimation");
        if (roleAnimation != null)
        {
            string endMessage = message.GetString();

            List<ushort> ListWinner = new List<ushort>();
            int winnerCount = message.GetInt();
            for (int i = 0; i < winnerCount; i++)
                ListWinner.Add(message.GetUShort());

            for (int i = 0; i < ListWinner.Count; i++)
            {
                // Edit Stage
                roleAnimation.transform.GetChild(0).GetChild(i+1).gameObject.SetActive(true);

                // Change player color in stage 
                Material[] playerMaterials = roleAnimation.transform.GetChild(0).GetChild(i+1).GetChild(1).GetComponent<Renderer>().materials;
                Player.ChangePlayerTexture(playerMaterials, Player.list[ListWinner[i]].oldColor);
            }

            // Activate everything in Role Animation Object
            roleAnimation.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = endMessage;
            roleAnimation.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = "Close application and reset the server";
            roleAnimation.transform.GetChild(0).gameObject.SetActive(true);
            roleAnimation.transform.GetChild(1).gameObject.SetActive(true);
            roleAnimation.transform.GetChild(2).gameObject.SetActive(true);
            roleAnimation.transform.GetChild(3).gameObject.SetActive(true);
            roleAnimation.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        }
    }
    #endregion
}
