using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet packet)
    {
        string message = packet.ReadString();
        int myId = packet.ReadInt();
        Debug.Log($"Message from server: {message}");
        Client.Instance.myId = myId;
        ClientSend.WelcomeReceived();

        Client.Instance.udp.Connect(((IPEndPoint) Client.Instance.tcp.Socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet packet)
    {
        int id = packet.ReadInt();
        string userName = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();
        
        GameManager.Instance.SpawnPlayer(id, userName, position, rotation);
    }

    public static void PlayerPosition(Packet packet)
    {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        GameManager.Players[id].transform.position = position;
    }
    
    public static void PlayerRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.Players[id].transform.rotation = rotation;
    }

}