using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PixivDownload;
using System.IO;
using System.Web;
using System.Threading;
namespace WindowsFormsApplication2
{

    public partial class Form1 : Form
    {
        SynchronizationContext context;
        DownloadProcess downloadProcess;
        public Form1()
        {
            InitializeComponent();
            context=SynchronizationContext.Current;
        }

        String[] readFile()
        {
            try
            {
                FileStream fs = new FileStream("./user.txt", FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                String tempStr;
                String[] reStr = new String[2];
                reStr[0] = sr.ReadLine();
                reStr[1] = sr.ReadLine();
                for (int i = 0; i < reStr.Length; i++) { if (reStr[i] == null) { return null; } }
                fs.Close();
                sr.Close();
                return reStr;
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            String[] tempStr=readFile();
            if (tempStr != null)
            {
                userBox.Text = tempStr[0];
                pwdBox.Text = tempStr[1];
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            banButton("1");
            String username = userBox.Text;
            String password = pwdBox.Text;
            String id = idBox.Text;

            //MessageBox.Show(username + " " + password + " " + id);

            Pixiv pixiv = new Pixiv(HttpUtility.UrlEncode(username), password);//需要转换@为%40

            if (pixiv.login() == false)
            {
                MessageBox.Show("登录失败");
                return;
            }
            else
            {
                FileStream fs = new FileStream("./user.txt", FileMode.Create);
                StreamWriter sr = new StreamWriter(fs, Encoding.UTF8);
                sr.Write(userBox.Text + "\n" + pwdBox.Text);
                sr.Close();
                fs.Close();
            }
            downloadProcess=pixiv.downloadImages(id, sameDirCheckBox.Checked);
            if (downloadProcess == null)
            {
                MessageBox.Show("下载失败");
            }

            downloadProcess.start();
            Thread thread = new Thread(this.run);
            thread.Start();
        }

        //监控线程
        void run()
        {
            for (; ; )
            {
                Thread.Sleep(100);
                context.Post(setInfoLabel, "正在下载第"+downloadProcess.PicPage+"张");
                if (!downloadProcess.Started)
                {
                    context.Post(setInfoLabel, "下载成功!!");
                    downloadProcess = null;
                    context.Post(banButton, "0");
                    break;
                }
            }
        }

        //用于post
        void setInfoLabel(object text)
        {
            info_label.Text = text.ToString();
        }

        void banButton(object ban)
        {
            if (ban.ToString().Equals("1"))
            {
                downloadButton.Enabled = false;
                downloadButton.Text = "正在下载";
            }
            else 
            {
                downloadButton.Enabled = true;
                downloadButton.Text = "下载图片";
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
