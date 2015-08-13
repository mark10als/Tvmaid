using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tvmaid
{
    public partial class SleepCountdown : Form
    {
        int count = 60;

        public SleepCountdown()
        {
            InitializeComponent();
            countProgress.Maximum = count;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            count--;
            countLabel.Text = count + " 秒";
            countProgress.Value = count;
            if(count < 0)
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void sleepButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
