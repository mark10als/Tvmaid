using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;

namespace Tvmaid
{
    //チューナモニタ
    public partial class TunerMon : Form
    {
        //表示データ
        class ServiceInfo
        {
            public int Id;
            public string Name;
            public string EventTime = "";
            public string EventTitle = "";
            public int EventId = -1;
        }

        Sql sql;            //破棄は、TunerMon.Designer.csのDisposeで行う
        Tuner curTuner;     //選択中チューナ
        List<ServiceInfo> services = new List<ServiceInfo>();
        static TunerMon singleObj = null;

        public TunerMon()
        {
            InitializeComponent();

            try
            {
                this.Icon = new System.Drawing.Icon(System.IO.Path.Combine(Util.GetUserPath(), "Tvmaid.ico"));
                EnableDoubleBuffer(serviceView);

                DefineFile def = Program.StateDef;
                this.Left = def["tunermon.window.left"].ToInt();
                this.Top = def["tunermon.window.top"].ToInt();
                this.Width = def["tunermon.window.width"].ToInt();
                this.Height = def["tunermon.window.height"].ToInt();
                this.split1.SplitterDistance = def["tunermon.window.tuner.width"].ToInt();
                this.split2.SplitterDistance = def["tunermon.window.tuner.height"].ToInt();
                this.nameHeader.Width = def["tunermon.window.service.name.width"].ToInt();
                this.timeHeader.Width = def["tunermon.window.service.time.width"].ToInt();
                this.eventHeader.Width = def["tunermon.window.service.event.width"].ToInt();
                this.Visible = def["tunermon.window.visible"] == "1";
                
                sql = new Sql(true);
                InitTunerView();
                this.ActiveControl = tunerView;
                if (this.tunerView.Nodes.Count > 0) { this.tunerView.SelectedNode = this.tunerView.Nodes[0]; }
            }
            catch (Exception e)
            {
                MessageBox.Show("エラーが発生しました。[詳細] " + e.Message);
            }
        }

        //ダブルバッファ
        static void EnableDoubleBuffer(Control c)
        {
            PropertyInfo prop = c.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            prop.SetValue(c, true, null);
        }

        //チューナーをセット
        private void InitTunerView()
        {
            sql.Text = "select * from tuner order by id";

            using (DataTable table = sql.GetTable())
            {
                while (table.Read())
                {
                    var tuner = new Tuner(table);
                    var node = new TreeNode(tuner.Name);
                    node.Tag = tuner;
                    tunerView.Nodes.Add(node);
                }
            }
            tunerTimer.Enabled = true;
        }

        public static TunerMon GetInstance()
        {
            if (singleObj == null)
            {
                singleObj = new TunerMon();
            }
            return singleObj;
        }

        private void MonitorForm_FormClosing(object sender, FormClosingEventArgs arg)
        {
            //ユーザの閉じる操作の時は、隠すだけ
            if (arg.CloseReason == CloseReason.UserClosing)
            {
                arg.Cancel = true;
                Hide();
            }

            try
            {
                var def = Program.StateDef;
                def["tunermon.window.visible"] = this.Visible ? "1" : "0";
                def["tunermon.window.left"] = this.Left.ToString();
                def["tunermon.window.top"] = this.Top.ToString();
                def["tunermon.window.width"] = this.Width.ToString();
                def["tunermon.window.height"] = this.Height.ToString();
                def["tunermon.window.tuner.width"] = this.split1.SplitterDistance.ToString();
                def["tunermon.window.tuner.height"] = this.split2.SplitterDistance.ToString();
                def["tunermon.window.service.name.width"] = this.nameHeader.Width.ToString();
                def["tunermon.window.service.time.width"] = this.timeHeader.Width.ToString();
                def["tunermon.window.service.event.width"] = this.eventHeader.Width.ToString();
            }
            catch(Exception e)
            {
                MessageBox.Show("エラーが発生しました。[詳細] " + e.Message);
            }
        }

