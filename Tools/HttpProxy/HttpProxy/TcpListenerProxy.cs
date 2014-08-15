using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HttpProxy
{
    public class TcpListenerProxy
    {
        public const int PACKET_BUFFSIZE = 2048; // packet buffer in bytes
        private const string LOCAL_OUT_IPADDR = "192.168.1.102"; // active localhost external address (set your one)
        private const string TARGET_IPADDR = "www.baidu.com"; // destination address (google.com)
        private static readonly int[] TARGET_PORTS = new int[] { 
            80,
            //8080
        };

        public static int packet_no = 0; // packet counter

        private static ArrayList listeners = new ArrayList(); // array of references to all listeners
        private static ArrayList sockets = new ArrayList(); // array of references to all sockets

        public static void Start(string[] args)
        {
            listeners = ArrayList.Synchronized(listeners);
            sockets = ArrayList.Synchronized(sockets);

            //#region print starting parameters
            //Console.WriteLine("PACKET_BUFFSIZE = {0}", PACKET_BUFFSIZE);
            //Console.WriteLine("LOCAL_OUT_IPADDR = {0}", LOCAL_OUT_IPADDR);
            //Console.WriteLine("TARGET_IPADDR = {0}", TARGET_IPADDR);
            //Console.Write("TARGET_PORTS: ");
            //foreach (int port in TARGET_PORTS) { Console.Write("{0} ", port); }
            //Console.WriteLine("\n");
            //#endregion

            //#region test local ip interfaces
            //// This block is responsible for checking if LOCAL_OUT_IPADDR and TARGET_IPADDR values
            //// are possible to set to the active ip interfaces.

            //IPAddress[] ap_arr = Dns.GetHostAddresses(Dns.GetHostName());
            //IPAddress target_addr = IPAddress.None;
            //IPAddress local_out_addr = IPAddress.None;

            //int interface_no = 0;
            //Console.WriteLine("local active IP4 interfaces:");
            //foreach (IPAddress ia in ap_arr)
            //{
            //    if (ia.AddressFamily == AddressFamily.InterNetwork)
            //        Console.WriteLine(" {0}: " + ia.ToString(), interface_no++);

            //    // set addresses if available
            //    if (ia.ToString() == TARGET_IPADDR)
            //    {
            //        target_addr = ia; // set target
            //    }
            //    else if (ia.ToString() == LOCAL_OUT_IPADDR)
            //    {
            //        local_out_addr = ia; // set local
            //    }
            //}
            //Console.WriteLine();
            //Console.WriteLine("target interface address set to {0}", target_addr.ToString());
            //Console.WriteLine("local outgoing interface address set to {0}", local_out_addr.ToString());
            //if (target_addr.ToString() != TARGET_IPADDR)
            //{
            //    Console.WriteLine();
            //    Console.WriteLine("error: target interface not match");
            //    Console.WriteLine("exit:  interface unavailable");
            //    Console.ReadKey(true);
            //    return;
            //}
            //if (local_out_addr.ToString() != LOCAL_OUT_IPADDR)
            //{
            //    Console.WriteLine();
            //    Console.WriteLine("error: local interface not match");
            //    Console.WriteLine("exit:  interface unavailable");
            //    Console.ReadKey(true);
            //    return;
            //}
            //Console.WriteLine("--ok--\n");
            //Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.WriteLine("ANY KEY TO STOP AND CLOSE ALL\n");
            //Console.ResetColor();
            //#endregion

            #region run listeners
            // Start new listener for every port in TARGETS_PORT array asynchronously.
            // An object is needed as a parameter for Thread.
            for (int i = 0; i < TARGET_PORTS.Length; i++)
            {
                object[] o = new object[]{
                    //(object)target_addr,
                    //(object)local_out_addr,
                    (object)TARGET_PORTS[i],
                    (object)i
                };
                Thread t = new Thread(new ParameterizedThreadStart(RunListenerThread));
                t.Start(o);
            }
            #endregion

            #region close all and exit
            Console.ReadKey(true);
            Console.WriteLine();
            try
            {
                Console.WriteLine("closing listeners..");
                foreach (TcpListener l in listeners)
                {
                    l.Stop();
                }
                Console.WriteLine("closing sockets..");
                foreach (SocketStateObj s in sockets)
                {
                    s.in_socket.Close();
                    s.out_socket.Close();
                }
            }
            catch (Exception) { }
            Console.WriteLine("--ok--");
            Console.ReadKey(true);
            #endregion
        }

        private static void RunListenerThread(object o)
        {
            object[] oa = (object[])o;
            int listNo = (int)oa[3];
            try
            {
                //RunListener((IPAddress)(oa[0]), (IPAddress)(oa[1]), (int)(oa[2]), listNo);
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                // <-- listener exception handling code -->

                //Console.WriteLine("listener {0} exception: {1}", listNo, ex.Message);
            }
        }

        /// <summary>
        /// Start listener at selected port, accept connections and create remote socket. This function is blocking one.
        /// </summary>
        /// <param name="listenerInterface">local, client side interface</param>
        /// <param name="outgoingInterface">local, remote server side interface</param>
        /// <param name="listenerPort">port for both interfaces</param>
        /// <param name="listNo">listener number</param>
        public static void RunListener(IPAddress listenerInterface, int listenerPort)
        {
            listeners = ArrayList.Synchronized(listeners);
            sockets = ArrayList.Synchronized(sockets);

            TcpListener listener = new TcpListener(listenerInterface, listenerPort);
            listeners.Add(listener);
            listener.Start();
            Console.WriteLine("started at {0}:{1}", listenerInterface.ToString(), listenerPort);

            while (true)
            {
                Socket incoming_socket = listener.AcceptSocket();

                var endPoint = (IPEndPoint)incoming_socket.RemoteEndPoint;
                IPAddress outgoingInterface = endPoint.Address;
                int listNo = endPoint.Port;

                Console.WriteLine("listener {0} is waiting for a new client", listNo);
                Console.WriteLine("listener {0}: client connected", listNo);

                // connecting remote host
                Console.WriteLine("listener {0} is connecting to remote host {1}:{2}", listNo, TARGET_IPADDR, listenerPort);
                Socket remote_socket = ConnectSocket(new IPEndPoint(outgoingInterface, 0), TARGET_IPADDR, listenerPort);
                if (remote_socket == null)
                {
                    Console.WriteLine("listener {0}: outgoing connection failed", listNo);
                    continue;
                }
                Console.WriteLine("listener {0}: connected to remote host {1}:{2}", listNo, TARGET_IPADDR, listenerPort);

                // begin receive on input
                SocketStateObj iso = new SocketStateObj(incoming_socket, remote_socket);
                sockets.Add(iso);
                incoming_socket.BeginReceive(iso.buffer, 0, SocketStateObj.BUFF_SIZE, SocketFlags.None,
                    new AsyncCallback(Read_Callback), iso);

                // begin receive on output
                SocketStateObj oso = new SocketStateObj(remote_socket, incoming_socket);
                sockets.Add(oso);
                remote_socket.BeginReceive(oso.buffer, 0, SocketStateObj.BUFF_SIZE, SocketFlags.None,
                    new AsyncCallback(Read_Callback), oso);
            }
        }

        private static void Read_Callback(IAsyncResult ar)
        {
            Socket s1 = null;
            Socket s2 = null;
            try
            {
                // retrieve SocketStateObj
                SocketStateObj so = (SocketStateObj)ar.AsyncState;
                s1 = so.in_socket;
                s2 = so.out_socket;
                int count = s1.EndReceive(ar);

                if (count > 0)
                {
                    // copy of buffer data
                    byte[] tmpbuff = new byte[PACKET_BUFFSIZE];
                    so.buffer.CopyTo(tmpbuff, 0);

                    // async. wait for next packet
                    s1.BeginReceive(so.buffer, 0, SocketStateObj.BUFF_SIZE, SocketFlags.None,
                        new AsyncCallback(Read_Callback), so);

                    // <-- packet filtering -->

                    #region show packet info
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("[{0}] packet {1} -> S: {2}  D: {3}  len: {4}",
                        DateTime.Now.ToString("HH:mm:ss"),
                        packet_no++,
                        s2.LocalEndPoint.ToString(),
                        s1.LocalEndPoint.ToString(),
                        count);
                    Console.ResetColor();
                    #endregion

                    // send packet to destination (if not filtered)
                    s2.Send(tmpbuff, 0, count, SocketFlags.None);

                    // <-- packet injection or re-sending -->
                }
                else
                {
                    // close connections if packet has empty data                    
                    Console.WriteLine("connection {0} <-> {1} closed", s2.LocalEndPoint.ToString(), s1.LocalEndPoint.ToString());
                    s1.Close();
                    s2.Close();
                }
            }
            catch (Exception ex)
            {
                // <-- socket exception handling code -->

                //Console.WriteLine("Read_Callback exception: {0}", ex.Message);
                try
                {
                    s1.Close();
                    s2.Close();
                }
                catch (Exception) { }
            }
        }

        private static Socket ConnectSocket(IPEndPoint localEndpoint, string remoteServer, int remotePort)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Bind(localEndpoint);
            s.Connect(remoteServer, remotePort);
            return s;
        }
    }

    public class SocketStateObj
    {
        public static readonly int BUFF_SIZE = TcpListenerProxy.PACKET_BUFFSIZE;
        public Socket in_socket = null;
        public Socket out_socket = null;
        public byte[] buffer = new byte[BUFF_SIZE];

        public SocketStateObj(Socket in_socket)
        {
            this.in_socket = in_socket;
        }

        public SocketStateObj(Socket in_socket, Socket out_socket)
        {
            this.in_socket = in_socket;
            this.out_socket = out_socket;
        }
    }
}