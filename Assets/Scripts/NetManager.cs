using System;
using System.Collections;
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

    static int buffCount = 0;

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
            socket.Close();
        }
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = ar.AsyncState as Socket;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Successful");

            socket.BeginReceive(readBuff, buffCount, 1024 - buffCount, 0, ReceiveCallback, socket);
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

            buffCount += count;

            OnReceiveData();

            socket.BeginReceive(readBuff, buffCount, 1024 - buffCount, 0, ReceiveCallback, socket);
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

        byte[] bodyBytes = System.Text.Encoding.UTF8.GetBytes(sendStr);

        // add length bytes into package
        short len = (short)bodyBytes.Length;
        byte[] lenBytes = BitConverter.GetBytes(len);
        byte[] sendBytes = lenBytes.Concat(bodyBytes).ToArray();
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

        if (string.IsNullOrEmpty(msgStr))
        {
            return;
        }

        var split = msgStr.Split('|');
        var msgName = split[0];
        var msgArgs = split[1];

        Debug.Log($"NetManager -> Dequeue -> {msgName} -> {msgArgs}");

        if (listeners.ContainsKey(msgName))
        {
            listeners[msgName](msgArgs);
        }
    }

    public static IEnumerator DelaySend(float seconds, string msg)
    {
        yield return new WaitForSeconds(seconds);

        Send(msg);
    }

    private static void OnReceiveData()
    {
        // only length bytes
        if (buffCount <= 2)
        {
            return;
        }

        short bodyLength = BitConverter.ToInt16(readBuff, 0);

        // package is not completed
        if (buffCount < 2 + bodyLength)
        {
            return;
        }

        string recvStr = System.Text.Encoding.UTF8.GetString(readBuff, 2, bodyLength);

        msgQueue.Enqueue(recvStr);

        int start = 2 + bodyLength;
        int count = buffCount - start;

        Array.Copy(readBuff, start, readBuff, 0, count);

        buffCount -= start;

        // work utill return
        OnReceiveData();
    }
}
