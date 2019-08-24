using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PixivDownload
{
    class HttpRequest
    {
        CookieContainer cookies;

        public CookieContainer Cookies
        {
            get { return cookies; }
            set { }
        }
        
        public HttpRequest()
        {
            cookies = new CookieContainer();
        }

        public void download(String url,String path,String refer)
        {
            HttpWebRequest tempRequest;
            tempRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            tempRequest.Method="GET";
            tempRequest.CookieContainer=cookies;
            tempRequest.Referer=refer;
            tempRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586";
           

            using (WebResponse res = tempRequest.GetResponse())
            {
                Stream stream = res.GetResponseStream();//响应流 
                FileStream fileStream = new FileStream(path, FileMode.Create);//文件流
                BufferedStream bStream=new BufferedStream(stream);//缓冲响应流
                byte[] byteArr = new byte[1024];//暂存数据
                
                int size;
                while((size=stream.Read(byteArr,0,byteArr.Length))>0)
                {
                    fileStream.Write(byteArr,0,size);
                }
                fileStream.Close();
            }
        }

        String send(HttpWebRequest _HWR)
        {
            
            //初始化数据
            String _returnStr = "";
            HttpWebRequest tempRequest = _HWR;
            //tempRequest.ContentType = "text/html;charset=UTF-8";


            tempRequest.Referer = "https://accounts.pixiv.net/login?lang=zh&source=pc&view_type=page&ref=wwwtop_accounts_index";
            //tempRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/18.17763";
            tempRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586";


            //System.Net.WebException
            //会释放掉在语句中创建的对象
            using (WebResponse res = tempRequest.GetResponse())
            {
                Stream stream = res.GetResponseStream();
                StreamReader SReader = new StreamReader(stream, Encoding.UTF8);
                String tempString;
                while ((tempString = SReader.ReadLine()) != null)
                {
                    _returnStr = _returnStr + tempString + '\n';
                }
            }
            return _returnStr;
        }

        public String sendPost(String url,params String[] strArr) 
        {
            
            HttpWebRequest tempRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            tempRequest.CookieContainer = cookies;
            tempRequest.Referer = "https://accounts.pixiv.net/login?lang=zh&source=pc&view_type=page&ref=wwwtop_accounts_index";
            tempRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586";
            tempRequest.ContentType = "application/x-www-form-urlencoded";

            //添加头文件
            tempRequest.Method = "post";

            String writeStr="";
            for (int i = 0; i < strArr.Length;i++ )
            {
                if (i >0)
                {
                    writeStr += "&";
                }
                writeStr += strArr[i];
            }
            Console.Write(writeStr);

            //创建一个str写入
            byte[] postdatabyte = Encoding.UTF8.GetBytes(writeStr);
            tempRequest.ContentLength = postdatabyte.Length;

            Stream stream = tempRequest.GetRequestStream();
            stream.Write(postdatabyte, 0, postdatabyte.Length);
            return send(tempRequest);
        }
        
        public String sendGet(String url, params String[] strArr)
        {
            String _getUrl = url;
            String _returnStr = "";

            //将子数据传进GET中
            char connChar='?';
            for (int i=0;i<strArr.Length;i++)
            {
                if (i == 1)
                {
                    connChar = '&';
                }
                _getUrl = _getUrl + connChar + strArr[i];
            }

            //初始化数据
            HttpWebRequest tempRequest;
            tempRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            tempRequest.CookieContainer = cookies;
            tempRequest.Method = "GET";

            return send(tempRequest);
        }
    }
    
    class Pixiv
    {
        bool isLogin;
        String username;
        String password;
        HttpRequest httpRequest;
        String HttpKey;
        public Pixiv(String username,String password)
        {
            //this.username = System.Web.HttpUtility.UrlDecode(username);
            this.isLogin = false;
            this.username = username;
            this.password=password;
            httpRequest = new HttpRequest();

        }

        String getPostKey(String html)//在这里通过正则表达式找到key
        {
            String pattern="<input\\stype=\"hidden\"\\sname=\"post_key\"\\svalue=\"(.{32})\">";
            String returnStr=Regex.Match(html,pattern).Groups[1].Value;
            Console.Write("getPostKey:" + returnStr+"\n");
            return returnStr;
        }

        public bool login()
        {
            try
            {
                String strGet = httpRequest.sendGet("https://accounts.pixiv.net/login", "lang=zh");
                String post_key = getPostKey(strGet);
                String strPost = httpRequest.sendPost(
                    "https://accounts.pixiv.net/login?lang=zh",
                    "post_key=" + post_key,
                    "return_to=https%3A%2F%2Fwww.pixiv.net%2F",
                    "source=pc",
                    "pixiv_id=" + username,
                    "ref=wwwtop_accounts_index",
                    "password=" + password,
                    "g_recaptcha_response=",
                    "captcha="
                    );

                //检测是否登录成功
                Match match = Regex.Match(strPost, "请确认您所输入的pixiv ID,电邮以及密码是否正确。");
                if (match.Groups[0].Value != "")
                {
                    return false;
                }
                
            }
            catch (Exception e)
            {
                return false;
            }
            isLogin = true;
            return true;
        }

        class DownloadUrl
        {
            String downloadUrl;
            public String Url
            {
                get { return downloadUrl; }
                set { }
            }
            public DownloadUrl(String id, int page)
            { 
                downloadUrl="https://www.pixiv.net/member_illust.php?mode=manga_big&illust_id="+id+"&page="+page;
            }
            public DownloadUrl(String id)
            {
                downloadUrl="https://www.pixiv.net/member_illust.php?mode=manga_big&illust_id="+id;
            }
        }

        public bool downloadImages(String id,bool isSame=false)
        {
            String downloadDirectory;
            if (isSame)
            {
                downloadDirectory = ".\\directory";
            }
            else
            {
                downloadDirectory = ".\\" + id;
            }

            if (isLogin == false)
            {
                return false;
            }
            int picPage;


            Directory.CreateDirectory(downloadDirectory);


            DownloadUrl downloadUrl = new DownloadUrl(id);//网站显示页面
            String strGet = httpRequest.sendGet(downloadUrl.Url);
            //"original":"https:\/\/i.pximg.net\/img-original\/img\/2019\/08\/22\/21\/30\/44\/76401893_p0.jpg"
            String pattern="\"original\":\"(.+?)\"";
            Match match = Regex.Match(strGet, pattern);
            //Match match = Regex.Match(strGet, "<body><img src=\"(.+)\" onclick");
            String url = match.Groups[1].Value;//图片链接
            String model=url.Replace("\\", "");

            for (picPage=0; ;picPage++ )
            {
                try
                {
                    url=model.Replace("p0","p"+picPage);
                    //防止中途被删除
                    if (Directory.Exists(downloadDirectory))
                    {
                        httpRequest.download(url, downloadDirectory + "\\" + System.IO.Path.GetFileName(url), downloadUrl.Url);
                        Console.Write("\npicture url:" + url);
                        Console.Write("\nname:" + System.IO.Path.GetFileName(url));    
                    }

                }
                catch (System.Net.WebException)
                {
                    break;
                }
            }
            if (picPage < 1)
            {
                return false;
            }
            return true;

        }
    }
}
