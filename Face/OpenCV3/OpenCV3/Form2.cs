using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenCV3
{
    public partial class Form2 : Form
    {

        public Form2()
        {
            InitializeComponent();
            CallBackMy.callbackEventHandler = new CallBackMy.callbackEvent(this.Reload);
        }

        void Reload(double x1, double x2, double x3, double x4)
        {
            if (x1>0.0 && x2>0.0 && x3>0.0 && x4>0.0)
            {
                textBox_left.Text = " ";
                textBox_right.Text =" ";
                textBox_up.Text =  "";
                textBox_down.Text = "";
            }
            else
            {
                textBox_left.Text = x1.ToString("0.00") + " градусов";
                textBox_right.Text = x2.ToString("0.00") + " градусов";
                textBox_up.Text = x3.ToString("0.00") + " градусов";
                textBox_down.Text = x4.ToString("0.00") + " градусов";
            }
        }
    }
}
