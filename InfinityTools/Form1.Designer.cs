namespace InfinityTools
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startRelayChannel1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopRelayChannel1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.channel1TestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.resetCredentialsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateUserTokenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(986, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debugToolStripMenuItem,
            this.updateUserTokenToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(62, 22);
            this.toolStripDropDownButton1.Text = "Options";
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startRelayChannel1ToolStripMenuItem,
            this.stopRelayChannel1ToolStripMenuItem,
            this.channel1TestToolStripMenuItem,
            this.resetCredentialsToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // startRelayChannel1ToolStripMenuItem
            // 
            this.startRelayChannel1ToolStripMenuItem.Name = "startRelayChannel1ToolStripMenuItem";
            this.startRelayChannel1ToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.startRelayChannel1ToolStripMenuItem.Text = "Start Relay Channel 1";
            this.startRelayChannel1ToolStripMenuItem.Click += new System.EventHandler(this.startRelayChannel1ToolStripMenuItem_Click);
            // 
            // stopRelayChannel1ToolStripMenuItem
            // 
            this.stopRelayChannel1ToolStripMenuItem.Name = "stopRelayChannel1ToolStripMenuItem";
            this.stopRelayChannel1ToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.stopRelayChannel1ToolStripMenuItem.Text = "Stop Relay Channel 1";
            this.stopRelayChannel1ToolStripMenuItem.Click += new System.EventHandler(this.stopRelayChannel1ToolStripMenuItem_Click);
            // 
            // channel1TestToolStripMenuItem
            // 
            this.channel1TestToolStripMenuItem.Name = "channel1TestToolStripMenuItem";
            this.channel1TestToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.channel1TestToolStripMenuItem.Text = "Channel 1 Test";
            this.channel1TestToolStripMenuItem.Click += new System.EventHandler(this.channel1TestToolStripMenuItem_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton1.BackColor = System.Drawing.Color.IndianRed;
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(29, 22);
            this.toolStripButton1.Text = "Exit";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 25);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(986, 106);
            this.panel1.TabIndex = 1;
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.InfoText;
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.ForeColor = System.Drawing.SystemColors.Menu;
            this.richTextBox1.Location = new System.Drawing.Point(0, 131);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(986, 360);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            // 
            // resetCredentialsToolStripMenuItem
            // 
            this.resetCredentialsToolStripMenuItem.Name = "resetCredentialsToolStripMenuItem";
            this.resetCredentialsToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.resetCredentialsToolStripMenuItem.Text = "Reset Credentials";
            this.resetCredentialsToolStripMenuItem.Click += new System.EventHandler(this.resetCredentialsToolStripMenuItem_Click);
            // 
            // updateUserTokenToolStripMenuItem
            // 
            this.updateUserTokenToolStripMenuItem.Name = "updateUserTokenToolStripMenuItem";
            this.updateUserTokenToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.updateUserTokenToolStripMenuItem.Text = "Update User Token";
            this.updateUserTokenToolStripMenuItem.Click += new System.EventHandler(this.updateUserTokenToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(986, 491);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "Form1";
            this.Text = "InfinityTools";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem startRelayChannel1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopRelayChannel1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem channel1TestToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetCredentialsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateUserTokenToolStripMenuItem;
    }
}

