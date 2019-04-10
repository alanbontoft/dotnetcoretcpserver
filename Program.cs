using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace tcpserver
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress address = IPAddress.Parse("127.0.0.1");
            string iface = (args.Length > 0) ? args[0] : "eth0";
            bool found = false;
 
            // Get IP address of nic specified by args[]
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces()) 
            {
                if (found) break;

                if (nic.Name == iface)
                {
                    if(nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                address = ip.Address;
                                found = true;
                                break;
                            }
                        }
                    }  
                }
            } 

            TcpListener server=null;   
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 13000;
            
                // TcpListener server = new TcpListener(port);
                server = new TcpListener(address, port);

                // Start listening for client requests.
                server.Start();
                    
                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while(true) 
                {
                    Console.Write("Waiting for a connection... ");
                
                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();            
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while((i = stream.Read(bytes, 0, bytes.Length))!=0) 
                    {   
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("Received: {0}", data);
                
                    // Process the data sent by the client.
                    data = data.ToUpper();

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                    for (int j=0; j < msg.Length; j++)
                    {
                        msg[j]++;
                    }

                    data = System.Text.Encoding.ASCII.GetString(msg, 0, msg.Length);

                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    Console.WriteLine("Sent: {0}", data);            
                    }
                    
                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch(SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

        
            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }
}
