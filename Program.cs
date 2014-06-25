namespace AC0KG.FirmataWebApi
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using AC0KG.Utils.Log;
    using AC0KG.WindowsService;
    using System.ComponentModel;
    using Nancy.Hosting.Self;
    using System.Configuration;

    // This attribute tells Visual Studio to not use the designer for this file.
    [System.ComponentModel.DesignerCategory("")]
    [ServiceName("FirmataWebApi")]
    class Service : ServiceShell {}

    [RunInstaller(true)]
    [ServiceName("FirmataWebApi", DisplayName = "Firmata Web API", Description = "Web API bridge for Arduino Firmata")]
    public class Installer : InstallerShell {}

    class Program 
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("AppShell");
       
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new Log4netTraceListener());
            LogConfig.ConfigureLogger(Assembly.GetExecutingAssembly().Location);

            if (ServiceShell.ProcessInstallOptions(args))
                return;

            if (Environment.UserInteractive)
                Console.SetWindowSize(Math.Min(120, Console.LargestWindowWidth), Math.Min(20, Console.LargestWindowHeight));

            var apiPort = ConfigurationManager.AppSettings["ApiPort"] ?? "8000";
            var urlBase = ConfigurationManager.AppSettings["UrlBase"] ?? "";
            var uri = new Uri("http://localhost:" + apiPort + urlBase );
            log.Info("Hosting service at: " + uri);

            using (var host = new NancyHost(new HostConfiguration { UrlReservations = new UrlReservations() { CreateAutomatically = true } }, uri))
            {
                Service.StartService<Service>(
                    host.Start,
                    () => { host.Stop(); FirmataWebApi.Arduino.Dispose(); },
                    Environment.UserInteractive);
            }
        }
    }
}
