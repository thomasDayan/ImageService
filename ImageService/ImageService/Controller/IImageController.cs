using ImageService.Controller.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Controller
{
    public interface IImageController
    {
        void setDirectory(Dictionary<string, IDirectoryHandler> handlers);
        string ExecuteCommand(int commandID, string[] args, out bool result);          // Executing the Command Requet
    }
}
