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

    public static void UdpTest(Packet packet)
    {
        string msg = packet.ReadString();
        Debug.Log($"Recieved packed via UDP. Contains message: {msg}");
        ClientSend.UdpTestRecieved();
    }
}