using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tvmaid
{
    //独自の例外
    class AppException : ApplicationException
    {
        public AppException(string msg) : base(msg) { }
    }

    //拡張メソッド
    static class StringExtension
    {
        public static string Formatex(this string format, params object[] values)
        {
            return string.Format(format, values);
        }

        public static int ToInt(this string s)
        {
            return int.Parse(s);
        }

        public static long ToLong(this string s)
        {
            return long.Parse(s);
        }

        public static DateTime ToDateTime(this string s)
        {
            return DateTime.Parse(s);
        }
    }

    static class Util
    {
        //実行ファイルのフォルダ
        public static string GetBasePath()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        //Webサーバルートフォルダ
        public static string GetWwwRootPath()
        {
            return Path.Combine(Util.GetBasePath(), "wwwroot");
        }

        //ユーザ設定フォルダ
        public static string GetUserPath()
        {
            return Path.Combine(Util.GetBasePath(), "user");
        }

        public static string SetDoubleQuot(string str)
        {
            return "\"" + str + "\"";
        }

        //ユーザ設定ファイルがなければコピー
        public static void CopyUserFile()
        {
            var user = Util.GetUserPath();
            if (Directory.Exists(user) == false)
            {
                Directory.CreateDirectory(user);
            }

            var dir = new DirectoryInfo(Path.Combine(Util.GetBasePath(), "original"));
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var path = Path.Combine(user, file.Name);
                if (File.Exists(path) == false)
                {
                    file.CopyTo(path);
                }
            }
        }
    }

    //設定ファイル
    class DefineFile
    {
        Dictionary<string, string> list = new Dictionary<string, string>();
        string path;

        public DefineFile(string path)
        {
            this.path = path;
        }

        public string this[string key]
        {
            set { list[key] = value; }
            get { return list[key]; }
        }

        public Dictionary<string, string>.KeyCollection Keys
        {
            get { return list.Keys; }
        }

        public void Clear()
        {
            list.Clear();
        }

        public void Save()
        {
            using (var sw = new StreamWriter(path, false, Encoding.GetEncoding("utf-8")))
            {
                foreach (string key in list.Keys)
                {
                    sw.WriteLine(key + "=" + list[key]);
                }
            }
        }

        public void Load()
        {
            using (var sr = new StreamReader(path, Encoding.GetEncoding("utf-8")))
            {
                var text = sr.ReadToEnd();
                string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    if (line[0] == ';') { continue; }

                    int sepa = line.IndexOf('=');
                    if (sepa == -1) { continue; }

                    var key = line.Substring(0, sepa);
                    var val = "";
                    if (sepa + 1 < line.Length)
                    {
                        val = line.Substring(sepa + 1);
                    }

                    list[key] = val;
                }
            }
        }
    }

    //入場券
    //mutexを使いやすくしたクラス
    class Ticket : IDisposable
    {
        Mutex mutex;

        public Ticket(string name)
        {
            mutex = new Mutex(false, name);
        }

        public bool GetOwner(int timeout)
        {
            //放棄された場合(AbandonedMutexException)も正常終了とする
            try { return mutex.WaitOne(timeout); }
            catch (AbandonedMutexException) { return true; }
        }

        public void Release()
        {
            mutex.ReleaseMutex();
        }

        public void Dispose()
        {
            mutex.ReleaseMutex();
            mutex.Dispose();
        }
    }

    //テキストコンバータ
    //全角数字、全角アルファベット、半角カタカナ等を変換
    class TextConv
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();
        static TextConv singleObj = null;

        public static TextConv GetInstance()
        {
            if (singleObj == null)
            {
                singleObj = new TextConv();
            }
            return singleObj;
        }

        public TextConv()
        {
            var def = Path.Combine(Util.GetUserPath(), "convert.txt");

            using (var sr = new StreamReader(def, Encoding.GetEncoding("utf-8")))
            {
                var text = sr.ReadToEnd();

                string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    string[] item = line.Split(new char[] { ':' });
                    if (item.Length >= 2 && item[0] != "")
                    {
                        dic[item[0]] = item[1];
                    }
                }
            }
        }

        public string Convert(string src)
        {
            var sb = new StringBuilder(src);

            foreach (var pair in dic)
            {
                sb = sb.Replace(pair.Key, pair.Value);
            }
            return sb.ToString();
        }
    }

    class GenreConv
    {
        Dictionary<int, string> genres = new Dictionary<int, string>();
        Dictionary<int, string> subGenres = new Dictionary<int, string>();
        static GenreConv singleObj = null;

        public static GenreConv GetInstance()
        {
            if (singleObj == null)
            {
                singleObj = new GenreConv();
            }
            return singleObj;
        }

        public GenreConv()
        {
            LoadFile(genres, "genre.txt");
            LoadFile(subGenres, "subgenre.txt");
        }

        public void LoadFile(Dictionary<int, string> dic, string file)
        {
            var def = Path.Combine(Util.GetUserPath(), file);

            using (var sr = new StreamReader(def, Encoding.GetEncoding("utf-8")))
            {
                var text = sr.ReadToEnd();

                string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    string[] item = line.Split(new char[] { ':' });
                    if (item.Length == 1)
                    {
                        
                        int code = Convert.ToInt32(item[0], 16);
                        dic[code] = "";
                    }
                    else if(item.Length == 2)
                    {
                        int code = Convert.ToInt32(item[0], 16);
                        dic[code] = item[1];
                    }
                }
            }
        }

        public string GetText(int genre, int subGenre)
        {
            var text = "";
            if (genres.ContainsKey(genre))
            {
                text = genres[genre];
            }
            text += "/";

            var sub = (genre << 8) + subGenre;
            if (subGenres.ContainsKey(sub))
            {
                text += subGenres[sub];
            }
            return text;
        }
    }

    //復帰タイマー
    class WakeTimer
    {
        IntPtr handle = IntPtr.Zero;

        public void SetTimer(DateTime wake)
        {
            Cancel();

            handle = CreateWaitableTimer(IntPtr.Zero, true, "WaitableTimer");
            if(handle.ToInt32() == 0)
            {
                throw new AppException("復帰タイマーの設定に失敗しました。エラーコード = " + Marshal.GetLastWin32Error().ToString());
            }

            long interval = (wake - DateTime.Now).Ticks * -1;
            var ret = SetWaitableTimer(handle, ref interval, 0, IntPtr.Zero, IntPtr.Zero, true);
            if(ret == false)
            {
                throw new AppException("復帰タイマーの設定に失敗しました。エラーコード = " + Marshal.GetLastWin32Error().ToString());
            }
        }

        public void Cancel()
        {
            if (handle != IntPtr.Zero)
            {
                CancelWaitableTimer(handle);
                CloseHandle(handle);
                handle = IntPtr.Zero;
            }
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWaitableTimer(IntPtr hTimer, [In] ref long pDueTime, int lPeriod, IntPtr pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine, bool fResume);
		
        [DllImport("kernel32.dll")]
        static extern bool CancelWaitableTimer(IntPtr hTimer);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);
    }

    //システムスリープ状態
    class SleepState
    {
        static int sleepStopCount = 0;  //スリープ抑止カウンタ
        static object lockObj = 0;

        public static bool SleepStop
        {
            get { lock(lockObj) { return sleepStopCount > 0; } }
        }

        public static void SetSleepStop(bool stop)
        {
            lock (lockObj)
            {
                if (stop)
                {
                    sleepStopCount++;
                    if (sleepStopCount == 1)
                    {
                        //Log.Write("スリープ抑止をセット");
                        SleepState.SetState(true);
                    }
                }
                else
                {
                    sleepStopCount--;
                    if (sleepStopCount == 0)
                    {
                        SleepState.SetState(false);
                        //Log.Write("スリープ抑止を解除");
                    }
                }
            }
        }

        //state = trueでスリープ抑止
        static void SetState(bool stop)
        {
            if (stop)
            {
                SetThreadExecutionState(ExecutionState.SystemRequired | ExecutionState.Continuous);
            }
            else
            {
                SetThreadExecutionState(ExecutionState.Continuous);
            }
        }

        [DllImport("kernel32.dll")]
        extern static ExecutionState SetThreadExecutionState(ExecutionState esFlags);

        [FlagsAttribute]
        enum ExecutionState : uint
        {
	        SystemRequired = 1,     // スタンバイを抑止
	        DisplayRequired = 2,    // 画面OFFを抑止
	        Continuous = 0x80000000 // 効果を永続
        }
    }
}
