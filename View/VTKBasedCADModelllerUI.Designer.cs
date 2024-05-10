
using Kitware.VTK;

namespace WindowsFormsApp1
{
    partial class VTKBasedCADModelllerUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code     
       
        private void InitializeComponent()
        {
            this.renderWindowControl1 = new Kitware.VTK.RenderWindowControl();
            this.sketchButton = new System.Windows.Forms.Button();
            this.viewButton = new System.Windows.Forms.Button();
            this.circleButton = new System.Windows.Forms.Button();
            this.lineButton = new System.Windows.Forms.Button();
            this.pointButton = new System.Windows.Forms.Button();
            this.arcButton = new System.Windows.Forms.Button();
            this.ellipseButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.xyButton = new System.Windows.Forms.Button();
            this.yzButton = new System.Windows.Forms.Button();
            this.zxButton = new System.Windows.Forms.Button();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // renderWindowControl1
            // 
            this.renderWindowControl1.AddTestActors = false;
            this.renderWindowControl1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.renderWindowControl1.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.renderWindowControl1.Location = new System.Drawing.Point(13, 47);
            this.renderWindowControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.renderWindowControl1.Name = "renderWindowControl1";
            this.renderWindowControl1.Size = new System.Drawing.Size(872, 459);
            this.renderWindowControl1.TabIndex = 0;
            this.renderWindowControl1.TestText = null;
            this.renderWindowControl1.Load += new System.EventHandler(this.RenderWindowControl1_Load);
            // 
            // sketchButton
            // 
            this.sketchButton.Location = new System.Drawing.Point(10, 10);
            this.sketchButton.Name = "sketchButton";
            this.sketchButton.Size = new System.Drawing.Size(75, 23);
            this.sketchButton.TabIndex = 1;
            this.sketchButton.Text = "Sketch";
            this.sketchButton.Click += new System.EventHandler(this.SketchButton_Click);
            // 
            // viewButton
            // 
            this.viewButton.Location = new System.Drawing.Point(90, 10);
            this.viewButton.Name = "viewButton";
            this.viewButton.Size = new System.Drawing.Size(75, 23);
            this.viewButton.TabIndex = 2;
            this.viewButton.Text = "View";
            this.viewButton.Click += new System.EventHandler(this.ViewButton_Click);
            // 
            // circleButton
            // 
            this.circleButton.Location = new System.Drawing.Point(170, 10);
            this.circleButton.Name = "circleButton";
            this.circleButton.Size = new System.Drawing.Size(75, 23);
            this.circleButton.TabIndex = 3;
            this.circleButton.Text = "Circle";
            this.circleButton.Click += new System.EventHandler(this.CircleButton_Click);
            // 
            // lineButton
            // 
            this.lineButton.Location = new System.Drawing.Point(250, 10);
            this.lineButton.Name = "lineButton";
            this.lineButton.Size = new System.Drawing.Size(75, 23);
            this.lineButton.TabIndex = 4;
            this.lineButton.Text = "Line";
            this.lineButton.Click += new System.EventHandler(this.LineButton_Click);
            // 
            // pointButton
            // 
            this.pointButton.Location = new System.Drawing.Point(330, 10);
            this.pointButton.Name = "pointButton";
            this.pointButton.Size = new System.Drawing.Size(75, 23);
            this.pointButton.TabIndex = 5;
            this.pointButton.Text = "Point";
            this.pointButton.Click += new System.EventHandler(this.PointButton_Click);
            // 
            // arcButton
            // 
            this.arcButton.Location = new System.Drawing.Point(410, 10);
            this.arcButton.Name = "arcButton";
            this.arcButton.Size = new System.Drawing.Size(75, 23);
            this.arcButton.TabIndex = 6;
            this.arcButton.Text = "Arc";
            this.arcButton.Click += new System.EventHandler(this.ArcButton_Click);
            // 
            // ellipseButton
            // 
            this.ellipseButton.Location = new System.Drawing.Point(490, 10);
            this.ellipseButton.Name = "ellipseButton";
            this.ellipseButton.Size = new System.Drawing.Size(75, 23);
            this.ellipseButton.TabIndex = 7;
            this.ellipseButton.Text = "Ellipse";
            this.ellipseButton.Click += new System.EventHandler(this.EllipseButton_Click);
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(570, 10);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex = 8;
            this.deleteButton.Text = "Delete";
            this.deleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // xyButton
            // 
            this.xyButton.Location = new System.Drawing.Point(650, 10);
            this.xyButton.Name = "xyButton";
            this.xyButton.Size = new System.Drawing.Size(75, 23);
            this.xyButton.TabIndex = 9;
            this.xyButton.Text = "XY";
            this.xyButton.Click += new System.EventHandler(this.XYButton_Click);
            // 
            // yzButton
            // 
            this.yzButton.Location = new System.Drawing.Point(730, 10);
            this.yzButton.Name = "yzButton";
            this.yzButton.Size = new System.Drawing.Size(75, 23);
            this.yzButton.TabIndex = 10;
            this.yzButton.Text = "YZ";
            this.yzButton.Click += new System.EventHandler(this.YZButton_Click);
            // 
            // zxButton
            // 
            this.zxButton.Location = new System.Drawing.Point(810, 10);
            this.zxButton.Name = "zxButton";
            this.zxButton.Size = new System.Drawing.Size(75, 23);
            this.zxButton.TabIndex = 11;
            this.zxButton.Text = "ZX";
            this.zxButton.Click += new System.EventHandler(this.ZXButton_Click);
            // 
            // outputTextBox
            // 
            this.outputTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.outputTextBox.Location = new System.Drawing.Point(0, 513);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputTextBox.Size = new System.Drawing.Size(923, 150);
            this.outputTextBox.TabIndex = 4;
            // 
            // VTKBasedCADModelllerUI
            // 
            this.ClientSize = new System.Drawing.Size(923, 663);
            this.Controls.Add(this.outputTextBox);
            this.Controls.Add(this.renderWindowControl1);
            this.Controls.Add(this.sketchButton);
            this.Controls.Add(this.viewButton);
            this.Controls.Add(this.circleButton);
            this.Controls.Add(this.lineButton);
            this.Controls.Add(this.pointButton);
            this.Controls.Add(this.arcButton);
            this.Controls.Add(this.ellipseButton);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.xyButton);
            this.Controls.Add(this.yzButton);
            this.Controls.Add(this.zxButton);
            this.Name = "VTKBasedCADModelllerUI";
            this.Text = "Primitive CAD Modeler";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Kitware.VTK.RenderWindowControl renderWindowControl1;
        private System.Windows.Forms.Button sketchButton;
        private System.Windows.Forms.Button viewButton;
        private System.Windows.Forms.Button circleButton;
        private System.Windows.Forms.Button lineButton;
        private System.Windows.Forms.Button pointButton;
        private System.Windows.Forms.Button arcButton;
        private System.Windows.Forms.Button ellipseButton;
        private System.Windows.Forms.Button deleteButton; // New delete button
        private System.Windows.Forms.Button xyButton; // XY button
        private System.Windows.Forms.Button yzButton; // YZ button
        private System.Windows.Forms.Button zxButton; // ZX button
         private System.Windows.Forms.Button saveButton; // Declare the Save button


       

    }
}
