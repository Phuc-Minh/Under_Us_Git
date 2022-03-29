using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameplayManager : MonoBehaviour
{
    //Parametre général 
    private static GameObject connectUI;

    private void Start()
    {
        connectUI = GameObject.Find("GameplayScreen");
    }

    #region Message
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
        string playerName = "";

        MeetingScreen.gameObject.SetActive(false);
        deathAnimation.transform.GetChild(1).gameObject.SetActive(true);

        if (maxPlayer != 9999 && maxPlayer != 0)
        {
            bool isImpostor = message.GetBool();

            foreach (Player player in Player.list.Values)
            {
                if (player.Id == maxPlayer)
                {
                    playerName = player.GetName();
                    ChangePlayerColor(player.oldColor, deathAnimation.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Renderer>().materials);
                    break;
                }
            }

            if(isImpostor)
                connectUI.transform.GetChild(2).GetComponent<Text>().text = playerName + " was an impostor";
            else
                connectUI.transform.GetChild(2).GetComponent<Text>().text = playerName + " was not an impostor";

            deathAnimation.transform.GetChild(0).gameObject.SetActive(true);

            deathAnimation.transform.GetChild(0).gameObject.GetComponent<Animation>().Play("ThrowPlayer");
        }
        else
        {
            connectUI.transform.GetChild(2).GetComponent<Text>().text = "No one is ejected";
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
        connectUI.transform.GetChild(2).GetComponent<Text>().text = "Meeting ended";
        connectUI.transform.GetChild(2).gameObject.SetActive(true);
        connectUI.transform.GetChild(2).GetComponent<Animation>().Play("AppearRightNow");
        connectUI.transform.GetChild(2).GetComponent<Animation>().wrapMode = WrapMode.Once;
    }
    #endregion
}
