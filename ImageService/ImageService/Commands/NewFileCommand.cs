using ImageService.Infrastructure;
using ImageService.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Commands
{
    public class NewFileCommand : ICommand
    {
        private IImageServiceModal m_modal;

        public NewFileCommand(IImageServiceModal modal)
        {
            m_modal = modal; // Storing the Modal
        }

        public string Execute(string[] args, out bool result)
        {
            string result_ex = m_modal.AddFile(args[0], out result);
            return result_ex;
        }
    }
}
