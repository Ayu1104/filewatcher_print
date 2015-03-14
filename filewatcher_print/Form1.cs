/*フォルダ監視して自動で印刷してくれる
 * アプリケーションをめざして作ってます
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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

        private System.IO.FileSystemWatcher watcher = null;
        string filepath;

        private void button1_Click(object sender, EventArgs e)
        {
            if (watcher != null) return;

            watcher = new System.IO.FileSystemWatcher();
            
            //監視するフォルダを指定
            watcher.Path = @"C:\Users\1223138\Desktop\filewatch"; //ここ変える
            //カレントディレクトリを↑と同じにする
            System.IO.Directory.SetCurrentDirectory(@"C:\Users\1223138\Desktop\filewatch"); //ここも変える

            //最終アクセス、最終更新、ファイル、フォルダ名の変更を監視
            watcher.NotifyFilter =
                (System.IO.NotifyFilters.LastAccess
                |System.IO.NotifyFilters.LastWrite
                |System.IO.NotifyFilters.FileName
                |System.IO.NotifyFilters.DirectoryName);
            //全てのファイルを監視
            watcher.Filter = "";
            //UIのスレッドにマーシャリングする？よくわからんちん
            watcher.SynchronizingObject = this;

            //イベントハンドラの追加
            watcher.Changed += new System.IO.FileSystemEventHandler(watcher_Changed);
            watcher.Created += new System.IO.FileSystemEventHandler(watcher_Changed);
            watcher.Deleted += new System.IO.FileSystemEventHandler(watcher_Changed);
            watcher.Renamed += new System.IO.RenamedEventHandler(watcher_Renamed);

            //監視を開始する
            watcher.EnableRaisingEvents = true;
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
        private void watcher_Changed(System.Object source,System.IO.FileSystemEventArgs e)
        {

            filepath = e.FullPath; //フルパスで記憶

            switch(e.ChangeType)
            {
                case System.IO.WatcherChangeTypes.Changed:
                    Console.WriteLine(
                        "ファイル「" + e.FullPath + "」が変更された");
                    break;
                case System.IO.WatcherChangeTypes.Created:
                    Console.WriteLine(
                        "ファイル「" + e.FullPath + "」が作成された");
                    Console.WriteLine("印刷するぞい");
                    pd_Print(); //印刷するぞい

                    break;
                case System.IO.WatcherChangeTypes.Deleted:
                    Console.WriteLine(
                        "ファイル「" + e.FullPath + "」が削除された");
                    break;
            }
        }

        private void watcher_Renamed(System.Object source,System.IO.RenamedEventArgs e)
        {
            Console.WriteLine(
                "ファイル「" + e.FullPath + "」の名前が変更された");
        }

        //印刷イベントハンドラ
        private void pd_Print()
        {
            //PrintDocumentオブジェクトの作成
            System.Drawing.Printing.PrintDocument pd =
                new System.Drawing.Printing.PrintDocument();
            //PrintPageイベントハンドラの追加
            pd.PrintPage +=
                new System.Drawing.Printing.PrintPageEventHandler(pd_PrintPage);
            //印刷を開始する
            pd.Print();
        }

        private void pd_PrintPage(object sender,System.Drawing.Printing.PrintPageEventArgs e)
        {
            string filename = System.IO.Path.GetFileName(filepath); //ファイル名をとりだす

            //画像を読み込む
            Image img = Image.FromFile(filename);
            //画像を描画
            e.Graphics.DrawImage(img, e.MarginBounds);
            //次のページがないことを通知する(必要)
            e.HasMorePages = false;
            //後始末をする
            img.Dispose();
        }


    }
}
