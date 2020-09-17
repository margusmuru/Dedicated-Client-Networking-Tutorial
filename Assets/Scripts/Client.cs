using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client Instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public Tcp tcp;

    private delegate void PacketHandler(Packet packet);

    private static Dictionary<int, PacketHandler> _packetHandlers;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log("Instance of Client already exists, destroying object");
            Destroy(this);
        }
    }

    private void Start()
    {
        tcp = new Tcp();
    }

    public void ConnectToServer()
    {
        InitializeClientData();
        tcp.Connect();
    }

    public class Tcp
    {
        public TcpClient Socket;
        private NetworkStream _stream;
        private Packet _receivedData;
        private byte[] _recieveBuffer;

        public void Connect()
        {
            Socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };
            _recieveBuffer = new byte[dataBufferSize];
            Socket.BeginConnect(Instance.ip, Instance.port, ConnectCallback, Socket);
        }

        private void ConnectCallback(IAsyncResult result)
        {
            Socket.EndConnect(result);
            if (!Socket.Connected)
            {
                return;
            }

            _stream = Socket.GetStream();
            _receivedData = new Packet();
            _stream.BeginRead(_recieveBuffer, 0, dataBufferSize, RecieveCallback, null);
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (Socket != null)
                {
                    _stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception exception)
            {
                Debug.Log($"Error sending data to server via TCP: {exception}");
            }
        }

        private void RecieveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = _stream.EndRead(result);
                if (byteLength <= 0)
                {
                    // TODO disconnect
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(_recieveBuffer, data, byteLength);
                _receivedData.Reset(HandleData(data));
                _stream.BeginRead(_recieveBuffer, 0, dataBufferSize, RecieveCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recieving TCP data: {ex}");
                // TODO disconnect
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;
            _receivedData.SetBytes(data);
            if (_receivedData.UnreadLength() >= 4)
            {
                packetLength = _receivedData.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= _receivedData.UnreadLength())
            {
                byte[] packetBytes = _receivedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        _packetHandlers[packetId](packet);
                    }
                });

                packetLength = 0;
                if (_receivedData.UnreadLength() >= 4)
                {
                    packetLength = _receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                return true;
            }

            return false;
        }
    }

    private void InitializeClientData()
    {
        _packetHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int) ServerPackets.welcome, ClientHandle.Welcome}
        };
        Debug.Log("Initialized packets.");
    }
}