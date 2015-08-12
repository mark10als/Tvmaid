namespace Tvmaid
{
    partial class TunerMon
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
                sql.Dispose();
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
            this.serviceView = new System.Windows.Forms.ListView();
            this.nameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.timeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.eventHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.serviceMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.viewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.reserveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.stopRecMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.split1 = new System.Windows.Forms.SplitContainer();
            this.tunerView = new System.Windows.Forms.TreeView();
            this.split2 = new System.Windows.Forms.SplitContainer();
            this.logBox = new System.Windows.Forms.RichTextBox();
            this.serviceTimer = new System.Windows.Forms.Timer(this.components);
            this.tunerTimer = new System.Windows.Forms.Timer(this.components);
            this.logTimer = new System.Windows.Forms.Timer(this.components);
            this.logMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearLogMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serviceMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split1)).BeginInit();
            this.split1.Panel1.SuspendLayout();
            this.split1.Panel2.SuspendLayout();
            this.split1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split2)).BeginInit();
            this.split2.Panel1.SuspendLayout();
            this.split2.Panel2.SuspendLayout();
            this.split2.SuspendLayout();
            this.logMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // serviceView
            // 
            this.serviceView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.serviceView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameHeader,
            this.timeHeader,
            this.eventHeader});
            this.serviceView.ContextMenuStrip = this.serviceMenu;
            this.serviceView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.serviceView.FullRowSelect = true;
            this.serviceView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.serviceView.HideSelection = false;
            this.serviceView.Location = new System.Drawing.Point(0, 0);
            this.serviceView.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.serviceView.MultiSelect = false;
            this.serviceView.Name = "serviceView";
            this.serviceView.Size = new System.Drawing.Size(563, 255);
            this.serviceView.TabIndex = 8;
            this.serviceView.UseCompatibleStateImageBehavior = false;
            this.serviceView.View = System.Windows.Forms.View.Details;
            this.serviceView.VirtualMode = true;
            this.serviceView.ItemActivate += new System.EventHandler(this.serviceView_ItemActivate);
            this.serviceView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.serviceView_RetrieveVirtualItem);
            // 
            // nameHeader
            // 
            this.nameHeader.Text = "サービス";
            this.nameHeader.Width = 179;
            // 
            // timeHeader
            // 
            this.timeHeader.Text = "時間";
            this.timeHeader.Width = 133;
            // 
            // eventHeader
            // 
            this.eventHeader.Text = "番組";
            this.eventHeader.Width = 195;
            // 
            // serviceMenu
            // 
            this.serviceMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewMenuItem,
            this.toolStripMenuItem1,
            this.reserveMenuItem,
            this.toolStripMenuItem2,
            this.stopRecMenuItem});
            this.serviceMenu.Name = "contextMenu";
            this.serviceMenu.Size = new System.Drawing.Size(246, 88);
            // 
            // viewMenuItem
            // 
            this.viewMenuItem.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Bold);
            this.viewMenuItem.Name = "viewMenuItem";
            this.viewMenuItem.Size = new System.Drawing.Size(245, 24);
            this.viewMenuItem.Text = "見る(&V)";
            this.viewMenuItem.Click += new System.EventHandler(this.viewMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(242, 6);
            // 
            // reserveMenuItem
            // 
            this.reserveMenuItem.Name = "reserveMenuItem";
            this.reserveMenuItem.Size = new System.Drawing.Size(245, 24);
            this.reserveMenuItem.Text = "現在の番組を録画予約(&R)";
            this.reserveMenuItem.Click += new System.EventHandler(this.reserveMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(242, 6);
            // 
            // stopRecMenuItem
            // 
            this.stopRecMenuItem.Name = "stopRecMenuItem";
            this.stopRecMenuItem.Size = new System.Drawing.Size(245, 24);
            this.stopRecMenuItem.Text = "現在の録画を中断する(&A)";
            this.stopRecMenuItem.Click += new System.EventHandler(this.stopRecMenuItem_Click);
            // 
            // split1
            // 
            this.split1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.split1.Location = new System.Drawing.Point(0, 0);
            this.split1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.split1.Name = "split1";
            // 
            // split1.Panel1
            // 
            this.split1.Panel1.Controls.Add(this.tunerView);
            // 
            // split1.Panel2
            // 
            this.split1.Panel2.Controls.Add(this.serviceView);
            this.split1.Size = new System.Drawing.Size(786, 255);
            this.split1.SplitterDistance = 217;
            this.split1.SplitterWidth = 6;
            this.split1.TabIndex = 9;
            // 
            // tunerView
            // 
            this.tunerView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tunerView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tunerView.FullRowSelect = true;
            this.tunerView.HideSelection = false;
            this.tunerView.Location = new System.Drawing.Point(0, 0);
            this.tunerView.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.tunerView.Name = "tunerView";
            this.tunerView.ShowLines = false;
            this.tunerView.ShowPlusMinus = false;
            this.tunerView.ShowRootLines = false;
            this.tunerView.Size = new System.Drawing.Size(217, 255);
            this.tunerView.TabIndex = 8;
            this.tunerView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tunerView_AfterSelect);
            // 
            // split2
            // 
            this.split2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.split2.Location = new System.Drawing.Point(0, 0);
            this.split2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.split2.Name = "split2";
            this.split2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // split2.Panel1
            // 
            this.split2.Panel1.Controls.Add(this.split1);
            // 
            // split2.Panel2
            // 
            this.split2.Panel2.Controls.Add(this.logBox);
            this.split2.Size = new System.Drawing.Size(786, 385);
            this.split2.SplitterDistance = 255;
            this.split2.SplitterWidth = 6;
            this.split2.TabIndex = 11;
            // 
            // logBox
            // 
            this.logBox.BackColor = System.Drawing.SystemColors.Window;
            this.logBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logBox.ContextMenuStrip = this.logMenu;
            this.logBox.DetectUrls = false;
            this.logBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logBox.Font = new System.Drawing.Font("メイリオ", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.logBox.Location = new System.Drawing.Point(0, 0);
            this.logBox.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.logBox.Name = "logBox";
            this.logBox.ReadOnly = true;
            this.logBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.logBox.Size = new System.Drawing.Size(786, 124);
            this.logBox.TabIndex = 9;
            this.logBox.Text = "";
            // 
            // serviceTimer
            // 
            this.serviceTimer.Interval = 5000;
            this.serviceTimer.Tick += new System.EventHandler(this.serviceTimer_Tick);
            // 
            // tunerTimer
            // 
            this.tunerTimer.Interval = 2000;
            this.tunerTimer.Tick += new System.EventHandler(this.tunerTimer_Tick);
            // 
            // logTimer
            // 
            this.logTimer.Interval = 1000;
            this.logTimer.Tick += new System.EventHandler(this.logTimer_Tick);
            // 
            // logMenu
            // 
            this.logMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearLogMenuItem});
            this.logMenu.Name = "contextMenu";
            this.logMenu.Size = new System.Drawing.Size(176, 56);
            // 
            // clearLogMenuItem
            // 
            this.clearLogMenuItem.Name = "clearLogMenuItem";
            this.clearLogMenuItem.Size = new System.Drawing.Size(175, 24);
            this.clearLogMenuItem.Text = "すべてクリア(&C)";
            this.clearLogMenuItem.Click += new System.EventHandler(this.clearLogMenuItem_Click);
            // 
            // TunerMon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(786, 385);
            this.Controls.Add(this.split2);
            this.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TunerMon";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "チューナモニタ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MonitorForm_FormClosing);
            this.VisibleChanged += new System.EventHandler(this.TunerMon_VisibleChanged);
            this.serviceMenu.ResumeLayout(false);
            this.split1.Panel1.ResumeLayout(false);
            this.split1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.split1)).EndInit();
            this.split1.ResumeLayout(false);
            this.split2.Panel1.ResumeLayout(false);
            this.split2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.split2)).EndInit();
            this.split2.ResumeLayout(false);
            this.logMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView serviceView;
        private System.Windows.Forms.SplitContainer split1;
        private System.Windows.Forms.SplitContainer split2;
        private System.Windows.Forms.Timer serviceTimer;
        private System.Windows.Forms.ColumnHeader nameHeader;
        private System.Windows.Forms.ColumnHeader timeHeader;
        private System.Windows.Forms.ColumnHeader eventHeader;
        private System.Windows.Forms.TreeView tunerView;
        private System.Windows.Forms.RichTextBox logBox;
        private System.Windows.Forms.ContextMenuStrip serviceMenu;
        private System.Windows.Forms.ToolStripMenuItem viewMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem reserveMenuItem;
        private System.Windows.Forms.Timer tunerTimer;
        private System.Windows.Forms.Timer logTimer;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem stopRecMenuItem;
        private System.Windows.Forms.ContextMenuStrip logMenu;
        private System.Windows.Forms.ToolStripMenuItem clearLogMenuItem;
    }
}