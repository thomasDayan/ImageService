using ImageService.Controller;
using ImageService.Controller.Handlers;
using ImageService.Infrastructure.Enums;
using ImageService.Logging;
using ImageService.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using ImageService.Logging.Modal;
using Newtonsoft.Json.Linq;
using ImageService.Commands;

namespace ImageService.Server
{
    /// <summary>
    /// Class of the server .
    /// </summary>
    public class ImageServer
    {
        #region Members
        private IImageController m_controller;
        private ILoggingService m_logging;
        private TcpListener tcpListener;
        private EventLog eventLog1;
        Dictionary<string, IDirectoryHandler> handlers;
        private HandleLogs handleLogs = null;
        private List<string> deletedhandler;
        private Mutex mutex = new Mutex();
        private Mutex mutex2 = new Mutex();
        private Mutex mutex3 = new Mutex();
        private Mutex mutex4 = new Mutex();
        #endregion

        #region Properties
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;          // The event that notifies about a new Command being recieved
        #endregion

        /// <summary>
        /// Constructor for ImageServer.
        /// </summary>
        /// <param name="m"> Image Controller. </param>
        /// <param name="n"> Logging Service. </param>
        public ImageServer(IImageController m, ILoggingService n)
        {
            deletedhandler = new List<string>();
            m_controller = m;
            m_logging = n;

            ConnectServer();
        }
        /// <summary>
        /// Set function of the event log.
        /// </summary>
        /// <param name="e">The event log.</param>
        public void setEventLog(EventLog e) { this.eventLog1 = e; }
        /// <summary>
        /// Creates a handler.
        /// </summary>
        /// <param name="path"> Path. </param>
        public void CreateHandler(string path)
        {
            IDirectoryHandler directoryHandler = new DirectoryHandler(m_controller, m_logging);

            // add to events
            CommandRecieved += directoryHandler.OnCommandRecieved;
            directoryHandler.DirectoryClose += DeleteHandler;

            // start the handler
            directoryHandler.StartHandleDirectory(path);

            handlers.Add(path, directoryHandler);
        }
        /// <summary>
        /// Set function.set handler function.
        /// </summary>
        /// <param name="d">the hander.</param>
        public void setHandlers(Dictionary<string , IDirectoryHandler> d) { handlers = d; }
        /// <summary>
        /// When the server closes.
        /// </summary>
        public void onCloseServer()
        {
            SendCommand(new CommandRecievedEventArgs((int)CommandEnum.CloseCommand, null, null));
            closeServer();
        }
        public HandleLogs GetHandleLogs() { return handleLogs; }
        /// <summary>
        /// Sends the command that was received.
        /// </summary>
        /// <param name="e"> Event. </param>
        public void SendCommand(CommandRecievedEventArgs e)
        {
            CommandRecieved?.Invoke(this, e);
        }

        /// <summary>
        /// Deletes the handler.
        /// </summary>
        /// <param name="sender"> Sender. </param>
        /// <param name="e"> Event. </param>
        public void DeleteHandler(object sender, DirectoryCloseEventArgs e)
        {
            
            IDirectoryHandler handler = (IDirectoryHandler)sender;
            
            CommandRecieved -= handler.OnCommandRecieved;
            handler.DirectoryClose -= DeleteHandler;
            //m_logging.Log(e.Message + "directory is closed", Logging.Modal.MessageTypeEnum.INFO);
            
        }
        /// <summary>
        /// Function that connect the server to clients.
        /// </summary>
        public void ConnectServer()
        {
            Thread.Sleep(100);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);
            TcpListener listener = new TcpListener(ep);
            //first of all start the listener for searching connections.
            listener.Start();
            Console.Write("Waiting for connection...");
            HandleLogs handleLogs = new HandleLogs();

