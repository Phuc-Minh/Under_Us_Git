using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Task : MonoBehaviour
{
    private static GameObject connectUI;
    private static GameObject TaskUI;
    public static ushort idTask;

    //Parameter 
    static Sprite[] spriteArray;

    private void Start()
    {
        connectUI = GameObject.Find("GameplayScreen");
        TaskUI = GameObject.Find("TaskScreen");
        spriteArray = Resources.LoadAll<Sprite>("Electrical");
    }

    private void Update()
    {
        // Close all task 
        if (idTask == 0)
        {
            for (int i = 0; i < 1; i++)
            {
                TaskUI.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && idTask != 0)
        {
            TaskUI.transform.GetChild(idTask-1).gameObject.SetActive(!TaskUI.transform.GetChild(idTask - 1).gameObject.activeSelf);
            CameraController.ToggleCursorMode();
        }
    }

    #region MESSAGES
    [MessageHandler((ushort)ServerToClientId.interact)]
    private static void Interact(Message message)
    {
        bool UIState = message.GetBool();
        connectUI.transform.GetChild(0).gameObject.SetActive(UIState);

        if (UIState)
            idTask = message.GetUShort();
        else
            Task.idTask = 0;
    }
    #endregion

    #region ELECTRICAL
    public void ToggleElectricButton()
    {
        Transform buttonSection = EventSystem.current.currentSelectedGameObject.transform.parent;
        
        Message message = Message.Create(MessageSendMode.unreliable, ClientToServerId.electricButton);
        
        message.AddUShort(ushort.Parse(buttonSection.name.Substring(21)));

        if (buttonSection.GetChild(1).gameObject.GetComponent<Image>().sprite.name.Substring(0, 1) == "R")
            message.AddBool(true);
        else
            message.AddBool(false);

        NetworkManager.Singleton.Client.Send(message);
    }

    [MessageHandler((ushort)ServerToClientId.electricButton)]
    private static void DisplayElectricButton(Message message)
    {
        spriteArray = Resources.LoadAll<Sprite>("Electrical");
        TaskUI = GameObject.Find("TaskScreen");

        bool[] tableElectric = message.GetBools();

        for (int i = 0; i < tableElectric.Length; i++)
        {
            if (tableElectric[i])
            {
                TaskUI.transform.GetChild(0).GetChild(i+1).GetChild(0).gameObject.GetComponent<Image>().sprite = spriteArray[4];
                TaskUI.transform.GetChild(0).GetChild(i + 1).GetChild(1).gameObject.GetComponent<Image>().sprite = spriteArray[1];
            }
            else
            {
                TaskUI.transform.GetChild(0).GetChild(i + 1).GetChild(0).gameObject.GetComponent<Image>().sprite = spriteArray[3];
                TaskUI.transform.GetChild(0).GetChild(i + 1).GetChild(1).gameObject.GetComponent<Image>().sprite = spriteArray[2];
            }
        }

        foreach (bool electricButton in tableElectric)
        {
            
        }
    }
    #endregion
}
