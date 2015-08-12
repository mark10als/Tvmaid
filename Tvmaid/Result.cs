using System;

namespace Tvmaid
{
    //録画結果
    class Result
    {
        public int Id = -1;             //-1: 新規
        public string Title;
        public string ServiceName;
        public string File;

        public DateTime Start;
        public DateTime End;
        public DateTime SchStart;
        public DateTime SchEnd;

        public int Code = 0;
        public int Error = 0;
        public int Drop = 0;
        public int Scramble = 0;
        public string Message = "";

        public Result() { }

        //削除
        public void Remove(Sql sql)
        {
            sql.Text = "delete from result where id = " + Id;
            sql.Execute();
        }

        //追加
        public void Add(Sql sql)
        {
            if (Id == -1)
            {
                Id = sql.GetNextId("result");
            }

            sql.Text = @"insert into result values(
                            {0}, '{1}', '{2}', '{3}',
                            {4}, {5}, {6}, {7},
                            {8}, {9}, {10}, {11}, '{12}');".Formatex(
                            Id,
                            Sql.SqlEncode(Title),
                            Sql.SqlEncode(ServiceName),
                            Sql.SqlEncode(File),

                            Start.Ticks,
                            End.Ticks,
                            SchStart.Ticks,
                            SchEnd.Ticks,

                            Code,
                            Error,
                            Drop,
                            Scramble,
                            Sql.SqlEncode(Message)
                            );
            sql.Execute();
        }
    }
}
