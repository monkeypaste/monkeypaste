using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace wdi.ui
{
    /// <summary>
    /// WOW!  This is a Panel control that you can put things on the front of as well as the back
    /// of.  Then, you can call it's Flip() method to make it flip over and show what's on the
    /// other side.
    /// </summary>
    public partial class FlipPanel : Panel
    {
        private Bitmap m_PageA;
        private Bitmap m_PageB;
        private int m_X1;
        private int m_X2;
        private float m_Y1;
        private float m_Y2;
        private int m_AngleCounter = 0;
        private int m_XIncrement = 50;

        /// <summary>
        /// The interval between animation ticks in milliseconds
        /// </summary>
        public int TimerInterval
        {
            get { return timer1.Interval; }
            set { timer1.Interval = value; }
        }

        private Control m_Front;
        /// <summary>
        /// The control to put on the front of the container.  This will always be the front, even
        /// when the back control is in the front.
        /// </summary>
        public Control Front
        {
            get { return m_Front; }
            set { this.Controls.Add(value); m_Front = value; }
        }

        private Control m_Back;
        /// <summary>
        /// The control to put on the back of the container.  This will always be called the back
        /// even when it's on the front
        /// </summary>
        public Control Back
        {
            get { return m_Back; }
            set { this.Controls.Add(value);  m_Back = value; }
        }

        private bool m_DoShading;
        /// <summary>
        /// Set this to true if you want the control to calculate shading during the flip animation.
        /// I can't really tell if this actually does anything visually.
        /// </summary>
        public bool DoShading
        {
            get { return m_DoShading; }
            set { m_DoShading = value; }
        }


        /// <summary>
        /// Default constructor.
        /// </summary>
        public FlipPanel()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();        
        }        

        /// <summary>
        /// Call this to initiate a flip action on the panel
        /// </summary>
        public void Flip()
        {
            this.StartFlip();
            timer1.Enabled = true;
        }

        /// <summary>
        /// For debugging only.  Or, I guess if you want to step through the flip animation...
        /// </summary>
        public void DebugStart()
        {
            this.StartFlip();
        }

        /// <summary>
        /// For debugging only.  Simulates an animation timer tick.
        /// </summary>
        public void DebugStep()
        {
            this.Invalidate();
            this.m_AngleCounter++;
        }

        /// <summary>
        /// This is what starts/resets the variables and timer used to do a flip.
        /// </summary>
        private void StartFlip()
        {
            this.m_X1 = 0;
            this.m_X2 = this.Width;
            this.m_Y1 = 0;
            this.m_Y2 = this.Height;

            m_PageA = new Bitmap(this.Width, this.Height);
            m_Front.DrawToBitmap(m_PageA, new Rectangle(m_Front.Location, m_Front.Size));

            m_PageB = new Bitmap(this.Width, this.Height);
            m_Back.DrawToBitmap(m_PageB, new Rectangle(m_Back.Location, m_Back.Size));
            m_PageB.RotateFlip(RotateFlipType.RotateNoneFlipX);

            // hide the controls while drawing
            this.m_Front.Visible = false;
            this.m_Back.Visible = false;
        }

        /// <summary>
        /// Reset some variables and junk.
        /// </summary>
        private void FlipDone()
        {
            timer1.Enabled = false;
            // Swap front and back controls
            Control temp = m_Front;
            this.m_Front = this.m_Back;
            this.m_Back = temp;
            // Reset the temporary variables
            m_X1 = 0;
            m_X2 = this.Width;
            this.m_Y1 = 0;
            m_AngleCounter = 0;
            // Only show the control that's in front
            this.m_Front.Visible = true;
            this.m_Back.Visible = false;            
        }

        /// <summary>
        /// This is an owner-drawn control, so this is where I draw stuff.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (m_PageA == null || m_PageB == null)
                return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;                        

            PointF[] destPoints = { new PointF(m_X1, 0), new PointF(m_X2, m_Y1), new PointF(m_X1, this.Height) };

            // this is the current angle of the panel during a flip
            float ang = (float)(m_AngleCounter * (float)(180f / this.Width));

            if (m_X1 < this.Width / 2)
            {
                if(m_DoShading) e.Graphics.DrawImage(ShadeImage(m_PageA, ang), destPoints);
                else e.Graphics.DrawImage(m_PageA, destPoints);
                // this is what creates the perspective effect
                m_Y1 += 0.5f;
            }
            else
            {
                if(m_DoShading) e.Graphics.DrawImage(ShadeImage(m_PageB, ang), destPoints);
                else e.Graphics.DrawImage(m_PageB, destPoints);
                m_Y1 -= 0.5f;
            }

            m_X1 += m_XIncrement;
            m_X2 -= m_XIncrement;
            
            if (m_X2 < -1)
            {                
                FlipDone();                
            }           
        }

        /// <summary>
        /// Here's the exciting part!  This applies sort of a linear gradient over the half of
        /// the image that's moving away from the screen.  See my article for more details if
        /// you're interested.
        /// </summary>
        /// <param name="b">The current bitmap that's being drawn</param>
        /// <param name="angle">The current angle of the flip</param>
        /// <returns>A shaded bitmap, ready to draw</returns>
        private Bitmap ShadeImage(Bitmap b, float angle)
        {            
            Bitmap rval = new Bitmap(b);
            Graphics g = Graphics.FromImage(rval);
            g.DrawImage(b, 0, 0);

            // Convert from radians to degrees
            float ang = angle * (float)(Math.PI / 180);
            float tan = (float)(Math.Tan(ang));
            
            // Start at the middle of the image.  I don't want to shade the part that's moving
            // towards the screen.
            int start = this.Width / 2;
            for (int i = start; i < this.Width; i++)
            {
                float x = i - (this.Width / 2);                
                // light intensity decays in proportion to the square of distance
                // i put a really big fudge factor in, otherwise it drops of really really fast
                float color = ((x * tan) * (x * tan)) / 500;
                // convert to [0...255] integers
                int intColor = (int)(color * 255);
                // clamp to valid integer color values
                if (intColor > 255)
                    intColor = 255;
                if (intColor < 0)
                    intColor = 0;
                // create a pen with the shade found from the trigonometry
                Color shade = Color.FromArgb(intColor, this.BackColor);
                g.DrawLine(new Pen(shade), i, 0, i, this.Height);
            }
            return rval;
        }

        /// <summary>
        /// tick tick tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Invalidate();
            //angleCounter++;            
            m_AngleCounter += m_XIncrement;
        }
    }

    
}
