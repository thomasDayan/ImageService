using ImageService.Commands;
using ImageService.Controller.Handlers;
using ImageService.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Commands
{
    /*
     * 
     * need to implemanet this shit 
     * 
     * 
     * 
     * 
     * */


    public class CloseCommand : ICommand
    {
        private IImageServiceModal model;
        Dictionary<string, IDirectoryHandler> handlers;
        public string Execute(string[] args, out bool result)
        {
            result = true;
            try
            {
                if (args == null || args.Length == 0) { throw new Exception("invalid args");  }
                IDirectoryHandler handler = handlers[args[1]];
                string message = model.CloseHandler(handler, out result);
            } catch (Exception e) { result = false; }

            return "ok";
        }

        public CloseCommand(IImageServiceModal model , Dictionary<string, IDirectoryHandler> handlers)
        {
            this.model = model;
            this.handlers = handlers;
        }
    }
}
