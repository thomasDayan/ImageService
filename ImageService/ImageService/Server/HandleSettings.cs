using ImageService.Controller;
using ImageService.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Server
{
    /// <summary>
    /// Class that taking care the setting.
    /// </summary>
    class HandleSettings
    {
        /// <summary>
        /// Constractor.
        /// </summary>
        /// <param name="imageServer">The image server .</param>
        /// <param name="client"> The tcp client. </param>
        /// <param name="m_controller"> The image controller.</param>
        public HandleSettings(ImageServer imageServer , TcpClient client , IImageController m_controller)
        {
            imageServer.writeSocket(imageServer.AppConfigText() , client);
            NetworkStream ns = client.GetStream();
            //string r = imageServer.readSocket(ns);

            if (client.Connected)
            {
                while (true)
                {
                    string command = imageServer.readSocket(ns);
                    string[] split = command.Split('$');
                    int id = 0;
                    bool t = true;

                    if (split[0] == "Exit Server") { break; }
                    if (split[0] == "CloseCommand")
                    {
                        id = (int)CommandEnum.CloseCommand;
                        imageServer.delete_handler(split[1]);
                        m_controller.ExecuteCommand(id, split, out t);
                    }
                    if (split[0] == "Exit Server") { break; }

                    if (!client.Connected) { break; }

                    if (!client.Connected) { break; }
                }
            }
        }
    }
}
