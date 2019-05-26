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

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
                return reStr;
                fs.Close();
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
            String username = userBox.Text;
            String password = pwdBox.Text;
            String id = idBox.Text;

            //MessageBox.Show(username + " " + password + " " + id);

            Pixiv pixiv = new Pixiv(HttpUtility.UrlEncode(username), password);//需要转换@为%40

            if (pixiv.login() == false)
            {
                MessageBox.Show("登录失败");
            }
            else
            {
                FileStream fs = new FileStream("./user.txt", FileMode.Create);
                StreamWriter sr = new StreamWriter(fs, Encoding.UTF8);
                sr.Write(userBox.Text + "\n" + pwdBox.Text);
                sr.Close();
                fs.Close();
            }
            if (pixiv.downloadImages(id,sameDirCheckBox.Checked) == false)
            {
                MessageBox.Show("下载失败");
            }
            else
            {
                MessageBox.Show("下载成功");
            }


            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
