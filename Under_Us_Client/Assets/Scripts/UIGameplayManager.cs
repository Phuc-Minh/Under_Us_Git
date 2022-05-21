using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameplayManager : MonoBehaviour
{
    //Parametre général 
    private static GameObject connectUI;
    public static GameObject announcementScreen;
    public GameObject textObject;
    public static string[] messageList = new string[10];
    private void Start()
    {
        connectUI = GameObject.Find("GameplayScreen");
        announcementScreen = connectUI.transform.GetChild(7).gameObject;
    }

    public static void CleanScreen()
    {
        for (int i = 0; i < connectUI.transform.childCount; i++)
        {
            connectUI.transform.GetChild(i).gameObject.SetActive(false);
        }
    } 

    public static void AddMessageToAnnouncement(string message, bool forceToOpen)
    {
        message = $"<{message}>";
        if(messageList[9] == null)
        {
            // Add message to list
            for (int i = 0; i < messageList.Length; i++)
            {
                if (messageList[i] == null)
                {
                    messageList[i] = message;
                    break;
                }
            }
        }
        else
        {
            // Purge first message in list to add new message
            for (int i = 1; i < messageList.Length; i++)
                messageList[i - 1] = messageList[i];

            messageList[9] = message;
        }

        // Update Announcement Screen
        string messageToChange = "";
        for (int i = 0; i < messageList.Length; i++)
        {
            if(messageList[i] != "")
                messageToChange += $"{messageList[i]}\n";
        }

        announcementScreen.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Text>().text = messageToChange.Substring(0, messageToChange.Length - 1);

        if(forceToOpen)
            announcementScreen.SetActive(true);
    }

    #region Message
    [MessageHandler((ushort)ServerToClientId.message)]
    private static void Message(Message message)
    {
        AddMessageToAnnouncement(message.GetString(),message.GetBool());
    }

    [MessageHandler((ushort)ServerToClientId.meetingChoice)]
    private static void playerVoted(Message message)
    {
        ushort idPlayer = message.GetUShort();
        
        Transform MeetingScreen = connectUI.transform.GetChild(4);
        for (int i = 0; i <= 7; i++)
        {
            if (MeetingScreen.GetChild(i).name.Substring(13) == idPlayer.ToString())
                MeetingScreen.GetChild(i).GetChild(1).gameObject.SetActive(true);
        }
    }

    [MessageHandler((ushort)ServerToClientId.meetingResult)]
    private static void meetingResult(Message message)
    {
        Transform MeetingScreen = connectUI.transform.GetChild(4);
        connectUI.transform.GetChild(0).gameObject.SetActive(false);
        MeetingScreen.gameObject.SetActive(true);

        int nbPlayer = message.GetInt();
        ushort idPlayer;
        int voteCount;

        for (int i = 0; i < nbPlayer; i++)
        {
            idPlayer = message.GetUShort();
            voteCount = message.GetInt();

            for (int j = 0; j <= 7; j++)
            {
                MeetingScreen.GetChild(j).GetChild(1).gameObject.SetActive(false);
                if (MeetingScreen.GetChild(j).name.Substring(13) == idPlayer.ToString())
                {
                    MeetingScreen.GetChild(j).GetChild(0).GetChild(0).GetComponent<Text>().text = voteCount.ToString();
                    MeetingScreen.GetChild(j).GetChild(0).GetChild(0).gameObject.SetActive(true);
                    break;
                }
            }
        }
    }

    [MessageHandler((ushort)ServerToClientId.ejectResult)]
    private static void ejectResult(Message message)
    {
        Transform MeetingScreen = connectUI.transform.GetChild(4);
        GameObject deathAnimation = GameObject.Find("DeathAnimation");

        ushort maxPlayer = message.GetUShort();

        MeetingScreen.gameObject.SetActive(false);
        deathAnimation.transform.GetChild(1).gameObject.SetActive(true);

        if (maxPlayer != 9999 && maxPlayer != 0)
        {
            bool isImpostor = message.GetBool();

            //ChangePlayerColor(Player.list[maxPlayer].oldColor, deathAnimation.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Renderer>().materials);
            // OldColor + 1 because change color use player input which is 1 higher then indice inside color table
            Player.ChangePlayerColor(Player.list[maxPlayer].oldColor+1, deathAnimation.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Renderer>().materials);

            Debug.Log(Player.list[maxPlayer].GetName());
            Debug.Log("Id : " + maxPlayer);
            Debug.Log("Color : " + Player.list[maxPlayer].oldColor);

            if (isImpostor)
            {
                connectUI.transform.GetChild(2).GetComponent<Text>().text = Player.list[maxPlayer].GetName() + " was an impostor";
                AddMessageToAnnouncement(Player.list[maxPlayer].GetName() + " was an impostor", false);
            }
            else
            {
                connectUI.transform.GetChild(2).GetComponent<Text>().text = Player.list[maxPlayer].GetName() + " was not an impostor";
                AddMessageToAnnouncement(Player.list[maxPlayer].GetName() + " was not an impostor", false);
            }

            deathAnimation.transform.GetChild(0).gameObject.SetActive(true);

            deathAnimation.transform.GetChild(0).gameObject.GetComponent<Animation>().Play("ThrowPlayer");
        }
        else
        {
            connectUI.transform.GetChild(2).GetComponent<Text>().text = "No one is ejected";
            AddMessageToAnnouncement("No one is ejected", false);
        }

        connectUI.transform.GetChild(2).gameObject.SetActive(true);
        connectUI.transform.GetChild(2).GetComponent<Animation>().Play("Wait4SecThenAppear");
    }

    public static void ChangePlayerColor(int colorCode, Material[] playerMaterials)
    {
        Texture[] textureArray = Resources.LoadAll<Texture>("AstronautColor");
        Texture[] textureBackpackArray = Resources.LoadAll<Texture>("AstronautBackpackColor");

        playerMaterials[1].SetTexture("_MainTex", textureArray[colorCode]);
        playerMaterials[2].SetTexture("_MainTex", textureBackpackArray[colorCode]);
    }

    [MessageHandler((ushort)ServerToClientId.meetingEnd)]
    private static void meetingEnd(Message message)
    {
        //Player not in meeting
        PlayerController.inMeeting = false;

        //Reset camera and death animation
        GameObject deathAnimation = GameObject.Find("DeathAnimation");
        deathAnimation.transform.GetChild(0).gameObject.SetActive(false);
        deathAnimation.transform.GetChild(1).gameObject.SetActive(false);
        connectUI.transform.GetChild(2).gameObject.SetActive(false);

        //Hide meeting screen
        connectUI.transform.GetChild(4).gameObject.SetActive(false);
        if(Cursor.visible)
            CameraController.ToggleCursorMode();

        //Hide meeting timer
        connectUI.transform.GetChild(5).gameObject.SetActive(false);

        //Display meeting end notification
        UIGameplayManager.AddMessageToAnnouncement("Meeting Ended", true);
    }
    #endregion
}
