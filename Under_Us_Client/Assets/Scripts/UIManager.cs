using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //Attach network manager to gameObject and access that in code
    private static UIManager _singleton;

    public static UIManager Singleton
    {
        get => _singleton;
        private set
        {
            //Ensure that there is only one instance of network manager
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(UIManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    [Header("Connect")]
    [SerializeField] private GameObject connectUI;
    [SerializeField] private InputField usernameField;

    private void Awake()
    {
        Singleton = this;
    }

    public void ConnectClicked()
    {
        usernameField.interactable = false;
        connectUI.SetActive(false);

        NetworkManager.Singleton.Connect();
    }

    public void BackToMain()
    {
        usernameField.interactable = true;
        connectUI.SetActive(true);
    }

    public void SendName()
    {
        //Reliable mode ensure to send message to other end (Initialize name, etc)
        //Unreliable send message without guarantee (moving, etc)
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.name);
        message.AddString(usernameField.text);
        NetworkManager.Singleton.Client.Send(message);
    }
}
