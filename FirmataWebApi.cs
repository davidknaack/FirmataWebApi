using System;
using Nancy;
using Sharpduino;
using System.Configuration;
using System.Diagnostics;

// https://github.com/davidknaack/sharpduino
// http://www.codeproject.com/Articles/694907/Lift-your-Petticoats-with-Nancy

namespace AC0KG.FirmataWebApi
{
    /// <summary>
    /// Web interfaces for access to the hardware
    /// </summary>
    public class FirmataWebApi : NancyModule
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("AppShell");
        
        private static ArduinoUno _arduino;
        public static ArduinoUno Arduino
        {
            get
            {
                lock (log)
                {
                    if (_arduino == null)
                    {
                        var port = ConfigurationManager.AppSettings["CommPort"];
                        if (port == null)
                            Trace.TraceError("App.Config 'CommPort' setting is invalid or missing");
                        try
                        {
                            Trace.WriteLine("Opening Arduino on port " + port);
                            _arduino = new ArduinoUno(port);
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError("Error initializing Arduino connection: " + ex.Message);
                            throw;
                        }
                    }
                    return _arduino;
                }
            }
        }

        public FirmataWebApi()
        {
            // Reference Arduino as early as possible. After creation Firmata does some 
            // setup comms that must complete before it can be used.
            var arduino = Arduino;

            // Set a pin mode
            Get["/pm/{pinIdx:range(2,19)}/{pinMode}"] = parms =>
            {
                try
                {
                    log.DebugFormat("set pin {0} mode {1} from {2}", parms.pinIdx, parms.pinMode, Request.UserHostAddress);
                    Arduino.PinMode((int)parms.pinIdx, (string)parms.pinMode);
                    return Response.AsJson(new { 
                        Status = "OK", 
                        PinNum = parms.pinIdx, 
                        Mode = Arduino.GetCurrentPinState((int)parms.pinIdx).CurrentMode.ToString().ToLower() });
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("set pin mode failed: {0}", ex.Message);
                    return Response.AsJson(new { Status = ex.Message }, HttpStatusCode.InternalServerError);
                }
            };

            // Get Digital Pin status
            Get["/dr/{pinIdx:range(2,19)}"] = parms =>
            {
                try
                {
                    log.DebugFormat("get dpin {0} from {1}", parms.pinIdx, Request.UserHostAddress);
                    return Response.AsJson(new
                    {
                        Status = "OK",
                        PinNum = parms.pinIdx,
                        Value = Arduino.DigitalRead((int)parms.pinIdx)
                    });
                }
                catch ( Exception ex )
                {
                    log.ErrorFormat("get dpin value failed: {0}", ex.Message);
                    return Response.AsJson(new { Status = ex.Message });
                }
            };

            // Set Digital Pin status
            Get["/dw/{pinIdx:range(2,19)}/{pinVal:range(0,1)}"] = parms =>
            {
                try
                {
                    log.DebugFormat("set dpin {0} value {1} from {2}", parms.pinIdx, parms.pinVal, Request.UserHostAddress);
                    Arduino.DigitalWrite(parms.pinIdx, ((int)parms.pinVal) != 0);
                    return Response.AsJson(new
                    {
                        Status = "OK",
                        PinNum = parms.pinIdx,
                        Value = Arduino.DigitalRead((int)parms.pinIdx)
                    });
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("set dpin value failed: {0}", ex.Message);
                    return Response.AsJson(new { Status = ex.Message });
                }
            };

            // Get Analog Pin status
            Get["/ar/{pinIdx:range(0,5)}"] = parms =>
            {
                try
                {
                    log.DebugFormat("get apin {0} from {1}", parms.pinIdx, Request.UserHostAddress);
                    return Response.AsJson(new
                    {
                        Status = "OK",
                        PinNum = parms.pinIdx,
                        Value = Arduino.AnalogRead(parms.pinIdx)
                    });
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("get apin value failed: {0}", ex.Message);
                    return Response.AsJson(new { Status = ex.Message });
                }
            };

            // Set Analog Pin status : 3,5,6,9,10,11
            Get["/aw/{pinIdx:range(3,11)}/{pinVal:range(0,255)}"] = parms =>
            {
                try
                {
                    log.DebugFormat("set apin {0} value {1} from {2}", parms.pinIdx, parms.pinVal, Request.UserHostAddress);
                    Arduino.AnalogWrite(parms.pinIdx, parms.pinVal);
                    return Response.AsJson(new
                    {
                        Status = "OK",
                        PinNum = parms.pinIdx,
                        Value = parms.pinVal
                    });
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("set apin value failed: {0}", ex.Message);
                    return Response.AsJson(new { Status = ex.Message });
                }
            };
        }
    }
}
