namespace Tvmaid
{
    partial class Tasktray
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateTunerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.stopEpgMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateEpgMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.tunerMonMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.test2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sleepTimer = new System.Windows.Forms.Timer(this.components);
            this.sleepMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenu;
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitMenuItem,
            this.updateTunerMenuItem,
            this.toolStripMenuItem3,
            this.stopEpgMenuItem,
            this.updateEpgMenuItem,
            this.toolStripMenuItem2,
            this.sleepMenuItem,
            this.toolStripMenuItem1,
            this.tunerMonMenuItem,
            this.testToolStripMenuItem,
            this.test2ToolStripMenuItem});
            this.contextMenu.Name = "contextMenuStrip";
            this.contextMenu.Size = new System.Drawing.Size(219, 214);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(218, 24);
            this.exitMenuItem.Text = "終了(&X)";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // updateTunerMenuItem
            // 
            this.updateTunerMenuItem.Name = "updateTunerMenuItem";
            this.updateTunerMenuItem.Size = new System.Drawing.Size(218, 24);
            this.updateTunerMenuItem.Text = "チューナ更新(&T)";
            this.updateTunerMenuItem.Click += new System.EventHandler(this.updateTunerMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(215, 6);
            // 
            // stopEpgMenuItem
            // 
            this.stopEpgMenuItem.Name = "stopEpgMenuItem";
            this.stopEpgMenuItem.Size = new System.Drawing.Size(218, 24);
            this.stopEpgMenuItem.Text = "番組表取得を中止(&A)";
            this.stopEpgMenuItem.Click += new System.EventHandler(this.stopEpgMenuItem_Click);
            // 
            // updateEpgMenuItem
            // 
            this.updateEpgMenuItem.Name = "updateEpgMenuItem";
            this.updateEpgMenuItem.Size = new System.Drawing.Size(218, 24);
            this.updateEpgMenuItem.Text = "番組表取得(&E)";
            this.updateEpgMenuItem.Click += new System.EventHandler(this.updateEpgMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(215, 6);
            // 
            // tunerMonMenuItem
            // 
            this.tunerMonMenuItem.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Bold);
            this.tunerMonMenuItem.Name = "tunerMonMenuItem";
            this.tunerMonMenuItem.Size = new System.Drawing.Size(218, 24);
            this.tunerMonMenuItem.Text = "チューナモニタ(&M)";
            this.tunerMonMenuItem.Click += new System.EventHandler(this.tunerMonMenuItem_Click);
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(218, 24);
            this.testToolStripMenuItem.Text = "test";
            this.testToolStripMenuItem.Visible = false;
            this.testToolStripMenuItem.Click += new System.EventHandler(this.testToolStripMenuItem_Click);
            // 
            // test2ToolStripMenuItem
            // 
            this.test2ToolStripMenuItem.Name = "test2ToolStripMenuItem";
            this.test2ToolStripMenuItem.Size = new System.Drawing.Size(218, 24);
            this.test2ToolStripMenuItem.Text = "test2";
            this.test2ToolStripMenuItem.Visible = false;
            this.test2ToolStripMenuItem.Click += new System.EventHandler(this.test2ToolStripMenuItem_Click);
            // 
            // sleepTimer
            // 
            this.sleepTimer.Interval = 30000;
            this.sleepTimer.Tick += new System.EventHandler(this.sleepTimer_Tick);
            // 
            // sleepMenuItem
            // 
            this.sleepMenuItem.Name = "sleepMenuItem";
            this.sleepMenuItem.Size = new System.Drawing.Size(218, 24);
            this.sleepMenuItem.Text = "スリープ(&S)";
            this.sleepMenuItem.Click += new System.EventHandler(this.sleepMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(215, 6);
            // 
            // Tasktray
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(164, 29);
            this.Location = new System.Drawing.Point(-1000, -1000);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Tasktray";
            this.Opacity = 0D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Tasktray_FormClosing);
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateEpgMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tunerMonMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem test2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateTunerMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopEpgMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.Timer sleepTimer;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem sleepMenuItem;
    }
}