        //サービス更新タイマー
        private void serviceTimer_Tick(object sender, EventArgs arg)
        {
            if (this.Visible == false) { return; }

            try
            {
                SetService();
                serviceView.Invalidate();
            }
            catch (Exception e)
            {
                MessageBox.Show("エラーが発生しました。[詳細] " + e.Message);
            }
        }

        private void serviceView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var info = services[e.ItemIndex];

            var item = new ListViewItem();

            //サービス名
            item.Text = info.Name;

            //時間
            var time = new ListViewItem.ListViewSubItem(item, info.EventTime);
            item.SubItems.Add(time);

            //番組タイトル
            var title = new ListViewItem.ListViewSubItem(item, info.EventTitle);
            item.SubItems.Add(title);

            e.Item = item;
        }

        //サービス表示更新
        void SetService()
        {
            serviceTimer.Enabled = false;
            
            //バグ:このSQLだと番組情報が1つでもあるときに、その時間の番組が無いとサービスが表示されない
            //sql.Text = @"select service.id, name, start, end, title, event.id from service left join event on service.fsid = event.fsid
            //                            where driver = '{0}' and (start is null or (start < {1} and end > {1})) order by service.id"
            //                            .Formatex(Sql.SqlEncode(curTuner.Driver), DateTime.Now.Ticks);

            sql.Text = @"select service.id, name, start, end, title, event1.id from service 
                        left join
                        (select id,fsid, start,end, title from event
                        where start < {1} and end > {1}) as event1
                        on service.fsid = event1.fsid
                        where driver = '{0}' 
                        order by service.id"
                        .Formatex(Sql.SqlEncode(curTuner.Driver), DateTime.Now.Ticks);

            using (var table = sql.GetTable())
            {
                services.Clear();

                while (table.Read())
                {
                    const int id = 0;
                    const int name = 1;
                    const int start = 2;
                    const int end = 3;
                    const int title = 4;
                    const int eventId = 5;

                    ServiceInfo info = new ServiceInfo();
                    info.Id = table.GetInt(id);
                    info.Name = table.GetStr(name);

                    if (table.IsNull(start) == false)
                    {
                        var s = new DateTime(table.GetLong(start));
                        var e = new DateTime(table.GetLong(end));
                        info.EventTime = s.ToString("HH:mm") + "-" + e.ToString("HH:mm");
                        info.EventTitle = table.GetStr(title);
                        info.EventId = table.GetInt(eventId);
                    }
                    services.Add(info);
                }
            }

            serviceView.VirtualListSize = services.Count;
            serviceTimer.Enabled = true;
        }

        //チューナ選択
        private void tunerView_AfterSelect(object sender, TreeViewEventArgs arg)
        {
            try
            {
                curTuner = (Tuner)arg.Node.Tag;
                SetService();
                serviceView.Refresh();
            }
            catch (Exception e)
            {
                MessageBox.Show("エラーが発生しました。[詳細] " + e.Message);
            }
        }

        void WriteLog(string log)
        {
            //長くなったら削除する
            if (logBox.Text.Length > 1000 * 50)
            {
                logBox.SelectionStart = 0;
                logBox.SelectionLength = 500 * 50;
                logBox.SelectedText = "";
            }
            string text = DateTime.Now.ToLongTimeString() + " " + log + "\r\n";
            logBox.AppendText(text);

            logBox.SelectionStart = logBox.Text.Length;
            logBox.ScrollToCaret();
        }

        //サービス選択(タブルクリック)
        private void serviceView_ItemActivate(object sender, EventArgs arg)
        {
            View();
        }

        void View()
        {
            try
            {
                int id = services[serviceView.FocusedItem.Index].Id;
                var s = new Service(sql, id);
                curTuner.Open(true);
                curTuner.SetService(s);
            }
            catch(Exception e)
            {
                MessageBox.Show("エラーが発生しました。[詳細]" + e.Message, Program.Logo);
            }
        }

        private void viewMenuItem_Click(object sender, EventArgs e)
        {
            View();
        }

