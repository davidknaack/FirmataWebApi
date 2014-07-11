using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirmataWebApi.Core;
using System.Dynamic;

namespace FirmataWebApi.Core.Models
{
    public class ControlModel
    {
        public IEnumerable<dynamic> Pins
        {
            get
            {                
                var r = from pin in FirmataNancy.Arduino.Pins.Skip(2)
                        select new
                        {
                           Num = pin.PinNo,
                           Mode = pin.CurrentMode.ToString(),
                           Value = pin.CurrentValue,
                           Modes = (from c in pin.Capabilities
                                   where c.Value > 0
                                   select c.Key.ToString()).ToList(),
                           AnalogNum = FirmataNancy.Arduino.AnalogPins.IndexOf(pin)
                        };

                return r.Select(x =>
                {
                    dynamic y = new ExpandoObject();
                    y.Num = x.Num;
                    y.AnalogNum = x.AnalogNum;
                    y.Mode = x.Mode;
                    y.Value = x.Value;
                    y.Modes = x.Modes;
                    y.IsInput = x.Mode == "Input";
                    return y;
                });
            }

            set { ;}
        }

        //void a() { Pins.Count(); }
    }
}
