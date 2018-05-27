using ImageService.Logging;
using ImageService.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Commands
{
    public class LogCommand : ICommand
    {
        private HandleLogs handle;
        private ImageServer im_s;
        private ILoggingService loggingService;
        private TcpClient tcp;
        /// <summary>
        /// Constractor.
        /// </summary>
        /// <param name="handle">The handle log.</param>
        /// <param name="imageServer">The image server.</param>
        /// <param name="ll">the lloging sevice.</param>
        /// <param name="client">the tcp client.</param>
        public LogCommand(HandleLogs handle,ImageServer imageServer , ILoggingService ll , TcpClient client) {
            this.handle = handle;
            im_s = imageServer;
            loggingService = ll;
            tcp = client;
        }
        /// <summary>
        /// The execute the command.
        /// </summary>
        /// <param name="args">arguments.</param>
        /// <param name="result">success or not.</param>
        /// <returns></returns>
        public string Execute(string[] args, out bool result)
        {
            try
            {
                handle.SetAllProt(im_s, loggingService, tcp);
                result = true;
                return "not success";

            }
            catch (Exception e)
            {
                result = false;
                return "not success";
            }
        }
    }
}
