using System;
using System.Collections.Generic;

namespace Tvmaid
{
    //チューナ
    //TVTestをコントロールする
    class Tuner
    {
        TvProcess tvp;
        bool isStart = false;  //プロセスを起動したかどうか

        public int Id;
        public string Name;
        public string DriverPath;
        public int DriverIndex;

        public string Driver
        {
            get { return System.IO.Path.GetFileName(DriverPath); }
        }

        public string DriverId
        {
            get { return "{0}/{1}".Formatex(Driver, DriverIndex); }
        }

        public enum State
        {
            View,       //視聴
            Recoding,   //録画中
            Paused,     //録画一時停止中
            Stoped,     //停止
            Unknown     //不明
        }

        public Tuner(string name, string driverPath)
        {
            Id = -1;
            Name = name;
            DriverPath = driverPath;
            DriverIndex = 0;
            tvp = new TvProcess(DriverId, DriverPath);
        }

        public Tuner(DataTable t)
        {
            Init(t);
        }

        void Init(DataTable t)
        {
            Id = t.GetInt("id");
            Name = t.GetStr("name");
            DriverPath = t.GetStr("driver_path");
            DriverIndex = t.GetInt("driver_index");
            tvp = new TvProcess(DriverId, DriverPath);
        }

        public Tuner(Sql sql, string name)
        {
            sql.Text = @"select * from tuner where name = '{0}'".Formatex(Sql.SqlEncode(name));
            using (var t = sql.GetTable())
            {
                if (t.Read())
                {
                    Init(t);
                }
                else
                {
                    throw new AppException("チューナがありません。" + name);
                }
            }
        }

        public bool IsStart()
        {
            return isStart;
        }

        public bool IsOpen()
        {
            return tvp.IsOpen();
        }

        public void Open(bool show = false)
        {
            //開いていなければ開く
            if (IsOpen() == false)
            {
                isStart = true;
                tvp.Open(show);
            }
        }

        public void Close()
        {
            isStart = false;
            tvp.Close();
        }

        //状態を取得
        public State GetState()
        {
            try
            {
                if (IsOpen())
                {
                    return (State)tvp.GetState();
                }
                else
                {
                    return State.Stoped;
                }
            }
            catch
            {
                return State.Unknown;
            }
        }

        //サービス切り替え
        public void SetService(Service service)
        {
            try
            {
                var state = GetState();
                if (state == State.Recoding || state == State.Paused)
                {
                    throw new AppException("録画中はサービスを変更できません。");
                }

                tvp.SetService(service);
            }
            catch (TvProcessExceotion e)
            {
                if (e.Code == (int)TvProcess.ErrorCode.SetService)
                {
                    //リトライしてみる
                    Log.Write("■チャンネル変更に失敗しました。再度の変更を試みます。");
                    System.Threading.Thread.Sleep(1000);
                    tvp.SetService(service);
                }
                else
                {
                    throw;
                }
            }
        }

        public Event GetEventTime(Service service, int eid)
        {
            return tvp.GetEventTime(service, eid);
        }

        public TsStatus GetTsStatus()
        {
            return tvp.GetTsStatus();
        }

        public void StartRec(string file)
        {
            tvp.StartRec(file);
        }

        public void StopRec()
        {
            tvp.StopRec();
        }

        //サービスをDBへ追加
        public void Add(Sql sql)
        {
            Id = sql.GetNextId("tuner");

            sql.Text = "select count(id) from tuner where driver = '" + Sql.SqlEncode(Driver) + "'";
            object index = sql.GetData();
            DriverIndex = (int)(long)index;

            sql.Text = "insert into tuner values({0}, '{1}', '{2}', '{3}', {4});".Formatex(
                        Id,
                        Sql.SqlEncode(Name),
                        Sql.SqlEncode(DriverPath),
                        Sql.SqlEncode(Driver),
                        DriverIndex
                        );
            sql.Execute();
        }

        //番組表取得
        public List<Event> GetEvents(Sql sql, Service service)
        {
            List<Event> list = null;
            try
            {
                list = tvp.GetEvents(service);
            }
            catch (TvProcessExceotion e)
            {
                Log.Write(e.Message);
                return null;
            }

            try
            {
                sql.BeginTrans();

                foreach (Event ev in list)
                {
                    //同じeidの番組を探す
                    sql.Text = "select id from event where eid = {0} and fsid = {1}".Formatex(ev.Eid, ev.Fsid);
                    object id = sql.GetData();
                    if (id == null)
                    {
                        ev.Id = -1;
                    }
                    else
                    {
                        //存在する場合、削除して同じidで登録する
                        ev.Id = (int)(long)id;
                        sql.Text = "delete from event where id = " + ev.Id;
                        sql.Execute();
                    }
                    ev.Add(sql);
                }
                return list;
            }
            finally
            {
                sql.Commit();
            }
        }

        //チューナ更新
        public static void Update(Sql sql)
        {
            var def = new DefineFile(System.IO.Path.Combine(Util.GetUserPath(), "tuner.def"));
            def.Load();

            sql.Text = "delete from tuner";
            sql.Execute();

            foreach (var name in def.Keys)
            {
                var tuner = new Tuner(name, def[name]);

                tuner.Add(sql);
            }
        }

        //サービスリスト更新
        public void GetServices(Sql sql)
        {
            Open();
            var list = tvp.GetServices();
            Close();

            sql.BeginTrans();
            bool dup = false;

            foreach (Service s in list)
            {
                //fsidが0の場合は不正とみなして無視する
                //CATVがないのにBonDriverのChSet.txtでCATVが有効になっていると、このデータが返ってくる
                if (s.Fsid == 0) { continue; }

                sql.Text = "select id from service where driver = '{0}' and fsid = {1}".Formatex(Sql.SqlEncode(s.Driver), s.Fsid);
                var id = sql.GetData();
                if (id == null)
                {
                    s.Add(sql);
                }
                else
                {
                    dup = true;
                }
            }
            sql.Commit();

            if (dup)
            {
                throw new DupServiceException("");
            }
        }
    }

    class DupServiceException : AppException
    {
        public DupServiceException(string msg) : base(msg) { }
    }
}
