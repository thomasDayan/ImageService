using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageService.Controller.Handlers;

namespace ImageService.Modal
{
    public interface IImageServiceModal
    {
        /// <summary>
        /// The Function Addes A file to the system
        /// </summary>
        /// <param name="path">The Path of the Image from the file</param>
        /// <returns>Indication if the Addition Was Successful</returns>
        string AddFile(string path, out bool result);
        /// <summary>
        /// Close handler command.
        /// </summary>
        /// <param name="handler">The directory.</param>
        /// <param name="result">The result of the act.</param>
        /// <returns></returns>
        string CloseHandler(IDirectoryHandler handler, out bool result);
    }
}
