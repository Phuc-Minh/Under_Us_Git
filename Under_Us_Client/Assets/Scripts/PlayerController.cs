using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Parametre général 
    [SerializeField] private static GameObject connectUI;

    [SerializeField] private Transform camTransform;

    private bool[] inputs;
    public static bool inMeeting;
    Player player;

    private void Start()
    {
        player = this.GetComponent<Player>();
        inputs = new bool[6];
        connectUI = GameObject.Find("GameplayScreen");
    }

    private void Update()
    {
        //Player movement
        if (Input.GetKey(KeyCode.W))
            inputs[0] = true;

        if (Input.GetKey(KeyCode.S))
            inputs[1] = true;

        if (Input.GetKey(KeyCode.A))
            inputs[2] = true;

        if (Input.GetKey(KeyCode.D))
            inputs[3] = true;

        if (Input.GetKey(KeyCode.Space))
            inputs[4] = true;

        if (Input.GetKey(KeyCode.LeftShift))
            inputs[5] = true;

        //Change player color
        if (connectUI.transform.GetChild(1).gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0) && GameplayUIColorButtonIsActive(9))
            {
                player.SendPlayerChangeColor(9, player.oldColor);
            }
            if (Input.GetKeyDown(KeyCode.Alpha1) && GameplayUIColorButtonIsActive(0))
            {
                player.SendPlayerChangeColor(0, player.oldColor);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) && GameplayUIColorButtonIsActive(1))
            { 
                player.SendPlayerChangeColor(1, player.oldColor);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3) && GameplayUIColorButtonIsActive(2))
            {
                player.SendPlayerChangeColor(2, player.oldColor);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4) && GameplayUIColorButtonIsActive(3))
            {
                player.SendPlayerChangeColor(3, player.oldColor);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5) && GameplayUIColorButtonIsActive(4))
            {
                player.SendPlayerChangeColor(4, player.oldColor);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6) && GameplayUIColorButtonIsActive(5))
            {
                player.SendPlayerChangeColor(5, player.oldColor);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7) && GameplayUIColorButtonIsActive(6))
            {
                player.SendPlayerChangeColor(6, player.oldColor);
            }
            if (Input.GetKeyDown(KeyCode.Alpha8) && GameplayUIColorButtonIsActive(7))
            {
                player.SendPlayerChangeColor(7, player.oldColor);
            }
            if (Input.GetKeyDown(KeyCode.Alpha9) && GameplayUIColorButtonIsActive(8))
            {
                player.SendPlayerChangeColor(8, player.oldColor);
            }
        }

        //Interac
        if (!PlayerController.inMeeting)
        {
            // If player is not in a meeting
            // Send E keycode to server
            if (Input.GetKeyDown(KeyCode.E) && connectUI.transform.GetChild(0).gameObject.activeSelf)
                SendInteractive(0);

        } else if(Input.GetKeyDown(KeyCode.E)) {
            // If player is in a meeting 
            // Open meeting display
            connectUI.transform.GetChild(4).gameObject.SetActive(!connectUI.transform.GetChild(4).gameObject.activeSelf);
            CameraController.ToggleCursorMode();
        }

        //Kill other player
        if (connectUI.transform.GetChild(3).gameObject.activeSelf && player.Role == 2)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SendKill();
            }
        }
    }

    private void FixedUpdate()
    {
        SendInput();

        for (int i = 0; i < inputs.Length; i++)
            inputs[i] = false;
    }

    private bool GameplayUIColorButtonIsActive(int idButton)
    {
        return connectUI.transform.GetChild(1).GetChild(idButton).gameObject.activeSelf;
    }

    #region Messages
    private void SendInput()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ClientToServerId.input);
        message.AddBools(inputs, false);
        message.AddVector3(camTransform.forward);
        NetworkManager.Singleton.Client.Send(message);
    }
    private void SendInteractive(int idInteractive)
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.interative);
        message.AddInt(idInteractive);
        NetworkManager.Singleton.Client.Send(message);
    }

    private void SendKill()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.impostorSendKillNotif);
        message.AddVector3(this.transform.position);

        NetworkManager.Singleton.Client.Send(message);
    }

    [MessageHandler((ushort)ServerToClientId.taskZone)]
    private static void ColorMenu(Message message)
    {
        bool UIState = message.GetBool();
        connectUI.transform.GetChild(1).gameObject.SetActive(UIState);

        if (UIState)
        {
            bool[] tableColor = message.GetBools();
            for (int i = 0; i < tableColor.Length; i++)
            {
                connectUI.transform.GetChild(1).GetChild(i).gameObject.SetActive(!tableColor[i]);
            }
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerInteracKillZone)]
    private static void ToggleKillButton(Message message)
    {
        bool UIState = message.GetBool();
        connectUI.transform.GetChild(3).gameObject.SetActive(UIState);
    }

    [MessageHandler((ushort)ServerToClientId.playerInteract)]
    private static void ToggleInteractButton(Message message)
    {
        bool UIState = message.GetBool();
        connectUI.transform.GetChild(0).gameObject.SetActive(UIState);
    }
    #endregion
}