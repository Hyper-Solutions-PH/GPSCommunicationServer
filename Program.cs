using GPSCommunicationServer;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;


class DeviceListener
{

    public static void Main()
    {

        TcpListener server = null;
        try
        {
            // Set the TcpListener port .
            Int32 port = 5678;

            //To ask an IP Address every time that the server start
            Console.WriteLine("Write an IP to start the server:");
            String ip = Console.ReadLine();
            IPAddress localAddr = IPAddress.Parse(ip);

            //To have the same IP Address when the server start
            //IPAddress localAddr = IPAddress.Parse("192.168.1.138");

            // TcpListener server = new TcpListener(serverIP,port);
            server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            server.Start();


            // Enter the listening loop.
            while (true)
            {
                Console.WriteLine("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                TcpClient client = server.AcceptTcpClient();
                //if you have a successful connection
                Console.WriteLine("Connected!");

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();


                //Initializing thread with the stream and the client that sent a message
                CommunicationThread thread = new CommunicationThread(stream, client);


                //Starting thread to receive data from a client 
                Thread receive = new Thread(new ThreadStart(thread.ThreadReceiveGPS));
                receive.Start();
               
                ////Starting a second thread to send data to a client 
                Thread send = new Thread(new ThreadStart(thread.ThreadSendGPS));
                send.Start();

            }
        }
        catch (SocketException e)
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