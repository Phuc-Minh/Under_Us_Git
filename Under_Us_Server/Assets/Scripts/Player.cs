using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public string Username { get; private set; }
    /*============== ROLE =============
        1 = Comrade
        !progressBarScreen.activeSelf = Impostor
        3 = Ghost
        4 = Ghost Impostor
    */
    public ushort Role { get; set; }

    public PlayerMovement Movement => movement;

    [SerializeField] private PlayerMovement movement;

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public static void Spawn(ushort id, string username)
    {
        foreach (Player otherPlayer in list.Values)
            otherPlayer.SendSpawned(id);

        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0f, 2f, 0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {id}";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;

        player.SendSpawned();
        list.Add(id, player);

        if (username == "")
            username = "Someone";
        NetworkManager.AnnounceToClient(id, $"{username} just joined, say hi and pick your color!", true);
    }

    #region Messages
    private void SendSpawned()
    {
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)));
    }

    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)), toClientId);
    }

    private Message AddSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.position);
        return message;
    }

    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.input)]
    private static void Input(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            player.Movement.SetInput(message.GetBools(6), message.GetVector3());
    }

    [MessageHandler((ushort)ClientToServerId.playerChangeColor)]
    private static void ChangeColor(ushort fromClientId, Message message)
    {
        Message messageToSend = Message.Create(MessageSendMode.reliable, ServerToClientId.playerChangeColor);

        messageToSend.AddUShort(message.GetUShort());
        int newColor = message.GetInt();
        int oldColor = message.GetInt();
        messageToSend.AddInt(newColor);

        GameLogic.tableColor[newColor] = true;
        if(oldColor < GameLogic.tableColor.Length - 1)
        {
            GameLogic.tableColor[oldColor] = false;
        }

        NetworkManager.Singleton.Server.SendToAll(messageToSend);
    }
    #endregion
}