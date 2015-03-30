using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using GMD.DIO.Ctrl;

namespace DemoForm
{
    public partial class DemoForm
    {
        #region Full Control Tab
        private void AddOutputLine(string line)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((OutText)AddOutputLine, line);
                return;
            }

            string[] newLines = new string[textBox1.Lines.Length + 1];
            textBox1.Lines.CopyTo(newLines, 0);
            newLines[textBox1.Lines.Length] = line;

            textBox1.Lines = newLines;

            textBox1.Select(textBox1.Text.Length, 0);

            textBox1.ScrollToCaret();
        }

        private void ClearOutput()
        {
            string[] newLines = new string[0];

            textBox1.Lines = newLines;

            textBox1.Refresh();
        }

        private void LimitNUDs(int upperLimit)
        {
            numericUpDown1.Maximum = upperLimit;
            numericUpDown6.Maximum = upperLimit;
//            numericUpDown10.Maximum = upperLimit;

            int maxPorts = _Manager.GetCurrentController().GetPortCount() - 1;
            numericUpDown2.Maximum = maxPorts;
            numericUpDown3.Maximum = maxPorts;
            numericUpDown4.Maximum = maxPorts;
            numericUpDown5.Maximum = maxPorts;

            numericUpDown7.Maximum = byte.MaxValue;
            numericUpDown8.Maximum = byte.MaxValue;
            numericUpDown9.Maximum = byte.MaxValue;
        }

        private void EnableButtons()
        {
            foreach (Control c in tabControl1.TabPages[0].Controls)
            {
                if (c is Button && c.Name != "button1")
                    c.Enabled = true;
            }

            button1.Enabled = true;
            button10.Enabled = true;
            button11.Enabled = true;
        }

        private void btnInitialize_Click(object sender, EventArgs e)
        {
            _Manager.InitializeAll();

            AddOutputLine("Initialization sent");

            try
            {
                LimitNUDs((int)_Manager.NumberOfControllers - 1);
                EnableButtons();
            }
            catch (Exception ex)
            {
                AddOutputLine(string.Format("Unable to connect any DIO8-MCP22 devices: ", ex.ToString()));
            }
        }

        private void btnGetCount_Click(object sender, EventArgs e)
        {
            AddOutputLine(string.Format("There are {0} connected DIO8-MCP22 devices.", _Manager.NumberOfControllers));
        }

        private void btnGetCurrent_Click(object sender, EventArgs e)
        {
            AddOutputLine(string.Format("The DIO8-MCP22 device {0} is current.", _Manager.GetCurrentControllerID()));
        }

        private void btnSetCurrent_Click(object sender, EventArgs e)
        {
            ICtrl ctrl = _Manager.SetController((uint)numericUpDown1.Value);

            if (ctrl != null)
                AddOutputLine(string.Format("The DIO8-MCP22 device {0} is now current.", numericUpDown1.Value));
            else
                AddOutputLine(string.Format("The DIO8-MCP22 device {0} is NOT current.", numericUpDown1.Value));
        }

        private void btnGetInfo_Click(object sender, EventArgs e)
        {
            string info = _Manager.GetCurrentController().GetControllerInfo();

            AddOutputLine(info);
        }

        private void btnGetPortCount_Click(object sender, EventArgs e)
        {
            int cnt = _Manager.GetCurrentController().GetPortCount();

            AddOutputLine(string.Format("The DIO8-MCP22 device has {0} ports.", cnt));
        }

        private void btnGetPortMode_Click(object sender, EventArgs e)
        {
            CtrlMode mode = _Manager.GetCurrentController().GetPortMode((uint)numericUpDown2.Value);

            AddOutputLine(string.Format("The device port {0} is {1}.", numericUpDown2.Value, mode == CtrlMode.Input ? "Input" : "Output"));
        }

        private void btnSetPortMode_Click(object sender, EventArgs e)
        {
            _Manager.GetCurrentController().SetPortMode((uint)numericUpDown3.Value, (comboBox1.SelectedItem.ToString() == "Input" ? CtrlMode.Input : CtrlMode.Output));

            btnGetAllPortModes_Click(null, null);
        }

        private void btnGetPortState_Click(object sender, EventArgs e)
        {
            CtrlState state;

            try
            {
                state = _Manager.GetCurrentController().GetPortState((uint)numericUpDown4.Value);
            }
            catch (Exception ex)
            {
                AddOutputLine(string.Format("ERROR: {0}", ex));
                return;
            }

            AddOutputLine(string.Format("The device port {0} state is {1}.", numericUpDown4.Value, state == CtrlState.High ? "High" : "Low"));
        }

        private void btnSetPortState_Click(object sender, EventArgs e)
        {
            try
            {
                _Manager.GetCurrentController().SetPortState((uint)numericUpDown5.Value, (((string)comboBox2.SelectedItem) == "High" ? CtrlState.High : CtrlState.Low));
            }
            catch (Exception ex)
            {
                AddOutputLine(string.Format("ERROR: {0}", ex));
            }
            finally
            {
                btnGetAllPortStates_Click(null, null);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            AddOutputLine("Not Implemented");
        }

        private void btnGetControllerInfo_Click(object sender, EventArgs e)
        {
            string info = _Manager.GetControllerInfo((int)numericUpDown6.Value);

            AddOutputLine(info);
        }

        private void btnReadEEPROM_Click(object sender, EventArgs e)
        {
            byte readByte = ((DIO8_MCP22)_Manager.GetCurrentController()).ReadEEPROM((byte)numericUpDown7.Value);

            AddOutputLine(string.Format("The EEPROM value at address {0} is {1}.", numericUpDown7.Value, readByte));
        }

        private void btnReadAllEEPROM_Click(object sender, EventArgs e)
        {
            byte[] readBytes = ((DIO8_MCP22)_Manager.GetCurrentController()).ReadEEPROM();

            StringBuilder sb = new StringBuilder(1024);

            for (int i = 0; i < 256; i++)
                sb.Append(readBytes[i] + " ");

            AddOutputLine("EEPROM: ");
            AddOutputLine(sb.ToString());
        }

        private void btnWriteEEPROM_Click(object sender, EventArgs e)
        {
            bool retVal = ((DIO8_MCP22)_Manager.GetCurrentController()).WriteEEPROM((uint)numericUpDown8.Value, (byte)numericUpDown9.Value);

            AddOutputLine(string.Format("Write was {0} ", retVal ? "successful" : "not successful"));
        }

        private void btnWriteAllEEPROM_Click(object sender, EventArgs e)
        {
            AddOutputLine("Not yet implemented");
        }

        private void btnGetAllPortStates_Click(object sender, EventArgs e)
        {
            byte byteVal = ((DIO8_MCP22)_Manager.GetCurrentController()).GetAllPortStates();

            AddOutputLine(string.Format("Port Values: {0} {1} {2} {3} {4} {5} {6} {7}",
                                        (byteVal & 0x01) == 0x01 ? "High" : "Low",
                                        (byteVal & 0x02) == 0x02 ? "High" : "Low",
                                        (byteVal & 0x04) == 0x04 ? "High" : "Low",
                                        (byteVal & 0x08) == 0x08 ? "High" : "Low",
                                        (byteVal & 0x10) == 0x10 ? "High" : "Low",
                                        (byteVal & 0x20) == 0x20 ? "High" : "Low",
                                        (byteVal & 0x40) == 0x40 ? "High" : "Low",
                                        (byteVal & 0x80) == 0x80 ? "High" : "Low"
                                        ));
        }

        private void btnGetAllPortModes_Click(object sender, EventArgs e)
        {
            CtrlMode[] mode = new CtrlMode[8];

            for (uint i = 0; i < 8; i++)
            {
                mode[i] = ((DIO8_MCP22)_Manager.GetCurrentController()).GetPortMode(i);
            }

            AddOutputLine(string.Format("Port Modes: {0} {1} {2} {3} {4} {5} {6} {7}",
                                        mode[0] == CtrlMode.Output ? "Output" : "Input",
                                        mode[1] == CtrlMode.Output ? "Output" : "Input",
                                        mode[2] == CtrlMode.Output ? "Output" : "Input",
                                        mode[3] == CtrlMode.Output ? "Output" : "Input",
                                        mode[4] == CtrlMode.Output ? "Output" : "Input",
                                        mode[5] == CtrlMode.Output ? "Output" : "Input",
                                        mode[6] == CtrlMode.Output ? "Output" : "Input",
                                        mode[7] == CtrlMode.Output ? "Output" : "Input"
                                        ));
        }

        private void btnClearResults_Click(object sender, EventArgs e)
        {
            ClearOutput();
        }

        private void btnSetInterrupts_Click(object sender, EventArgs e)
        {
            _Manager.Interrupt += new IMultiCtrl_PortStateChanged(Control_Interrupt);

            for (uint i = 0; i < 8; i++)
            {
                if (((DIO8_MCP22)_Manager.GetCurrentController()).GetPortMode(i) == CtrlMode.Input)
                    _Manager.GetCurrentController().SetInterrupt(i, true);
                else
                    _Manager.GetCurrentController().SetInterrupt(i, false);
            }
        }

        void Control_Interrupt(uint port, CtrlState state, uint controllerNumber)
        {
            AddOutputLine(string.Format("State change interrupt received, port: {0}, new state:{1}", port, state));
        }

        private void btnClearInterrupts_Click(object sender, EventArgs e)
        {
            for (uint i = 0; i < 8; i++)
            {
                _Manager.GetCurrentController().SetInterrupt(i, false);
            }

            _Manager.Interrupt -= new IMultiCtrl_PortStateChanged(Control_Interrupt);
        }

        private void btnSelectController_Click(object sender, EventArgs e)
        {
//            _Manager.SetController((uint)numericUpDown10.Value);
        }
        #endregion Full Control Tab

    }
}
