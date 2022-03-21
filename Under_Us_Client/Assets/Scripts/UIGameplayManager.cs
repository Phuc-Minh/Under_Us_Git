using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameplayManager : MonoBehaviour
{
    //Parametre général 
    [SerializeField] private static GameObject connectUI;

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

    [MessageHandler((ushort)ServerToClientId.meetingEnd)]
    private static void DeadNotification(Message message)
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
