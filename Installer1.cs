using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace File_Monitoring
{
    [RunInstaller(true)]
    public partial class Installer1 : Installer
    {
        public Installer1()
        {
            InitializeComponent();

            ServiceInstaller ServiceInstaller = new ServiceInstaller
            {
                ServiceName = "FileMonitoringService",
                StartType = ServiceStartMode.Automatic,
                DisplayName = "File Monitoring Service",
                ServicesDependedOn = new string[] { "RpcSs", "EventLog", "LanmanWorkstation" } // Dependencies
            };

            ServiceProcessInstaller ServiceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalService,
            };

            Installers.Add(ServiceProcessInstaller);
            Installers.Add(ServiceInstaller);
        }
    }
}
