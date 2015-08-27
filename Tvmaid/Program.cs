using System;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Tvmaid
{
    static class Program
    {
        public static DefineFile UserDef = null;    //ユーザ定義ファイル
        public static DefineFile StateDef = null;   //状態保存ファイル
        public static string Logo = "Tvmaid";
        public static string Version = "R4 (´・ω・`) mod4 + mark10als";

        public static int cmd_multi_now = 0; //■追加　現在マルチスレッドでコマンド実行中なら1

#if DEBUG
        public static string AppVer = Logo + " " + Version + " (debug)";
#else
        public static string AppVer = Logo + " " + Version;
#endif

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            WebServer ws = null;

            var ticket = new Ticket("/tvmaid/mutex/main");
            try
            {
                if(ticket.GetOwner(60 * 1000) == false)
                {
                    ticket = null;
                    throw new AppException("時間内に二重起動が解消されませんでした。");
                }

                Log.Write(AppVer);
                LoadDef();

                //先に読み込んでおく
                GenreConv.GetInstance();
                TextConv.GetInstance();

                if (args.Length == 1 && args[0] == "-tunerupdate")
                {
                    UpdateTuner();
                }

                TaskList.StartNew(() => { RecTimer.GetInstance().Start(); });
                ws = new WebServer();
                TaskList.StartNew(() => { ws.Start(); });

                TunerMon.GetInstance();
                Application.Run(new Tasktray());

            }
            catch (Exception e)
            {
                MessageBox.Show("このエラーは回復できないため、アプリケーションは終了します。[詳細]"+ e.Message, Logo);
            }
            finally
            {
                if (ws != null) { ws.Dispose(); }
                RecTimer.GetInstance().Dispose();

                if (StateDef != null) { StateDef.Save(); }

                //スレッド終了待ち
                int i = 0;
                while (TaskList.GetInstance().IsFinish() == false)
                {
                    System.Threading.Thread.Sleep(100);
                    i++;
                    if (i > 300) { break; }
                }
                if (ticket != null) { ticket.Dispose(); }
            }
        }

        static void UpdateTuner()
        {
            try
            {
                using (var sql = new Sql(true))
                {
                    Log.Write("チューナを更新しています...");
                    Tuner.Update(sql);    //チューナ更新(DB更新)

                    //サービス更新
                    Log.Write("サービスを更新しています...");

                    sql.Text = "delete from service";
                    sql.Execute();

                    var tuners = new List<Tuner>();
                    sql.Text = "select * from tuner group by driver ";
                    using (var table = sql.GetTable())
                    {
                        while (table.Read())
                        {
                            tuners.Add(new Tuner(table));
                        }
                    }
                    bool dup = false;
                    foreach (Tuner tuner in tuners)
                    {
                        try
                        {
                            tuner.GetServices(sql); //サービスをTVTestから読み込み
                        }
                        catch (DupServiceException)
                        {
                            dup = true;
                        }
                    }

                    //以前から残っている番組で、新しくなったサービスにないものは削除する
                    sql.Text = "delete from event where fsid not in (select fsid from service group by fsid)";
                    sql.Execute();

                    //同ユーザ番組表
                    sql.Text = "delete from user_epg where fsid not in (select fsid from service group by fsid)";
                    sql.Execute();

                    //同予約
                    sql.Text = "delete from record where fsid not in (select fsid from service group by fsid)";
                    sql.Execute();

                    //予約でチューナがないものは削除
                    sql.Text = "delete from record where tuner not in (select name from tuner)";
                    sql.Execute();

                    if (dup)
                    {
                        MessageBox.Show("サービスが重複しています。\nこのままでも使用できますが、TVTestのチャンネルスキャンで同じ放送局を1つを残して他は無効(チェックを外す)にすることをおすすめします。", Program.Logo);
                    }
                }
                Log.Write("チューナ更新が完了しました。");
            }
            catch (Exception e)
            {
                MessageBox.Show("チューナの読み込みに失敗しました。[詳細]" + e.Message, Program.Logo);
                throw;
            }
        }

        static void LoadDef()
        {
            Util.CopyUserFile();

            UserDef = new DefineFile(Path.Combine(Util.GetUserPath(), "Tvmaid.def"));
            UserDef.Load();
            StateDef = new DefineFile(Path.Combine(Util.GetUserPath(), "Tvmaid.state.def"));
            StateDef.Load();

            if (File.Exists(UserDef["tvtest"]) == false)
            {
                throw new AppException("TVTestのパスが設定されていないか、間違っています。tvtest");
            }
            if (Directory.Exists(UserDef["record.folder"]) == false)
            {
                throw new AppException("録画フォルダのパスが設定されていないか、間違っています。record.folder");
            }
            int ret;
            if (Int32.TryParse(UserDef["record.margin.start"], out ret) == false)
            {
                throw new AppException("録画マージンは数値を指定してください。record.margin.start");
            }
            if (Int32.TryParse(UserDef["record.margin.end"], out ret) == false)
            {
                throw new AppException("録画マージンは数値を指定してください。record.margin.end");
            }
            //mark10als
            string recfile = Program.UserDef["record.file"];
            if (recfile == null)
            {
                throw new AppException("録画ファイル名が設定されていません。record.file");
            }
            if (Int32.TryParse(UserDef["epg.autoupdate.hour"], out ret) == false)
            {
                throw new AppException("番組表取得時刻は数値を指定してください。epg.autoupdate.hour");
            }
        }
    }

    //起動したスレッドを記録するクラス
    //終了待ちに使用する
    class TaskList
    {
        List<Task> list = new List<Task>();
        static TaskList singleObj = null;

        private TaskList() { }

        public static TaskList GetInstance()
        {
            if (singleObj == null)
            {
                singleObj = new TaskList();
            }
            return singleObj;
        }

        public static void StartNew(Action action)
        {
            var task = Task.Factory.StartNew(action);
            TaskList.GetInstance().Add(task);
        }

        public static void StartNew(Action<object> action, object state)
        {
            var task = Task.Factory.StartNew(action, state);
            TaskList.GetInstance().Add(task);
        }

        public void Add(Task task)
        {
            lock (list)
            {
                Clearn();
                list.Add(task);
            }
        }

        //すでに終了したスレッドの情報を削除する
        void Clearn()
        {
            lock (list)
            {
                if(list.Count == 0) { return; }

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    switch (list[i].Status)
                    {
                        case TaskStatus.RanToCompletion:
                        case TaskStatus.Faulted:
                        case TaskStatus.Canceled:
                            list.RemoveAt(i);
                            break;
                    }
                }
            }
        }

        public bool IsFinish()
        {
            Clearn();
            return list.Count == 0;
        }
    }
}
