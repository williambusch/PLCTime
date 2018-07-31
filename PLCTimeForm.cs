using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PLCTime
{
    public partial class PLCTimeForm : Form
    {
        //create instance of data reference
        PLCTime plcTime;
        //individual parts of Config string
        string selectedOption;
        string configPart1;
        string configPart2;
        
        public PLCTimeForm(PLCTime dataRef, bool readOnly)
        {
            InitializeComponent();
            plcTime = dataRef;
        }

        //which format is the time in the PLC
        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            selectedOption = rb.Name;
            if (selectedOption == "convertRadio")
            {
                label1.Text = "Initial Date";
                label2.Text = "Attribute Name for Number of Minutes since Initial Date";
            }
            else //selectedOption == "formatRadio"
            {
                label1.Text = "Attribute Name for Date";
                label2.Text = "Attribute Name for Time";
            }

        }

        //send all info from the gui into the ConfigString for the custom data reference
        private void okBtn_Click(object sender, EventArgs e)
        {
            configPart1 = textBox1.Text;
            configPart2 = textBox2.Text;
            plcTime.ConfigString = configPart1 + ";" + configPart2 + ";" + selectedOption;
            this.Close();
        }

        private void PLCTimeForm_Load(object sender, EventArgs e)
        {
            //populate the gui if there is an existing configstring
            if (plcTime.ConfigString != null)
            {
                string[] config = plcTime.ConfigString.Split(';');
                if (config.Length == 3)
                {
                    textBox1.Text = config[0];
                    textBox2.Text = config[1];
                    if (config[2] == "convertRadio") convertRadio.Checked = true;
                    else /*if (config[2] == "formatRadio")*/ formatRadio.Checked = true;

                }
            }
        }

        

    }
}
