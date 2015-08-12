using System;
using System.Collections.Generic;

namespace Tvmaid
{
    //録画予約
    class Record
    {
        public int Id = -1;             //-1: 新規
        public long Fsid = 0;
        public int Eid = -1;            //-1: 番組情報なし

        public DateTime StartTime = DateTime.Now;
        public int Duration = 0;

        public int Auto = -1;           //-1: 手動予約
        public int Status = (int)Record.RecStatus.Enable | (int)Record.RecStatus.EventMode;
        public string Title = "未定";
        public string TunerName = "";   //"": チューナ自動選択

        public DateTime EndTime
        {
            get
            {
                long x = Duration;
                return new DateTime(StartTime.Ticks + (x * 10 * 1000 * 1000));
            }
        }

        public enum RecStatus
        {
            Enable = 1,
            EventMode = 2,
            Duplication = 32,
            Recoding = 64,
            Complete = 128
        };

        public Record() { }

        public Record(DataTable t)
        {
            Init(t);
        }

        void Init(DataTable t)
        {
            Id = t.GetInt("id");

            Fsid = t.GetLong("fsid");
            Eid = t.GetInt("eid");
            StartTime = new DateTime(t.GetLong("start"));
            Duration = t.GetInt("duration");

            Auto = t.GetInt("auto");
            Status = t.GetInt("status");
            Title = t.GetStr("title");
            TunerName = t.GetStr("tuner");
        }

        public Record(Sql sql, int id)
        {
            sql.Text = "select * from record where id = " + id;
            using (var t = sql.GetTable())
            {
                if (t.Read())
                {
                    Init(t);
                }
                else
                {
                    throw new AppException("予約が見つかりません。");
                }
            }
        }

        //予約追加
        public void Add(Sql sql)
        {
            if(this.EndTime < DateTime.Now)
            {
                throw new AppException("過去の番組は予約できません。");
            }

            bool newId = this.Id == -1;

            //チューナが指定されているか？
            if (TunerName != "")
            {
                var tuner = new Tuner(sql, TunerName);   //チューナが不正でないか作成してみる
                sql.Text = "select id from service where driver = '{0}' and fsid = {1}".Formatex(Sql.SqlEncode(tuner.Driver), this.Fsid);
                using (var t = sql.GetTable())
                {
                    if (t.Read() == false)
                    {
                        throw new AppException("指定されたチューナに、サービスがありません。チューナを変更してください。");
                    }
                }

                AddDb(sql);
            }
            else
            {
                //空きを探して予約する
                var list = new List<Tuner>();

                //チューナを列挙
                sql.Text = "select * from tuner where driver in (select driver from service where fsid = {0}) order by id".Formatex(Fsid);
                using (var table = sql.GetTable())
                {
                    while (table.Read())
                    {
                        list.Add(new Tuner(table));
                    }
                }
                if (list.Count == 0)
                {
                    throw new AppException("チューナが見つかりません。");
                }

                foreach (var tuner in list)
                {
                    if (IsReserved(sql, tuner.Name, StartTime, EndTime))
                    {
                        TunerName = tuner.Name;
                        break;
                    }
                }
                //空きが見つからなかった
                if (TunerName == "")
                {
                    TunerName = list[0].Name;
                }
                AddDb(sql);
            }
            if (newId)
            {
                Log.Write("予約しました。" + this.Title);
            }
            else
            {
                Log.Write("予約を変更しました。" + this.Title);
            }
        }

        //予約削除
        public void Remove(Sql sql, bool removeOnly = false)
        {
            sql.Text = "delete from record where id = " + Id;
            sql.Execute();

            if (removeOnly == false)
            {
                SetDuplication(sql);
                Log.Write("予約を取り消しました。" + this.Title);
            }
        }
        /*
        public bool Exists(Sql sql)
        {
            sql.Text = "select * from record where id = " + this.Id;
            using (var t = sql.GetTable())
            {
                if(t.Read())
                {
                    var rec = new Record(t);
                    return (rec.Status & (int)RecStatus.Enable) == 0;
                }
                else
                {
                    return false;
                }
            }
        }
        */
        //チューナの空きを探す
        public bool IsReserved(Sql sql, string name, DateTime start, DateTime end)
        {
            sql.Text = "select id from record where tuner = '{0}' and {1} < end and {2} > start".Formatex(name, start.Ticks, end.Ticks);
            return sql.GetData() == null;
        }

        //フラグをセット
        void SetStatus(Sql sql, RecStatus status, bool flag)
        {
            if (flag)
            {
                sql.Text = "update record set status = status | {0} where id = {1}".Formatex((int)status, Id);
            }
            else
            {
                sql.Text = "update record set status = status & ~{0} where id = {1}".Formatex((int)status, Id);
            }
            sql.Execute();
        }

        //有効/無効にセット
        public void SetEnable(Sql sql, bool flag)
        {
            SetStatus(sql, RecStatus.Enable, flag);
        }

        //待機中/録画中にセット
        public void SetRecoding(Sql sql, bool flag)
        {
            SetStatus(sql, RecStatus.Recoding, flag);
        }

        //完了にセット
        public void SetComplete(Sql sql)
        {
            SetEnable(sql, false);
            SetRecoding(sql, false);
            SetStatus(sql, RecStatus.Complete, true);
        }

        //重複判定をセット
        void SetDuplication(Sql sql)
        {
            sql.BeginTrans();
            try
            {
                //重複フラグを0にする
                sql.Text = @"update record set status = status & ~{1} where tuner = '{0}'".Formatex(this.TunerName, (int)RecStatus.Duplication);
                sql.Execute();

                //自己結合して、時間の重なっている予約を取り出し更新
                sql.Text = @"update record set status = status | {1}
                            where id in (
                            select r1.id from record r1
                            join record r2
                            on r1.id <> r2.id
                            where r1.tuner = '{0}'
                            and r2.tuner = '{0}'
                            and r1.start < r2.end
                            and r1.end > r2.start
                            and r1.status & {2}
                            and r2.status & {2}
                            )".Formatex(this.TunerName, (int)RecStatus.Duplication, (int)RecStatus.Enable);
                sql.Execute();
                sql.Commit();
            }
            catch { sql.Rollback(); throw; }
        }

        //予約を追加
        void AddDb(Sql sql)
        {
            if (Id == -1)
            {
                Id = sql.GetNextId("record");
            }
            else
            {
                Remove(sql, true);
            }

            sql.Text = @"insert into record values(
                            {0}, {1}, {2},
                            {3}, {4}, {5},
                            {6}, {7}, '{8}', '{9}');".Formatex(
                            Id,
                            Fsid,
                            Eid,

                            StartTime.Ticks,
                            EndTime.Ticks,
                            Duration,

                            Auto,
                            Status,
                            Sql.SqlEncode(Title),
                            Sql.SqlEncode(TunerName)
                            );
            sql.Execute();

            SetDuplication(sql);
        }
    }
}
