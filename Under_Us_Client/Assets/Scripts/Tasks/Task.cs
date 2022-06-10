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
        StepOrder,
    }

    private static Transform progressBarScreen;
    private static GameObject connectUI;
    private static GameObject TaskUI;
    public static ushort idTask;

    //Parameter 
    static Sprite[] spriteElectrical;
    static Sprite[] spriteLava;
    static Sprite[] spriteStep;


    private void Start()
    {
        connectUI = GameObject.Find("GameplayScreen");
        TaskUI = GameObject.Find("TaskScreen");
        spriteElectrical = Resources.LoadAll<Sprite>("Electrical");
        spriteLava = Resources.LoadAll<Sprite>("Lava");
        spriteStep = Resources.LoadAll<Sprite>("Step");
        progressBarScreen = connectUI.transform.GetChild(6);
    }

    private void Update()
    {
        // Close all task 
        if (idTask == 0)
        {
            for (int i = 0; i < TaskUI.transform.childCount; i++)
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
        TaskUI = GameObject.Find("TaskScreen");

        bool[] tableElectric = message.GetBools();

        for (int i = 0; i < tableElectric.Length; i++)
        {
            if (tableElectric[i])
            {
                TaskUI.transform.GetChild(0).GetChild(i + 1).GetChild(0).gameObject.GetComponent<Image>().sprite = spriteElectrical[4];
                TaskUI.transform.GetChild(0).GetChild(i + 1).GetChild(1).gameObject.GetComponent<Image>().sprite = spriteElectrical[1];
            }
            else
            {
                TaskUI.transform.GetChild(0).GetChild(i + 1).GetChild(0).gameObject.GetComponent<Image>().sprite = spriteElectrical[3];
                TaskUI.transform.GetChild(0).GetChild(i + 1).GetChild(1).gameObject.GetComponent<Image>().sprite = spriteElectrical[2];
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
        TaskUI = GameObject.Find("TaskScreen");

        int lavaTemp = message.GetInt();
        int lavaMeter = message.GetInt();

        TaskUI.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = lavaMeter.ToString();
        TaskUI.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = lavaTemp.ToString();

        if(lavaTemp == lavaMeter)
        {
            TaskUI.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().sprite = spriteLava[4];
            TaskUI.transform.GetChild(1).GetChild(0).GetChild(1).gameObject.GetComponent<Image>().sprite = spriteLava[0];
            TaskUI.transform.GetChild(1).GetChild(0).GetChild(2).gameObject.GetComponent<Image>().sprite = spriteLava[2];
        }
        else
        {
            TaskUI.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().sprite = spriteLava[5];
            TaskUI.transform.GetChild(1).GetChild(0).GetChild(1).gameObject.GetComponent<Image>().sprite = spriteLava[1];
            TaskUI.transform.GetChild(1).GetChild(0).GetChild(2).gameObject.GetComponent<Image>().sprite = spriteLava[3];
        }
    }
    #endregion

    #region STEP
    [MessageHandler((ushort)ServerToClientId.stepButton)]
    private static void DisplayStepTask(Message message)
    {
        TaskUI = GameObject.Find("TaskScreen");
        TaskUI.transform.GetChild(2).GetChild(8).gameObject.GetComponent<Text>().text = $"Turn {message.GetInt()}";
        int step = message.GetInt();

        //Reset all button color
        for (int i = 1; i < 7; i++)
        {
            if (TaskUI.transform.GetChild(2).GetChild(i).GetChild(0).GetComponent<Image>().sprite.name != "Square")
                TaskUI.transform.GetChild(2).GetChild(i).GetChild(0).GetComponent<Image>().sprite = spriteStep[0];
        }
        TaskUI.transform.GetChild(2).GetChild(step).GetChild(0).GetComponent<Image>().sprite = spriteStep[1];
    }

    [MessageHandler((ushort)ServerToClientId.stepButtonInformation)]
    private static void DisplayStepTaskInformation(Message message)
    {
        TaskUI = GameObject.Find("TaskScreen");
        
        TaskUI.transform.GetChild(2).GetChild(8).gameObject.GetComponent<Text>().text = $"Turn {message.GetInt()}";
        string instruction = message.GetString();
        TaskUI.transform.GetChild(2).GetChild(9).gameObject.GetComponent<Text>().text = instruction.Substring(1);

        //Reset all button color
        for (int i = 1; i < 7; i++)
        {
            if (instruction.Substring(0,1) == "1")
                TaskUI.transform.GetChild(2).GetChild(i).GetChild(0).GetComponent<Image>().sprite = spriteStep[2];
            else if(instruction.Substring(0, 1) == "2")
                TaskUI.transform.GetChild(2).GetChild(i).GetChild(0).GetComponent<Image>().sprite = spriteStep[3];
            else
                TaskUI.transform.GetChild(2).GetChild(i).GetChild(0).GetComponent<Image>().sprite = spriteStep[0];
        }
    }

    public void SendStepTask()
    {
        Transform buttonSection = EventSystem.current.currentSelectedGameObject.transform.parent;

        //Reset all button color
        for (int i = 1; i < 7; i++)
        {
            if (TaskUI.transform.GetChild(2).GetChild(i).GetChild(0).GetComponent<Image>().sprite.name != "Square")
                TaskUI.transform.GetChild(2).GetChild(i).GetChild(0).GetComponent<Image>().sprite = spriteStep[0];
        }
        buttonSection.GetChild(0).GetComponent<Image>().sprite = spriteStep[1];

        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.stepButton);
        message.AddUShort(idTask);
        message.AddBool(false);

        message.AddUShort(ushort.Parse(buttonSection.name.Substring(buttonSection.name.IndexOf('_') + 1)));

        NetworkManager.Singleton.Client.Send(message);
    }

    public void StartResetStepTask()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.stepButton);
        message.AddUShort(idTask);
        message.AddBool(true);

        NetworkManager.Singleton.Client.Send(message);
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
                    task.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                    progressBarScreen.GetChild(3).GetChild(j).gameObject.SetActive(true);
                    progressBarScreen.GetChild(3).GetChild(j).gameObject.GetComponent<Text>().text = taskName;
                    progressBarScreen.GetChild(3).GetChild(j).gameObject.GetComponent<Text>().color = Color.green;
                }
                else
                {
                    // If task is unfinished
                    task.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
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
        // Check if message is to start or end sabotage / True = Start
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
                            player.camTransform.GetComponent<Animation>().Play("CameraLightOff");
                            UIGameplayManager.AddMessageToAnnouncement("Light is out! Open map and go fix it.", true);
                            break;
                        }
                    }

                    GameObject task = GameObject.Find(message.GetString());
                    if (task != null)
                    {
                        task.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                        task.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
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
                            player.camTransform.GetComponent<Animation>().Play("CameraLightOn");
                            UIGameplayManager.AddMessageToAnnouncement("Light is back on!", false);
                            break;
                        }
                    }

                    GameObject task = GameObject.Find(message.GetString());
                    if (task != null)
                    {
                        task.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                        task.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
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
