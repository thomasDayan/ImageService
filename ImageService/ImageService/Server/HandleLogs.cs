using ImageService.Logging;
using ImageService.Logging.Modal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Server
{
    /// <summary>
    /// Function that handle with the logs .
    /// </summary>
    public class HandleLogs : INotifyPropertyChanged
    {
        private TcpClient tCP;

        private int i = 0;
        private ImageServer imageServer;
        private ILoggingService loggingService;
        private ObservableCollection<MessageRecievedEventArgs> sended_logs;
        private ObservableCollection<MessageRecievedEventArgs> path_list;

        public ObservableCollection<MessageRecievedEventArgs> ListLogg
        { get { return path_list; } set { path_list = loggingService.listOfLoggins(); } }
        /// <summary>
        /// Function that apply when the list changed.
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="e"> </param>
        public void collectionChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            onPropertyChanged("ListLogg");

        }
        /// <summary>
        /// Set function.
        /// </summary>
        /// <param name="imageServer1">The image server.</param>
        /// <param name="loggingService1">The logging service.</param>
        /// <param name="tCP1">The tcp network stream.</param>
        public void SetAllProt(ImageServer imageServer1, ILoggingService loggingService1, TcpClient tCP1)
        {
            this.tCP = tCP1;
            this.imageServer = imageServer1;
            this.loggingService = loggingService1;
            sended_logs = new ObservableCollection<MessageRecievedEventArgs>();

            ListLogg = loggingService.listOfLoggins();
            ListLogg.CollectionChanged += collectionChange;

            //add all the logs message to the application.
            //send it by seperate by $.
            foreach (MessageRecievedEventArgs m in path_list.ToList())
            {
                string b = m.Message;
                string c = m.Status.ToString();
                string g = "$";
                string colour = "";
                if (c == "INFO")
                {
                    colour = "Green";
                }
                if (c == "ERROR")
                {
                    colour = "Red";
                }
                if (c == "WARNING")
                {
                    colour = "Yello";
                }
                imageServer.writeSocket("colour=" + colour + g + "log=" + c + g + "message="
                    + b + g, tCP1);
                i++;
                sended_logs.Add(m);
            }
        }
        /// <summary>
        /// Constractor.
        /// </summary>
        public HandleLogs() { }
        
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        /// <summary>
        /// Function that send log to client.
        /// </summary>
        /// <param name="g"> Message of the log. </param>
        public void SendLog(MessageRecievedEventArgs g)
        {
            try
            {
                //send all the new messages.
                foreach (MessageRecievedEventArgs m in path_list.ToList())
                {
                    if (!sended_logs.Contains(m))
                    {
                        string b = m.Message;
                        string c = m.Status.ToString();
                        string f = "$";
                        string colour = "";
                        if (c == "INFO")
                        {
                            colour = "Green";
                        }
                        if (c == "ERROR")
                        {
                            colour = "Red";
                        }
                        if (c == "WARNING")
                        {
                            colour = "Yello";
                        }
                        imageServer.writeSocket("colour=" + colour + f + "log=" + c + f + "message="
                            + b + f, tCP);

                        sended_logs.Add(m);
                    }

                }
            }
            catch (Exception e)
            {

            }
        }
        /// <summary>
        /// Taking care when the list changed.
        /// </summary>
        /// <param name="func">function we need to run for making the change.</param>
        public void onPropertyChanged(string func)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(func));
            SendLog(null);
        }

    }
}