        private void reserveMenuItem_Click(object sender, EventArgs arg)
        {
            try
            {
                var info = services[serviceView.FocusedItem.Index];
                var rec = new Record();
                var ev = new Event(sql, info.EventId);

                rec.Fsid = ev.Fsid;
                rec.Eid = ev.Eid;
                rec.StartTime = ev.Start;
                rec.Duration = ev.Duration;
                rec.Title = ev.Title;
                rec.Add(sql);
            }
            catch (Exception e)
            {
                MessageBox.Show("予約できませんでした。[詳細] " + e.Message, Program.Logo);
            }
        }

        private void stopRecMenuItem_Click(object sender, EventArgs arg)
        {
            try
            {
                Record rec = null;
                sql.Text = "select * from record where status & {0} and tuner = '{1}'".Formatex((int)Record.RecStatus.Recoding, curTuner.Name);
                using (var t = sql.GetTable())
                {
                    if (t.Read())
                    {
                        rec = new Record(t);
                    }
                }

                if (rec != null)
                {
                    //自動予約の場合は、削除せずに無効にする(または終わっている番組)
                    if (rec.Auto == -1 || rec.EndTime < DateTime.Now)
                    {
                        rec.Remove(sql);
                    }
                    else
                    {
                        rec.SetEnable(sql, false);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("エラーが発生しました。[詳細] " + e.Message);
            }
        }

        //チューナ更新タイマー
        private void tunerTimer_Tick(object sender, EventArgs arg)
        {
            if (this.Visible == false) { return; }

            try
            {
                foreach (TreeNode node in tunerView.Nodes)
                {
                    var str = new string[] { " - VIEW", " - REC", " - REC PAUSE", "", " - UNKNOWN" };
                    var tuner = (Tuner)node.Tag;
                    var state = tuner.GetState();
                    node.Text = "{0}{1}".Formatex(tuner.Name, str[(int)state]);
                }
                tunerView.Invalidate();
            }
            catch (Exception e)
            {
                MessageBox.Show("エラーが発生しました。[詳細] " + e.Message);
            }
        }

        //ログ更新タイマー
        private void logTimer_Tick(object sender, EventArgs arg)
        {
            if (this.Visible == false) { return; }

            try
            {
                var text = Log.Read();
                if (text != null)
                {
                    //ログは1度に全て読みだして表示するのでなく、1行ずつ表示する
                    //連続でエラーが出た時などにGUIが操作不能になるのを避けるため
                    logBox.AppendText(text + "\r\n");
                    logBox.SelectionStart = logBox.Text.Length;
                    logBox.ScrollToCaret();
                    logTimer.Interval = 100;
                }
                else
                {
                    logTimer.Interval = 1000;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("エラーが発生しました。[詳細] " + e.Message);
            }
        }

        private void TunerMon_VisibleChanged(object sender, EventArgs e)
        {
            logTimer.Enabled = this.Visible;
        }

        private void clearLogMenuItem_Click(object sender, EventArgs e)
        {
            this.logBox.Clear();
        }
    }

    //ログ
    //一時的な保管所。一定時間ごとにチューナモニタが読み出す
    class Log
    {
        List<string> list = new List<string>();
        static Log singleObj = null;

        Log() { }

        public static Log GetInstance()
        {
            if (singleObj == null)
            {
                singleObj = new Log();
            }
            return singleObj;
        }

        public void WriteLog(string text)
        {
            lock (list)
            {
                text = DateTime.Now.ToLongTimeString() + " " + text;
                list.Add(text);
            }
        }

        public static void Write(string text)
        {
            GetInstance().WriteLog(text);
        }

        public string ReadLog()
        {
            lock (list)
            {

                if (list.Count == 0)
                {
                    return null;
                }
                else
                {
                    string text = list[0];
                    list.RemoveAt(0);
                    return text;
                }
            }
        }

        public static string Read()
        {
            return GetInstance().ReadLog();
        }
    }
}
