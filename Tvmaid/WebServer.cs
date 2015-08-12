using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Tvmaid
{
    //内蔵WEBサーバ
    //listener.Start()で失敗しないように、「HTTP URL名前空間」の予約を行う。
    //コマンドラインで、以下のコマンドを実行する(管理者権限)。
    //netsh http add urlacl url=http://+:20000/ user=Everyone
    class WebServer : IDisposable
    {
        HttpListener listener = new HttpListener();
        bool stop = false;

        public void Start()
        {
            var root = Util.GetWwwRootPath(); // ドキュメント・ルート
            var prefix = "http://+:20000/"; // 受け付けるURL

            try
            {
                listener.Prefixes.Add(prefix); // プレフィックスの登録
                listener.Start();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Webサーバの初期化に失敗しました。\n"
                                                    + "[原因] 二重起動した。「URL名前空間」の予約を行っていない。\n"
                                                    + "[詳細] " + e.Message, Program.Logo);
                System.Windows.Forms.Application.Exit(); // 終了
            }

            while (stop == false)
            {
                HttpListenerContext context;
                try
                {
                    context = listener.GetContext();
                    TaskList.StartNew(Excute, context);
                }
                catch (HttpListenerException) { return; }
                catch (ObjectDisposedException) { return; }
            }
        }

        public void Dispose()
        {
            listener.Close();
            stop = true;
        }

        void Excute(object state)
        {
            var context = (HttpListenerContext)state;

            var req = context.Request;
            var res = context.Response;

            //Log.Write(req.Url.OriginalString);

            if (req.Url.AbsolutePath == "/webapi")
            {
                try
                {
                    //WebApi実行
                    var api = new WebApi(req, res);
                    api.Exec();
                }
                catch (Exception e) { Log.Write(e.Message + e.StackTrace); }

                try
                {
                    res.Close();
                }
                catch (Exception) {}

            }
            else
            {
                //ファイル転送
                //TODO:NotModified受付
                try
                {
                    // リクエストされたURLからファイルのパスを求める
                    string path = Util.GetWwwRootPath() + req.Url.AbsolutePath;

                    // ファイルが存在すればレスポンス・ストリームに書き出す
                    if (File.Exists(path))
                    {
                        byte[] content = File.ReadAllBytes(path);
                        res.ContentType = GetContentType(path);
                        res.OutputStream.Write(content, 0, content.Length);
                        res.Close();
                    }
                    else
                    {
                        //NotFound
                        res.StatusCode = (int)HttpStatusCode.NotFound;
                        res.StatusDescription = "NotFound";
                        byte[] content = File.ReadAllBytes(Path.Combine(Util.GetUserPath(), "404.html"));
                        res.ContentType = "text/html";
                        res.OutputStream.Write(content, 0, content.Length);
                        res.Close();
                    }
                }
                catch (Exception e)
                {
                    Log.Write("Webサーバのファイル転送時にエラーが発生しましたが、プログラムは続行します。\n[詳細]" + e.Message);
                }
            }
        }

        //ContentTypeを取得
        public string GetContentType(string path)
        {
            var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(Path.GetExtension(path));
            var val = key.GetValue("Content Type");
            return (key == null || val == null) ? "" : val.ToString();
        }
    }
}