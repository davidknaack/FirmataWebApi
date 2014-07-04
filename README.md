Firmata Web Api
==============
Firmata Web Api is a web service interface that provides a bridge between the web and an 
Arduino running Firmata. The web API uses HTTP GET so it is easy to use from anywhere.
It will run either as a Windows Service or under Mono on Linux (init.d script included).
Both may also be run from the console for testing or short-term use.

Note that there is no security whatsoever. Keep this in mind when you expose the server.
Some kind of authentication is on the TODO list.

The Arduino interface is via a branch of the Sharpduino project, and the web interface
is via self-hosted NancyFX.

API functions
--------------
The API has 5 functions, PinMode, DigitalRead, DigitalWrite, AnalogRead, and AnalogWrite.
These are a subset of the functions provided by Firmata, and are named to be similar to 
the functions you might use in an Arduino sketch.

Each function returns a JSON string indicating whether the operation was successful
and what the relevant status of the pin is. If there is an error the "Status" field
will contain the error message instead of "OK".

API Examples
--------------
PinMode sets the mode of a pin and may be one of: input, output, analog, pwm

    http://host:8000/firmata/pm/{pinIdx:range(2,19)}/{pinMode}

Response: 

    {"Status":"OK","PinNum":13,"Mode":"output"}

DigitalRead:

    http://host:8000/firmata/dr/{pinIdx:range(2,19)}

Response: 

    {"Status":"OK","PinNum":13,"Value":0}

DigitalWrite: 

    http://host:8000/firmata/dw/{pinIdx:range(2,19)}/{pinVal:range(0,255)}

Response: 

    {"Status":"OK","PinNum":13,"Value":1}

AnalogRead:

    http://host:8000/firmata/ar/{pinIdx:range(0,5)}

Response: 

    {"Status":"OK","PinNum":2,"Value":172}


AnalogWrite: 

    http://host:8000/firmata/ar/{pinIdx:range(3,11)}/{pinVal:range(0,255)}

Response: 

    {"Status":"OK","PinNum":3,"Value":170}
