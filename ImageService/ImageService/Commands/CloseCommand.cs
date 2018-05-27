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
    /// <summary>
    /// The close command class.
    /// </summary>
    public class CloseCommand : ICommand
    {
        private IImageServiceModal model;
        Dictionary<string, IDirectoryHandler> handlers;
        /// <summary>
        /// apply When execute function.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="result">The result of the arguments.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Constractor.
        /// </summary>
        /// <param name="model">The image service model.</param>
        /// <param name="handlers">The directory handler. </param>
        public CloseCommand(IImageServiceModal model , Dictionary<string, IDirectoryHandler> handlers)
        {
            this.model = model;
            this.handlers = handlers;
        }
    }
}
