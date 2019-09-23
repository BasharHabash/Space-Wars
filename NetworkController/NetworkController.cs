//                                           Documentation
//----------------------------------------------------------------------------------------------------
//    Namespace:      NetworkController
//    Author:         Bashar Al Habash (7abash), Cole Perschon (coleschon)         Date: Nov. 17, 2017
//----------------------------------------------------------------------------------------------------
///                                               Notes
///---------------------------------------------------------------------------------------------------
///    N/A:           N/A.
///                   
///                   (SEE README.txt)
///---------------------------------------------------------------------------------------------------





using System;
using System.Net;
using System.Net.Sockets;
using System.Text;





/// <summary>
/// Used for opening the sockets between the client and the server and providing helper functions for 
/// sending and receiving data. It is meant to be as general purpose as possible, allowing it to be
/// used by other programs in the future.
/// </summary>
namespace NetworkController
{
    public delegate void NetworkAction(SocketState state);

    /// <summary>
    /// This is to tell us what to do when data comes in.
    /// Determines:
    /// (1) Which socket is the data on 
    /// (2) What previous data has arrived 
    /// (3) what function to call when data arrives.
    /// </summary>
    public class SocketState
    {
        public Socket theSocket;
        public int ID;

        // This is the buffer where we will receive data from the socket
        public byte[] messageBuffer = new byte[1024];

        // This is a larger (growable) buffer, in case a single receive does not contain the full
        // message
        public StringBuilder sb = new StringBuilder();

        // This is how the networking library will "notify" users when a connection is made, or 
        // when data is received
        public NetworkAction callMe;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="s">Socket passed in</param>
        /// <param name="id">The socket passed (in)'s id</param>
        public SocketState(Socket s, int id)
        {
            theSocket = s;
            ID = id;
        }
    }


    public class ConnectionState
    {
        public TcpListener listener;

        // This is how the networking library will "notify" users when a connection is made, or 
        // when data is received
        public NetworkAction callMe;

        public ConnectionState()
        {
        }
    }


    /// <summary>
    /// Contains a number of "helper" functions to handle networking. 
    /// </summary>
    public static class Networking
    {
        public const int DEFAULT_PORT = 11000;
        
        /// <summary>
        /// Creates a Socket object for the given host string.
        /// </summary>
        /// <param name="hostName">The host name or IP address</param>
        /// <param name="socket">The created Socket</param>
        /// <param name="ipAddress">The created IPAddress</param>
        public static void MakeSocket(string hostName, out Socket socket, out IPAddress ipAddress)
        {
            ipAddress = IPAddress.None;
            socket = null;
            try
            {
                // Establish the remote endpoint for the socket
                IPHostEntry ipHostInfo;

                // Determine if the server address is a URL or an IP
                try
                {
                    ipHostInfo = Dns.GetHostEntry(hostName);
                    bool foundIPV4 = false;
                    foreach (IPAddress addr in ipHostInfo.AddressList)
                        if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            foundIPV4 = true;
                            ipAddress = addr;
                            break;
                        }
                    // Didn't find any IPV4 addresses
                    if (!foundIPV4)
                    {
                        System.Diagnostics.Debug.WriteLine("Invalid addres: " + hostName);
                        throw new ArgumentException("Invalid address");
                    }
                }
                catch (Exception)
                {
                    // see if host name is actually an ipaddress, i.e., 155.99.123.456
                    System.Diagnostics.Debug.WriteLine("using IP");
                    ipAddress = IPAddress.Parse(hostName);
                }

                // Create a TCP/IP socket.
                socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

                // Disable Nagle's algorithm - can speed things up for tiny messages, 
                // such as for a game
                socket.NoDelay = true;

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to create socket. Error occured: " + e);
                throw new ArgumentException("Invalid address");
            }
        }


