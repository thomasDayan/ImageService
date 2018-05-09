using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImageService.Server;
using ImageService.Controller;
using ImageService.Modal;
using ImageService.Logging;
using ImageService.Logging.Modal;
using System.Configuration;
using ImageService.Infrastructure;
using System.Runtime.Remoting;
using ImageService.Controller.Handlers;

namespace ImageService
{
    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };

    public partial class ImageService : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        private System.ComponentModel.IContainer components;

        private ImageServer m_imageServer;          // The Image Server
		private IImageServiceModal modal;
		private IImageController controller;
        private EventLog eventLog1;
        private ILoggingService logging;
        private int eventId = 1;
        Dictionary<string, IDirectoryHandler> handlers;

        protected override bool CanRaiseEvents => base.CanRaiseEvents;

        public override ISite Site { get => base.Site; set => base.Site = value; }

        public override EventLog EventLog => base.EventLog;

        /// <summary>
        /// Constructor for ImageService.
        /// </summary>
        /// <param name="args"> Arguments. </param>
        public ImageService(string[] args)
        {
            InitializeComponent();
            string eventSourceName = ConfigurationManager.AppSettings.Get("SourceName");
            string logName = ConfigurationManager.AppSettings.Get("LogName");
            string outputDir = ConfigurationManager.AppSettings.Get("OutputDir");
            string handler = ConfigurationManager.AppSettings.Get("Handler");
            int ThumbnailSize = int.Parse(ConfigurationManager.AppSettings.Get("ThumbnailSize"));

            if (args.Count() > 0)
            {
                eventSourceName = args[0];
            }
            if (args.Count() > 1)
            {
                logName = args[1];
            }
            if (eventSourceName == null)
            {
                eventSourceName = "MySource";
            }
            if (logName == null)
            {
                logName = "MyNewLog";
            }

            eventLog1 = new System.Diagnostics.EventLog();
            try {
                if (!System.Diagnostics.EventLog.SourceExists(eventSourceName))
                {
                    System.Diagnostics.EventLog.CreateEventSource(eventSourceName, logName);
                }
            } catch(Exception e)
            {
                return;
            }
            
            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;
            logging = new LoggingService();
            logging.MessageRecieved += MessageTypes;

            // create image service model
            modal = new ImageServiceModal(outputDir, ThumbnailSize);

            // create image controller

            controller = new ImageController(modal);
            m_imageServer = new ImageServer(controller, logging);
            int i = 1;
            handlers = new Dictionary<string, IDirectoryHandler>();
            m_imageServer.setHandlers(handlers);
            while(true) {
                string handlers = ConfigurationManager.AppSettings.Get("Handler" + i);
                if (handlers != " ")
                {
                    if (!String.IsNullOrEmpty(handlers))
                    {
                        m_imageServer.CreateHandler(handlers);
                        i++;
                    }
                    else { break; }
                }
            }
            m_imageServer.CreateHandler(handler);
            controller.setDirectory(handlers);
        }

        public ImageService(IContainer components, ImageServer m_imageServer, IImageServiceModal modal, IImageController controller, EventLog eventLog1, ILoggingService logging, int eventId)
        {
            this.components = components;
            this.m_imageServer = m_imageServer;
            this.modal = modal;
            this.controller = controller;
            this.eventLog1 = eventLog1;
            this.logging = logging;
            this.eventId = eventId;
        }

        public ImageService()
        {
        }

        /// <summary>
        /// Invoked when the service has started.
        /// </summary>
        /// <param name="args"> Arguments. </param>
        protected override void OnStart(string[] args)
        {
            System.Threading.Thread.Sleep(10000);


            eventLog1.WriteEntry("In OnStart");
            // Set up a timer to trigger every minute.  
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60000; // 60 seconds  
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();

            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

        }

        /// <summary>
        /// Monitores the system every time the timer goes off.
        /// </summary>
        /// <param name="sender"> Sender. </param>
        /// <param name="args"> Args. </param>
        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.  
            eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
        }

        /// <summary>
        /// Invoked when the service is stopped.
        /// </summary>
        protected override void OnStop()
        {
            // Update the service state to stop Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(ServiceHandle, ref serviceStatus);

            // write stop to event log
            eventLog1.WriteEntry("In onStop.");

            // close the server
            m_imageServer.onCloseServer();
            
            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }

        /// <summary>
        /// Invoked when the service is continued.
        /// </summary>
        protected override void OnContinue()
        {
            // write continue to event log
            eventLog1.WriteEntry("In OnContinue.");
        }

        /// <summary>
        /// Initializes the event log.
        /// </summary>
        private void InitializeComponent()
        {
            this.eventLog1 = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();

        }

        /// <summary>
        /// Determines the message type, and writes it in the log.
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The event. </param>
        private void MessageTypes(object sender , MessageRecievedEventArgs e)
        {
            EventLogEntryType type;
            switch (e.Status)
            {
                case MessageTypeEnum.INFO: type = EventLogEntryType.Information; break;
                case MessageTypeEnum.WARNING: type = EventLogEntryType.Warning; break;
                case MessageTypeEnum.FAIL: type = EventLogEntryType.Error; break;
                default: type = EventLogEntryType.Information; break;
            }
            eventLog1.WriteEntry(e.Message, type);
            
        }

        public override bool Equals(object obj)
        {
            var service = obj as ImageService;
            return service != null &&
                   EqualityComparer<IContainer>.Default.Equals(components, service.components) &&
                   EqualityComparer<ImageServer>.Default.Equals(m_imageServer, service.m_imageServer) &&
                   EqualityComparer<IImageServiceModal>.Default.Equals(modal, service.modal) &&
                   EqualityComparer<IImageController>.Default.Equals(controller, service.controller) &&
                   EqualityComparer<EventLog>.Default.Equals(eventLog1, service.eventLog1) &&
                   EqualityComparer<ILoggingService>.Default.Equals(logging, service.logging) &&
                   eventId == service.eventId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override object InitializeLifetimeService()
        {
            return base.InitializeLifetimeService();
        }

        public override ObjRef CreateObjRef(Type requestedType)
        {
            return base.CreateObjRef(requestedType);
        }

        protected override object GetService(Type service)
        {
            return base.GetService(service);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return base.OnPowerEvent(powerStatus);
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        protected override void OnCustomCommand(int command)
        {
            base.OnCustomCommand(command);
        }
    }
}
