using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Tvmaid
{
    //TSの状態
    class TsStatus
    {
        public int Error;       //エラー数
        public int Drop;        //ドロップ数
        public int Scramble;    //復号化エラー数
    }

    //TVプロセス
    //TVTestプラグインとの通信を行う
    class TvProcess
    {
        string driverId;
        string driverPath;
        const int timeout = 60 * 1000;

        //コールナンバー(ウインドウメッセージ番号)
        enum CallNum
        {
            Close = 0xb000,
            GetState,
            GetServices,
            GetEvents,
            SetService,
            StartRec,
            StopRec,
            GetEventTime,
            GetTsStatus
        }

        //TVTestプラグインエラーコード
        public enum ErrorCode
        {
            NoError = 0,
            CreateShared,
            CreateWindow,
            CreateMutex,
            StartRec,
            StopRec,
            SetService,
            GetEvents,
            GetState,
            GetEnv,
            GetEventTime,
            GetTsStatus
        }

        public TvProcess(string driverId, string driverPath)
        {
            this.driverId = driverId;
            this.driverPath = driverPath;
        }

        string ShareName
        {
            get { return "/tvmaid/map/" + driverId; }
        }

        //指定チューナを使用中のレコーダが存在するかどうか
        public bool IsOpen()
        {
            var handle = FindWindow("/tvmaid/win", driverId);
            return handle != IntPtr.Zero;
        }

        public void Open(bool show)
        {
            string param = show ? "" : " /nodshow /min /silent";

            var p = new Process();
            p.StartInfo.FileName = Program.UserDef["tvtest"];
            p.StartInfo.Arguments = string.Format("/d \"{0}\"" + param, driverPath);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.EnvironmentVariables.Add("DriverId", driverId);
            p.Start();
            p.WaitForInputIdle();

            //TVTestのウインドウがアイドルになっても、プラグインの初期化が終わっているとは限らない
            //プラグインの通信用ウインドウが作成されるのを待つ
            for (int i = 0; i < timeout / 100; i++)
            {
                if (IsOpen())
                {
                    return;
                }
                Thread.Sleep(100);
            }

            throw new AppException("TVTestの初期化が時間内に終了しませんでした。[原因]TVTestが初期化中にエラーになったか、PCの負荷が高過ぎる等が考えられます。");
        }

        public void Close()
        {
            LockCall(() =>
            {
                Call(CallNum.Close);

                //閉じるのを待つ
                //待たないと、すぐに次の予約があるとき録画が失敗する
                for (int i = 0; i < timeout / 100; i++)
                {
                    if (IsOpen() == false)
                    {
                        return;
                    }
                    Thread.Sleep(100);
                }
            });
        }

        public int GetState()
        {
            return LockCall<int>(() =>
            {
                Call(CallNum.GetState);

                using (var shared = new Shared(ShareName))
                {
                    string ret = shared.Read();
                    return Convert.ToInt32(ret);
                }
            });
        }

        T LockCall<T>(Func<T> action)
        {
            var ticket = new Ticket("/tvmaid/mutex/call/" + driverId);
            try
            {
                if (ticket.GetOwner(timeout))
                {
                    return action();
                }
                else
                {
                    throw new AppException("TVTest呼び出しに失敗しました。時間内で呼び出しができませんでした。");
                }
            }
            finally { ticket.Dispose(); }
        }


        //サービスリストを取得
        public List<Service> GetServices()
        {
            return LockCall<List<Service>>(() =>
            {
                Call(CallNum.GetServices);

                using (var shared = new Shared(ShareName))
                {
                    var list = new List<Service>();
                    while (true)
                    {
                        string ret = shared.Read();
                        if (ret == null) { break; }

                        string[] data = ret.Split(new char[] { '\x1' }, StringSplitOptions.RemoveEmptyEntries);
                        var s = new Service();
                        s.Driver = System.IO.Path.GetFileName(driverPath);
                        s.Nid = Convert.ToInt32(data[0]);
                        s.Tsid = Convert.ToInt32(data[1]);
                        s.Sid = Convert.ToInt32(data[2]);
                        s.Name = data[3];
                        list.Add(s);
                    }
                    return list;
                }
            });
        }

        //サービス切り替え
        public void SetService(Service service)
        {
            LockCall(() =>
            {
                using (var shared = new Shared(ShareName))
                {
                    var arg = string.Format("{0}\x1{1}\x1{2}\x0", service.Nid, service.Tsid, service.Sid);
                    shared.Write(arg);
                    Call(CallNum.SetService);
                }
            });
        }

        //指定イベントの時間を取得
        //録画時の追従用
        //EventのStartとDurationのみセットする
        public Event GetEventTime(Service service, int eid)
        {
            return LockCall<Event>(() =>
            {
                using (var shared = new Shared(ShareName))
                {
                    var arg = "{0}\x1{1}\x1{2}\x1{3}\x0".Formatex(service.Nid, service.Tsid, service.Sid, eid);
                    shared.Write(arg);

                    Call(CallNum.GetEventTime);

                    var ret = shared.Read();
                    string[] data = ret.Split(new char[] { '\x1' });

                    var ev = new Event();
                    ev.Start = Convert.ToDateTime(data[0]);
                    ev.Duration = Convert.ToInt32(data[1]);
                    return ev;
                }
            });
        }

        //TSの状態を取得
        public TsStatus GetTsStatus()
        {
            return LockCall<TsStatus>(() =>
            {
                using (var shared = new Shared(ShareName))
                {
                    Call(CallNum.GetTsStatus);

                    var ret = shared.Read();
                    string[] data = ret.Split(new char[] { '\x1' });

                    var ts = new TsStatus();
                    ts.Error = Convert.ToInt32(data[0]);
                    ts.Drop = Convert.ToInt32(data[1]);
                    ts.Scramble = Convert.ToInt32(data[2]);
                    return ts;
                }
            });
        }

        //番組表取得
        public List<Event> GetEvents(Service service)
        {
            return LockCall<List<Event>>(() =>
            {
                using (var shared = new Shared(ShareName))
                {
                    var arg = string.Format("{0}\x1{1}\x1{2}\x0", service.Nid, service.Tsid, service.Sid);
                    shared.Write(arg);

                    Call(CallNum.GetEvents);

                    var list = new List<Event>();
                    while (true)
                    {
                        var ret = shared.Read();
                        if (ret == null) { break; }

                        //データの一部が、""になっている場合があるので、
                        //StringSplitOptions.RemoveEmptyEntriesをつけてはいけない
                        string[] data = ret.Split(new char[] { '\x1' });

                        var ev = new Event();
                        ev.Eid = Convert.ToInt32(data[0]);
                        ev.Start = Convert.ToDateTime(data[1]);
                        ev.Duration = Convert.ToInt32(data[2]);
                        ev.Title = data[3];
                        ev.Desc = data[4];
                        ev.LongDesc = data[5];
                        ev.Genre = Convert.ToInt32(data[6]);
                        ev.SubGenre = Convert.ToInt32(data[7]);
                        ev.Fsid = service.Fsid;
                        list.Add(ev);
                    }
                    return list;
                }
            });
        }

        public void StartRec(string file)
        {
            LockCall(() =>
            {
                using (var shared = new Shared(ShareName))
                {
                    var arg = string.Format("{0}\0", file);
                    shared.Write(arg);

                    Call(CallNum.StartRec);
                }
            });
        }

        public void StopRec()
        {
            LockCall(() => Call(CallNum.StopRec));
        }

        void LockCall(Action action)
        {
            var ticket = new Ticket("/tvmaid/mutex/call/" + driverId);
            try
            {
                if (ticket.GetOwner(timeout))
                {
                    action();
                }
                else
                {
                    throw new AppException("TVTest呼び出しに失敗しました。時間内で呼び出しができませんでした。");
                }
            }
            finally { ticket.Dispose(); }
        }

        //TVTest呼び出し
        void Call(CallNum cn)
        {
            var handle = FindWindow("/tvmaid/win", driverId);
            if(handle == IntPtr.Zero)
            {
                throw new AppException("TVTest呼び出しに失敗しました。通信用ウインドウがありません。");
            }

            UIntPtr result;
            IntPtr ret = SendMessageTimeout(handle, (uint)cn, UIntPtr.Zero, IntPtr.Zero, (uint)SendMessageTimeoutFlags.SMTO_NORMAL, timeout, out result);
            if (ret.ToInt32() == 0)
            {
                throw new AppException("TVTest呼び出しに失敗しました。時間内に応答がありません。");
            }

            uint code = result.ToUInt32();
            if (code != 0)
            {
                throw new TvProcessExceotion(code, "TVTestでエラーが発生しました。エラーコード: " + code);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [Flags]
        public enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            UIntPtr wParam,
            IntPtr lParam,
            uint fuFlags,
            uint uTimeout,
            out UIntPtr lpdwResult
        );
    }
    
    //独自の例外
    class TvProcessExceotion : AppException
    {
        public uint Code;

        public TvProcessExceotion(uint code, string msg) : base(msg)
        {
            Code = code;
        }
    }

    //共有メモリ
    class Shared : IDisposable
    {
        MemoryMappedFile map = null;
        MemoryMappedViewAccessor acc = null;
        long position;

        const int argSize = 1 * 1000 * sizeof(char);

        public Shared(string name)
        {
            try
            {
                map = MemoryMappedFile.OpenExisting(name);
                acc = map.CreateViewAccessor();
                position = argSize;
            }
            catch(System.IO.FileNotFoundException)
            {
                throw new AppException("TVTestが起動していません。");
            }
        }

        public void Dispose()
        {
            acc.Dispose(); 
            map.Dispose(); 
        }
        
        public void Write(string str)
        {
            byte[] arr = Encoding.Unicode.GetBytes(str);
            acc.WriteArray(0, arr, 0, arr.Length);
        }

        public string Read()
        {
            var str = new StringBuilder();
            while (true)
            {
                char c = acc.ReadChar(position);
                if (c == '\x0') { return null; }
                position += sizeof(char);
                if (c == '\x2') { return str.ToString(); }
                str.Append(c);
            }
        }
    }
}
