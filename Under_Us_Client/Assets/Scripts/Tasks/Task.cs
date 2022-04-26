using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Task : MonoBehaviour
{
    public enum TaskId : ushort
    {
        Electrical = 1,
        ElectricalMeeting,
        ElectricalO2,
        ElectricalSpeciment,
        ElectricalLabo1,
        ElectricalLabo2,
        ElectricalLabo3,
        LavaMeter,
    }

    private static Transform progressBarScreen;
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
        progressBarScreen = connectUI.transform.GetChild(6);
    }

    private void Update()
    {
        // Close all task 
        if (idTask == 0)
        {
            for (int i = 0; i < 2; i++)
            {
                TaskUI.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && idTask != 0)
        {
            // Task 0 to 7 is the same screen (Electrical)
            if(idTask < 8)
                TaskUI.transform.GetChild(0).gameObject.SetActive(!TaskUI.transform.GetChild(0).gameObject.activeSelf);
            else
            {
                TaskUI.transform.GetChild(idTask - 7).gameObject.SetActive(!TaskUI.transform.GetChild(idTask - 7).gameObject.activeSelf);
            }

            CameraController.ToggleCursorMode();
        }
    }

    #region MESSAGES
    // Toggle Task Screen and Interact button
    [MessageHandler((ushort)ServerToClientId.interact)]
    private static void Interact(Message message)
    {
        bool UIState = message.GetBool();
        connectUI.transform.GetChild(0).gameObject.SetActive(UIState);

        if (UIState)
            idTask = message.GetUShort();
        else
        {
            Task.idTask = 0;
            
            // Disappear cursor and lock in screen
            Cursor.visible = false;
            if (Cursor.lockState == CursorLockMode.None)
                Cursor.lockState = CursorLockMode.Locked;
        }
    }
    #endregion

    #region ELECTRICAL
    public void ToggleElectricButton()
    {
        Transform buttonSection = EventSystem.current.currentSelectedGameObject.transform.parent;
        
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.electricButton);

        //string taskName = EventSystem.current.currentSelectedGameObject.transform.parent.parent.name;
        //message.AddUShort(ushort.Parse(taskName.Substring(taskName.IndexOf('_')+1)));
        message.AddUShort(idTask);

        message.AddUShort(ushort.Parse(buttonSection.name.Substring(buttonSection.name.IndexOf('_') + 1)));

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
                TaskUI.transform.GetChild(0).GetChild(i + 1).GetChild(0).gameObject.GetComponent<Image>().sprite = spriteArray[4];
                TaskUI.transform.GetChild(0).GetChild(i + 1).GetChild(1).gameObject.GetComponent<Image>().sprite = spriteArray[1];
            }
            else
            {
                TaskUI.transform.GetChild(0).GetChild(i + 1).GetChild(0).gameObject.GetComponent<Image>().sprite = spriteArray[3];
                TaskUI.transform.GetChild(0).GetChild(i + 1).GetChild(1).gameObject.GetComponent<Image>().sprite = spriteArray[2];
            }
        }
    }
    #endregion

    #region LAVAMETER
    public void RaiseLavaMeter()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.lavaButton);
        message.AddBool(true);

        NetworkManager.Singleton.Client.Send(message);
    }

    public void ReduceLavaMeter()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.lavaButton);
        message.AddBool(false);

        NetworkManager.Singleton.Client.Send(message);
    }

    [MessageHandler((ushort)ServerToClientId.lavaButton)]
    private static void DisplayLavaButton(Message message)
    {
        spriteArray = Resources.LoadAll<Sprite>("Lava");
        TaskUI = GameObject.Find("TaskScreen");

        int lavaTemp = message.GetInt();
        int lavaMeter = message.GetInt();

        TaskUI.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = lavaMeter.ToString();
        TaskUI.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = lavaTemp.ToString();

        if(lavaTemp == lavaMeter)
        {
            TaskUI.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().sprite = spriteArray[4];
            TaskUI.transform.GetChild(1).GetChild(0).GetChild(1).gameObject.GetComponent<Image>().sprite = spriteArray[0];
            TaskUI.transform.GetChild(1).GetChild(0).GetChild(2).gameObject.GetComponent<Image>().sprite = spriteArray[2];
        }
        else
        {
            TaskUI.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().sprite = spriteArray[5];
            TaskUI.transform.GetChild(1).GetChild(0).GetChild(1).gameObject.GetComponent<Image>().sprite = spriteArray[1];
            TaskUI.transform.GetChild(1).GetChild(0).GetChild(2).gameObject.GetComponent<Image>().sprite = spriteArray[3];
        }
    }
    #endregion

    // Toggle Task Light in game and minimap
    // If task is finished Set GameObject to false 
    [MessageHandler((ushort)ServerToClientId.taskList)]
    private static void TaskLight(Message message)
    {
        int taskCount = message.GetInt();

        // Edit progress bar 
        GameObject ProgressBar = progressBarScreen.GetChild(2).gameObject;
        ProgressBar.GetComponent<Image>().fillAmount = message.GetFloat();

        // Edit task icon on minimap
        int j = 0;
        for (int i = 0; i < taskCount; i++)
        {
            string taskName = message.GetString();
            
            GameObject task = GameObject.Find(taskName);
            bool statusTask = message.GetBool();
            if (task != null)
            {
                if (statusTask)
                {
                    // If task is finished
                    task.transform.GetChild(0).gameObject.SetActive(false);
                    progressBarScreen.GetChild(3).GetChild(j).gameObject.SetActive(true);
                    progressBarScreen.GetChild(3).GetChild(j).gameObject.GetComponent<Text>().text = taskName;
                    progressBarScreen.GetChild(3).GetChild(j).gameObject.GetComponent<Text>().color = Color.green;
                }
                else
                {
                    // If task is unfinished
                    task.transform.GetChild(0).gameObject.SetActive(true);
                    progressBarScreen.GetChild(3).GetChild(j).gameObject.SetActive(true);
                    progressBarScreen.GetChild(3).GetChild(j).gameObject.GetComponent<Text>().text = taskName;
                    progressBarScreen.GetChild(3).GetChild(j).gameObject.GetComponent<Text>().color = Color.yellow;
                }
                j++;
            }
        }
    }

    [MessageHandler((ushort)ServerToClientId.sabotage)]
    private static void Sabotage(Message message)
    {
        // Check if message is to start or end sabotage
        if (message.GetBool())
        {
            // Check what kind of sabotage
            // 0 Error
            // 1 Light
            // 2 Door
            switch (message.GetUShort())
            {
                case 1:
                    foreach (Player player in Player.list.Values)
                    {
                        if (player.IsLocal)
                        {
                            player.camTransform.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
                            player.camTransform.GetComponent<Camera>().farClipPlane = 10f;
                            break;
                        }
                    }
                    break;
                case 2:
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (message.GetUShort())
            {
                case 1:
                    foreach (Player player in Player.list.Values)
                    {
                        if (player.IsLocal)
                        {
                            player.camTransform.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
                            player.camTransform.GetComponent<Camera>().farClipPlane = 500f;
                            break;
                        }
                    }
                    break;
                case 2:
                    break;
                default:
                    break;
            }
        }
    }
}
