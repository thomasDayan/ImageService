using ImageService.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ImageService.Controller.Handlers;

namespace ImageService.Modal
{
    public class ImageServiceModal : IImageServiceModal
    {
        #region Members
        private string m_OutputFolder;            // The Output Folder
        private int m_thumbnailSize;              // The Size Of The Thumbnail Size
        #endregion

        /// <summary>
        /// Constructor for ImageServiceModal.
        /// </summary>
        /// <param name="m"> Output folder path. </param>
        /// <param name="size"> Thumbnail size. </param>
        public ImageServiceModal(string m, int size)
        {
            this.m_OutputFolder = m;
            this.m_thumbnailSize = size;
        }

        /// <summary>
        /// Adds the file to all relevant directories.
        /// </summary>
        /// <param name="path"> To the picture. </param>
        /// <param name="result"> Wether the adding worked. </param>
        /// <returns></returns>
        public string AddFile(string path, out bool result)
        {
            if (File.Exists(path))
            {
                try
                {
                    if (!Directory.Exists(m_OutputFolder))
                    {
                        DirectoryInfo outputDirectory = Directory.CreateDirectory(m_OutputFolder);
                        // Setting the output directory to be hidden
                        outputDirectory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                    }

                    DateTime dt = new DateTime();
                    dt = File.GetCreationTime(path);
                    
                    string month = dt.Month.ToString();
                    string year = dt.Year.ToString();
                    
                    // Create the thumbnails folder
                    Directory.CreateDirectory(m_OutputFolder + "\\Thumbnails");

                    string year_month_Path = "\\" + year + "\\" + month;
                    Directory.CreateDirectory(m_OutputFolder + year_month_Path);
                    Directory.CreateDirectory(m_OutputFolder + "\\Thumbnails" + year_month_Path);

                    // Move the file if not exist already
                    if (!File.Exists(m_OutputFolder + year_month_Path + "\\" + Path.GetFileName(path)))
                    {
                        File.Move(path, m_OutputFolder + year_month_Path + "\\" + Path.GetFileName(path));
                        Image image = Image.FromFile(path);
                        Image thumbnail_image = image.GetThumbnailImage(this.m_thumbnailSize, 
                            this.m_thumbnailSize, () => false, IntPtr.Zero);
                        thumbnail_image.Save(m_OutputFolder + "\\" + "Thumbnails" + year_month_Path + 
                            "\\" + Path.GetFileName(path));
                        image.Dispose();
                    }
                    result = true;
                    return m_OutputFolder + year_month_Path;
                }
                catch (Exception e)
                {
                    result = false;
                    throw e;
                }
            }
            else
            {
                result = false;
                return "File doesn't exist!";
            }
        }

        public string CloseHandler(IDirectoryHandler handler , out bool result)
        {
            try
            {
                handler.Watcher.EnableRaisingEvents = false;
                handler.Watcher.Created -= new FileSystemEventHandler(handler.Create);
                handler.InvokeCloseEvent();
                result = true;
            } catch (Exception e) { result = false; }
            return "success";
        }
    }
}
