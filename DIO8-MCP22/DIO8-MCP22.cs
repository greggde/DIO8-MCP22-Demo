//Cheesy Licensing:
//If you use this code in a shared manner, open-source, closed-source, commercially, or otherwise, provide this
//text: Original source written and owned by GMD Communications, Sachse, Texas, USA.
//Derived works remain property of the author, but if you make any money using this code, kick back a nominal fee
//to GMD Communications.  Note, "nominal" is proportional to your revenue and subject to negotiation :).

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using GMD.DIO.Ctrl;
using SimpleIO;

namespace GMD.DIO.Ctrl
{
    public class DIO8_MCP22_Manager : IMultiCtrl, IDisposable
    {
        private List<DIO8_MCP22> _controllers = new List<DIO8_MCP22>();
        private uint VID = 0x04d8;
        private uint PID = 0xF4AA;
        private uint _deviceCount;

        public event IMultiCtrl_PortStateChanged Interrupt;

        #region Properties
        public uint VendorID
        {
            get { return VID; }
            set { VID = value; }
        }

        public uint ProductID
        {
            get { return PID; }
            set { PID = value; }
        }

        public uint NumberOfControllers
        {
            get { return _deviceCount; }
        }
        #endregion Properties

        public void InitializeAll()
        {
            SimpleIOClass.InitMCP2200(VendorID, ProductID);

            _deviceCount = SimpleIOClass.GetNoOfDevices();

            for (uint i = 0; i < _deviceCount; i++)
            {
                _controllers.Add(new DIO8_MCP22(i));

                _controllers[(int)i].InitializeController();
                _controllers[(int)i].Interrupt += new ICtrl_PortStateChanged(DIO8_MCP22_Manager_Interrupt);
            }
        }

        void DIO8_MCP22_Manager_Interrupt(uint port, CtrlState state, uint controller)
        {
            if (Interrupt != null)
                Interrupt(port, state, controller);
        }

        public ICtrl SetController(uint controllerNumber)
        {
            int curController = SimpleIOClass.GetSelectedDevice();

            if (SimpleIOClass.SelectDevice(controllerNumber) == 0)
                return _controllers[(int)controllerNumber];
            else
                return _controllers[curController];
        }

        public uint GetCurrentControllerID()
        {
            return (uint)SimpleIOClass.GetSelectedDevice();
        }

        public string GetControllerInfo(int controllerNumber = -1)
        {
            if (controllerNumber == -1)
                return GetCurrentController().GetControllerInfo();
            else
                return _controllers[controllerNumber].GetControllerInfo();
        }

        public ICtrl GetCurrentController()
        {
            return _controllers[SimpleIOClass.GetSelectedDevice()];
        }

        public ICtrl this[uint controllerID]
        {
            get { return _controllers[(int)controllerID]; }
        }

        public void Dispose()
        {
            foreach(ICtrl ictrl in _controllers)
            {
                ictrl.Interrupt -= new ICtrl_PortStateChanged(DIO8_MCP22_Manager_Interrupt);
                ictrl.Dispose();
            }
        }
    }

    public class DIO8_MCP22 : ICtrl, IDisposable
    {
        public const long DEBOUNCE_MIN_TICKS = 2000;

        #region Private Members
        private uint VID = 0x04d8;
        private uint PID = 0xf4;
        private byte _ioModeMap = 0xFF;
        private byte _ioStateMap = 0x00;
        private byte _interruptMap = 0x00;

        private uint _controllerNumber;

        private ManualResetEvent _pollFlag;
        private ManualResetEvent _quitFlag;

        private Thread _pollingThread;

        private byte[] PortMask = new byte[]{0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80};
        #endregion Private Members

        #region Constructors
        public DIO8_MCP22(uint controllerNumber)
        {
            _controllerNumber = controllerNumber;

            _pollFlag = new ManualResetEvent(false);
            _quitFlag = new ManualResetEvent(false);

            _pollingThread = new Thread(PollingThreadProc);

            _pollingThread.Name = "GMD.DIO.Ctrl.MCP22.DIO8_MCP22 Poll Thread";

            _pollingThread.Start();
        }
        #endregion Constructors

        public event ICtrl_PortStateChanged Interrupt;

        private void FirePollEvent(uint port, CtrlState state)
        {
            if (Interrupt != null)
                Interrupt(port, state, _controllerNumber);
        }

        #region Properties
        public uint VendorID
        {
            get { return VID; }
            set { VID = value; }
        }

        public uint ProductID
        {
            get { return PID; }
            set { PID = value; }
        }
        #endregion Properties

        #region DIO8_MCP22 Specific Functionality
        public string GetControllerInfo()
        {
            return SimpleIOClass.GetDeviceInfo((uint)SimpleIOClass.GetSelectedDevice());
        }

        public byte ReadEEPROM(byte address)
        {
            return (byte)SimpleIOClass.ReadEEPROM((uint)address);
        }

        public byte[] ReadEEPROM()
        {
            byte[] retVal = new byte[256];

            for (int i = 0; i<256; i++ )
            {
                retVal[i] = (byte)SimpleIOClass.ReadEEPROM((uint)i);
            }

            return retVal;
        }

        public bool WriteEEPROM(uint address, byte value)
        {
            int retVal = SimpleIOClass.WriteEEPROM(address, value);

            if (retVal == 0)
                return true;
            else
                return false;
        }

        public bool WriteEEPROM(byte[] value)
        {
            int retVal = 0;

            for (int i = 0; i < 256; i++)
            {
                retVal += SimpleIOClass.WriteEEPROM((uint)i, value[i]);
            }

            return retVal == 0;
        }

