//Cheesy Licensing:
//If you use this code in a shared manner, open-source, closed-source, commercially, or otherwise, provide this
//text: Original source written and owned by GMD Communications, Sachse, Texas, USA.
//Derived works remain property of the author, but if you make any money using this code, kick back a nominal fee
//to GMD Communications.  Note, "nominal" is proportional to your revenue and subject to negotiation :).

using System;
using System.Collections.Generic;
using System.Text;

namespace GMD.DIO.Ctrl
{
    /// <summary>
    /// Possible port modes
    /// </summary>
    public enum CtrlMode
    {
        Output,
        Input,
    }

    /// <summary>
    /// Possible port states
    /// </summary>
    public enum CtrlState
    {
        Low,
        High,
        Floating
    }

    public delegate void ICtrl_PortStateChanged(uint port, CtrlState state, uint controller);
    public delegate void IMultiCtrl_PortStateChanged(uint port, CtrlState state, uint controller);

    public interface IMultiCtrl
    {
        event IMultiCtrl_PortStateChanged Interrupt;

        void Dispose();
    }

    public interface ICtrl
    {
        event ICtrl_PortStateChanged Interrupt;

        /// <summary>
        /// Initialize controller
        /// Generally required to be first call
        /// </summary>
        void InitializeController();

        /// <summary>
        /// Get number of this type controller connected
        /// </summary>
        /// <returns>Number of controllers</returns>
        uint GetConnectedControllerCount();

        /// <summary>
        /// Get the number of the currently selected controller
        /// </summary>
        /// <returns></returns>
        uint GetController();

        /// <summary>
        /// Get controller specific information for the current controller
        /// </summary>
        /// <returns>Implementation specific descriptor string</returns>
        string GetControllerInfo();

        /// <summary>
        /// Get the number of ports on the current controller
        /// </summary>
        /// <returns>Number of ports</returns>
        int GetPortCount();

        /// <summary>
        /// Get the mode (input/output) of the specified port on the current controller
        /// </summary>
        /// <param name="port">Zero-based number of the port</param>
        /// <returns>CtrlMode of the port</returns>
        CtrlMode GetPortMode(uint port);

        /// <summary>
        /// Set the mode (input/output) of the specified port on the current controller
        /// </summary>
        /// <param name="port">Zero-based number of the port to set</param>
        /// <param name="mode">CtrlMode to set the port to</param>
        void SetPortMode(uint port, CtrlMode mode);

        /// <summary>
        /// Return the state of the given port; port must be CtrlMode.Input
        /// </summary>
        /// <param name="port">Zero-based number of the port to read</param>
        /// <returns>CtrlState of the specified port</returns>
        CtrlState GetPortState(uint port);

        /// <summary>
        /// Set the state of the given port on the current controller; port must be CtrlMode.Output
        /// </summary>
        /// <param name="port">Zero-based number of the port to set</param>
        /// <param name="state">CtrlState to set the port to</param>
        void SetPortState(uint port, CtrlState state);

        /// <summary>
        /// Sets the port to fire an interrupt on change of state
        /// </summary>
        /// <param name="port">Zero-based number of the port to monitor</param>
        /// <param name="on">Whether to turn interrupt on or off</param>
        void SetInterrupt(uint port, bool on);

        /// <summary>
        /// Gets the state of all ports on the current controller in one call
        /// </summary>
        /// <param name="portState">Bitwise map of states</param>
        /// <returns>Array of port states</returns>
        unsafe CtrlState[] GetAllPortStateArray(ref byte portState);

        /// <summary>
        /// Get the state of all ports on the current controller in one call
        /// </summary>
        /// <param name="portState">byte to hold port states</param>
        unsafe void GetAllPortState(ref byte portState);

        /// <summary>
        /// Set all port states at once
        /// </summary>
        /// <param name="ctrlState">Bitwise map of states to set</param>
        /// <returns>True on success, else false</returns>
        bool SetAllPortState(byte ctrlState);

        /// <summary>
        /// Reset a controller
        /// </summary>
        void Reset();

        void Dispose();
    }
}
