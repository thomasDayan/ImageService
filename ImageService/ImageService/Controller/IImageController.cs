using ImageService.Controller.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Controller
{
    /// <summary>
    /// The interface of the image controller.
    /// </summary>
    public interface IImageController
    {
        void setDirectory(Dictionary<string, IDirectoryHandler> handlers);
        string ExecuteCommand(int commandID, string[] args, out bool result);          // Executing the Command Requet
    }
}
