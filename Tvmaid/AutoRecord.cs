
namespace Tvmaid
{
    //自動録画予約
    class AutoRecord
    {
        public int Id = -1;             //-1: 新規
        public string SqlText = "";
        public string Option = "";
        public int Status = (int)Record.RecStatus.Duplication | (int)Record.RecStatus.EventMode;
        public string Title = "未定";

        public AutoRecord() { }

        public AutoRecord(DataTable t)
        {
            Init(t);
        }

        void Init(DataTable t)
        {
            Id = t.GetInt("id");

            SqlText = t.GetStr("sql");
            Option = t.GetStr("option");

            Status = t.GetInt("status");
            Title = t.GetStr("title");
        }

        public AutoRecord(Sql sql, int id)
        {
            sql.Text = "select * from auto_record where id = " + id;
            using (var t = sql.GetTable())
            {
                if (t.Read())
                {
                    Init(t);
                }
                else
                {
                    throw new AppException("自動予約が見つかりません。");
                }
            }
        }

        //削除
        public void Remove(Sql sql)
        {
            //自動で行われた予約を削除
            sql.Text = "delete from record where auto = " + Id;
            sql.Execute();

            sql.Text = "delete from auto_record where id = " + Id;
            sql.Execute();
        }

        //追加
        public void Add(Sql sql)
        {
            if(this.Title == "")
            {
                throw new AppException("タイトルを入力してください。");
            }

            if (Id == -1)
            {
                Id = sql.GetNextId("auto_record");
            }
            else
            {
                Remove(sql);    //自動で行われた予約も削除
            }

            sql.Text = @"insert into auto_record values(
                            {0}, '{1}', '{2}', {3}, '{4}', {5});".Formatex(
                            Id,
                            Sql.SqlEncode(SqlText),
                            Sql.SqlEncode(Option),
                            Status,
                            Sql.SqlEncode(Title),
                            Id  //暫定的にIDと同じ数値を入れておく
                            );
            sql.Execute();
        }

        //有効/無効にセット
        public void SetEnable(Sql sql, bool flag)
        {
            sql.Text = "update auto_record set status = {1} where id = {0}".Formatex(Id, flag ? 1 : 0);
            sql.Execute();
        }
    }
}
