using System;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tvmaid;
using Codeplex.Data;

namespace Maidmon
{
    public partial class RecordMon : Form
    {
        List<Record> records = new List<Record>();  //予約リスト
        List<string> hosts = new List<string>();    //ホストリスト
        Dictionary<string, bool> actives = new Dictionary<string, bool>();  //ホストがアクティブかを記録
        object downloadLock = 0;    //ダウンロード用ロックオブジェクト
        DefineFile stateDef;   //状態保存ファイル

        //表示用予約
        class Record
        {
            public int Id;
            public string Title;
            public string Service;
            public DateTime Start;
            public DateTime End;
            public int Status;
            public string Host;
            public string Tuner;
            public string Time;
        }

        enum RecStatus
        {
            Enable = 1,
            EventMode = 2,
            Duplication = 32,
            Recoding = 64,
            Complete = 128
        };

        public RecordMon()
        {
            InitializeComponent();

            try
            {
                this.Icon = new System.Drawing.Icon(System.IO.Path.Combine(Util.GetUserPath(), "Tvmaid.ico"));
                EnableDoubleBuffer(listView);

                LoadDef();
                var def = stateDef;
                this.Left = def["maidmon.window.left"].ToInt();
                this.Top = def["maidmon.window.top"].ToInt();
                this.Width = def["maidmon.window.width"].ToInt();
                this.Height = def["maidmon.window.height"].ToInt();
                this.titleHeader.Width = def["maidmon.window.title.width"].ToInt();
                this.serviceHeader.Width = def["maidmon.window.service.width"].ToInt();
                this.timeHeader.Width = def["maidmon.window.time.width"].ToInt();
                this.statusHeader.Width = def["maidmon.window.status.width"].ToInt();
                this.hostHeader.Width = def["maidmon.window.host.width"].ToInt();
                this.tunerHeader.Width = def["maidmon.window.tuner.width"].ToInt();

                SetHost();
                Download();

                downloadTimer.Enabled = true;
                timer.Enabled = true;
            }
            catch (Exception e)
            {
                MessageBox.Show("エラーが発生しました。[詳細] " + e.Message, this.Text);
            }
        }

        //ホストをセット
        void SetHost()
        {
            if (Environment.GetCommandLineArgs().Length == 1)
            {
                hosts.Add("localhost");
            }
            else
            {
                hosts.AddRange(Environment.GetCommandLineArgs());
                hosts.RemoveAt(0);
            }

            foreach (var host in hosts)
            {
                var button = new ToolStripButton();
                button.DisplayStyle = ToolStripItemDisplayStyle.Text;
                button.Enabled = false;
                button.Margin = new Padding(10, 1, 0, 2);
                button.Name = host;
                button.Text = host;
                button.Click += hostButton_Click;
                toolBar.Items.Add(button);

                actives[host] = false;
            }
        }

        //ホストボタン
        void hostButton_Click(object sender, EventArgs arg)
        {
            try
            {
                System.Diagnostics.Process.Start("http://" + ((ToolStripButton)sender).Text + ":20000/maid/record.html");
            }
            catch (Exception e)
            {
                MessageBox.Show("ブラウザの起動に失敗しました。[詳細]" + e.Message, this.Text);
            }
        }

        //ダブルバッファ
        static void EnableDoubleBuffer(Control c)
        {
            PropertyInfo prop = c.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            prop.SetValue(c, true, null);
        }

        private void MaidMon_FormClosing(object sender, FormClosingEventArgs e)
        {
            var def = stateDef;
            def["maidmon.window.left"] = this.Left.ToString();
            def["maidmon.window.top"] = this.Top.ToString();
            def["maidmon.window.width"] = this.Width.ToString();
            def["maidmon.window.height"] = this.Height.ToString();
            def["maidmon.window.title.width"] = this.titleHeader.Width.ToString();
            def["maidmon.window.service.width"] = this.serviceHeader.Width.ToString();
            def["maidmon.window.time.width"] = this.timeHeader.Width.ToString();
            def["maidmon.window.status.width"] = this.statusHeader.Width.ToString();
            def["maidmon.window.host.width"] = this.hostHeader.Width.ToString();
            def["maidmon.window.tuner.width"] = this.tunerHeader.Width.ToString();
            stateDef.Save();
        }

        void LoadDef()
        {
            Util.CopyUserFile();
            stateDef = new DefineFile(Path.Combine(Util.GetUserPath(), "Maidmon.state.def"));
            stateDef.Load();
        }

