
namespace Tvmaid
{
    //サービス
    class Service
    {
        public int Id;
        public string Driver;
        public int Nid;
        public int Tsid;
        public int Sid;
        public string Name;

        public Service() { }

        public Service(DataTable t)
        {
            Init(t);
        }

        void Init(DataTable t)
        {
            Id = t.GetInt("id");
            Driver = t.GetStr("driver");
            Fsid = t.GetLong("fsid");
            Name = t.GetStr("name");
        }

        public long Fsid
        {
            get
            {
                long x = Nid;   
                return (x << 32) + (Tsid << 16) + Sid; 
            }
            set
            {
                Nid = (int)(value >> 32 & 0xffff);
                Tsid = (int)(value >> 16 & 0xffff);
                Sid = (int)(value & 0xffff);
            }
        }

        public Service(Sql sql, long fsid)
        {
            sql.Text = @"select * from service where fsid = {0}".Formatex(fsid);
            using (var t = sql.GetTable())
            {
                if (t.Read())
                {
                    Init(t);
                }
                else
                {
                    throw new AppException("サービスが見つかりません。" + fsid);
                }
            }
        }

        public Service(Sql sql, int id)
        {
            sql.Text = "select * from service where id = " + id;
            using (var t = sql.GetTable())
            {
                if(t.Read())
                {
                    Init(t);
                }
                else
                {
                    throw new AppException("サービスが見つかりません。");
                }
            }
        }

        public void Add(Sql sql)
        {
            Id = sql.GetNextId("service");

            sql.Text = @"insert into service values({0}, '{1}', {2}, '{3}');".Formatex(
                        Id,
                        Sql.SqlEncode(Driver),
                        Fsid,
                        Sql.SqlEncode(Name)
                        );
            sql.Execute();
        }
    }
}
