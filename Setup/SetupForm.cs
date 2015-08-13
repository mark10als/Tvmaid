using System;
using System.IO;
using System.Windows.Forms;
using Tvmaid;

namespace Setup
{
    public partial class SetupForm : Form
    {
        DefineFile tunerDef;
        DefineFile tvmaidDef;
        string logo = "Tvmaid";

        public SetupForm()
        {
            InitializeComponent();

            var ticket = new Ticket("/tvmaid/mutex/main");
            while (ticket.GetOwner(1000) == false)
            {
                var res = MessageBox.Show("Tvmaidが起動中です。終了してください。キャンセルすると終了します。", logo, MessageBoxButtons.RetryCancel);
                if (res == DialogResult.Cancel)
                {
                    throw new AppException("Tvmaidが終了されませんでした。");
                }
            }
            ticket.Dispose();

            Util.CopyUserFile();

            tvmaidDef = new DefineFile(Path.Combine(Util.GetUserPath(), "Tvmaid.def"));
            tvmaidDef.Load();
            tunerDef = new DefineFile(Path.Combine(Util.GetUserPath(), "tuner.def"));
            tunerDef.Load();

            tvtestBox.Text = tvmaidDef["tvtest"];
            recDirBox.Text = tvmaidDef["record.folder"];

            foreach (var key in tunerDef.Keys)
            {
                tunerListBox.Items.Add(key + "=" + tunerDef[key]);
            }
        }

        private void tvtestRefButton_Click(object sender, EventArgs e)
        {
            var res = this.tvtestDialog.ShowDialog();
            if(res == DialogResult.OK)
            {
                tvtestBox.Text = tvtestDialog.FileName;
            }
        }

        private void recDirRefButton_Click(object sender, EventArgs e)
        {
            var res = recDirDialog.ShowDialog();
            if(res == DialogResult.OK)
            {
                recDirBox.Text = recDirDialog.SelectedPath;
            }
        }

        private void driverRefButton_Click(object sender, EventArgs e)
        {
            
            if(File.Exists(tvtestBox.Text))
            {
                driverDialog.InitialDirectory = Path.GetDirectoryName(tvtestBox.Text);
            }

            var res = driverDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                driverBox.Text = driverDialog.FileName;
            }
        }

        private void tunerAddButton_Click(object sender, EventArgs arg)
        {
            try
            {
                if (tunerNameBox.Text == "")
                {
                    throw new AppException("チューナ名を入力してください。");
                }
                if (tunerNameBox.Text.IndexOf('=') != -1)
                {
                    throw new AppException("チューナ名に「=」は使えません。");
                }
                if (fileNameOnlyCheck.Checked == false && File.Exists(driverBox.Text) == false)
                {
                    throw new AppException("Bonドライバのパスが間違っています。");
                }

                foreach (string item in tunerListBox.Items)
                {
                    var tuner = item.Split(new char[] { '=' });
                    if(tunerNameBox.Text == tuner[0])
                    {
                        throw new AppException("同じ名前のチューナを複数指定できません。");
                    }
                }

                var driver = fileNameOnlyCheck.Checked ? Path.GetFileName(driverBox.Text) : driverBox.Text;

                tunerListBox.Items.Add(tunerNameBox.Text + "=" + driver);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, logo);
            }
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            if (tunerListBox.SelectedIndex > 0)
            {
                var obj = tunerListBox.SelectedItem;
                tunerListBox.Items[tunerListBox.SelectedIndex] = tunerListBox.Items[tunerListBox.SelectedIndex - 1];
                tunerListBox.SelectedIndex--;
                tunerListBox.Items[tunerListBox.SelectedIndex] = obj;
            }
        }

        private void downButton_Click(object sender, EventArgs e)
        {
            if (tunerListBox.SelectedIndex > -1 && tunerListBox.SelectedIndex < tunerListBox.Items.Count - 1)
            {
                var obj = tunerListBox.SelectedItem;
                tunerListBox.Items[tunerListBox.SelectedIndex] = tunerListBox.Items[tunerListBox.SelectedIndex + 1];
                tunerListBox.SelectedIndex++;
                tunerListBox.Items[tunerListBox.SelectedIndex] = obj;
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (tunerListBox.SelectedIndex > -1)
            {
                tunerListBox.Items.RemoveAt(tunerListBox.SelectedIndex);
            }            
        }

        private void endButton_Click(object sender, EventArgs arg)
        {
            try
            {
                //フォルダが正しいか確認
                if (File.Exists(tvtestBox.Text) == false)
                {
                    throw new AppException("TVTestのパスが設定されていないか、間違っています。tvtest");
                }
                if (Directory.Exists(recDirBox.Text) == false)
                {
                    throw new AppException("録画フォルダのパスが設定されていないか、間違っています。record.folder");
                }

                //プラグインをコピーする
                try
                {
                    var plugin = Path.Combine(Util.GetBasePath(), "TvmaidPlugin.tvtp");
                    var pluginDir = Path.Combine(Path.GetDirectoryName(tvtestBox.Text), "Plugins");
                    var copyPlugin = Path.Combine(pluginDir, "TvmaidPlugin.tvtp");
                    File.Copy(plugin, copyPlugin, true);
                }
                catch (Exception e)
                {
                    throw new AppException("TVTestのプラグインフォルダに、Tvmaidプラグインをコピーできません。[詳細]" + e.Message);
                }

                //設定ファイルに入力
                tvmaidDef["tvtest"] = tvtestBox.Text;
                tvmaidDef["record.folder"] = recDirBox.Text;
                tvmaidDef.Save();

                tunerDef.Clear();
                foreach (var item in tunerListBox.Items)
                {
                    var data = ((string)item).Split(new char[] { '=' });
                    tunerDef[data[0]] = data[1];
                }
                tunerDef.Save();

                //チューナ更新モードで起動
                System.Diagnostics.Process.Start(Path.Combine(Util.GetBasePath(), "Tvmaid.exe"), "-tunerupdate");

                this.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, logo);
            }
        }
    }
}
