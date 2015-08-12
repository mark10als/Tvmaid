using System;

namespace Tvmaid
{
    //番組
    class Event
    {
        public int Id;
        public long Fsid;
        public int Eid;
        public DateTime Start;
        public int Duration;
        public string Title;
        public string Desc;
        public string LongDesc;
        public int Genre;
        public int SubGenre;

        public DateTime End
        {
            get 
            {
                long x = Duration;
                return new DateTime(Start.Ticks + (x * 10 * 1000 * 1000));
            }
        }

        string GenreText
        {
            get
            {
                return GenreConv.GetInstance().GetText(Genre, SubGenre);
            }
        }

        public int Week
        {
            get { return (int)Start.DayOfWeek; }
        }

        public Event() { }

        public Event(DataTable t)
        {
            Init(t);
        }

        void Init(DataTable t)
        {
            Id = t.GetInt("id");
            Fsid = t.GetLong("fsid");
            Eid = t.GetInt("eid");
            Start = new DateTime(t.GetLong("start"));
            Duration = t.GetInt("duration");
            Title = t.GetStr("title");
            Desc = t.GetStr("desc");
            LongDesc = t.GetStr("longdesc");
            Genre = t.GetInt("genre");
            SubGenre = t.GetInt("subgenre");
        }

        public Event(Sql sql, long fsid, int eid)
        {
            sql.Text = "select * from event where fsid = {0} and eid = {1}".Formatex(fsid, eid);
            using (var t = sql.GetTable())
            {
                if (t.Read())
                {
                    Init(t);
                }
                else
                {
                    throw new AppException("番組が見つかりません。");
                }
            }
        }
        
        public Event(Sql sql, int id)
        {
            sql.Text = "select * from event where id = " + id;
            using (var t = sql.GetTable())
            {
                if (t.Read())
                {
                    Init(t);
                }
                else
                {
                    throw new AppException("番組が見つかりません。");
                }
            }
        }
        
        /*
        public void Remove(Sql sql)
        {
            sql.Text = "delete from event where id = " + Id;
            sql.Execute();
        }
        */
        //オブジェクトをDBへ登録
        public void Add(Sql sql)
        {
            //idが-1のときはidを取得
            if (Id == -1)
            {
                Id = sql.GetNextId("event");
            }

            var conv = TextConv.GetInstance();

            sql.Text = @"insert into event values(
                            {0}, {1}, {2}, 
                            {3}, {4}, {5},
                            '{6}', '{7}', '{8}', {9}, {10},
                            {11}, '{12}');".Formatex(
                            Id,
                            Fsid,
                            Eid,

                            Start.Ticks,
                            End.Ticks,
                            Duration,

                            Sql.SqlEncode(conv.Convert(Title)),
                            Sql.SqlEncode(conv.Convert(Desc)),
                            Sql.SqlEncode(conv.Convert(LongDesc)),
                            Genre,
                            SubGenre,

                            Week,
                            GenreText
                            );
            sql.Execute();
        }
    }
}