        /// <summary>
        /// Start attempting to connect to the server.
        /// </summary>
        /// <param name="hostName">Server to connect to</param>
        /// <returns>The socket from the state</returns>
        public static Socket ConnectToServer(NetworkAction callMe, string hostName)
        {
            System.Diagnostics.Debug.WriteLine("connecting to " + hostName);

            // Create a TCP/IP socket
            Socket socket;
            IPAddress ipAddress;
            MakeSocket(hostName, out socket, out ipAddress);

            // Create a new state for the data
            SocketState state = new SocketState(socket, -1);

            // Establish the state's callback method
            state.callMe = callMe;

            // Begin connection, return the socket from the state
            state.theSocket.BeginConnect(ipAddress, DEFAULT_PORT, ConnectedCallback, state);
            return state.theSocket;
        }


        /// <summary>
        /// This function is "called" by the operating system when the remote site acknowledges 
        /// connect request.
        /// </summary>
        /// <param name="ar"></param>
        private static void ConnectedCallback(IAsyncResult ar)
        {
            // Make a new state for the data
            SocketState state = (SocketState)ar.AsyncState;

            try
            {
                // Complete the connection.
                state.theSocket.EndConnect(ar);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to connect to server. Error occured: " + e);
                return;
            }

            // Invoke the client's delegate so it can take whatever action it desires
            state.callMe(state);

        }


        /// <summary>
        /// GetData is just a wrapper for BeginReceive.
        /// This is the public entry point for asking for data.
        /// Necessary so that we can separate networking concerns from client concerns.
        /// </summary>
        /// <param name="state"></param>
        public static void GetData(SocketState state)
        {
            state.theSocket.BeginReceive(state.messageBuffer, 0, state.messageBuffer.Length,
                SocketFlags.None, ReceiveCallback, state);
        }
        

        /// <summary>
        /// This function is "called" by the operating system when data is available on the socket.
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            SocketState state = (SocketState)ar.AsyncState;
            int bytesRead = 0;
            try
            {
                bytesRead = state.theSocket.EndReceive(ar);
            }
            catch { }

            // If the socket is still open
            if (bytesRead > 0)
            {
                string theMessage = Encoding.UTF8.GetString(state.messageBuffer, 0, bytesRead);
                // Append the received data to the growable buffer.
                // It may be an incomplete message, so we need to start building it up piece by piece
                state.sb.Append(theMessage);

                // Invoke the client's delegate, so it can take whatever action it desires.
                state.callMe(state);
            }
        }


        /// <summary>
        /// Converts the data into bytes and then sends them using socket.BeginSend.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        public static void Send (Socket socket, String data)
        {
            // Encoding data into UTF8 bytes
            byte[] dataByte = Encoding.UTF8.GetBytes(data);
            //byte[] dataByte = Encoding.UTF8.GetBytes(data + "\n");

            // Begin sending the data byte
            try
            {
            socket.BeginSend(dataByte, 0, dataByte.Length, SocketFlags.None, SendCallback, socket);
            }
            catch { }
        }


        /// <summary>
        /// Extracts the Socket out of the IAsyncResult, and then call socket.EndSend.
        /// </summary>
        /// <param name="ar"></param>
        private static void SendCallback(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            // Nothing much to do here, just conclude the send operation so the socket is happy
            s.EndSend(ar);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="callMe"></param>
        public static void ServerAwaitingClientLoop(NetworkAction callMe)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, DEFAULT_PORT);
            listener.Start();
            ConnectionState cs = new ConnectionState();
            cs.listener = listener;
            cs.callMe = callMe;
            listener.BeginAcceptSocket(AcceptNewClient, cs);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public static void AcceptNewClient(IAsyncResult ar)
        {
            ConnectionState cs = (ConnectionState)ar.AsyncState;
            Socket s = cs.listener.EndAcceptSocket(ar);
            SocketState ss = new SocketState(s, -1);
            ss.theSocket = s;
            ss.callMe = cs.callMe;
            ss.callMe(ss);
            cs.listener.BeginAcceptSocket(AcceptNewClient, cs);
        }
    }
}