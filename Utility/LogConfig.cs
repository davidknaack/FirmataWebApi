using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using log4net.Appender;

namespace AC0KG.Utils.Log
{
    public static class LogConfig
    {
        /// <summary>
        /// Configure log4net logging system, must be called before logging will work.
        /// </summary>
        /// <param name="exe">Name of exe doing the logging, without file extension. 
        /// Used to name the log file. Navigate to the function for example code to get this.</param>
        public static void ConfigureLogger(string exe, string logDir = "", string configDir = "")
        {
            // Here's a typical way to call this
            //  string exe = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            //  LogConfig.ConfigureLogger(exe);                

            if (string.IsNullOrEmpty(logDir))
                logDir = Path.GetDirectoryName(exe);
            if (string.IsNullOrEmpty(configDir))
                configDir = Path.GetDirectoryName(exe);

            log4net.Config.XmlConfigurator.Configure(new FileInfo(Path.Combine(configDir, "log4net.config")));

            var log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            foreach (var a in log.Logger.Repository.GetAppenders().Where(a => a is FileAppender).Select(a => (FileAppender)a))
            {
                // Point file appendeders to the log file directory and set the name to the current EXE name
                // Todo: seems like there should be a good way to make this automatic with the config file.
                a.File = Path.Combine(logDir, string.Format("{0}.txt", exe));
                a.ActivateOptions();
            }
        }
    }

    public class Log4netTraceListener : System.Diagnostics.TraceListener
    {
        private readonly log4net.ILog _log;

        public Log4netTraceListener()
        {
            _log = log4net.LogManager.GetLogger("System.Diagnostics.Redirection");
        }

        public Log4netTraceListener(log4net.ILog log)
        {
            _log = log;
        }

        public override void Write(string message)
        {
            if (_log != null)
            {
                _log.Debug(message);
            }
        }

        public override void WriteLine(string message)
        {
            if (_log != null)
            {
                _log.Debug(message);
            }
        }
    }
}
