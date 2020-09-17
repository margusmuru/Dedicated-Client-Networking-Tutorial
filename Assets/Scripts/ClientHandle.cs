using UnityEditor.VersionControl;
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
    }
}