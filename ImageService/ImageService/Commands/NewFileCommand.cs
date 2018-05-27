using ImageService.Infrastructure;
using ImageService.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Commands
{
    /// <summary>
    /// New file command class.
    /// </summary>
    public class NewFileCommand : ICommand
    {
        private IImageServiceModal m_modal;

        /// <summary>
        /// Constructor for NewFileCommand.
        /// </summary>
        /// <param name="modal"> Image service modal. </param>
        public NewFileCommand(IImageServiceModal modal)
        {
            m_modal = modal; // Storing the Modal
        }

        /// <summary>
        /// Executes the add file command.
        /// </summary>
        /// <param name="args"> Arguments. </param>
        /// <param name="result"> Determines wether the adding worked. </param>
        /// <returns></returns>
        public string Execute(string[] args, out bool result)
        {
            string result_ex = m_modal.AddFile(args[0], out result);
            return result_ex;
        }
    }
}
