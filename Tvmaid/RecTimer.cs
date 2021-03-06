﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace Tvmaid
{
    class RecTimer : IDisposable
    {
        bool epg = false;                           //番組表取得フラグ
        EpgServiceQu epgQu = new EpgServiceQu();    //番組表取得用キュー
        DateTime nextEpgTime;                       //次回番組表取得時間
        bool stop = false;                          //終了フラグ
        Object thisLock = new Object();             //ロック用

        static RecTimer singleObj = null;           //シングルトン

        const int mainSleep = 10 * 1000;
        const int tunerSleep = 5 * 1000;
        const int epgSleep = 25 * 1000;
        const int recSleep = 5 * 1000;

        private RecTimer() { }

        public static RecTimer GetInstance()
        {
            if (singleObj == null)
            {
                singleObj = new RecTimer();
            }
            return singleObj;
        }

        public void Dispose()
        {
            stop = true;
        }

        public void Start()
        {
            using (var sql = new Sql(true))
            {
                //チューナ毎のスレッドを起動
                sql.Text = @"select * from tuner";
                using (var t = sql.GetTable())
                {
                    while (t.Read())
                    {
                        var tuner = new Tuner(t);
                        TaskList.StartNew(StartThread, tuner);
                    }
                }

                //番組表取得時間を初期化
                nextEpgTime = DateTime.Now.Date;
                nextEpgTime = nextEpgTime.AddHours(Program.UserDef["epg.autoupdate.hour"].ToInt());

                //メインループ
                //番組表取得、自動予約の監視
                while (stop == false)
                {
                    try
                    {
                        if (nextEpgTime <= DateTime.Now)
                        {
                            //古い予約と番組情報の削除
                            //先に予約を消すこと(予約に番組情報への参照があるため)
                            ClearnRecord(sql);
                            ClearnEvent(sql);

                            //番組表取得1時間以内なら実行
                            if (nextEpgTime + TimeSpan.FromHours(1) >= DateTime.Now)
                            {
                                epg = true;
                            }
                            //次回を翌日にセット
                            nextEpgTime = DateTime.Now.Date;
                            nextEpgTime = nextEpgTime.AddDays(1);   //翌日
                            nextEpgTime = nextEpgTime.AddHours(Program.UserDef["epg.autoupdate.hour"].ToInt());
                        }

                        //番組表取得
                        if (epg)
                        {
                            epgQu.Enqu();
                            epg = false;
                        }

                        StartAutoRecord(sql);   //自動予約

                        Thread.Sleep(mainSleep);
                    }
                    catch (Exception e)
                    {
                        Log.Write("エラーが発生しました。[詳細] " + e.Message);
                    }
                }
            }
        }

        public DateTime NextEpgTime
        {
            get { return nextEpgTime; }
        }

        public bool EpgUpdating
        {
            get { return epgQu.Count > 0; }
        }

        //番組情報のクリーンアップ
        void ClearnEvent(Sql sql)
        {
            Log.Write("古い番組情報を削除しています...");
            var time = DateTime.Now - new TimeSpan(24, 0, 0);    //現時刻 - 24時間(終了時より24時間以上経っている番組情報を削除)
            sql.Text = "delete from event where end < {0}".Formatex(time.Ticks);
            sql.Execute();
        }

        //予約のクリーンアップ
        void ClearnRecord(Sql sql)
        {
            Log.Write("古い予約を削除しています...");
            var time = DateTime.Now - new TimeSpan(1, 0, 0);    //現時刻 - 1時間(録画終了時より1時間以上経っている予約を削除)
            sql.Text = "delete from record where end < {0}".Formatex(time.Ticks);
            sql.Execute();
        }

        //自動予約
        void StartAutoRecord(Sql sql)
        {
            //自動予約を取得
            var autoList = new List<AutoRecord>();
            sql.Text = "select * from auto_record where status = 1 and sql <> ''";
            using (var t = sql.GetTable())
            {
                while (t.Read())
                {
                    autoList.Add(new AutoRecord(t));
                }
            }

            //検索条件に合致して、まだ予約されていない番組を予約
            foreach (var auto in autoList)
            {
                var events = new List<Event>();

                //現時刻+マージン(マージンを足しておかないと、終了した番組を予約してしまうため)
                int margin = Program.UserDef["record.margin.end"].ToInt();
                var now = DateTime.Now + new TimeSpan(0, 1, margin);    //+1分(1分以内は無視する)

                sql.Text = @"select * from event
                        left join record on event.fsid = record.fsid and event.eid = record.eid
                        where
                        event.end > {0} and record.id is null and {1}".Formatex(now.Ticks, auto.SqlText);

                using (var t = sql.GetTable())
                {
                    while (t.Read())
                    {
                        events.Add(new Event(t));
                    }
                }

                //missAutoRecordCount以上ヒットした場合、その自動予約を無効にする(間違った自動予約と判定する)
                const int missAutoRecordCount = 30;
                if (events.Count > missAutoRecordCount)
                {
                    auto.SetEnable(sql, false);
                    Log.Write("自動予約 '{0}' を無効にしました。{1} 件以上ヒットします。条件を見なおしてください。".Formatex(auto.Title, missAutoRecordCount));
                }
                else
                {
                    foreach (var ev in events)
                    {
                        var rec = new Record();
                        rec.Fsid = ev.Fsid;
                        rec.Eid = ev.Eid;
                        rec.StartTime = ev.Start;
                        rec.Duration = ev.Duration;
                        rec.Title = ev.Title;
                        rec.Auto = auto.Id;
                        rec.Add(sql);
                    }
                }
            }
        }

        //チューナ毎のスレッドを起動
        void StartThread(object tunerObj)
        {
            using (var sql = new Sql(true))
            {
                var tuner = (Tuner)tunerObj;
                bool epgOpen = false;

                //■追加
                int flag_pre = 0;
                int tvtest_no_dummy = 0;
                try
                {
                    tvtest_no_dummy = Program.UserDef["tvtest.no.dummy"].ToInt();
                }
                catch {}

                long retry_times = 0; //■追加　何回トライしたか

                while (stop == false)
                {
                    Record rec = null;
                    Record rec_pre = null; //■追加
                    try
                    {
                        //■追加
                        //■同一サービス連続録画に備えてダミーTVTestをスルーするためのギミック
                        if (flag_pre < 0)
                        {
                            flag_pre++;
                        }
                        //連続録画でもサービスIDが違う場合はダミーTVTestを起動しようかと思ったが、チャンネル変更が成功することに賭けることにした
                        //■録画開始2分前にダミーTVTestを起動する
                        if (tvtest_no_dummy == 0)
                        {
                            if (flag_pre >= 0 && flag_pre < 2) //2回目までは実行する
                            {
                                rec_pre = GetEnableRecord_pre(sql, tuner);
                                if (rec_pre != null)
                                {
                                    flag_pre++;
                                    StartRec_pre(sql, rec_pre, tuner);
                                    Thread.Sleep(5000); //念のため5秒空ける　が、エラーの場合はここは通らない
                                    flag_pre = 9; //すでにダミーTVTestを1度起動して成功したことを記録
                                }
                            }
                        }

                        rec = GetEnableRecord(sql, tuner);

                        if (rec != null)
                        {
                            flag_pre = -2; //■追加 1度はダミーTVTest起動チェックをスルーするようにセット（同一サービス連続録画の場合に備えて）
                            StartRec(sql, rec, tuner, retry_times);  //録画開始
                            retry_times = 0; //正常終了すれば再録画回数をクリア
                        }
                        else
                        {
                            //予約がないときは、番組表取得
                            var service = epgQu.Dequ(tuner);
                            if (service != null)
                            {
                                flag_pre = 0; //■追加 念のため
                                Log.Write(tuner.Name + ": 番組表を取得しています... " + service.Name);
                                tuner.Open();
                                epgOpen = true;
                                tuner.SetService(service);
                                //■メモ　ここでEPG取得25秒間
                                Thread.Sleep(epgSleep);

                                var list = tuner.GetEvents(sql, service);    //番組表取得
                                if (CheckRetry(list))
                                {
                                    //開始予定の予約がないか確認
                                    var r = GetEnableRecord(sql, tuner);
                                    if (r == null)
                                    {
                                        //リトライ
                                        Log.Write(tuner.Name + ": 情報が少ないため、再度番組表を取得しています... " + service.Name);
                                        Thread.Sleep(epgSleep);
                                        list = tuner.GetEvents(sql, service);
                                    }
                                }

                                UpdateRecordTime(sql, service);
                            }
                            else
                            {
                                if (epgOpen)
                                {
                                    try { tuner.Close(); }
                                    catch { }
                                    epgOpen = false;
                                }

                                Thread.Sleep(tunerSleep);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Write(tuner.Name + ": 録画を中断しました。[詳細]" + e.Message);
                        Thread.Sleep(5000); //■5秒待機
                        retry_times++; //エラーで終了した回数を記録
                    }
                }
                if (epgOpen)
                {
                    try { tuner.Close(); }
                    catch { }
                }
            }
        }

        //開始時間が過ぎていて、終了前で、有効な予約を取得
        private Record GetEnableRecord(Sql sql, Tuner tuner)
        {
            Record rec = null;
            //■削除 int margin = Program.UserDef["record.margin.start"].ToInt();
            int margin = Program.UserDef["record.margin.start"].ToInt() - 15; //■追加　きっちり録画開始＆1度のチャンネル変更失敗を吸収するため、録画開始15秒前に起動するようにした
            var now = DateTime.Now - new TimeSpan(0, 0, margin);

            sql.Text = @"select * from record where tuner = '{0}' and status & {2} and start <= {1} and end > {2} order by start limit 1"
                .Formatex(tuner.Name, now.Ticks, (int)Record.RecStatus.Enable);
            using (var t = sql.GetTable())
            {
                if (t.Read())
                {
                    rec = new Record(t);
                }
            }
            return rec;
        }

        //■ダミーTVTest 開始時間前に起動＆終了するためのクエリ
        private Record GetEnableRecord_pre(Sql sql, Tuner tuner)
        {
            Record rec = null;
            int margin = Program.UserDef["record.margin.start"].ToInt() - 90; //90秒前
            var now = DateTime.Now - new TimeSpan(0, 0, margin);

            sql.Text = @"select * from record where tuner = '{0}' and status & {2} and start <= {1} and end > {2} order by start limit 1"
                .Formatex(tuner.Name, now.Ticks, (int)Record.RecStatus.Enable);
            using (var t = sql.GetTable())
            {
                if (t.Read())
                {
                    rec = new Record(t);
                }
            }
            return rec;
        }

        //リトライすべきか
        //4日以内に歯抜けの時間が24時間以上ならtrue
        private bool CheckRetry(List<Event> list)
        {
            var curr = DateTime.Now;                    //カレント時刻
            var end = curr + new TimeSpan(4, 0, 0, 0);  //4日後 
            var sum = new TimeSpan(0, 0, 0);            //歯抜けの合計時間

            foreach (var ev in list)
            {
                if (ev.Start > end) { break; }

                var time = ev.Start - curr;
                if (time.Ticks > 0)
                {
                    sum += time;
                }
                curr = ev.End;
            }
            return sum > new TimeSpan(24, 0, 0);
        }

        //変更のあった番組の予約の時間を更新
        void UpdateRecordTime(Sql sql, Service service)
        {
            sql.Text = @"update record set
                        start = (select start from event where record.fsid = event.fsid and record.eid = event.eid),
                        end = (select end from event where record.fsid = event.fsid and record.eid = event.eid),
                        duration = (select duration from event where record.fsid = event.fsid and record.eid = event.eid)
                        where 
                        status & {0} > 0 and id in (
                        select record.id from record left join event
                        on record.fsid = event.fsid and record.eid = event.eid
                        where record.start <> event.start or record.duration <> event.duration)".Formatex((int)Record.RecStatus.EventMode);
            sql.Execute();
        }

        public void StartEpg()
        {
            epg = true;
        }

        public void StopEpg()
        {
            epgQu.Clear();
            Log.Write("番組表取得を中止しました。");
        }

        //ファイル名に使えない文字を変換
        string ConvertFileName(string name)
        {
            var list = new char[] { '\\', '/', ':', '*', '?', '\"', '<', '>', '|' };
            foreach (var c in list)
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        //ファイル名マクロ変換
        //public string ConvertFileMacro(Record rec, string name) //■削除
        public string ConvertFileMacro(Record rec, string name, string servicename) //■追加
        {
            var dic = new Dictionary<string, string>();
            dic.Add("{title}", rec.Title);
            dic.Add("{start-time}", rec.StartTime.ToString("yyMMdd-HHmm"));
            //サービスID //■追加
            long sid10 = rec.Fsid % (256 * 256);
            string sid_str10 = sid10.ToString("D");
            string sid_str16 = Convert.ToString(sid10, 16);
            dic.Add("{service-id}", sid_str16);
            //放送局名 //■追加
            dic.Add("{service-name}", servicename);
            //mark10als
            dic.Add("{end-time}", rec.EndTime.ToString("yyMMdd-HHmm"));

            //mark10als
            dic.Add("{start-time6}", rec.StartTime.ToString("HHmmss"));
            dic.Add("{end-time6}", rec.EndTime.ToString("HHmmss"));

            dic.Add("{start-date}", rec.StartTime.ToString("yyyyMMdd"));
            dic.Add("{start-year}", rec.StartTime.ToString("yyyy"));
            dic.Add("{start-year2}", rec.StartTime.ToString("yy"));
            dic.Add("{start-month2}", rec.StartTime.ToString("MM"));
            dic.Add("{start-day2}", rec.StartTime.ToString("dd"));
            dic.Add("{start-hour2}", rec.StartTime.ToString("HH"));
            dic.Add("{start-minute2}", rec.StartTime.ToString("mm"));
            dic.Add("{start-second2}", rec.StartTime.ToString("ss"));
            dic.Add("{start-day-of-week}", rec.StartTime.ToString("dddd"));

            dic.Add("{end-date}", rec.EndTime.ToString("yyyyMMdd"));
            dic.Add("{end-year}", rec.EndTime.ToString("yyyy"));
            dic.Add("{end-year2}", rec.EndTime.ToString("yy"));
            dic.Add("{end-month2}", rec.EndTime.ToString("MM"));
            dic.Add("{end-day2}", rec.EndTime.ToString("dd"));
            dic.Add("{end-hour2}", rec.EndTime.ToString("HH"));
            dic.Add("{end-minute2}", rec.EndTime.ToString("mm"));
            dic.Add("{end-second2}", rec.EndTime.ToString("ss"));
            dic.Add("{end-day-of-week}", rec.EndTime.ToString("dddd"));

            foreach (var macro in dic)
            {
                name = name.Replace(macro.Key, macro.Value);
            }
            return ConvertFileName(name);
        }

        //ファイルがすでに存在した場合は「(2)」をつける。
        string CheckFilePath(string src)
        {
            string dest = src;
            int i = 2;
            while (File.Exists(dest))
            {
                dest = "{0}({1}){2}".Formatex(Path.Combine(Path.GetDirectoryName(src), Path.GetFileNameWithoutExtension(src)), i, Path.GetExtension(src));
                i++;
            }
            return dest;
        }

        //録画開始
        void StartRec(Sql sql, Record rec, Tuner tuner, long retry_times)  //修正
        {
            var result = new Result();
            var tsStatus = new TsStatus();
            var recContinue = false;    //録画継続フラグ(録画時間変更に使用)

            var path = ""; //■追加

            try
            {
                //Log.Write(tuner.Name + ": 録画を開始します。" + rec.Title); //■削除
                Log.Write(tuner.Name + ": 録画準備をしています。" + rec.Title);  //■追加
                SleepState.SetSleepStop(true);
                rec.SetRecoding(sql, true); //■メモ　ここで今録画中だとフラグをセットしている

                tuner.Open();
                var service = new Service(sql, rec.Fsid);
                tuner.SetService(service);

                //string file = ConvertFileMacro(rec, Program.UserDef["record.file"]); //■削除
                string file = ConvertFileMacro(rec, Program.UserDef["record.file"], service.Name); //■追加
                //var path = System.IO.Path.Combine(Program.UserDef["record.folder"], file); //■削除
                path = System.IO.Path.Combine(Program.UserDef["record.folder"], file); //■追加
                path = CheckFilePath(path);
                //■追加　きっちり時間が来るまで待機
                int seconds_prev = Program.UserDef["record.margin.start"].ToInt();
                while (DateTime.Now < rec.StartTime + new TimeSpan(0, 0, seconds_prev))
                {
                    Thread.Sleep(0);
                }
                Log.Write(tuner.Name + ": 録画を開始します。" + rec.Title);

                tuner.StartRec(path);

                result.Title = rec.Title;
                result.ServiceName = service.Name;
                result.File = Path.GetFileName(path);
                result.SchStart = rec.StartTime;
                result.SchEnd = rec.EndTime;
                result.Start = DateTime.Now;

                var eventTimeError = false; //番組時間取得エラーフラグ

                int margin = Program.UserDef["record.margin.end"].ToInt();

                while (DateTime.Now < rec.EndTime + new TimeSpan(0, 0, margin))
                {
                    if (stop)
                    {
                        throw new AppException(tuner.Name + ": アプリケーション終了のため、録画を中断します。");
                    }

                    //現在の予約が有効かどうか確認
                    sql.Text = "select status from record where id = " + rec.Id;
                    object status = sql.GetData();
                    if (status == null)  //予約が取り消された
                    {
                        throw new AppException(tuner.Name + ": 予約が取り消されたため、録画を中断します。");
                    }
                    else if (((int)(long)status & (int)Record.RecStatus.Enable) == 0)  //予約が無効にされた
                    {
                        throw new AppException(tuner.Name + ": 予約が無効にされたため、録画を中断します。");
                    }

                    //追従モード
                    if ((rec.Status & (int)Record.RecStatus.EventMode) > 0)
                    {
                        //番組の時間に変更がないか確認
                        Event ev = null;
                        try
                        {
                            ev = tuner.GetEventTime(service, rec.Eid);
                            eventTimeError = false; //エラーフラグをリセット
                        }
                        catch
                        {
                            if (eventTimeError == false)    //何度もエラーが表示されないようにする
                            {
                                Log.Write(tuner.Name + ": 番組時間の取得に失敗しました。番組がなくなった可能性があります。録画は続行します。" + rec.Title);
                                eventTimeError = true;
                            }
                        }

                        if (ev != null)
                        {
                            if (ev.Start != rec.StartTime || ev.Duration != rec.Duration)
                            {
                                Log.Write(tuner.Name + ": 番組時間が変更されました。" + rec.Title);

                                //予約を変更
                                rec.StartTime = ev.Start;
                                rec.Duration = ev.Duration;
                                tuner.GetEvents(sql, service);  //番組表を更新する
                                UpdateRecordTime(sql, service); //予約を更新

                                //番組開始が、現時刻より1分以上後に変更された場合、一旦録画を終了する
                                if (rec.StartTime - DateTime.Now > TimeSpan.FromMinutes(1))
                                {
                                    recContinue = true;
                                    throw new AppException(tuner.Name + ": 番組の開始時間が遅れているため、録画を中断します。");
                                }
                            }
                        }
                    }

                    if (tuner.GetState() != Tuner.State.Recoding)
                    {
                        throw new AppException(tuner.Name + ": 録画が中断しました。");
                    }

                    tsStatus = tuner.GetTsStatus();

                    Thread.Sleep(recSleep);
                }

                if (eventTimeError)
                {
                    result.Code = 1;
                    result.Message = "番組時間の取得に失敗しました。録画は続行しました。";
                }
            }
            catch (Exception e)
            {
                result.Code = 1;
                result.Message = e.Message;
                throw;
            }
            finally
            {
                if (retry_times < 2 && result.Message.IndexOf("TVTestでエラーが発生しました。エラーコード: 6") >= 0)
                {
                    //■チャンネル変更エラーで終了した場合（2回目まで）
                    try
                    {
                        Log.Write(tuner.Name + ": チャンネル変更に失敗しました。リトライします。" + result.Message);
                        //録画開始前の状態に戻す    
                        rec.SetRecoding(sql, false);

                        //チューナーを閉じる
                        try
                        {
                            if (tuner != null)
                            {
                                tuner.Close();
                            }
                        }
                        catch (Exception ex9)
                        {
                            Log.Write("チューナー終了中エラーが発生しました(6)。[詳細] " + ex9.Message);
                            TVTest_ForceStop(); //強制終了
                        }

                        SleepState.SetSleepStop(false);
                    }
                    catch (Exception e)
                    {
                        Log.Write("チャンネル変更失敗時のエラー処理中にエラーが発生しました。[詳細] " + e.Message);
                    }
                }
                else
                {
                    //■修正　録画を終了させる
                    try
                    {
                        if (tuner != null)
                        {
                            tuner.StopRec();
                            //■削除　tuner.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Write("録画終了中エラーが発生しました。[詳細] " + e.Message);
                    }

                    //■追加 チューナーを閉じる
                    try
                    {
                        if (tuner != null)
                        {
                            tuner.Close();
                        }
                    }
                    catch (Exception ex9)
                    {
                        Log.Write("チューナー終了中エラーが発生しました。[詳細] " + ex9.Message);
                        TVTest_ForceStop(); //強制終了
                    }

                    try
                    {
                        if (recContinue)
                        {
                            rec.SetRecoding(sql, false); //■メモ　前番組の延長により放送時間が変更になった場合
                        }
                        else
                        {
                            rec.SetComplete(sql);
                        }

                        SleepState.SetSleepStop(false);

                        result.Error = tsStatus.Error;
                        result.Drop = tsStatus.Drop;
                        result.Scramble = tsStatus.Scramble;
                        result.End = DateTime.Now;
                        result.Add(sql);
                    }
                    catch (Exception e)
                    {
                        Log.Write("録画終了中エラーが発生しました。[詳細] " + e.Message);
                    }

                    Log.Write(tuner.Name + ": 録画が終了しました。" + rec.Title);
                }
            }

            //■録画後処理　録画が正常終了した場合のみ実行される
            try
            {
                if (Program.UserDef["exe1"].Length > 0)
                {
                    Thread.Sleep(rec.Title.Length); //少々ばらつきを持たせるか・・

                    Thread t = new Thread(new ParameterizedThreadStart(Execute_exe));
                    string path2 = Path.GetDirectoryName(path);
                    if (path2.Length > 0)
                    {
                        path2 = path2 + "\\";
                    }
                    t.Start(path2 + result.File);
                }
            }
            catch
            {
                //実行するコマンドが無い。Program.UserDef["exe1"]が存在しなかった
            }
        }

        //■ダミーTVTest起動＆終了
        void StartRec_pre(Sql sql, Record rec, Tuner tuner)
        {
            var result = new Result();
            var tsStatus = new TsStatus();

            try
            {
                Log.Write(tuner.Name + ": ダミーTVTestを起動します。" + rec.Title);
                SleepState.SetSleepStop(true);

                tuner.Open();
                var service = new Service(sql, rec.Fsid);
                tuner.SetService(service);

                Thread.Sleep(1000 * 12); //12秒視聴
            }
            catch (Exception e)
            {
                result.Code = 1;
                result.Message = e.Message;
                throw;
            }
            finally
            {
                try
                {
                    if (tuner != null)
                    {
                        tuner.Close();
                    }
                }
                catch (Exception e)
                {
                    Log.Write("ダミーTVTest終了中エラーが発生しました。[詳細] " + e.Message);
                    TVTest_ForceStop(); //エラーが起こった場合は強制終了
                }

                try
                {
                    SleepState.SetSleepStop(false);
                }
                catch (Exception e)
                {
                    Log.Write("ダミーTVTest終了中エラーが発生しました2。[詳細] " + e.Message);
                }

                Log.Write(tuner.Name + ": ダミーTVTestを終了しました。" + rec.Title);
            }
        }

        //■コマンド実行（マルチスレッド）
        static void Execute_exe(object f)
        {
            Program.cmd_multi_now++; //使用中
            if (Program.cmd_multi_now == 1)
            {
                Program.cmd_multi_now = 0;
            }
            else
            {
                Program.cmd_multi_now = 1;
            }
            //同時実行があれば無くなるまで待機
            int tu = 10 * 60 * 60 * 7; //7時間
            while (Program.cmd_multi_now > 0 && tu > 0)
            {
                Thread.Sleep(100);
                tu--;
            }
            if (tu == 0)
            {
                //タイムアウト　7時間経っても終わらない
                Log.Write("7時間待機しても現在実行中のプロセスが終了しないのでコマンド実行を諦めました。");
                return;
            }
            Program.cmd_multi_now = 1; //使用中
            
            //渡されたファイル名 キャストが必要
            string FullpathFile = (string)f;
            //Log.Write("FullpathFile=" + FullpathFile);

            int i = 1;
            try
            {
                while (Program.UserDef["exe" + i.ToString("D")].Length > 0)
                {
                    //実行ファイル名取得
                    string exe_str = Program.UserDef["exe" + i.ToString("D")];
                    exe_str = exe_str.Replace("\"", ""); //"を削除
                    //パラメーター取得
                    string para = "";
                    try
                    {
                        para = Program.UserDef["para" + i.ToString("D")];
                    }
                    catch
                    {
                        para = "";
                    }
                    //パラメータ入れ替え
                    para = para.Replace("{file}", FullpathFile);

                    //実行
                    execute_exe(exe_str, para);

                    i++;
                }
            }
            catch //(Exception e)
            {
                //Log.Write(e.Message);
            }
            
            Program.cmd_multi_now = 0; //使用終了
        }

        //■外部プログラム実行
        static void execute_exe(string exestr, string para)
        {
            //カレントディレクトリ変更
            try
            {
                string exefolder = System.IO.Path.GetDirectoryName(exestr);
                System.IO.Directory.SetCurrentDirectory(exefolder);
            }
            catch (Exception ex)
            {
                Log.Write("カレントディレクトリ変更でエラーが発生しました。" + ex.Message);
            }

            //実行
            try
            {
                //ProcessStartInfoオブジェクトを作成する
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                //mark10als
                //起動するファイルのパスを指定する
                psi.FileName = exestr;
                //コマンドライン引数を指定する
                psi.Arguments = para;
                psi.RedirectStandardInput = false;
                //mark10als
//                psi.RedirectStandardOutput = true;
                psi.RedirectStandardOutput = false;
                psi.UseShellExecute = false;
                //作業フォルダ指定
                string exefolder = System.IO.Path.GetDirectoryName(exestr);
                psi.WorkingDirectory = exefolder;
                //作業フォルダ
                //ウィンドウを表示しないようにする
                psi.CreateNoWindow = true;
                //アプリケーションを起動する
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(psi);

                //mark10als
//                int do_wait = (10 * 60) * 60 * 6;
                int do_wait = (1000 * 60) * 60 * 6;
                //最大6時間待つ
                Log.Write(exestr + " , " + para + "を実行開始しました");
                //mark10als
//                while (p.HasExited == false & do_wait > 0)
//                {
//                    //終わるまで待つ
//                    do_wait -= 1;
//                    System.Threading.Thread.Sleep(100);
//                }
//                if (do_wait == 0)
//                {
                p.WaitForExit(do_wait);
                if (p.HasExited == false) {
                    //タイムアウト
                    Log.Write(exestr + " , " + para + "の実行がタイムアウトしました");
                    Log.Write(exestr + " , " + para + "を強制終了します");
                    p.Kill();
                }
                else
                {
                    Log.Write(exestr + " , " + para + "が実行終了しました");
                }
                //mark10als
                p.Close();
            }
            catch (Exception ex)
            {
                Log.Write("【エラー】" + exestr + "の実行に失敗しました。" + ex.Message);
            }
            //カレントディレクトリを戻す
            string ppath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            System.IO.Directory.SetCurrentDirectory(ppath);

            Thread.Sleep(1000); //1秒待機
        }

        //■TVTestを強制終了
        private void TVTest_ForceStop()
        {
            int tvtest_no_kill = 0;
            try
            {
                tvtest_no_kill = Program.UserDef["tvtest.no.kill"].ToInt();
            }
            catch { }

            if (tvtest_no_kill == 0)
            {
                System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName("TVTest");

                foreach (System.Diagnostics.Process p in ps)
                {
                    //クローズメッセージを送信する
                    p.CloseMainWindow();

                    //プロセスが終了するまで最大10秒待機する
                    p.WaitForExit(10000);
                    //プロセスが終了したか確認する
                    if (p.HasExited)
                    {
                        Log.Write("TVTestを終了させました。");
                    }
                    else
                    {
                        //終了に応じないときは強制的に終了
                        p.Kill();
                        Log.Write("TVTestを強制的に終了させました。");
                    }
                }
            }
        }
    }

    //番組表を取得するサービスのキュー
    class EpgServiceQu
    {
        List<Service> list = new List<Service>();   //キュー

        public int Count
        {
            get { return list.Count; }
        }

        //キューにサービスリストを入れる
        public void Enqu()
        {
            lock (list)
            {
                if (list.Count > 0) { return; }

                SleepState.SetSleepStop(true);  //スリープを抑止

                using (var sql = new Sql(true))
                {
                    sql.Text = "select * from service id order by id";
                    using (var t = sql.GetTable())
                    {
                        while (t.Read())
                        {
                            list.Add(new Service(t));
                        }
                    }

                    //番兵をチューナ毎に入れる
                    //番組表取得終了を確認するために使用(番兵が全て取り出されたら終了と判断)
                    //Service.DriverにDriverIdを入れる
                    sql.Text = "select * from tuner";
                    using (var t = sql.GetTable())
                    {
                        while (t.Read())
                        {
                            var tuner = new Tuner(t);
                            var s = new Service();
                            s.Id = -1;
                            s.Fsid = 0;
                            s.Driver = tuner.DriverId;
                            list.Add(s);
                        }
                    }
                }
            }
        }

        //キューをクリア
        public void Clear()
        {
            lock (list)
            {
                list.Clear();
                SleepState.SetSleepStop(false);
            }
        }

        //チューナで使用可能なサービスを取り出す
        public Service Dequ(Tuner tuner)
        {
            lock (list)
            {
                if (list.Count == 0)
                {
                    return null;
                }

                //ドライバで検索
                Service service = null;
                foreach (var s in list)
                {
                    if (s.Driver == tuner.Driver)
                    {
                        service = s;
                        break;
                    }
                    else if (s.Driver == tuner.DriverId)    //番兵かどうか
                    {
                        //番兵を削除する
                        list.RemoveAll(sv => sv.Driver == tuner.DriverId);
                        break;
                    }
                }

                //取り出したサービスをキューから削除
                if (service != null)
                {
                    //チューナが複数ある場合、同じサービスが複数含まれているためRemoveAll
                    list.RemoveAll(s => s.Fsid == service.Fsid);
                }

                //削除の結果キューが0になったらスリープ抑止を解除
                if (list.Count == 0)
                {
                    SleepState.SetSleepStop(false);
                }

                return service;
            }
        }
    }
}
