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

        public ImageServer(IImageController m , ILoggingService n)
        {
            m_controller = m;
            m_logging = n;

        
        }
       
        public void CreateHandler(string path)
        {
            IDirectoryHandler directoryHandler = new DirectoryHandler(m_controller, m_logging);
            // add to events
            
            CommandRecieved += directoryHandler.OnCommandRecieved;
            // start the handler
            directoryHandler.StartHandleDirectory(path);
        }

        public void onCloseServer(object sender, DirectoryCloseEventArgs e)
        {
            DirectoryHandler handler = (DirectoryHandler)sender;
            CommandRecieved -= handler.OnCommandRecieved;
        }

        public void SendCommand(CommandRecievedEventArgs e)
        {
            CommandRecieved?.Invoke(this, e);
        }
    }
}
