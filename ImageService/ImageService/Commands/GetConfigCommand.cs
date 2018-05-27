using ImageService.Controller;
using ImageService.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Commands
{
    public class GetConfigCommand : ICommand
    {
        private ImageServer imageServer;
        private TcpClient tcpClient;
        private IImageController im_c;
        /// <summary>
        /// Constractor.
        /// </summary>
        /// <param name="image">The image server </param>
        /// <param name="tcp">The tcp client.</param>
        /// <param name="im">The image contoller.</param>
        public GetConfigCommand(ImageServer image , TcpClient tcp, IImageController im )
        {
            imageServer = image;
            tcpClient = tcp;
            im_c = im;
        }
        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="args">the arguments.</param>
        /// <param name="result">the result if succes or neither.</param>
        /// <returns></returns>
        public string Execute(string[] args, out bool result)
        {
            try
            {
                HandleSettings h = new HandleSettings(imageServer, tcpClient, im_c);
                result = true;
                return "success";
            } catch (Exception e)
            {
                result = false;
                return "failed";
            }
        }
    }
}
