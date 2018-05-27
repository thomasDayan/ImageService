using ImageService.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Commands
{
    /// <summary>
    /// Interface of command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// function of Execute the command.
        /// </summary>
        /// <param name="args">Arguments of the function.</param>
        /// <param name="result">The result of the execute.</param>
        /// <returns></returns>
        string Execute(string[] args, out bool result);          // The Function That will Execute The 
    }
}
