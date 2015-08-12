using System;
using System.Net;
using System.Collections.Specialized;
using System.Reflection;

namespace Tvmaid
{
    //web api
    class WebApi
    {
        HttpListenerRequest req;    //リクエスト
        HttpListenerResponse res;   //レスポンス
        NameValueCollection query;  //HttpListenerRequest.QueryStringは日本語が文字化けするため、こちらを使用すること
        WebRet ret = new WebRet();  //リターンオブジェクト

        public WebApi(HttpListenerRequest req, HttpListenerResponse res)
        {
            this.req = req;
            this.res = res;
            query = System.Web.HttpUtility.ParseQueryString(req.RawUrl);
            ret.SetCode(0, "");
        }

        //メソッドを検索して呼び出す
        public void Exec()
        {
            string api = req.QueryString["api"];
            try
            {
                //メソッド呼び出し
                this.GetType().InvokeMember(
                    api,
                    BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    this,
                    null);
            }
            catch (TargetInvocationException tie)
            {
                //呼び出した関数で例外が発生したとき
                //Exceptionだと、InvokeMemberのエラーしか取得できない
                ret.SetCode(1, tie.InnerException.Message);
            }
            catch (MissingMethodException)
            {
                ret.SetCode(1, "指定されたWeb Apiはありません。" + api);
            }
            catch (Exception e)
            {
                Log.Write(e.Message);
                ret.SetCode(1, e.Message);
            }

            //JSON形式のテキストをクライアントへ返す
            string json = Codeplex.Data.DynamicJson.Serialize(ret);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(json);

            res.ContentEncoding = new System.Text.UTF8Encoding();
            res.ContentType = "application/json";

            try
            {
                res.OutputStream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Log.Write(e.Message);
            }
        }

        void CheckQuery(string q)
        {
            if(query[q] == null)
            {
                throw new AppException("必須のパラメータがありません。[" + q + "]");
            }
        }

        public void GetTable()
        {
            CheckQuery("sql");

            using (var sql = new Sql(true))
            {
                sql.Text = query["sql"];
                ret.Data1 = sql.GetList();
            }
        }

        public void RemoveRecord()
        {
            CheckQuery("id");
            var id = query["id"];

            using (var sql = new Sql(true))
            {
                var rec = new Record(sql, id.ToInt());
                //自動予約の場合は、削除せずに無効にする(または終わっている番組)
                if(rec.Auto == -1 || rec.EndTime < DateTime.Now)
                {
                    rec.Remove(sql);
                }
                else
                {
                    rec.SetEnable(sql, false);
                }
            }
        }

        void SetRecordQuery(Record rec)
        {
            if (query["id"] != null) { rec.Id = query["id"].ToInt(); }
            if (query["fsid"] != null) { rec.Fsid = query["fsid"].ToLong(); }
            if (query["eid"] != null) { rec.Eid = query["eid"].ToInt(); }

            if (query["start"] != null) { rec.StartTime = new DateTime(query["start"].ToLong()); }
            if (query["duration"] != null) { rec.Duration = query["duration"].ToInt(); }

            if (query["status"] != null) { rec.Status = query["status"].ToInt(); }
            if (query["title"] != null) { rec.Title = query["title"]; }
            if (query["tuner"] != null) { rec.TunerName = query["tuner"]; }
        }

        public void AddRecord()
        {
            CheckQuery("eid");

            var rec = new Record();
            using (var sql = new Sql(true))
            {
                //送信された値を入力
                SetRecordQuery(rec);

                //追従モードなら番組情報を入力
                if ((rec.Status & (int)Record.RecStatus.EventMode) > 0)
                {
                    CheckQuery("fsid");

                    var ev = new Event(sql, rec.Fsid, rec.Eid);
                    rec.Fsid = ev.Fsid;
                    rec.Eid = ev.Eid;
                    rec.StartTime = ev.Start;
                    rec.Duration = ev.Duration;
                    if (query["title"] == null) { rec.Title = ev.Title; }
                }
                rec.Add(sql);
            }
            ret.Data1 = rec.Id;
        }

        public void RemoveAutoRecord()
        {
            CheckQuery("id");
            var id = query["id"];

            using (var sql = new Sql(true))
            {
                var rec = new AutoRecord(sql, id.ToInt());
                rec.Remove(sql);
            }
        }

        void SetAutoRecordQuery(AutoRecord rec)
        {
            if (query["id"] != null) { rec.Id = query["id"].ToInt(); }
            if (query["sql"] != null) { rec.SqlText = query["sql"]; }
            if (query["option"] != null) { rec.Option = query["option"]; }
            if (query["status"] != null) { rec.Status = query["status"].ToInt(); }
            if (query["title"] != null) { rec.Title = query["title"]; }
        }

        public void AddAutoRecord()
        {
            var rec = new AutoRecord();
            using (var sql = new Sql(true))
            {
                //送信された値を入力
                SetAutoRecordQuery(rec);
                rec.Add(sql);
            }
            ret.Data1 = rec.Id;
        }

        public void SetUserEpg()
        {
            CheckQuery("id");
            CheckQuery("fsid");
            var id = query["id"].ToInt();
            var list = query["fsid"].Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                        
            using (var sql = new Sql(true))
            {
                try
                {
                    sql.BeginTrans();

                    sql.Text = "delete from user_epg where id = " + id;
                    sql.Execute();
                    
                    for (var i = 0; i < list.Length; i++)
                    {
                        var fsid = list[i].ToLong();
                        sql.Text = @"insert into user_epg values({0}, {1}, {2});".Formatex(
                            id,
                            fsid,
                            i
                            );
                        sql.Execute();
                    }
                    sql.Commit();
                }
                catch
                {
                    sql.Rollback();
                    throw;
                }
            }
        }
    }

    //web apiリターンオブジェクト
    //jsonに変換してクライアントへ返す
    class WebRet
    {
        public int Code { get; set; }       //リターンコード。0:成功、1:失敗
        public string Message { get; set; } //リターンメッセージ

        public object Data1 { get; set; }
        //public object Data2;

        public WebRet()
        {
            SetCode(0, "");
        }

        public void SetCode(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
