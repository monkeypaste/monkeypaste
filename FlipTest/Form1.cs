using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace FlipTest
{
    public partial class Form1 : Form
    {        
        // debug
        bool started = false;

        public Form1()
        {
            InitializeComponent();

            TextBox t1 = new TextBox();
            t1.Multiline = true;
            t1.Size = new Size(201, 201);
            t1.Text = "this is the front";
            flipPanel1.Front = t1;

            TextBox t2 = new TextBox();
            t2.Multiline = true;
            t2.Size = new Size(201, 201);
            t2.Text = "this is the back";
            flipPanel1.Back = t2;

            flipPanel1.TimerInterval = 10;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            flipPanel1.Flip();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!started)
            {
                flipPanel1.DebugStart();
                started = true;
            }
            else
            {
                flipPanel1.DebugStep();
            }

        }
    }
}