        private void listView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            lock (records)
            {
                try
                {
                    Record rec;
                    if (e.ItemIndex >= records.Count)
                    {
                        //リストにないときでも、e.Itemをセットせずにコールバックから出てはいけない
                        rec = new Record();
                        rec.Id = -1;
                        rec.Service = "";
                        rec.Start = DateTime.Now;
                        rec.End = DateTime.Now;
                        rec.Title = "";
                        rec.Status = 0;
                        rec.Tuner = "";
                        rec.Host = "";
                        rec.Time = "";
                    }
                    else
                    {
                        rec = records[e.ItemIndex];
                    }

                    var item = new ListViewItem();

                    item.Text = rec.Title;
                    item.SubItems.Add(rec.Service);
                    item.SubItems.Add(rec.Time);

                    var status = "OK";
                    if ((rec.Status & (int)RecStatus.Duplication) > 0)
                    {
                        status = "重複";
                        item.BackColor = System.Drawing.Color.Gold;
                    }
                    if ((rec.Status & (int)RecStatus.Recoding) > 0)
                    {
                        status = "録画中";
                        item.BackColor = System.Drawing.Color.LightCoral;
                    }
                    item.SubItems.Add(status);

                    item.SubItems.Add(rec.Host);
                    item.SubItems.Add(rec.Tuner);

                    e.Item = item;
                }
                catch { }
            }
        }

        //ダウンロードタイマ
        private void downloadTimer_Tick(object sender, EventArgs e)
        {
            Download();
        }

        //表示更新タイマ
        private void timer_Tick(object sender, EventArgs e)
        {
            lock (records)
            {
                this.listView.VirtualListSize = records.Count;
                this.listView.Invalidate();
            }

            foreach(var host in hosts)
            {
                toolBar.Items[host].Enabled = actives[host];
            }
        }

        //予約データダウンロード
        void Download()
        {
            Task.Factory.StartNew(() =>
            {
                //ダウンロードが重複しないようにする
                lock (downloadLock)
                {
                    var list = new Dictionary<string, string>();

                    list.Clear();
                    
                    //ホストごとにダウンロード
                    foreach (var host in hosts)
                    {
                        list[host] = Download(host);
                        actives[host] = list[host] != null;
                    }

                    //予約リストにセット
                    lock(records)
                    {
                        records.Clear();
                        foreach (var host in hosts)
                        {
                            SetRecord(host, list[host]);
                        }

                        records.Sort((x, y) =>
                        {
                            if (x.Start == y.Start) { return 0; }
                            else if (x.Start < y.Start) { return -1; }
                            else { return 1; }
                        });
                    }
                }
            });
        }

        //ダウンロードしたデータを予約リストにセット
        void SetRecord(string host, string data)
        {   
            if(data == null) { return; }

            var ret = DynamicJson.Parse(data);
            if (ret.Code == 0)
            {
                foreach (var record in (dynamic[])ret.Data1)
                {
                    var rec = new Record();
                    rec.Id = (int)record[0];
                    rec.Service = record[1];
                    rec.Start = new DateTime((long)record[2]);
                    rec.End = new DateTime((long)record[3]);
                    rec.Title = record[4];
                    rec.Status = (int)record[5];
                    rec.Tuner = record[6];
                    rec.Host = host;
                    rec.Time = rec.Start.ToString("dd(ddd) HH:mm-") + rec.End.ToString("HH:mm");
                    records.Add(rec);
                }
            }
        }

        //ホストごとのダウンロード
        string Download(string host)
        {
            var web = new WebClient();
            try
            {
                web.Encoding = Encoding.UTF8;
                var sql = "select record.id, service.name, start, end, title, status, tuner from record"
                    + " left join service on record.fsid = service.fsid"
                    + " where status & 1 and end > {0} group by record.id order by start".Formatex(DateTime.Now.Ticks);
                sql = System.Web.HttpUtility.UrlEncode(sql, Encoding.UTF8);

                var url = "http://" + host + ":20000/webapi?api=GetTable&sql=" + sql;
                return web.DownloadString(url);
            }
            catch
            {
                return null;    //失敗したらnull
            }
            finally
            {
                web.Dispose();
            }
        }

        //更新ボタン
        private void updateButton_Click(object sender, EventArgs e)
        {
            Download();
        }

        //予約ダブルクリック
        private void listView_ItemActivate(object sender, EventArgs arg)
        {
            try
            {
                lock (records)
                {
                    var rec = records[listView.FocusedItem.Index];
                    System.Diagnostics.Process.Start("http://" + rec.Host + ":20000/maid/record-edit.html?id=" + rec.Id);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("ブラウザの起動に失敗しました。[詳細]" + e.Message, this.Text);
            }
        }
    }
}