        public byte GetAllPortModes()
        {
            return (byte)SimpleIOClass.ReadPortValue();
        }

        public byte GetAllPortStates()
        {
            return (byte)SimpleIOClass.ReadPortValue();
        }
        #endregion DIO8_MCP22 Specific Functionality

        #region Polling Thread
        private void PollingThreadProc()
        {
            WaitHandle[] waits = new WaitHandle[] { _pollFlag, _quitFlag };
            int waitResult = WaitHandle.WaitAny(waits);

            if (waitResult != 0)
                return;

            long[] _portDebounce = new long[8];
            GetAllPortState(ref _ioStateMap);
            _ioModeMap = GetAllPortModes();

            while (!_quitFlag.WaitOne(0))
            {
                if (_pollFlag.WaitOne(25))
                {
                    byte curStates = (byte)SimpleIOClass.ReadPortValue();

                    if ( curStates != _ioStateMap )
                    {
                        DateTime now = DateTime.Now;

                        for (uint i = 0; i < 8; i++)
                        {
//                            if ((_ioModeMap & PortMask[i]) > 0 && (_ioStateMap & PortMask[i]) != (curStates & PortMask[i]))
                            if ((_ioStateMap & PortMask[i]) != (curStates & PortMask[i]))
                            {
                                if ( now.Ticks - _portDebounce[i] > DEBOUNCE_MIN_TICKS )
                                FirePollEvent(i, ((_ioStateMap & PortMask[i]) == PortMask[i]) ? CtrlState.High : CtrlState.Low);
#if DEBUG
                                Trace.WriteLine(string.Format("Firing Input Poll Event Port {0}, State {1}", i, ((_ioStateMap & PortMask[i]) == PortMask[i]) ? CtrlState.High : CtrlState.Low));
#endif
                            }
                        }

                        _ioStateMap = curStates;
                    }
                }

                Thread.Sleep(10);
            }
        }
        #endregion Polling Thread

        #region ICtrl Implementation
        public void InitializeController()
        {
            //SimpleIOClass.ConfigureMCP2200(0xFF, 115200, 0, 0, false, false, false, false);

            if ( !SimpleIOClass.IsConnected() )
                SimpleIOClass.InitMCP2200(VendorID, ProductID);
        }

        public uint GetConnectedControllerCount()
        {
            return SimpleIOClass.GetNoOfDevices();
        }

        public uint GetController()
        {
            return _controllerNumber;
        }

        public int GetPortCount()
        {
            return 8;
        }

        public CtrlMode GetPortMode(uint port)
        {
            if ((_ioModeMap & PortMask[port]) == PortMask[port])
                return CtrlMode.Input;
            else
                return CtrlMode.Output;
        }

        public void SetPortMode(uint port, CtrlMode mode)
        {
            if (mode == CtrlMode.Output)
                _ioModeMap = (byte)(_ioModeMap & ~PortMask[port]);
            else
                _ioModeMap |= PortMask[port];

            SimpleIOClass.ConfigureIO(_ioModeMap);
            SimpleIOClass.WritePort(_ioStateMap);
        }

        public unsafe CtrlState[] GetAllPortStateArray(ref byte portState)
        {
            CtrlState[] retVal = new CtrlState[8];
            byte states = 0;

            GetAllPortState(ref states);

            for (int i = 0; i < 8; i++)
            {
                retVal[i] = ((states >> i) & 0x01) == 0x01 ? CtrlState.High : CtrlState.Low;
            }

            portState = (byte)states;

            return retVal;
        }

        public unsafe void GetAllPortState(ref byte portState)
        {
            uint states = 0;

            if (SimpleIOClass.ReadPort(&states))
                _ioStateMap = (byte)states;

            portState = _ioStateMap;
        }

        public CtrlState GetPortState(uint port)
        {
            CtrlState retVal = CtrlState.Floating;

            if ((_ioModeMap & PortMask[port]) != PortMask[port])
                throw new ArgumentException("Port must be in input state to be read.", "port");

            int stateVal = SimpleIOClass.ReadPortValue();

            if (stateVal == 0x8000)
                throw new Exception("Error reading port values");
            else
            {
                _ioStateMap = (byte)stateVal;
                retVal = ((_ioStateMap & PortMask[port]) == PortMask[port]) ? CtrlState.High : CtrlState.Low;
            }

            return retVal;
        }

        public bool SetAllPortState(byte ctrlState)
        {
            bool retVal = SimpleIOClass.WritePort((uint)ctrlState);

            if (retVal)
                _ioStateMap = ctrlState;

            return retVal;
        }

        public void SetPortState(uint port, CtrlState state)
        {
            if ((_ioModeMap & PortMask[port]) == PortMask[port])
                throw new ArgumentException("Port must be in output state to be set.", "port");
             
            if (state == CtrlState.High)
                _ioStateMap |= PortMask[port];
            else
                _ioStateMap &= (byte)~PortMask[port];

            SimpleIOClass.WritePort(_ioStateMap);
        }

        public void SetInterrupt(uint port, bool on)
        {
            if (on)
                _interruptMap |= PortMask[port];
            else
                //_interruptMap = (byte)(_interruptMap & ~PortMask[port]);
                _interruptMap &= (byte)~PortMask[port];

            if (_interruptMap == 0)
                _pollFlag.Reset();
            else
                _pollFlag.Set();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
        #endregion ICtrl Implementation

        public void Dispose()
        {
            _pollFlag.Reset();
            _quitFlag.Set();
        }
    }
}
