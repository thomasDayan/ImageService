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

namespace ImageService.Server
{
    public class ImageServer
    {
        #region Members
        private IImageController m_controller;
        private ILoggingService m_logging;
        private NetworkStream ns;
        private TcpClient client;
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
        }

        /// <summary>
        /// When the server closes.
        /// </summary>
        public void onCloseServer()
        {
            SendCommand(new CommandRecievedEventArgs((int)CommandEnum.CloseCommand, null, null));
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
            bool done = false;

            TcpListener listener = new TcpListener(8888);

            listener.Start();
            while (!done)
            {
                try
                {
                    Console.Write("Waiting for connection...");
                    TcpClient client = listener.AcceptTcpClient();
                    this.client = client;
                    Console.WriteLine("Connection accepted.");
                    NetworkStream ns = client.GetStream();
                    this.ns = client.GetStream();
                    byte[] byteTime = Encoding.ASCII.GetBytes(DateTime.Now.ToString());
                    ns.Write(byteTime, 0, byteTime.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }


            listener.Stop();
        }
        public void closeServer()
        {
            ns.Close();
            client.Close();
        }

        public void writeSocket(string write) {
            byte[] byteTime = Encoding.ASCII.GetBytes(write);
            ns.Write(byteTime, 0, byteTime.Length);
        }

        public String readSocket()
        {
            byte[] bytes = new byte[1024];
            int bytesRead = ns.Read(bytes, 0, bytes.Length);

            return Encoding.ASCII.GetString(bytes, 0, bytesRead);
        }

        public String AppConfigText()
        {
            List<String> handlers = new List<string>();
            string eventSourceName = ConfigurationManager.AppSettings.Get("SourceName");
            string logName = ConfigurationManager.AppSettings.Get("LogName");
            string outputDir = ConfigurationManager.AppSettings.Get("OutputDir");
            handlers.Add( ConfigurationManager.AppSettings.Get("Handler"));
            int ThumbnailSize = int.Parse(ConfigurationManager.AppSettings.Get("ThumbnailSize"));
            /*
             * we need to add the handlers !!!!!!! 
             * */
            char c = (char)32;
            string s = "SourceName=" + eventSourceName + c + "LogName=" + logName + c + "OutputDir=" +
                outputDir + c + "ThumbSize=" + ThumbnailSize + c;

            return s;
        }
    }
}
