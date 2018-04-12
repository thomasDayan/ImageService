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

namespace ImageService.Modal
{
    public class ImageServiceModal : IImageServiceModal
    {
        #region Members
        private string m_OutputFolder;            // The Output Folder
        private int m_thumbnailSize;              // The Size Of The Thumbnail Size
        #endregion

        public ImageServiceModal(string m, int size)
        {
            this.m_OutputFolder = m;
            this.m_thumbnailSize = size;
        }

        public string AddFile(string path, out bool result)
        {
            if (File.Exists(path))
            {
                try
                {
                    DateTime dt = new DateTime();
                    dt = File.GetCreationTime(path);

                    string day = dt.Day.ToString();
                    string month = dt.Month.ToString();
                    string year = dt.Year.ToString();

                    Directory.CreateDirectory(m_OutputFolder);
                    // create the thumbnails folder
                    Directory.CreateDirectory(m_OutputFolder + "\\Thumbnails");

                    string year_month_Path = "\\" + year + "\\" + month;
                    Directory.CreateDirectory(m_OutputFolder + year_month_Path);
                    Directory.CreateDirectory(m_OutputFolder + "\\Thumbnails" + year_month_Path);

                    // copy the file if not exist already
                    if (!File.Exists(m_OutputFolder + year_month_Path + "\\" + Path.GetFileName(path)))
                    {
                        File.Copy(path, m_OutputFolder + year_month_Path + "\\" + Path.GetFileName(path));
                        Image image = Image.FromFile(path);
                        Image thumbnail_image = image.GetThumbnailImage(this.m_thumbnailSize, this.m_thumbnailSize, () => false, IntPtr.Zero);
                        thumbnail_image.Save(m_OutputFolder + "\\" + "Thumbnails" + year_month_Path + "\\" + Path.GetFileName(path));
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

    }
}
