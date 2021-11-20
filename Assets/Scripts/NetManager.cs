using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class NetManager
{
    static Socket socket;

    static byte[] readBuff = new byte[1024];

    public delegate void MsgListener(string str);

    // listening list
    private static Dictionary<string, MsgListener> listeners = new Dictionary<string, MsgListener>();

    // message queue
    private static Queue<string> msgQueue = new Queue<string>();

    public static void AddListener(string msgName, MsgListener listener)
    {
        if (listeners.ContainsKey(msgName))
        {
            listeners[msgName] = listener;
        }
        else
        {
            listeners.Add(msgName, listener);
        }
    }

    public static string GetDesc()
    {
        if (socket == null)
        {
            return "";
        }

        if (!socket.Connected)
        {
            return "";
        }

        return socket.LocalEndPoint.ToString();
    }

    public static void Connect(string ip, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        socket.BeginConnect(ip, port, ConnectCallback, socket);
    }

    public static void Disconnect()
    {
        if (socket != null && socket.Connected)
        {
            socket.Disconnect(false);
        }
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = ar.AsyncState as Socket;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Successful");

            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log($"Socket Connect Failed: {ex}");
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = ar.AsyncState as Socket;

            var count = socket.EndReceive(ar);

            string recvStr = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);

            msgQueue.Enqueue(recvStr);

            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log($"Socket Receive Failed: {ex}");
        }
    }

    public static void Send(string sendStr)
    {
        if (socket == null)
        {
            return;
        }

        if (!socket.Connected)
        {
            return;
        }

        byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(sendStr);
        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = ar.AsyncState as Socket;

            var count = socket.EndSend(ar);

            Debug.Log($"Socket Send {count} bytes");
        }
        catch (SocketException ex)
        {
            Debug.Log($"Socket Send Failed: {ex}");
        }
    }

    public static void Update()
    {
        if (msgQueue.Count == 0)
        {
            return;
        }

        var msgStr = msgQueue.Dequeue();

        var split = msgStr.Split('|');
        var msgName = split[0];
        var msgArgs = split[1];

        if (listeners.ContainsKey(msgName))
        {
            listeners[msgName](msgArgs);
        }
    }
}
