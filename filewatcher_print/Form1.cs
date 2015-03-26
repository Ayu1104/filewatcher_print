/*フォルダ監視して自動で印刷してくれる
 * アプリケーションをめざして作ってます
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace filewatcher_print
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private FileSystemWatcher watcher = null;
        string filepath;
        string foldername = string.Empty;

        private void button1_Click(object sender, EventArgs e)
        {
            if (watcher != null) return;
            watcher = new FileSystemWatcher();

            // StreamReader の新しいインスタンスを生成する
            StreamReader cReader = (
                new System.IO.StreamReader(@"fconfig.txt", System.Text.Encoding.Default)
            );

            //読み込みできる文字がなくなるまで繰り返す
            while (cReader.Peek() >= 0)
            {
                //ファイルを1行ずつ読み込む
                string stBuffer = cReader.ReadLine();
                //読み込んだものを追加で格納する
                foldername += stBuffer;
            }

            //cReaderとじる→オブジェクトの破棄を保証する
            cReader.Close();

            //監視するフォルダを指定
            //watcher.Path = @"C:\Users\1223138\Desktop\filewatch"; //ここ変える
            //カレントディレクトリを↑と同じにする↓
            //Directory.SetCurrentDirectory(@"C:\Users\1223138\Desktop\filewatch"); //ここも変える

            watcher.Path = @foldername;
            Directory.SetCurrentDirectory(@foldername);

            //最終アクセス、最終更新、ファイル、フォルダ名の変更を監視
            watcher.NotifyFilter =
                (NotifyFilters.LastAccess
                |NotifyFilters.LastWrite
                |NotifyFilters.FileName
                |NotifyFilters.DirectoryName);
            //全てのファイルを監視
            watcher.Filter = "";
            //UIのスレッドにマーシャリングする？よくわからんちん
            watcher.SynchronizingObject = this;

            //イベントハンドラの追加
            watcher.Changed += new FileSystemEventHandler(watcher_Changed);
            watcher.Created += new FileSystemEventHandler(watcher_Changed);
            watcher.Deleted += new FileSystemEventHandler(watcher_Changed);
            watcher.Renamed += new RenamedEventHandler(watcher_Renamed);

            //監視を開始する
            try
            {
                watcher.EnableRaisingEvents = true;
            }
            catch
            {
                Console.Write(foldername + "が見当たりません");
            }
            Console.WriteLine("監視を開始"); //コンソールに出力
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //監視を終了させる
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;
            Console.WriteLine("監視を終了");
        }
        
        //ファイル監視イベントハンドラ
        private void watcher_Changed(System.Object source, FileSystemEventArgs e)
        {

            filepath = e.FullPath; //フルパスで記憶

            switch(e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    Console.WriteLine(
                        "ファイル「" + e.FullPath + "」が変更された");
                    break;
                case WatcherChangeTypes.Created:
                    Console.WriteLine(
                        "ファイル「" + e.FullPath + "」が作成された");
                    Console.WriteLine("印刷するぞい");
                    pd_Print(); //印刷するぞい

                    break;
                case WatcherChangeTypes.Deleted:
                    Console.WriteLine(
                        "ファイル「" + e.FullPath + "」が削除された");
                    break;
            }
        }

        private void watcher_Renamed(System.Object source,RenamedEventArgs e)
        {
            Console.WriteLine(
                "ファイル「" + e.FullPath + "」の名前が変更された");
        }

        //印刷イベントハンドラ
        private void pd_Print()
        {
            using (PrintDocument doc = new PrintDocument())
            {
                /*
                               doc.DefaultPageSettings.Landscape = true;

                // プリンタがサポートしている用紙サイズを調べる
                foreach (PaperSize ps in doc.PrinterSettings.PaperSizes)
                {
                    // A4用紙に設定
                    if (ps.Kind == PaperKind.A4)
                    {
                        doc.DefaultPageSettings.PaperSize = ps;
                        break;
                    }
                }
                */

                //PrintDocumentオブジェクトの作成
                System.Drawing.Printing.PrintDocument pd =
                    new System.Drawing.Printing.PrintDocument();
                //PrintPageイベントハンドラの追加
                pd.PrintPage +=
                    new System.Drawing.Printing.PrintPageEventHandler(pd_PrintPage);

                // 用紙の向きを設定(横：true、縦：false)
                pd.DefaultPageSettings.Landscape = true;

                // プリンタがサポートしている用紙サイズを調べる
                foreach (PaperSize ps in pd.PrinterSettings.PaperSizes)
                {
                    // A4用紙に設定
                    if (ps.Kind == PaperKind.A4)
                    {
                        doc.DefaultPageSettings.PaperSize = ps;
                        break;
                    }
                }

                //印刷を開始する
                pd.Print();
            }
        }

        private void pd_PrintPage(object sender,System.Drawing.Printing.PrintPageEventArgs e)
        {
            string filename = Path.GetFileName(filepath); //ファイル名をとりだす

            //画像を読み込む
            Image img = Image.FromFile(filename);
            //画像を描画
            //e.Graphics.DrawImage(img, e.MarginBounds);
            e.Graphics.DrawImage(img,580,0,img.Width*0.8f,img.Height*0.8f);
            //次のページがないことを通知する(必要)
            e.HasMorePages = false;
            //後始末をする
            img.Dispose();
        }


    }
}
