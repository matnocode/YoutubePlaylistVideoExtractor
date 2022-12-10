using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TrafficCapture
{
    class Capture
    {
        //file format= {datetime-ip-port}
        public int Port;
        public IPAddress Ip;
        StreamWriter file;
        Thread CaptureThread;
        string Query;
        string Web;
        public string[]? Videos;
        public Capture(int port, IPAddress ip, string web,string query)
        {
            Port = port;
            Web = web;
            Query = query;
            Ip = ip;
            file = new StreamWriter(System.IO.File.Create($"data/{DateTime.Now.Millisecond.ToString()}-{ip.ToString()}-{port}.txt"));          
            CaptureThread = new Thread(new ThreadStart(Start));
            CaptureThread.Start();

        }
        void Start()
        {

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var req = HttpWebRequest.Create(Web+Query);
            
            var response = req.GetResponse();
            
            StreamReader stream = new StreamReader(response.GetResponseStream());
            var html = stream.ReadToEnd();
            List<string> hrefs = new List<string>();
            //get href's in id=video-title
            while (html.Contains("/watch?v=")) 
            {
                //find /watch?v=
                html = html.Substring(html.IndexOf("/watch?v=")+ "/watch?v=".Length);
                var href = html.Substring(0, "/watch?v=".Length + html.IndexOf("\","));
                hrefs.Add("/watch?v="+href);
            }

            List<string> videos = new List<string>();
            foreach (string href in hrefs)
            {
                string str = (href.Substring(0, href.Length - 9));
                str = str.Substring(0, href.IndexOf("list")-6);
                str = "https://youtube.com" + str;
                videos.Add(str);
                file.WriteLine(str);
            }
            file.Flush();
            Videos = videos.ToArray();
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter Youtube Playlist URL. Playlist be public");
            string url = Console.ReadLine();
            Uri uri = new Uri(url);
            Capture httpCapture = new Capture(443, Dns.GetHostAddresses("youtube.com")[0], "https://www.youtube.com/playlist", uri.Query);

        }
    }
}