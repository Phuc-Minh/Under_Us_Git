using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameplayManager : MonoBehaviour
{
    //Parametre général 
    [SerializeField] private static GameObject connectUI;
    [SerializeField] GameObject deathPlayerAnimation;

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

        int nbPlayer = message.GetInt();
        ushort maxPlayer = message.GetUShort();
        int maxCount = message.GetInt();
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

        MeetingScreen.gameObject.SetActive(true);

        if (maxPlayer != 9999 && maxPlayer != 0)
        {
            GameObject deathAnimation = GameObject.Find("DeathAnimation");
            deathAnimation.transform.GetChild(0).gameObject.SetActive(true);
            deathAnimation.transform.GetChild(1).gameObject.SetActive(true);
            deathAnimation.transform.GetChild(0).GetComponent<Animation>().Play();
        }

    }

    [MessageHandler((ushort)ServerToClientId.meetingEnd)]
    private static void meetingEnd(Message message)
    {
        //Player not in meeting
        PlayerController.inMeeting = false;

        //Hide meeting screen
        connectUI.transform.GetChild(4).gameObject.SetActive(false);
        if(Cursor.visible)
            CameraController.ToggleCursorMode();

        //Hide meeting timer
        connectUI.transform.GetChild(5).gameObject.SetActive(false);

        //Display meeting end notification
        connectUI.transform.GetChild(2).GetComponent<Text>().text = "Meeting ended";
        connectUI.transform.GetChild(2).gameObject.SetActive(true);

        connectUI.transform.GetChild(2).GetComponent<Animation>().Play();
    }
    #endregion
}
