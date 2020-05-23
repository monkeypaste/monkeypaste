using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace TestApp
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{      
      private System.Windows.Forms.PropertyGrid propertyGrid1;
      private MJW.Guardian.RuntimeComponents.ShadowLabel shadowLabel1;
      private System.ComponentModel.IContainer components;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
         this.components = new System.ComponentModel.Container();
         this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
         this.shadowLabel1 = new MJW.Guardian.RuntimeComponents.ShadowLabel(this.components);
         this.SuspendLayout();
         // 
         // propertyGrid1
         // 
         this.propertyGrid1.CommandsVisibleIfAvailable = true;
         this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Right;
         this.propertyGrid1.LargeButtons = false;
         this.propertyGrid1.LineColor = System.Drawing.SystemColors.ScrollBar;
         this.propertyGrid1.Location = new System.Drawing.Point(268, 0);
         this.propertyGrid1.Name = "propertyGrid1";
         this.propertyGrid1.SelectedObject = this.shadowLabel1;
         this.propertyGrid1.Size = new System.Drawing.Size(392, 405);
         this.propertyGrid1.TabIndex = 1;
         this.propertyGrid1.Text = "propertyGrid1";
         this.propertyGrid1.ViewBackColor = System.Drawing.SystemColors.Window;
         this.propertyGrid1.ViewForeColor = System.Drawing.SystemColors.WindowText;
         // 
         // shadowLabel1
         // 
         this.shadowLabel1.Angle = 0F;
         this.shadowLabel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.shadowLabel1.EndColor = System.Drawing.Color.LightSkyBlue;
         this.shadowLabel1.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
         this.shadowLabel1.ForeColor = System.Drawing.Color.LightSkyBlue;
         this.shadowLabel1.Location = new System.Drawing.Point(20, 16);
         this.shadowLabel1.Name = "shadowLabel1";
         this.shadowLabel1.ShadowColor = System.Drawing.Color.Black;
         this.shadowLabel1.Size = new System.Drawing.Size(216, 44);
         this.shadowLabel1.StartColor = System.Drawing.Color.White;
         this.shadowLabel1.TabIndex = 2;
         this.shadowLabel1.Text = "shadowLabel1";
         this.shadowLabel1.XOffset = 1F;
         this.shadowLabel1.YOffset = 1F;
         // 
         // Form1
         // 
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.ClientSize = new System.Drawing.Size(660, 405);
         this.Controls.Add(this.shadowLabel1);
         this.Controls.Add(this.propertyGrid1);
         this.Name = "Form1";
         this.Text = "Shadow Label Demo Application";
         this.ResumeLayout(false);

      }
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}
	}
}
