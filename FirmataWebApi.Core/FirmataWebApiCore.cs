using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy.Hosting.Self;
using System.Configuration;

namespace FirmataWebApi.Core
{
    public class FirmataWebApiCore : IDisposable
    {
    	#region IDisposable Members

		~FirmataWebApiCore()
		{
			Dispose(false);
		}
		
        /// <summary>
        /// Internal variable which checks if Dispose has already been called
        /// </summary>
        private Boolean disposed;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(Boolean disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                host.Dispose();
            }
            //TODO: Unmanaged cleanup code here

            disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Call the private Dispose(bool) helper and indicate 
            // that we are explicitly disposing
            this.Dispose(true);

            // Tell the garbage collector that the object doesn't require any
            // cleanup when collected since Dispose was called explicitly.
            GC.SuppressFinalize(this);
        }

        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("AppCore");
        private NancyHost host;

        public FirmataWebApiCore()
        {
            var apiPort = ConfigurationManager.AppSettings["ApiPort"] ?? "8000";
            var urlBase = ConfigurationManager.AppSettings["UrlBase"] ?? "";
            var uri = new Uri("http://localhost:" + apiPort + urlBase );
            log.Info("Hosting service at: " + uri);

            host = new NancyHost(new HostConfiguration { UrlReservations = new UrlReservations() { CreateAutomatically = true } }, uri);
        }

        public void Start()
        {
            host.Start();
        }

        public void Stop()
        {
            host.Stop();
            FirmataNancy.Arduino.Dispose();
        }
    }
}
