using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTcpData(Packet packet)
    {
        packet.WriteLength();
        Client.Instance.tcp.SendData(packet);
    }

    private static void SendUdpData(Packet packet)
    {
        packet.WriteLength();
        Client.Instance.udp.SendData(packet);
    }
    
    #region Packets

    public static void WelcomeReceived()
    {
        using (Packet packet = new Packet((int) ClientPackets.welcomeReceived))
        {
            packet.Write(Client.Instance.myId);
            packet.Write(UIManager.Instance.userNameField.text);
            
            SendTcpData(packet);
        }
    }

    public static void UdpTestRecieved()
    {
        using (Packet packet = new Packet((int) ClientPackets.udpTestReceived))
        {
            packet.Write("Received a UDP packet.");
            SendUdpData(packet);
        }
    }
    #endregion
}
