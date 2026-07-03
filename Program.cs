using System;
using System.ServiceProcess;

namespace File_Monitoring
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                FileMonitoringService CMFileMonitoring = new FileMonitoringService();
                CMFileMonitoring.DebugMode();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new FileMonitoringService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
