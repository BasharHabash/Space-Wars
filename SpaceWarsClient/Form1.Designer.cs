namespace SpaceWars
{
    partial class Form1
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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label_Server = new System.Windows.Forms.Label();
            this.textBox_Server = new System.Windows.Forms.TextBox();
            this.textBox_Name = new System.Windows.Forms.TextBox();
            this.label_Name = new System.Windows.Forms.Label();
            this.button_Connect = new System.Windows.Forms.Button();
            this.button_Controls = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_Server
            // 
            this.label_Server.AutoSize = true;
            this.label_Server.Location = new System.Drawing.Point(12, 9);
            this.label_Server.Name = "label_Server";
            this.label_Server.Size = new System.Drawing.Size(41, 13);
            this.label_Server.TabIndex = 0;
            this.label_Server.Text = "Server:";
            // 
            // textBox_Server
            // 
            this.textBox_Server.Location = new System.Drawing.Point(59, 6);
            this.textBox_Server.Name = "textBox_Server";
            this.textBox_Server.Size = new System.Drawing.Size(192, 20);
            this.textBox_Server.TabIndex = 1;
            this.textBox_Server.Text = "localhost";
            // 
            // textBox_Name
            // 
            this.textBox_Name.Location = new System.Drawing.Point(301, 6);
            this.textBox_Name.Name = "textBox_Name";
            this.textBox_Name.Size = new System.Drawing.Size(118, 20);
            this.textBox_Name.TabIndex = 2;
            // 
            // label_Name
            // 
            this.label_Name.AutoSize = true;
            this.label_Name.Location = new System.Drawing.Point(257, 9);
            this.label_Name.Name = "label_Name";
            this.label_Name.Size = new System.Drawing.Size(38, 13);
            this.label_Name.TabIndex = 3;
            this.label_Name.Text = "Name:";
            // 
            // button_Connect
            // 
            this.button_Connect.Location = new System.Drawing.Point(434, 4);
            this.button_Connect.Name = "button_Connect";
            this.button_Connect.Size = new System.Drawing.Size(75, 23);
            this.button_Connect.TabIndex = 4;
            this.button_Connect.Text = "Connect";
            this.button_Connect.UseVisualStyleBackColor = true;
            this.button_Connect.Click += new System.EventHandler(this.button_Connect_Click);
            // 
            // button_Controls
            // 
            this.button_Controls.Location = new System.Drawing.Point(859, 4);
            this.button_Controls.Name = "button_Controls";
            this.button_Controls.Size = new System.Drawing.Size(75, 23);
            this.button_Controls.TabIndex = 5;
            this.button_Controls.Text = "Controls";
            this.button_Controls.UseVisualStyleBackColor = true;
            this.button_Controls.MouseClick += new System.Windows.Forms.MouseEventHandler(this.button_Controls_MouseClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 761);
            this.Controls.Add(this.button_Controls);
            this.Controls.Add(this.button_Connect);
            this.Controls.Add(this.label_Name);
            this.Controls.Add(this.textBox_Name);
            this.Controls.Add(this.textBox_Server);
            this.Controls.Add(this.label_Server);
            this.Name = "Form1";
            this.Text = "Space Case Invaders";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.Form1_PreviewKeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label_Server;
        private System.Windows.Forms.TextBox textBox_Server;
        private System.Windows.Forms.TextBox textBox_Name;
        private System.Windows.Forms.Label label_Name;
        private System.Windows.Forms.Button button_Connect;
        private System.Windows.Forms.Button button_Controls;
    }
}

