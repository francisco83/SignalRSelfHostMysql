using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.IO;
using Mono.Data.Sqlite;
using System.Data;


namespace SignalRSelfHost
{
    class Program
    {

        static void Main(string[] args)
        {
            // This will *ONLY* bind to localhost, if you want to bind to all addresses
            // use http://*:8080 to bind to all addresses. 
            // See http://msdn.microsoft.com/library/system.net.httplistener.aspx 
            // for more information.
            string url = "http://localhost:8888";
            /*
            const string connectionString = "URI=file:dbPanelControl.db";
            IDbConnection dbcon = new SqliteConnection(connectionString);
            dbcon.Open();

            IDbCommand dbcmd = dbcon.CreateCommand();

            const string sql =
               "SELECT temperatura, humedad " +
               "FROM registros";
            dbcmd.CommandText = sql;
            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                string temperatura = reader.GetFloat(0).ToString();
                string humedad = reader.GetFloat(1).ToString();
                Console.WriteLine("Name: {0} {1}",
                    temperatura, humedad);
            }
            // clean up
            reader.Dispose();
            dbcmd.Dispose();
            dbcon.Close();
            */

            using (WebApp.Start(url))
            {
                Console.WriteLine("Server running on {0}", url);
                Console.ReadLine();
                SqliteConnection.CreateFile("MyDatabase.sqlite");
            }




        }
    }


    class Startup
    {

        Socket clientsock;
        byte[] MsgRecvBuff = new byte[1024];

        const string connectionString = "URI=file:dbPanelControl.db";
        IDbConnection dbcon = new SqliteConnection(connectionString);
        string sql = "";
        string[] sql_aux;


        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();

            string strIP;
            string strport;
            int port;

            // Get the IP text value
            strIP = "192.168.0.40";
            // Get the port text value
            strport = "23";
            port = Int32.Parse(strport);


            IPAddress peerIP = IPAddress.Parse(strIP);
            // There are IP or port information in the localEP
            IPEndPoint peerEP = new IPEndPoint(peerIP, port);

            // Client socket memory allocation
            clientsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            Console.Write("Connecting.. \n");

            try
            {
                // Send the signal for connect with server 
                // This application will be run asynchronously by this function.
                // If the client accept the socekt, this function called a ConnectCallback function.
                clientsock.BeginConnect(peerEP, new AsyncCallback(ConnectCallback), clientsock);

            }
            catch (SocketException er)
            {
                // If the socket exception is occurrence, the application display a error message.  
                Console.WriteLine(er.Message);
            }

        }

        public void ConnectCallback(IAsyncResult ar)
        {
            byte[] a = new byte[1024];
            try
            {
                // If the client socket connect with the server, return the socket
                clientsock = (Socket)ar.AsyncState;
                clientsock.EndConnect(ar);
                clientsock.ReceiveTimeout = 5000;

                Console.WriteLine("Conectado");

                //Inicializo para que comience a leer los datos
                clientsock.Send(a);


                // If the client transmitted the messages, this function called a Callback_ReceiveMsg function.
                clientsock.BeginReceive(MsgRecvBuff, 0, MsgRecvBuff.Length, SocketFlags.None, new AsyncCallback(CallBack_ReceiveMsg), clientsock);
            }
            catch (SocketException er)
            {
                Console.WriteLine(er.Message);
            }

        }

        // BeginReceive()'s call back fucntion
        public void CallBack_ReceiveMsg(IAsyncResult ar)
        {
            
            int length;
            String MsgRecvStr = null;
            CheckHub a = new CheckHub();

            try
            {
                // If the socket close, return the 0 value 
                length = clientsock.EndReceive(ar);

                if (length > 0)
                {
                    
                    MsgRecvStr = Encoding.Default.GetString(MsgRecvBuff, 0, length);

                    Console.WriteLine("Dato recibido:" + MsgRecvStr + "\n");


                    //Seteo la temperatura
                    a.SetTemp(MsgRecvStr);

                    //Guardo en base de datos los registros
                    dbcon.Open();

                    IDbCommand dbcmd = dbcon.CreateCommand();

                    /*sql =
                       "SELECT temperatura, humedad " +
                       "FROM registros";*/
                    sql_aux = MsgRecvStr.Split(' ');
                    sql = "INSERT INTO registros (fecha,temperatura,humedad,luminosidad,voltspanel,voltsbateria)" +
                        "VALUES('" + DateTime.Now.ToString() + "'," + sql_aux[0] + "," + sql_aux[1] + "," + sql_aux[2] + "," + sql_aux[3] + "," + 0 + ")";                       
                    dbcmd.CommandText = sql;
                    IDataReader reader = dbcmd.ExecuteReader();
                    /*while (reader.Read())
                    {
                        string temperatura = reader.GetFloat(0).ToString();
                        string humedad = reader.GetFloat(1).ToString();
                        Console.WriteLine("Valores: {0} {1}",
                            temperatura, humedad);
                    }*/
                    // clean up
                    reader.Dispose();
                    dbcmd.Dispose();
                    dbcon.Close();


                    // The socket can continuedly wait the receive message because of this function 
                    clientsock.BeginReceive(MsgRecvBuff, 0, MsgRecvBuff.Length, SocketFlags.None, new AsyncCallback(CallBack_ReceiveMsg), clientsock);
                }
                else
                {
                    // If the socket close, return the 0 value 
                    // Then, socket close.
                    clientsock.Close();

                }

            }
            catch (Exception er)
            {
                Console.WriteLine(er.Message);
            }
        }

    }
}