using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using AC0KG.Utils.Log;
using System.Reflection;
using Mono.Unix;
using Mono.Unix.Native;

namespace FirmataWebApi.Posix
{
    // https://github.com/ServiceStack/ServiceStack/wiki/Run-ServiceStack-as-a-daemon-on-Linux

    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("AppShell");

        static void Main(string[] args)
        {
            Trace.Listeners.Add(new Log4netTraceListener());
            LogConfig.ConfigureLogger(Assembly.GetExecutingAssembly().Location);

            using (var appCore = new FirmataWebApi.Core.FirmataWebApiCore())
            {
                appCore.Start();
                
                var signals = new UnixSignal[] { 
                    new UnixSignal(Signum.SIGINT), 
                    new UnixSignal(Signum.SIGTERM), 
                };

                for (var exit = false; !exit; )
                {
                    var id = UnixSignal.WaitAny(signals);

                    if ((id >= 0) && (id < signals.Length))
                        if (signals[id].IsSet) 
                            exit = true;
                }

                appCore.Stop();
                System.Environment.Exit(0);
            }
        }
    }
}
