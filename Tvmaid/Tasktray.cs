using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Win32;
using Tvmaid.Properties;

namespace Tvmaid
{
    //タスクトレイ
    public partial class Tasktray : Form
    {
        WakeTimer wake = new WakeTimer();
                        
        public Tasktray()
        {
            InitializeComponent();

            try
            {
                this.notifyIcon.Icon = Resources.Tvmaid;
                this.notifyIcon.Text += Program.AppVer;
            }
            catch (Exception e)
            {
                MessageBox.Show("エラーが発生しました。[詳細] " + e.Message);
            }
        }

        //スリープに関する注意事項
        //
        //.netでは、スリープイベントのためのPowerModeChangedイベントがあるが、これを使わずにWM_POWERBROADCASTで処理している。
        //PowerModeChangedのPowerModes.Resumeには問題があるため。
        //PowerModes.Resumeは、ユーザの入力があって初めてイベントが起こるので、復帰したタイミングで処理できない。
        //復帰したタイミングは、PowerModeChangedには無いPBT_APMRESUMEAUTOMATICを受け取る必要がある。
        //Windowsの復帰は、まずRESUME-AUTOMATIC(自動復帰)モードで復帰し、ユーザの入力があると通常復帰になる(これがPowerModes.Resume)。
        //
        //また、Application.AddMessageFilterでは、WM_POWERBROADCASTが受け取れないため、トップレベルウインドウを一つ用意する必要がある。
        //今後、ウインドウを持たないサービスアプリにしたりする場合は、別の方法を考える必要がある。
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            const int WM_POWERBROADCAST = 0x0218;   //電源に関するメッセージ
            const int PBT_APMSUSPEND = 0x0004;      //スリープに入る
            const int PBT_APMRESUMEAUTOMATIC = 0x0012;  //復帰した

            if (WM_POWERBROADCAST == m.Msg)
            {
                switch (m.WParam.ToInt32())
                {
                    case PBT_APMRESUMEAUTOMATIC: OnResume(); break;
                    case PBT_APMSUSPEND: OnSuspend(); break;
                }
            }
        }

        private void Tasktray_FormClosing(object sender, FormClosingEventArgs e)
        {
            wake.Cancel();  //復帰タイマーキャンセル
        }

        //一番早い有効な予約、または番組表取得の時間を取得
        DateTime GetNextTime()
        {
            //次の番組表取得
            var epg = RecTimer.GetInstance().NextEpgTime;
            DateTime rec;

            //次の予約
            long tick = GetNextRecordTime();
            if (tick == -1)
            {
                rec = new DateTime(9999, 1, 1);
            }
            else
            {
                rec = new DateTime(tick);
            }
            //早い方を返す
            return rec < epg ? rec : epg;
        }

        private void OnSuspend()
        {
            Log.Write("スリープ状態に入ります。");

            var time = GetNextTime();
            time -= new TimeSpan(0, 2, 0);    //2分前に復帰させる

            //2分以内に次の予約がある
            if(time < DateTime.Now)
            {
                time = DateTime.Now + new TimeSpan(0, 0, 30);   //すぐ復帰させる(30秒後)
            }

            wake.SetTimer(time);
            Log.Write("復帰タイマーを次の時間にセットしました。" + time.ToString("MM/dd HH:mm:ss"));
        }

        private void OnResume()
        {
            Log.Write("スリープから復帰しました。");
            wake.Cancel();

            //現在の時間が次の予約の3分前以内なら、自動復帰したと判断し、再スリープするよう準備
            var time = GetNextTime();
            var span = time - DateTime.Now;
            if (span < new TimeSpan(0, 3, 0))
            {
                sleepTimer.Start(); //スリープタイマー開始
                Log.Write("スリープモードで自動復帰したため、録画後再スリープします。");
            }
            else
            {
                Log.Write("自動復帰でないため、再スリープしません。");
            }
        }

        void sleepTimer_Tick(object sender, EventArgs e)
        {
            if (SleepState.SleepStop)
            {
                return;            
            }

            //次の予約がまで10分以上ならスリープ
            var time = GetNextTime();
            var span = time - DateTime.Now;
            if (span > new TimeSpan(0, 10, 0))
            {
                Sleep();
            }
        }

        //録画中かどうか
        bool IsRecording()
        {
            using (var sql = new Sql(true))
            {
                sql.Text = "select id from record where status & {0} and start < {1} and end > {1} order by start".Formatex((int)Record.RecStatus.Enable, DateTime.Now.Ticks);
                using (var t = sql.GetTable())
                {
                    return t.Read();
                }
            }
        }

        //スリープ実行
        private void Sleep()
        {
            sleepTimer.Stop();

            var countdown = new SleepCountdown();
            var res = countdown.ShowDialog();
            countdown.Dispose();

            if (res == DialogResult.OK)
            {
                Log.Write("自動的にスリープします。");
                OnSuspend();    //自分で呼び出す
                Application.SetSuspendState(PowerState.Suspend, false, false);
            }
            else
            {
                Log.Write("スリープをキャンセルしました。");
            }
        }

        //次の予約の時間を取得
        long GetNextRecordTime()
        {
            using (var sql = new Sql(true))
            {
                sql.Text = "select start from record where status & {0} and start > {1} order by start".Formatex((int)Record.RecStatus.Enable, DateTime.Now.Ticks);
                using (var t = sql.GetTable())
                {
                    return t.Read() ? t.GetLong(0) : -1;
                }
            }
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("終了していいですか？", Program.Logo, MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                notifyIcon.Visible = false; // アイコンをトレイから取り除く
                Application.Exit(); // 終了
            }
        }

        private void tunerMonMenuItem_Click(object sender, EventArgs e)
        {
            TunerMon.GetInstance().Show();
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            TunerMon.GetInstance().Show();
            TunerMon.GetInstance().Activate();
        }

        //チューナ更新
        private void updateTunerMenuItem_Click(object sender, EventArgs ea)
        {
            if (MessageBox.Show("チューナ更新していいですか？\n続行すると、アプリが再起動し、更新処理を行います。", Program.Logo, MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                Application.Exit();
                System.Diagnostics.Process.Start(Application.ExecutablePath, "-tunerupdate");
            }
        }

        private void updateEpgMenuItem_Click(object sender, EventArgs e)
        {
            RecTimer.GetInstance().StartEpg();
        }

        private void stopEpgMenuItem_Click(object sender, EventArgs e)
        {
            RecTimer.GetInstance().StopEpg();
        }

        private void sleepMenuItem_Click(object sender, EventArgs e)
        {
            Log.Write("ユーザの手動操作で、スリープモードにしました。");
            sleepTimer.Start();
            sleepTimer_Tick(null, null);
        }

        //-------- test --------
        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Application.SetSuspendState(PowerState.Suspend, false, false);
            //OnResume();
            //sleepTimer.Enabled = true;

        }
        
        private void test2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
    }
}
