
using ImageService.Logging.Modal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageService.Logging
{
    public class LoggingService : ILoggingService
    {
        public event EventHandler<MessageRecievedEventArgs> MessageRecieved;
        ObservableCollection<MessageRecievedEventArgs> list;
        private Mutex mutex = new Mutex();

        /// <summary>
        /// Writes to the log.
        /// </summary>
        /// <param name="message"> Message. </param>
        /// <param name="type"> Message type. </param>
        public void Log(string message, MessageTypeEnum type)
        {
            mutex.WaitOne();

            MessageRecievedEventArgs msg = new MessageRecievedEventArgs();
            msg.Message = message + " " + DateTime.Now.ToString();
            msg.Status = type;
            MessageRecieved?.Invoke(this, msg);
            list.Add(msg);

            mutex.ReleaseMutex();

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoggingService()
        {
            list = new ObservableCollection<MessageRecievedEventArgs>();
        }
        /// <summary>
        /// Get the list of the list.
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<MessageRecievedEventArgs> listOfLoggins()
        {
            return list;
        }

    }
}
