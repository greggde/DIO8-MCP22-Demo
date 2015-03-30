# DIO8-MCP22-Demo
Demo program for usage of the GMD Communications DIO8-MCP22
INCOMPLETE!
GMD.DIO.Ctrl.ICtrl and GMD.DIO.Ctrl.MCP22 should hold up well, but DemoForm needs some work.  It will convey the concept,
however.

The DIO8-MCP22 I/O board has 8 programmable I/O ports and an unpopulated UART port.  
The board is oriented correctly, for the purposes of this description, when the short end with a single mounting hole and J1,
J2, and J3 is on the left edge, while the right edge has two mounting holes and a mini-USB connector.

J1, from left to right, is pins
 V+ 4 5 6 7

J2, from left to right, is pins
GND 0 1 2 3

J3, from left to right is UART
TX RTS RX CTS
NOTE! These are 3.3V signals, not the +/-12V of RS-232.

J4, on the right edge, is the mini-USB connector.

S1 at the top is the reset switch

U1 is the Microchip, Inc. MCP2200, heart of the board.

Remaining components:
X1 is the clock crystal; C1, C2, C3 and R1, R2 are assorted capacitors and resistors per MCP2200 requirements.
R3 - R10 on the bottom of the board are pull up resistors.

The provided C# code allows you to monitor and control the DIO8-MCP22 digital I/O.  Control of the CDC virtual com port
utilizing J3, the UART pins, is achieved through a CDC driver found at:
http://www.microchip.com/wwwproducts/Devices.aspx?dDocName=en546923#developmentTools

With the provided DIO8_MCP22 class, you can:
Designate pins to be output, and set the value.
Designate pins to be input, and read the value.
Set the value of all (applicable) pins.
Read the value of all (applicale) pins.
Subscribe to "interrupts" simulated for you via polling.
Read and write to/from EEPROM.
...and a few other tasks.

With the provided DIO8_MCP22_Manager class, you can:
Discover the number of DIO8_MCP22 controllers connected.
Choose the "current" controller, if more than one.
...etc.
