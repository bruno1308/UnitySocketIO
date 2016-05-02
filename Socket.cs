using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
namespace UnitySocket
{

    public class Socket
    {
        private static Dictionary<OpCodeReceive, Emitter.Listener> callbacks = new Dictionary<OpCodeReceive, Emitter.Listener>();
        //private SslStream sslStream;
        System.Net.Sockets.Socket client;
        private string ip;
        private int port;
        // Receive buffer.
        public byte[] buffer;
        public void On(OpCodeReceive opCode, Emitter.Listener callback)
        {
            callbacks.Add(opCode, callback);
        }

        public void Off(OpCodeReceive opCode, Emitter.Listener callback)
        {
            callbacks.Remove(opCode);
        }

        public void Emit(OpCodeEmit opCode, byte[] args)
        {
            Debug.Log("Emitting packet with data: " + args.ToString());
            byte[] packet = new byte[1 + args.Length];
            packet[0] = (byte)opCode;
            args.CopyTo(packet, 1);
            Send(packet);
        }
        public Socket(string ip, int port, Emitter.Listener connectCallback)
        {
            this.ip = ip;
            this.port = port;
            callbacks.Add(OpCodeReceive.ONCONNECT, connectCallback);
            StartClient();
        }


        public void Disconnect()
        {
            Debug.Log("Disconnecting");
            //this.Emit(OpCodeEmit.DISCONNECT, null);
            callbacks.Clear();
            // Release the socket.
            if (client != null)
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
        }

        private void StartClient()
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                IPHostEntry ipHostInfo = Dns.GetHostEntry(ip);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, this.port);

                // Create a TCP/IP socket.
                this.client = new System.Net.Sockets.Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.


                IAsyncResult result = client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);

                bool success = result.AsyncWaitHandle.WaitOne(5000, true);

                if (!success)
                {
                    // NOTE, MUST CLOSE THE SOCKET

                    this.client.Close();
                    Debug.Log("Failed to connect server.");
                    throw new ApplicationException("Failed to connect server.");
                }

            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                System.Net.Sockets.Socket client = (System.Net.Sockets.Socket)ar.AsyncState;

                callbacks[OpCodeReceive.ONCONNECT].call(null);

                // Complete the connection.
                client.EndConnect(ar);

                Debug.Log("Socket connected to " +
                    client.RemoteEndPoint.ToString());
                Receive(client);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private void Send(byte[] byteData)
        {
            Debug.Log("Begin send... ");
            // Convert the string data to byte data using ASCII encoding.
            //  byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            this.client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                System.Net.Sockets.Socket client = (System.Net.Sockets.Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Debug.Log("Sent " + bytesSent + " bytes to server.");

                // Signal that all bytes have been sent.
                // sendDone.Set();
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private void Receive(System.Net.Sockets.Socket client)
        {
            if (!SocketConnected(client))
            {
                Debug.LogError("Server disconnected ungracefully!");
                return;
            }
            try
            {
                Debug.Log("Started listening for message...");
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;
                state.expected = -1;
                state.offset = 0;
                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, state.offset, 256, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }


        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Debug.Log("Receive callback ");

                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                System.Net.Sockets.Socket client = state.workSocket;
                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                Debug.Log("bytes read: "+bytesRead.ToString());
                if (bytesRead > 0)
                {
                    if (state.expected == -1)
                    {
                        state.expected = PacketUtils.GetSizeFromOpCodeReceived((OpCodeReceive)state.buffer[0]);
                    }
                    state.offset += bytesRead;
                    Debug.Log("state offset: " + state.offset.ToString());
                    // Get the rest of the data.
                    if (state.offset < state.expected)
                    {
                        Debug.Log("State.buffer = " + state.buffer.Length.ToString());
                        Debug.Log("State.offset = " + state.offset.ToString());
                        Debug.Log("StateObject.Buffersize = " + StateObject.BufferSize.ToString());

                        client.BeginReceive(state.buffer, state.offset, StateObject.BufferSize-state.offset, 0,
                            new AsyncCallback(ReceiveCallback), state);
                        Debug.Log("lesser than expected");
                    }
                    else if (state.offset == state.expected)
                        finishRead(state);
                    else
                        Debug.Log("Too many packets arrived");
                }
                else {
                    // All the data has arrived; put it in response.
                    if (state.offset == state.expected)
                        finishRead(state);
                    else
                        Receive(client);

                    // Signal that all bytes have been received.
                    // receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private void finishRead(StateObject state)
        {
            Debug.Log("Received a total number of byes of: " + state.offset);
            OpCodeReceive opCode = ((OpCodeReceive)state.buffer[0]);
            if (state.offset > 0 && callbacks.ContainsKey(opCode))
                callbacks[opCode].call(state.buffer);
            else
                Debug.Log("Not Registered OpCode Callback");

            Receive(client);
        }
        bool SocketConnected(System.Net.Sockets.Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
    }

    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public System.Net.Sockets.Socket workSocket = null;
        // Size of receive buffer (max packet size).
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        //Expected number of bytes
        public int expected;
        //Currently received number of bytes
        public int offset;
    }
}