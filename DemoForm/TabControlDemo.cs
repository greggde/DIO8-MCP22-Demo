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
        #region DemoTab
        #region DemoMarch
        private ManualResetEvent stopShow = new ManualResetEvent(false);
        private bool[] showThreadStarted = new bool[8];
        private int demoDelay = 125;
        private Button[] inputLights;

        private void button1_Click(object sender, EventArgs e)
        {
            EnableInputDemo(false);
            EnableOutputDemo(false);
            EnableMarchDemo(true);

            ICtrl ctrlr = _Manager.GetCurrentController();

            if (!showThreadStarted[_Manager.GetCurrentControllerID()])
                ThreadPool.QueueUserWorkItem(ThreadPoolThreadProc, ctrlr);

            if (!stopShow.WaitOne(0, false))
                stopShow.Set();
            else
                stopShow.Reset();
        }

        private void ThreadPoolThreadProc(object state)
        {
            ICtrl ctrlr = (ICtrl)state;
            uint port = 0;
            CtrlMode curMode = CtrlMode.Output;
            CtrlState curState = CtrlState.High;

            for (uint i = 0; i < 8; i++)
                ctrlr.SetPortMode(i, curMode);

            while (true)
            {
                if (!stopShow.WaitOne(0, false))
                    break;

                ctrlr.SetAllPortState((byte)255);

                for (int j = 0; j < 8; j++)
                {
                    if (++port > 7) port = 0;

                    ctrlr.SetPortState(port, curState);

                    curState = curState == CtrlState.High ? CtrlState.Low : CtrlState.High;

                    Thread.Sleep(demoDelay);
                }

                for (int k = 0; k < 3; k++)
                {
                    byte tempState = 0;
                    ctrlr.GetAllPortState(ref tempState);
                    tempState = (byte)~tempState;
                    ctrlr.SetAllPortState(tempState);

                    Thread.Sleep(demoDelay * 2);
                }

                for (int l = 0; l < 8; l++)
                {
                    if (--port > 7) port = 7; //port is a uint, so it will go high instead of negative

                    stopShow.WaitOne();

                    ctrlr.SetPortState(port, curState);

                    curState = curState == CtrlState.High ? CtrlState.Low : CtrlState.High;

                    Thread.Sleep(demoDelay);
                }

                for (int m = 0; m < 3; m++)
                {
                    byte tempState = 0;
                    ctrlr.GetAllPortState(ref tempState);
                    tempState = (byte)~tempState;
                    ctrlr.SetAllPortState(tempState);

                    Thread.Sleep(demoDelay * 2);
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //if (checkBox1.Checked)
            //{
            //    checkBox2.Checked = false;
            //    checkBox3.Checked = false;
            //    checkBox4.Checked = false;
            //    checkBox5.Checked = false;
            //}
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            //    checkBox1.Checked = false;
            //    checkBox3.Checked = false;
            //    checkBox4.Checked = false;
            //    checkBox5.Checked = false;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            //checkBox1.Checked = false;
            //checkBox2.Checked = false;
            //checkBox4.Checked = false;
            //checkBox5.Checked = false;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            //checkBox1.Checked = false;
            //checkBox2.Checked = false;
            //checkBox3.Checked = false;
            //checkBox5.Checked = false;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            //checkBox1.Checked = false;
            //checkBox2.Checked = false;
            //checkBox3.Checked = false;
            //checkBox4.Checked = false;
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            demoDelay = 30; //30 ms delay in demo loops
            base.OnClick(e);
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            demoDelay = 62; //62 ms delay in demo loops
            base.OnClick(e);
        }

        private void checkBox3_Click(object sender, EventArgs e)
        {
            demoDelay = 125; //125 ms delay in demo loops
            base.OnClick(e);
        }

        private void checkBox4_Click(object sender, EventArgs e)
        {
            demoDelay = 250; //250 ms delay in demo loops
            base.OnClick(e);
        }

        private void checkBox5_Click(object sender, EventArgs e)
        {
            demoDelay = 500; //500 ms delay in demo loops
            base.OnClick(e);
        }

        private void EnableMarchDemo(bool enable)
        {
            if (!enable)
                stopShow.Set();

            checkBox1.Enabled = enable;
            checkBox2.Enabled = enable;
            checkBox3.Enabled = enable;
            checkBox4.Enabled = enable;
            checkBox5.Enabled = enable;

            DemoMarch = enable;
        }
        #endregion DemoMarch

        #region DemoInput
        private void button10_Click(object sender, EventArgs e)
        {
            byte state = 0;

            if (!DemoInput)
            {
                if (inputLights == null)
                    inputLights = new Button[] { button12, button13, button14, button15, button16, button17, button18, button19 };

                _Manager.GetCurrentController().Interrupt += new ICtrl_PortStateChanged(ctrlr_Interrupt);

                EnableMarchDemo(false);
                EnableOutputDemo(false);
                EnableInputDemo(true);

                for (uint i = 0; i < 8; i++)
                {
                    _Manager.GetCurrentController().SetPortMode(i, CtrlMode.Input);
                    _Manager.GetCurrentController().SetInterrupt(i, true);
                }

                _Manager.GetCurrentController().GetAllPortState(ref state);

                for (byte k = 0; k < 8; k++)
                {
                    if (((state >> k) & 0x01) == 0x01)
                        inputLights[k].ImageAlign = ContentAlignment.BottomCenter;
                    else
                        inputLights[k].ImageAlign = ContentAlignment.TopCenter;
                }
            }
            else
            {
                _Manager.GetCurrentController().Interrupt -= new ICtrl_PortStateChanged(ctrlr_Interrupt);

                for (byte l = 0; l < 8; l++)
                {
                    if (((state >> l) & 0x01) == 0x01)
                        inputLights[l].ImageAlign = ContentAlignment.TopCenter;
                }

                EnableInputDemo(false);
            }
        }

        void ctrlr_Interrupt(uint port, CtrlState state, uint controller)
        {
            inputLights[port].ImageAlign = state == CtrlState.High ? ContentAlignment.TopCenter : ContentAlignment.BottomCenter;
        }

        private void EnableInputDemo(bool enable)
        {
            if (!enable)
                ;//reset button state

            button12.Enabled = enable;
            button13.Enabled = enable;
            button14.Enabled = enable;
            button15.Enabled = enable;
            button16.Enabled = enable;
            button17.Enabled = enable;
            button18.Enabled = enable;
            button19.Enabled = enable;

            DemoInput = enable;
        }
        #endregion DemoInput

        #region DemoOutput
        private void button2_Click(object sender, EventArgs e)
        {
            ContentAlignment align = button2.ImageAlign;

            if (button2.ImageAlign == ContentAlignment.TopCenter)
            {
                button2.ImageAlign = ContentAlignment.BottomCenter;
                _Manager.GetCurrentController().SetPortState(0, CtrlState.Low);
            }
            else
            {
                button2.ImageAlign = ContentAlignment.TopCenter;
                _Manager.GetCurrentController().SetPortState(0, CtrlState.High);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ContentAlignment align = button3.ImageAlign;

            if (button3.ImageAlign == ContentAlignment.TopCenter)
            {
                button3.ImageAlign = ContentAlignment.BottomCenter;
                _Manager.GetCurrentController().SetPortState(1, CtrlState.Low);
            }
            else
            {
                button3.ImageAlign = ContentAlignment.TopCenter;
                _Manager.GetCurrentController().SetPortState(1, CtrlState.High);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ContentAlignment align = button4.ImageAlign;

            if (button4.ImageAlign == ContentAlignment.TopCenter)
            {
                button4.ImageAlign = ContentAlignment.BottomCenter;
                _Manager.GetCurrentController().SetPortState(2, CtrlState.Low);
            }
            else
            {
                button4.ImageAlign = ContentAlignment.TopCenter;
                _Manager.GetCurrentController().SetPortState(2, CtrlState.High);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ContentAlignment align = button5.ImageAlign;

            if (button5.ImageAlign == ContentAlignment.TopCenter)
            {
                button5.ImageAlign = ContentAlignment.BottomCenter;
                _Manager.GetCurrentController().SetPortState(3, CtrlState.Low);
            }
            else
            {
                button5.ImageAlign = ContentAlignment.TopCenter;
                _Manager.GetCurrentController().SetPortState(3, CtrlState.High);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ContentAlignment align = button6.ImageAlign;

            if (button6.ImageAlign == ContentAlignment.TopCenter)
            {
                button6.ImageAlign = ContentAlignment.BottomCenter;
                _Manager.GetCurrentController().SetPortState(4, CtrlState.Low);
            }
            else
            {
                button6.ImageAlign = ContentAlignment.TopCenter;
                _Manager.GetCurrentController().SetPortState(4, CtrlState.High);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ContentAlignment align = button7.ImageAlign;

            if (button7.ImageAlign == ContentAlignment.TopCenter)
            {
                button7.ImageAlign = ContentAlignment.BottomCenter;
                _Manager.GetCurrentController().SetPortState(5, CtrlState.Low);
            }
            else
            {
                button7.ImageAlign = ContentAlignment.TopCenter;
                _Manager.GetCurrentController().SetPortState(5, CtrlState.High);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ContentAlignment align = button8.ImageAlign;

            if (button8.ImageAlign == ContentAlignment.TopCenter)
            {
                button8.ImageAlign = ContentAlignment.BottomCenter;
                _Manager.GetCurrentController().SetPortState(6, CtrlState.Low);
            }
            else
            {
                button8.ImageAlign = ContentAlignment.TopCenter;
                _Manager.GetCurrentController().SetPortState(6, CtrlState.High);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ContentAlignment align = button9.ImageAlign;

            if (button9.ImageAlign == ContentAlignment.TopCenter)
            {
                button9.ImageAlign = ContentAlignment.BottomCenter;
                _Manager.GetCurrentController().SetPortState(7, CtrlState.Low);
            }
            else
            {
                button9.ImageAlign = ContentAlignment.TopCenter;
                _Manager.GetCurrentController().SetPortState(7, CtrlState.High);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            EnableMarchDemo(false);
            EnableInputDemo(false);
            EnableOutputDemo(true);
        }

        private void EnableOutputDemo(bool enable)
        {
            gbOutputDemo.Enabled = enable;
            button2.Enabled = enable;
            button3.Enabled = enable;
            button4.Enabled = enable;
            button5.Enabled = enable;
            button6.Enabled = enable;
            button7.Enabled = enable;
            button8.Enabled = enable;
            button9.Enabled = enable;

            DemoOutput = enable;

            if (enable)
            {
                for (uint i = 0; i < 8; i++)
                    _Manager.GetCurrentController().SetPortMode(i, CtrlMode.Output);

                _Manager.GetCurrentController().SetAllPortState((byte)255);
            }
        }
        #endregion DemoOutput
        #endregion DemoTab
    }
}
