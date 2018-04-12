using ImageService.Modal;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageService.Infrastructure;
using ImageService.Infrastructure.Enums;
using ImageService.Logging;
using ImageService.Logging.Modal;
using System.Text.RegularExpressions;

namespace ImageService.Controller.Handlers
{
    public class DirectoryHandler : IDirectoryHandler
    {
        #region Members
        private IImageController m_controller;              // The Image Processing Controller
        private ILoggingService m_logging;
        private FileSystemWatcher m_dirWatcher;             // The Watcher of the Dir
        private string m_path;                              // The Path of directory
        #endregion


        public DirectoryHandler(IImageController m , ILoggingService n)
        {
            m_controller = m;
            m_logging = n;
        }

        public event EventHandler<DirectoryCloseEventArgs> DirectoryClose;              // The Event That Notifies that the Directory is being closed

        public void OnCommandRecieved(object sender, CommandRecievedEventArgs e)
        {
            bool result;
            if (e.CommandID == (int) CommandEnum.NewFileCommand)
            {
                if (!e.RequestDirPath.Equals(m_path)) { return; }
                string r = m_controller.ExecuteCommand(e.CommandID, e.Args , out result);
                if (!result)
                {
                    m_logging.Log("Command failed." , MessageTypeEnum.FAIL);
                    return;
                }
                m_logging.Log("Command successful.", MessageTypeEnum.INFO);
            }
            else if (e.CommandID == (int)CommandEnum.CloseCommand)
            {
                m_logging.Log("Command close success", MessageTypeEnum.INFO);
                EndHandler();
                return;
            }
        }
       
        public void StartHandleDirectory(string dirPath)
        {
            m_path = dirPath;
            m_dirWatcher = new FileSystemWatcher(dirPath);
            m_dirWatcher.Created += new FileSystemEventHandler(Create);
            m_dirWatcher.Changed += new FileSystemEventHandler(Create);
            m_dirWatcher.EnableRaisingEvents = true;

        }

        public void Create(object sender, FileSystemEventArgs e)
        {
            bool f;
            string[] correct_files = { ".jpg" , ".png" , ".gif" , ".bmp"};
            string path = Path.GetExtension(e.FullPath).ToLower();
            if (correct_files.Contains(path))
            {
                string[] array_path = { e.FullPath };
                m_controller.ExecuteCommand((int)CommandEnum.NewFileCommand, array_path, out f);
            }
        }

        private void EndHandler()
        {
            m_dirWatcher.EnableRaisingEvents = false;

            m_dirWatcher.Created -= new FileSystemEventHandler(Create);
            m_dirWatcher.Changed -= new FileSystemEventHandler(Create);

            DirectoryCloseEventArgs d = new DirectoryCloseEventArgs(m_path, "Closing directory" + m_path);
            DirectoryClose?.Invoke(this , d);
        }
    }
}
