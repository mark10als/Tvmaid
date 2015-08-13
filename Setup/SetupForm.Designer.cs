namespace Setup
{
    partial class SetupForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupForm));
            this.label1 = new System.Windows.Forms.Label();
            this.tvtestBox = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.recDirRefButton = new System.Windows.Forms.Button();
            this.tvtestRefButton = new System.Windows.Forms.Button();
            this.recDirBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.fileNameOnlyCheck = new System.Windows.Forms.CheckBox();
            this.driverRefButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.driverBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tunerAddButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.downButton = new System.Windows.Forms.Button();
            this.upButton = new System.Windows.Forms.Button();
            this.tunerListBox = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tunerNameBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.endButton = new System.Windows.Forms.Button();
            this.tvtestDialog = new System.Windows.Forms.SaveFileDialog();
            this.recDirDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.driverDialog = new System.Windows.Forms.SaveFileDialog();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 27);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "TVTestの場所";
            // 
            // tvtestBox
            // 
            this.tvtestBox.Location = new System.Drawing.Point(107, 25);
            this.tvtestBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tvtestBox.Name = "tvtestBox";
            this.tvtestBox.Size = new System.Drawing.Size(394, 25);
            this.tvtestBox.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.recDirRefButton);
            this.panel1.Controls.Add(this.tvtestRefButton);
            this.panel1.Controls.Add(this.recDirBox);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.tvtestBox);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(18, 18);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(624, 111);
            this.panel1.TabIndex = 2;
            // 
            // recDirRefButton
            // 
            this.recDirRefButton.BackColor = System.Drawing.SystemColors.Control;
            this.recDirRefButton.Location = new System.Drawing.Point(505, 62);
            this.recDirRefButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.recDirRefButton.Name = "recDirRefButton";
            this.recDirRefButton.Size = new System.Drawing.Size(91, 24);
            this.recDirRefButton.TabIndex = 5;
            this.recDirRefButton.Text = "参照...";
            this.recDirRefButton.UseVisualStyleBackColor = false;
            this.recDirRefButton.Click += new System.EventHandler(this.recDirRefButton_Click);
            // 
            // tvtestRefButton
            // 
            this.tvtestRefButton.BackColor = System.Drawing.SystemColors.Control;
            this.tvtestRefButton.Location = new System.Drawing.Point(505, 24);
            this.tvtestRefButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tvtestRefButton.Name = "tvtestRefButton";
            this.tvtestRefButton.Size = new System.Drawing.Size(91, 24);
            this.tvtestRefButton.TabIndex = 4;
            this.tvtestRefButton.Text = "参照...";
            this.tvtestRefButton.UseVisualStyleBackColor = false;
            this.tvtestRefButton.Click += new System.EventHandler(this.tvtestRefButton_Click);
            // 
            // recDirBox
            // 
            this.recDirBox.Location = new System.Drawing.Point(107, 62);
            this.recDirBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.recDirBox.Name = "recDirBox";
            this.recDirBox.Size = new System.Drawing.Size(394, 25);
            this.recDirBox.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(34, 65);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 18);
            this.label3.TabIndex = 2;
            this.label3.Text = "録画の場所";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.fileNameOnlyCheck);
            this.panel2.Controls.Add(this.driverRefButton);
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.driverBox);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.tunerAddButton);
            this.panel2.Controls.Add(this.removeButton);
            this.panel2.Controls.Add(this.downButton);
            this.panel2.Controls.Add(this.upButton);
            this.panel2.Controls.Add(this.tunerListBox);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.tunerNameBox);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Location = new System.Drawing.Point(18, 146);
            this.panel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(624, 354);
            this.panel2.TabIndex = 3;
            // 
            // fileNameOnlyCheck
            // 
            this.fileNameOnlyCheck.AutoSize = true;
            this.fileNameOnlyCheck.Location = new System.Drawing.Point(107, 128);
            this.fileNameOnlyCheck.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.fileNameOnlyCheck.Name = "fileNameOnlyCheck";
            this.fileNameOnlyCheck.Size = new System.Drawing.Size(407, 22);
            this.fileNameOnlyCheck.TabIndex = 15;
            this.fileNameOnlyCheck.Text = "Bonドライバはファイル名のみを追加(ファイルの存在確認をしません)";
            this.fileNameOnlyCheck.UseVisualStyleBackColor = true;
            // 
            // driverRefButton
            // 
            this.driverRefButton.BackColor = System.Drawing.SystemColors.Control;
            this.driverRefButton.Location = new System.Drawing.Point(505, 55);
            this.driverRefButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.driverRefButton.Name = "driverRefButton";
            this.driverRefButton.Size = new System.Drawing.Size(91, 24);
            this.driverRefButton.TabIndex = 13;
            this.driverRefButton.Text = "参照...";
            this.driverRefButton.UseVisualStyleBackColor = false;
            this.driverRefButton.Click += new System.EventHandler(this.driverRefButton_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(46, 162);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 18);
            this.label7.TabIndex = 12;
            this.label7.Text = "チューナ";
            // 
            // driverBox
            // 
            this.driverBox.Location = new System.Drawing.Point(107, 56);
            this.driverBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.driverBox.Name = "driverBox";
            this.driverBox.Size = new System.Drawing.Size(394, 25);
            this.driverBox.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(34, 20);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(390, 18);
            this.label6.TabIndex = 10;
            this.label6.Text = "チューナ名、Bonドライバを指定して、追加ボタンを押してください。";
            // 
            // tunerAddButton
            // 
            this.tunerAddButton.BackColor = System.Drawing.SystemColors.Control;
            this.tunerAddButton.Location = new System.Drawing.Point(505, 91);
            this.tunerAddButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tunerAddButton.Name = "tunerAddButton";
            this.tunerAddButton.Size = new System.Drawing.Size(91, 24);
            this.tunerAddButton.TabIndex = 9;
            this.tunerAddButton.Text = "追加";
            this.tunerAddButton.UseVisualStyleBackColor = false;
            this.tunerAddButton.Click += new System.EventHandler(this.tunerAddButton_Click);
            // 
            // removeButton
            // 
            this.removeButton.BackColor = System.Drawing.SystemColors.Control;
            this.removeButton.Location = new System.Drawing.Point(505, 239);
            this.removeButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(91, 24);
            this.removeButton.TabIndex = 8;
            this.removeButton.Text = "削除";
            this.removeButton.UseVisualStyleBackColor = false;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // downButton
            // 
            this.downButton.BackColor = System.Drawing.SystemColors.Control;
            this.downButton.Location = new System.Drawing.Point(505, 199);
            this.downButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.downButton.Name = "downButton";
            this.downButton.Size = new System.Drawing.Size(91, 24);
            this.downButton.TabIndex = 7;
            this.downButton.Text = "下へ";
            this.downButton.UseVisualStyleBackColor = false;
            this.downButton.Click += new System.EventHandler(this.downButton_Click);
            // 
            // upButton
            // 
            this.upButton.BackColor = System.Drawing.SystemColors.Control;
            this.upButton.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.upButton.Location = new System.Drawing.Point(505, 162);
            this.upButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.upButton.Name = "upButton";
            this.upButton.Size = new System.Drawing.Size(91, 24);
            this.upButton.TabIndex = 6;
            this.upButton.Text = "上へ";
            this.upButton.UseVisualStyleBackColor = false;
            this.upButton.Click += new System.EventHandler(this.upButton_Click);
            // 
            // tunerListBox
            // 
            this.tunerListBox.FormattingEnabled = true;
            this.tunerListBox.HorizontalScrollbar = true;
            this.tunerListBox.ItemHeight = 18;
            this.tunerListBox.Location = new System.Drawing.Point(107, 162);
            this.tunerListBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tunerListBox.Name = "tunerListBox";
            this.tunerListBox.Size = new System.Drawing.Size(394, 166);
            this.tunerListBox.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(24, 58);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 18);
            this.label5.TabIndex = 3;
            this.label5.Text = "Bonドライバ";
            // 
            // tunerNameBox
            // 
            this.tunerNameBox.Location = new System.Drawing.Point(107, 92);
            this.tunerNameBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tunerNameBox.Name = "tunerNameBox";
            this.tunerNameBox.Size = new System.Drawing.Size(394, 25);
            this.tunerNameBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 94);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 18);
            this.label2.TabIndex = 0;
            this.label2.Text = "チューナ名";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ForeColor = System.Drawing.Color.Red;
            this.label8.Location = new System.Drawing.Point(63, 64);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(524, 18);
            this.label8.TabIndex = 14;
            this.label8.Text = "注意！チューナ名を変更したり、チューナを削除すると、それに関連する予約も削除されます。";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.label8);
            this.panel3.Controls.Add(this.endButton);
            this.panel3.Location = new System.Drawing.Point(18, 515);
            this.panel3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(624, 140);
            this.panel3.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(147, 18);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(332, 36);
            this.label4.TabIndex = 7;
            this.label4.Text = "上記の設定でよければ、設定完了ボタンを押してください。\r\nチューナ設定を更新し、Tvmaidが起動します。";
            // 
            // endButton
            // 
            this.endButton.BackColor = System.Drawing.SystemColors.Control;
            this.endButton.Location = new System.Drawing.Point(256, 102);
            this.endButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.endButton.Name = "endButton";
            this.endButton.Size = new System.Drawing.Size(91, 24);
            this.endButton.TabIndex = 6;
            this.endButton.Text = "設定完了";
            this.endButton.UseVisualStyleBackColor = false;
            this.endButton.Click += new System.EventHandler(this.endButton_Click);
            // 
            // tvtestDialog
            // 
            this.tvtestDialog.CheckFileExists = true;
            this.tvtestDialog.Filter = "TVTest|TVTest.exe";
            this.tvtestDialog.OverwritePrompt = false;
            this.tvtestDialog.Title = "TVTestの場所";
            // 
            // recDirDialog
            // 
            this.recDirDialog.Description = "録画フォルダを選択してください。";
            this.recDirDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // driverDialog
            // 
            this.driverDialog.CheckFileExists = true;
            this.driverDialog.Filter = "BonDriver|Bondriver*.dll";
            this.driverDialog.OverwritePrompt = false;
            this.driverDialog.Title = "Bonドライバ";
            // 
            // panel4
            // 
            this.panel4.Location = new System.Drawing.Point(18, 660);
            this.panel4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(624, 22);
            this.panel4.TabIndex = 5;
            // 
            // SetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(701, 368);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tvmaid セットアップ";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tvtestBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button recDirRefButton;
        private System.Windows.Forms.Button tvtestRefButton;
        private System.Windows.Forms.TextBox recDirBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button tunerAddButton;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Button downButton;
        private System.Windows.Forms.Button upButton;
        private System.Windows.Forms.ListBox tunerListBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tunerNameBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button endButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.SaveFileDialog tvtestDialog;
        private System.Windows.Forms.FolderBrowserDialog recDirDialog;
        private System.Windows.Forms.Button driverRefButton;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox driverBox;
        private System.Windows.Forms.SaveFileDialog driverDialog;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox fileNameOnlyCheck;
    }
}

