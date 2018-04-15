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

namespace ImageService.Server
{
    public class ImageServer
    {
        #region Members
        private IImageController m_controller;
        private ILoggingService m_logging;
        #endregion

        #region Properties
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;          // The event that notifies about a new Command being recieved
        #endregion

        /// <summary>
        /// Constructor for ImageServer.
        /// </summary>
        /// <param name="m"> Image Controller. </param>
        /// <param name="n"> Logging Service. </param>
        public ImageServer(IImageController m , ILoggingService n)
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
    }
}
