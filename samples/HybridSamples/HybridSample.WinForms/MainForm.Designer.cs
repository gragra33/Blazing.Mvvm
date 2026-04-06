using Microsoft.AspNetCore.Components.WebView.WindowsForms;

namespace HybridSample.WinForms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// The BlazorWebView control.
        /// </summary>
        private BlazorWebView blazorWebView1;

        /// <summary>
        /// The navigation panel.
        /// </summary>
        private Panel navigationPanel;

        /// <summary>
        /// The main content panel.
        /// </summary>
        private Panel contentPanel;

        /// <summary>
        /// The status label.
        /// </summary>
        private Label statusLabel;

        /// <summary>
        /// Splitter control to allow resizing between navigation and content.
        /// </summary>
        private Splitter splitter1;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // First dispose the BlazorWebView to prevent project GUID error
                try
                {
                    if (blazorWebView1 != null)
                    {
                        blazorWebView1.RootComponents.Clear();
                        blazorWebView1.Services = null;
                        blazorWebView1.Dispose();
                        blazorWebView1 = null;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing BlazorWebView: {ex.Message}");
                }
                
                // Then dispose the components container
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            navigationPanel = new Panel();
            splitter1 = new Splitter();
            contentPanel = new Panel();
            blazorWebView1 = new BlazorWebView();
            statusLabel = new Label();
            contentPanel.SuspendLayout();
            SuspendLayout();
            // 
            // navigationPanel
            // 
            navigationPanel.AutoScroll = true;
            navigationPanel.BackColor = SystemColors.ControlLight;
            navigationPanel.BorderStyle = BorderStyle.FixedSingle;
            navigationPanel.Dock = DockStyle.Left;
            navigationPanel.Location = new Point(0, 0);
            navigationPanel.Name = "navigationPanel";
            navigationPanel.Padding = new Padding(5);
            navigationPanel.Size = new Size(200, 775);
            navigationPanel.TabIndex = 2;
            // 
            // splitter1
            // 
            splitter1.BackColor = SystemColors.ControlDark;
            splitter1.Location = new Point(200, 0);
            splitter1.Name = "splitter1";
            splitter1.Size = new Size(3, 775);
            splitter1.TabIndex = 1;
            splitter1.TabStop = false;
            // 
            // contentPanel
            // 
            contentPanel.Controls.Add(blazorWebView1);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(203, 0);
            contentPanel.Name = "contentPanel";
            contentPanel.Size = new Size(997, 775);
            contentPanel.TabIndex = 0;
            // 
            // blazorWebView1
            // 
            blazorWebView1.Dock = DockStyle.Fill;
            blazorWebView1.Location = new Point(0, 0);
            blazorWebView1.Name = "blazorWebView1";
            blazorWebView1.Padding = new Padding(5);
            blazorWebView1.Size = new Size(997, 775);
            blazorWebView1.StartPath = "/";
            blazorWebView1.TabIndex = 0;
            blazorWebView1.Text = "blazorWebView1";
            // 
            // statusLabel
            // 
            statusLabel.BackColor = SystemColors.ControlLight;
            statusLabel.BorderStyle = BorderStyle.FixedSingle;
            statusLabel.Dock = DockStyle.Bottom;
            statusLabel.Font = new Font("Segoe UI", 8F);
            statusLabel.Location = new Point(0, 775);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(1200, 25);
            statusLabel.TabIndex = 3;
            statusLabel.Text = "Click on the BlazorWebView control, then CTRL-SHIFT-I or F12 to open the Browser DevTools window...";
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 800);
            Controls.Add(contentPanel);
            Controls.Add(splitter1);
            Controls.Add(navigationPanel);
            Controls.Add(statusLabel);
            MinimumSize = new Size(800, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "WinForms MVVM Blazor Hybrid Sample Application";
            WindowState = FormWindowState.Maximized;
            contentPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}