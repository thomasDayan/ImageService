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

namespace ImageService.Server
{
    public class ImageServer
    {
        #region Members
        private IImageController m_controller;
        private ILoggingService m_logging;
        private NetworkStream ns;
        private TcpClient client;
        private TcpListener tcpListener;
        Dictionary<string, IDirectoryHandler> handlers;
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
            m_controller = m;
            m_logging = n;

            ConnectServer();
        }

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
        public void setHandlers(Dictionary<string , IDirectoryHandler> d) { handlers = d; }
        /// <summary>
        /// When the server closes.
        /// </summary>
        public void onCloseServer()
        {
            SendCommand(new CommandRecievedEventArgs((int)CommandEnum.CloseCommand, null, null));
            closeServer();
        }

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
            m_logging.Log(e.Message + "directory is closed", Logging.Modal.MessageTypeEnum.INFO);
            
        }

        public void ConnectServer()
        {
            Thread.Sleep(100);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);
            TcpListener listener = new TcpListener(ep);

            listener.Start();
            Console.Write("Waiting for connection...");
            Task ta = new Task(() =>
            {
                while (true)
                {
                    try
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        this.client = client;
                        Console.WriteLine("Connection accepted.");
                        NetworkStream ns = client.GetStream();
                        this.ns = client.GetStream();
                        byte[] byteTime = Encoding.ASCII.GetBytes(AppConfigText());
                        ns.Write(byteTime, 0, byteTime.Length);
                        if (!client.Connected) { break; }
                        string command = readSocket();
                        string []split = command.Split('$');
                        int id = 0;
                        bool t = true;
                        if (split[0] == "CloseCommand")
                        {
                            id = (int)CommandEnum.CloseCommand;
                            delete_handler(split[1]);
                            m_controller.ExecuteCommand(id, split, out t);
                        }
                        if (!client.Connected) { break; }
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

              //  listener.Stop();
            } );
            ta.Start();
            tcpListener = listener;
            //ta.Wait();
            //listener.Stop();
            
        }
        public void closeServer()
        {
            tcpListener.Stop();   
            /*
            ns.Close();
            client.Close();*/
        }

        public void writeSocket(string write) {
            byte[] byteTime = Encoding.ASCII.GetBytes(write);
            ns.Write(byteTime, 0, byteTime.Length);
        }

        public string readSocket()
        {
            byte[] bytes = new byte[1024];
            if (!client.Connected) { return "cc"; }

            int bytesRead = ns.Read(bytes, 0, bytes.Length);

            return Encoding.ASCII.GetString(bytes, 0, bytesRead);
        }

        public string AppConfigText()
        {
            List<String> handlers = new List<string>();
            string eventSourceName = ConfigurationManager.AppSettings.Get("SourceName");
            string logName = ConfigurationManager.AppSettings.Get("LogName");
            string outputDir = ConfigurationManager.AppSettings.Get("OutputDir");
            string handler= ConfigurationManager.AppSettings.Get("Handler");
            int ThumbnailSize = int.Parse(ConfigurationManager.AppSettings.Get("ThumbnailSize"));

            string c = "$";
            string s = "SourceName=" + eventSourceName + c + "LogName=" + logName + c + "OutputDir=" +
                outputDir + c + "ThumbSize=" + ThumbnailSize + c + "Handler=" + handler + c;
            int i = 1;
            while(true)
            {
                string f = "Handler" + i;
                string b = ConfigurationManager.AppSettings.Get(f);
                if (!String.IsNullOrEmpty(b))
                {
                    if (b != " ")
                    {
                        s = s + f + "=" + ConfigurationManager.AppSettings.Get(f) + c;
                    }
                    i++;
                } else { break; }
            }
            return s;
        }

        public void delete_handler(string path)
        {
            int i = 0;

            while(true) {

                if(i > 0) {
                    string g = ConfigurationManager.AppSettings.Get("Handler" + i);
                    if (g == path)
                    {
                        ConfigurationManager.AppSettings.Set("Handler" + i, " ");
                        break;
                    }
                }
                else {
                    string g = ConfigurationManager.AppSettings.Get("Handler");
                    if (g == path)
                    {
                        ConfigurationManager.AppSettings.Set("Handler", " ");

                        break;
                    }
                }
                i++;
            }
        }
    }
}
