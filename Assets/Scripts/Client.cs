using System;
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
        tcp.Connect();
    }

    public class Tcp
    {
        public TcpClient Socket;
        private NetworkStream _stream;
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
            _stream.BeginRead(_recieveBuffer, 0, dataBufferSize, RecieveCallback, null);
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

                _stream.BeginRead(_recieveBuffer, 0, dataBufferSize, RecieveCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recieving TCP data: {ex}");
                // TODO disconnect
            }
        }
    }
}