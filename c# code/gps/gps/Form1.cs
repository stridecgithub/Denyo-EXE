using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Device.Location;
using System.Net;
using System.IO;
namespace gps
{
    public partial class Form1 : Form
    {
        private String gid = "DCA60E515356";
        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - this.Width,
                                   Screen.PrimaryScreen.WorkingArea.Height - this.Height);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            getDID();
            Geo();
        }

        private void Geo()
        {
            GeoCoordinateWatcher watcher;
            watcher = new GeoCoordinateWatcher();
            label1.Text = "Loading..";
            watcher.PositionChanged += (sender, e) =>
            {
                var coordinate = e.Position.Location;
                var prm = "lat=" + coordinate.Latitude.ToString() + "&long=" + coordinate.Longitude.ToString();
                label1.Text = coordinate.Latitude.ToString() + "--" + coordinate.Longitude.ToString();
                watcher.Stop();
                try
                {
                    MyWebRequest myRequest = new MyWebRequest("http://denyoapi.stridecdev.com/vij.php?gid=" + gid + "&" + prm, "GET");
                    var str = myRequest.GetResponse();
                }
                catch (WebException ex) { MessageBox.Show(ex.Message); }
            //    MessageBox.Show(str);
               
                //  browsor.Navigate("javascript:" + f + "('" + coordinate.Latitude.ToString() + "','" + coordinate.Longitude.ToString() + "')");

                // Uncomment to get only one event.

            };

            // Begin listening for location updates.
            watcher.Start();
        }
        private void getDID()
        {
            var app_dir = Path.GetDirectoryName(Application.ExecutablePath);
            app_dir = app_dir.Replace("bin\\Debug", "");// MessageBox.Show(gid + "===" + app_dir);
            string id = readf(app_dir + "\\id.txt"); gid = id;
            //  browsor.Navigate("http://denyoapi.stridecdev.com/system.php?gid=" + id);
        }
        public string readf(string f)
        {
            // MessageBox.Show(f + ".qa");
            String s = File.ReadAllText(f);
            return s;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Geo();
        }

        public class MyWebRequest
        {
            private WebRequest request;
            private Stream dataStream;

            private string status;

            public String Status
            {
                get
                {
                    return status;
                }
                set
                {
                    status = value;
                }
            }

            public MyWebRequest(string url)
            {
                // Create a request using a URL that can receive a post.

                request = WebRequest.Create(url);
            }

            public MyWebRequest(string url, string method)
                : this(url)
            {

                if (method.Equals("GET") || method.Equals("POST"))
                {
                    // Set the Method property of the request to POST.
                    request.Method = method;
                    if (!WebHeaderCollection.IsRestricted("user-agent")) request.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    

                }
                else
                {
                    throw new Exception("Invalid Method Type");
                }
            }

            public string GetResponse()
            {
                // Get the original response.
                WebResponse response = request.GetResponse();

                this.Status = ((HttpWebResponse)response).StatusDescription;

                // Get the stream containing all content returned by the requested server.
                dataStream = response.GetResponseStream();

                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);

                // Read the content fully up to the end.
                string responseFromServer = reader.ReadToEnd();

                // Clean up the streams.
                reader.Close();
                dataStream.Close();
                response.Close();

                return responseFromServer;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Geo();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
