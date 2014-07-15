using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy.Hosting.Self;
using System.Configuration;
using SuperWebSocket;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Threading;
using Sharpduino.Constants;

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
        private WebSocketServer webSocketServer;
        private List<WebSocketSession> webSocketSessions = new List<WebSocketSession>();
        private CancellationTokenSource cts = new CancellationTokenSource();
        private SuperSocket.SocketEngine.SocketServerFactory ssf; // TODO: adjust .Posix and .Win so SocketEngine goes in bin without this.
        public static int ApiPort; // TODO: cache this setting somewhere that FirmataNancy can use it

        public FirmataWebApiCore()
        {
            FirmataWebApiCore.ApiPort = int.Parse(ConfigurationManager.AppSettings["ApiPort"] ?? "8000");
            var urlBase = ConfigurationManager.AppSettings["UrlBase"] ?? "";
            var uri = new Uri("http://localhost:" + FirmataWebApiCore.ApiPort.ToString() + urlBase);

            log.Info("Hosting service at: " + uri);

            host = new NancyHost(new HostConfiguration { UrlReservations = new UrlReservations() { CreateAutomatically = true } }, uri);

            webSocketServer = new WebSocketServer();
            webSocketServer.Setup(new RootConfig(),
                new ServerConfig()
                {
                    Name = "FirmataWebSocket",
                    Ip = "Any",
                    Port = FirmataWebApiCore.ApiPort + 2,                    
                    Mode = SuperSocket.SocketBase.SocketMode.Tcp
                });
            webSocketServer.NewSessionConnected += new SuperSocket.SocketBase.SessionHandler<WebSocketSession>(webSocketServer_NewSessionConnected);
            webSocketServer.SessionClosed += new SuperSocket.SocketBase.SessionHandler<WebSocketSession, SuperSocket.SocketBase.CloseReason>(webSocketServer_SessionClosed);
        }

        void webSocketServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            log.Debug("removing web socket session : " + value.ToString());
            lock (webSocketSessions)
                webSocketSessions.Remove(session);
        }

        void webSocketServer_NewSessionConnected(WebSocketSession session)
        {
            log.Debug("new web socket session :" + session.Origin);
            lock (webSocketSessions)
                webSocketSessions.Add(session);
        }

        void sendPinValue(int pin, int value)
        {
            lock (webSocketSessions)
                foreach (var ses in webSocketSessions)
                    ses.Send(JObject.FromObject(new { pin, value }).ToString());
        }

        public void Start()
        {
            log.Debug("starting websocket server");
            webSocketServer.Start();
            log.Debug("starting Nancy server");
            host.Start();

            Task.Factory.StartNew(() =>
            {
                while (!cts.Token.WaitHandle.WaitOne(500))
                    lock (webSocketSessions)
                        foreach (var pin in FirmataNancy.Arduino.Pins.Where(p => (p.PinNo > 1) && ((p.CurrentMode == PinModes.Analog) || (p.CurrentMode == PinModes.Input))).ToList())
                            foreach (var ses in webSocketSessions)
                                ses.Send(JObject.FromObject(new { pin=pin.PinNo, mode=pin.CurrentMode.ToString(), value=pin.CurrentValue }).ToString());
            }, cts.Token);
        }

        public void Stop()
        {
            log.Debug("stopping Nancy server");
            host.Stop();
            log.Debug("stopping websocket server");
            webSocketServer.Stop();
            FirmataNancy.Arduino.Dispose();
        }
    }
}