            //thread for alwayes search for cnnections.
            Task ta = new Task(() =>
            {
            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    //this.client = client;
                    Console.WriteLine("Connection accepted.");
                        m_logging.Log("Connection to server accepted", MessageTypeEnum.INFO);

                        mutex3.WaitOne();
                    NetworkStream ns = client.GetStream();
                    byte[] bytes = new byte[1024];

                    //this.ns = client.GetStream();
                    int bytesRead = ns.Read(bytes, 0, bytes.Length);
                    string  r = Encoding.ASCII.GetString(bytes, 0, bytesRead);                        
                    mutex3.ReleaseMutex();

                    Task task2 = new Task(() =>
                    {
                        if (client.Connected)
                        {
                            if (r == "GetConfigCommand")
                            {
                                ICommand command = new GetConfigCommand(this , client , m_controller);
                                bool success;
                                command.Execute(null , out success);

                                //HandleSettings h = new HandleSettings(this, client, m_controller);
                            }
                            else if (r == "LogCommand")
                            {
                                ICommand command = new LogCommand(handleLogs, this, m_logging, client);
                                bool success;
                                command.Execute(null, out success);

                                //handleLogs.SetAllProt(this, m_logging , client);
                            }
                        }
                    }); task2.Start();
                        /*
                         * need to take care the close 
                         * when the client close the screen we should out this while ! 
                         * 
                         * */
                    }
                    catch (Exception e)
                    {
                        break;
                    }
                }
            } );
            ta.Start();
            tcpListener = listener;
            
        }
        /// <summary>
        /// Function that close the server connection.
        /// </summary>
        public void closeServer()
        {
            m_logging.Log("Server close connection. ", MessageTypeEnum.INFO);
            tcpListener.Stop();   
            /*
            ns.Close();
            client.Close();*/
        }
        /// <summary>
        /// write to socket stream function.
        /// </summary>
        /// <param name="write">The text that send.</param>
        /// <param name="client">the client who get the message. </param>
        public void writeSocket(string write , TcpClient client) {
            //mutex.WaitOne();
            NetworkStream ns = client.GetStream();
            byte[] byteTime = Encoding.ASCII.GetBytes(write);
            ns.Write(byteTime, 0, byteTime.Length);

            //m_logging.Log("Write to client: " + write, MessageTypeEnum.INFO);
            //eventLog1.WriteEntry("Write to client = " + write);
            /*need to write to log*/
            //mutex.ReleaseMutex();
        }
        /// <summary>
        /// function that read from the socket.
        /// </summary>
        /// <param name="ns">the stream we are reading from. </param>
        /// <returns></returns>
        public string readSocket(NetworkStream ns)
        {
            string s;
            mutex2.WaitOne();
            //NetworkStream ns = client.GetStream();

            byte[] bytes = new byte[4096];
            /*need to write to log*/

            int bytesRead = ns.Read(bytes, 0, bytes.Length);
            s =  Encoding.ASCII.GetString(bytes, 0, bytesRead);
           // m_logging.Log("Read from client: " + s, MessageTypeEnum.INFO);
            //eventLog1.WriteEntry("Read from client = " + s);
            mutex2.ReleaseMutex();
            
            return s;
        }
        /// <summary>
        /// Function that translate the app config to text and send him.
        /// </summary>
        /// <returns>Configuration as string .</returns>
        public string AppConfigText()
        {
            JObject jObject = new JObject();

            jObject["SourceName"] = ConfigurationManager.AppSettings.Get("SourceName");
            jObject["LogName"] = ConfigurationManager.AppSettings.Get("LogName");
            jObject["OutputDir"] = ConfigurationManager.AppSettings.Get("OutputDir");
            jObject["Handler"] = ConfigurationManager.AppSettings.Get("Handler");
            jObject["ThumbnailSize"] = ConfigurationManager.AppSettings.Get("ThumbnailSize");
            string dele = "";
            foreach (string s in deletedhandler)
            {
                dele = dele + s + ";";
            }
            jObject["DeleteHandler"] = dele;
            

            return jObject.ToString();
           
        }
        /// <summary>
        /// Function that taking care of delete handler.
        /// </summary>
        /// <param name="path"></param>
        public void delete_handler(string path)
        {
            m_logging.Log("Delete handler: " + path, MessageTypeEnum.INFO);
            deletedhandler.Add(path);
        }
    }
}
