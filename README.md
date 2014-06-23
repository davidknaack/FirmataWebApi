Firmata Web Api
==============
Firmata Web Api is a Windows Service that provides a bridge between the web and an 
Arduino running Firmata. The web API uses HTTP GET so it is easy to use from anywhere.

The Arduino interface is via a branch of the Sharpduino project, and the web interface
is via self-hosted NancyFX.

This project is in use by the Omaha Maker Group, and the base URL and service name
are configured for that installation. Feel free to fork the project and customize it
for your own system.

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
    http://host:8000/mmm/oma/pm/{pinIdx:range(2,19)}/{pinMode}
Response: 
    {"Status":"OK","PinNum":13,"Mode":"output"}

DigitalRead:
    http://host:8000/mmm/oma/dr/{pinIdx:range(2,19)}
Response: 
    {"Status":"OK","PinNum":13,"Value":0}

DigitalWrite: 
    http://host:8000/mmm/oma/dw/{pinIdx:range(2,19)}/{pinVal:range(0,255)}
Response: 
    {"Status":"OK","PinNum":13,"Value":1}

AnalogRead:
    http://host:8000/mmm/oma/ar/{pinIdx:range(0,5)}
Response: 
    {"Status":"OK","PinNum":2,"Value":172}

AnalogWrite: 
    http://host:8000/mmm/oma/ar/{pinIdx:range(3,11)}/{pinVal:range(0,255)}
Response: 
    {"Status":"OK","PinNum":3,"Value":170}