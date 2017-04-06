using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PCComm;
using System.IO.Ports;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Net;
using System.IO;
using System.Security.Permissions;
using System.Threading;
using denyo;

namespace PCComm
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
    public partial class Genarator : Form
    {
        CommunicationManager comm = new CommunicationManager();
        Microsoft.Win32.RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        string transType = string.Empty;
        public System.Threading.Thread newThread = null;
        public enum TransmissionType { Text, Hex }
        public enum MessageType { Incoming, Outgoing, Normal, Warning, Error };
        private string _baudRate = string.Empty;
        private string dvname = "device";
        private int hexcodenumt=0;
        private string _parity = string.Empty;
        private string _stopBits = string.Empty;
        private string _dataBits = string.Empty;
        private string _portName = string.Empty;
        private string rtstr = string.Empty;
        private string deviceID = string.Empty;
        private string[] _HCV = { };
        private TransmissionType _transType;
        private RichTextBox _displayWindow;
        private String gid="DCA60E515356";
        private int noresponse = 0;
        private int hexcodenum=0;
        private int hexcodenumr = 0;
        private int hexrotate = 0;
        private int hexrotatelen = 0;
       // private int retdelay = 0;
        private int hexcodesent = 0;
        private bool waitrply = false;
        private int enginestate = 0;

        private string lasthxname = string.Empty;
        private string wriddata1 = string.Empty;
        private string wriddata2 = string.Empty;
        private string alarmlists = string.Empty;
        private int serverrequest = 0;
       
        private string[] hexcodesreturn =  new string[50];
        //global manager variables
        private Color[] MessageColor = { Color.Blue, Color.Green, Color.Black, Color.Orange, Color.Red };
        private SerialPort comPort = new SerialPort();
       
        public Genarator()
        {
            InitializeComponent();
           // browsor.ObjectForScripting = new ScriptManager(this);
            browsor.ObjectForScripting = this;
           // webBrowser1.ObjectForScripting = new ScriptManager(this);
            if (rkApp.GetValue("minipcportstartup") == null)
            {
                rkApp.SetValue("minipcportstartup", System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }

        public void setDvName(String tx)
        {
            dvname = tx;
          //  Geoposition geoPosition = await new Geolocator().GetGeopositionAsync();
        }
        public void CallMe(String tx)
        {
          //  string str = tx;
          //  MessageBox.Show(tx + "  = data sending..");
            //   byte[] bytes = str.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
            //   serialport.Write(bytes, 0, bytes.Length);
            //  comm.CurrentTransmissionType = PCComm.CommunicationManager.TransmissionType.Hex;
            try
            {
                if (tx == "Port is Listioning") { }
                else if (tx == "restart-exe") { Application.Restart(); }
                else if (tx == "restart-websocket") { browsor.Refresh(); }
                else { MessageBox.Show("server"); serverrequest = 1; if (wriddata1 == string.Empty)wriddata1 = tx; else wriddata2 = tx; }
            }
            catch (Exception ex) {  }

            //.... this method can be called in javascript via window.external.CallMe()
        }
        private void checkStatus()
        {
            WebRequest request = WebRequest.Create("http://www.contoso.com/default.html");
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Display the status.
            Console.WriteLine(response.StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            Console.WriteLine(responseFromServer);
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
        }
        private void getURL(String url)
        {
            WebRequest request = WebRequest.Create(url);
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Display the status.
            Console.WriteLine(response.StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            Console.WriteLine(responseFromServer);
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
        }

        /// <summary>
        /// property to hold our TransmissionType
        /// of our manager class
        /// </summary>
        public TransmissionType CurrentTransmissionType
        {
            get { return _transType; }
            set { _transType = value; }
        }

        /// <summary>
        /// property to hold our display window
        /// value
        /// </summary>
        public RichTextBox DisplayWindow
        {
            get { return _displayWindow; }
            set { _displayWindow = value; }
        }
        
        private void frmMain_Load(object sender, EventArgs e)
        {
            denyo.Class1 denyo = new denyo.Class1();
            _HCV = denyo.Get();
            LoadValues();
            SetDefaults();
            SetControlState();
            startport();
            getDID();
          // browsor.Navigate("http://raja.stridecdev.com/system.php");
          // startport();
        }
       
        private void getDID()
        {
            var app_dir = Path.GetDirectoryName(Application.ExecutablePath);
            app_dir = app_dir.Replace("bin\\Debug", "");// MessageBox.Show(gid + "===" + app_dir);
            string id = readf(app_dir + "\\id.txt"); gid = id;
            browsor.Navigate("http://denyoapi.stridecdev.com/system.php?gid=" + id);
        }
        public string readf(string f)
        {
            // MessageBox.Show(f + ".qa");
            String s = File.ReadAllText(f);
            return s;
        }
        private void cmdOpen_Click(object sender, EventArgs e)
        {
            if (comPort.IsOpen == true) comPort.Close();

            comPort.PortName = Convert.ToString(cboPort.Text);
            comPort.Parity = (Parity)Enum.Parse(typeof(Parity), cboParity.Text);
            comPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cboStop.Text); ;
            comPort.DataBits = int.Parse(cboData.Text);
            comPort.BaudRate = int.Parse(cboBaud.Text);
            comPort.ReadTimeout = 500;
            comPort.WriteTimeout = 500;
           // comPortDisplayWindow = rtbDisplay;
            
                //  comm.RtsEnable = true;
           // comm.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            comPort.Open();
            DisplayData(MessageType.Normal, "Port opened at " + DateTime.Now + "\n");
            
            cmdOpen.Enabled = false;
            cmdClose.Enabled = true;
            cmdSend.Enabled = true;
        }
        private void startport()
        {
            try
            {
            if (comPort.IsOpen == true) comPort.Close();             
            comPort.PortName = Convert.ToString(cboPort.Text);
            comPort.Parity = (Parity)Enum.Parse(typeof(Parity), cboParity.Text);
            comPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cboStop.Text); ;
            comPort.DataBits = int.Parse(cboData.Text);
            comPort.BaudRate = int.Parse(cboBaud.Text);
            comPort.DtrEnable = true;
            comPort.RtsEnable = true; 
           // comPortDisplayWindow = rtbDisplay;
            comPort.Open();
                //  comm.RtsEnable = true;
           // comm.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            comPort.ErrorReceived += new System.IO.Ports.SerialErrorReceivedEventHandler(port_ErrorReceived);
            
            DisplayData(MessageType.Normal, "Port opened at " + DateTime.Now + "\n");            
            cmdOpen.Enabled = false;
            cmdClose.Enabled = true;
            cmdSend.Enabled = true;
            timer1.Enabled = true;
            }
            catch (IOException en) { MessageBox.Show("Comap is offline"); }
        }

        void port_ErrorReceived(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            //spinfo.Text += "" + e.ToString(); //MessageBox.Show("error port:" + e.ToString());
        }
        void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            hexcodesent = 0; serverrequest = 0;
            hexcodenumr += 1; //determine the mode the user selected (binary/string)
           // label6.Text = "Data Recived...";
           try
           {
               int bytes = comPort.BytesToRead;
               byte[] comBuffer = new byte[bytes];
               //read the data and store it
               comPort.Read(comBuffer, 0, bytes);
              // rtstr += comBuffer;
               String st = ByteToHex(comBuffer); rtstr += st;
             //  DisplayData(MessageType.Incoming, ByteToHex(comBuffer) + "\n");
                }
           catch (TimeoutException)
           {
               
           }
           
            /*try
            {
                //retrieve number of bytes in the buffer
                int bytes = comPort.BytesToRead;
               // MessageBox.Show(comPort.ReadExisting() + "===" + comPort.ReadBufferSize);
                //create a byte array to hold the awaiting data
                byte[] comBuffer = new byte[bytes];
                //read the data and store it
                comPort.Read(comBuffer, 0, bytes);*/
             //   comPort.
                //display the data to the user
             /*   String st = ByteToHex(comBuffer); //MessageBox.Show(st);
                if (serverrequest == 1)
                {
                    rec_DataHex(st);
                    serverrequest = 0;
                }
                else
                {
                     //waitrply = true; //button1.Text = "H" + hexcodenumr.ToString();
                  //  txtSend.Text = lasthxname;
                    //if (lasthxname == "alarm") { alarmlists = st; } else { rtstr += st; }
                    rtstr += st;
                    waitrply = false;  hexcodesent = 0; serverrequest = 0;
                    hexcodenumr += 1; 
                 
                }*/
           //   DisplayData(MessageType.Incoming, ByteToHex(comBuffer) + "\n");
          //  }
          //  catch (IOException ex) { MessageBox.Show("error recive:" + ex.ToString()); }
           
        }
        void rec_DataHex(String st)
        {

            try { browsor.Navigate("javascript:recive('" + st + "','" + dvname + "')"); }
            catch (Exception ex) { /*MessageBox.Show("error callme:" + ex.ToString());*/ }
        }
        /// <summary>
        /// Method to initialize serial port
        /// values to standard defaults
        /// </summary>
        private void SetDefaults()
        {
            try { cboPort.SelectedIndex = 0; }
            catch (Exception e) { }
            cboBaud.SelectedText = "19200";
            cboParity.SelectedIndex = 0;
            cboStop.SelectedIndex = 1;
            cboData.SelectedIndex = 1;
        }

        public void WriteData(string msg)
        {
           // label6.Text = "Data Send...";
                    try
                    {
                        
                        //convert the message to byte array
                        byte[] newMsg = HexToByte(msg.Trim());
                        //send the message to the port
                        comPort.Write(newMsg, 0, newMsg.Length);
                        //convert back to hex and display
                        DisplayData(MessageType.Outgoing, ByteToHex(newMsg) + "\n");
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("error write:" + ex.ToString());
                        //display error message
                        DisplayData(MessageType.Error, ex.Message);
                    }
                    
            
        }
        private void DisplayData(MessageType type, string msg)
        {
            try
            {
                rtbDisplay.SelectedText = string.Empty;
                rtbDisplay.SelectionFont = new Font(rtbDisplay.SelectionFont, FontStyle.Bold);
                rtbDisplay.SelectionColor = MessageColor[(int)type];
                rtbDisplay.AppendText(msg);
                rtbDisplay.ScrollToCaret();

            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// methos to load our serial
        /// port option values
        /// </summary>
        private void LoadValues()
        {
            comm.SetPortNameValues(cboPort);
            comm.SetParityValues(cboParity);
            comm.SetStopBitValues(cboStop);
        }

        /// <summary>
        /// method to set the state of controls
        /// when the form first loads
        /// </summary>
        private void SetControlState()
        {
            rdoText.Checked = true;
            cmdSend.Enabled = false;
            cmdClose.Enabled = false;
        }

        private void cmdSend_Click(object sender, EventArgs e)
        {
            string str = txtSend.Text;
         //   byte[] bytes = str.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
         //   serialport.Write(bytes, 0, bytes.Length);
          //  comm.CurrentTransmissionType = PCComm.CommunicationManager.TransmissionType.Hex;
            WriteData(txtSend.Text);
          //  MessageBox.Show(txtSend.Text+"  = data sending..");
            
        }

        private void rdoHex_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoHex.Checked == true)
            {
                comm.CurrentTransmissionType = PCComm.CommunicationManager.TransmissionType.Hex;
            }
            else
            {
                comm.CurrentTransmissionType = PCComm.CommunicationManager.TransmissionType.Text;
            }
        }

        private void cboPort_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        
        private string ByteToHex(byte[] comByte)
        {
            try
            { 
            //create a new StringBuilder object
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            
            //loop through each byte in the array
            foreach (byte data in comByte)
                //convert the byte to a string and add to the stringbuilder
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
        
            //return the converted value
            return builder.ToString().ToUpper();
            }
            catch (Exception ex) { return ""; }
        }
        private byte[] HexToByte(string msg)
        {
            
                //remove any spaces from the string
                msg = msg.Replace(" ", "");
                //create a byte array the length of the
                //string divided by 2
                byte[] comBuffer = new byte[msg.Length / 2];
                //loop through the length of the provided string
                try
                {
                    for (int i = 0; i < msg.Length; i += 2)
                        //convert each set of 2 characters to a byte
                        //and add to the array
                        comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
                }
                catch (Exception ex) { }
                //return the array
                return comBuffer;
            
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            if (comPort.IsOpen == true) comPort.Close();
            cmdOpen.Enabled = true;
            cmdClose.Enabled = false;
            cmdSend.Enabled = false;
            timer1.Enabled = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (hexcodesent == 1 && noresponse >= 5) { hexcodesent = 0; noresponse = 0; }
            else if (hexcodesent == 1) { noresponse += 1; }
            if (hexrotate==0) { hexrotatelen = _HCV.Length; }
            else if (hexrotate >= 1 && hexrotate <= 9) { hexrotatelen = _HCV.Length; }
            else if (hexrotate >= 10) { hexrotatelen = _HCV.Length; hexrotate = 0; }
            //MessageBox.Show(waitrply+"=="+hexcodesent + "==" + serverrequest + "==" + hexcodenum + "==" + hexcodes.Length);
            //labelinfo.Text = hexrotatelen.ToString() + "===" + hexrotate.ToString(); hexcodenumt += 1;
            if (wriddata1 != "")
            {
                try {  serverrequest = 1; WriteData(wriddata1.Trim()); wriddata1 = ""; }catch(Exception ex){}
 }
            else if (wriddata2 != "") { serverrequest = 1; WriteData(wriddata2.Trim()); wriddata2 = ""; }
            else if(waitrply)
            {
                try
                {
                    //labelinfo.Text = retdelay.ToString();
                    // retdelay += 1; if (retdelay >= 5) { waitrply = false; retdelay = 1; }
                  //  if (tm2 >= 0)
                //    {
                        waitrply = false;  hexcodesent = 0; serverrequest = 0;
                        hexcodenumr += 1;
                    // hexcodesreturn[hexcodenumr] = rtstr + "," + lasthxname;
                      //  textBox1.Text = rtstr + "," + lasthxname; 
                       
                         //tm2 = 0;
                 //   }
                  //  tm2 += 1;
                }catch(Exception ex){}
            }
            else if (hexcodesent == 0 && serverrequest == 0 && hexcodenum < hexrotatelen)
            {
                try
                {
                    if (rtstr.Length>=10) funcall(rtstr + "," + lasthxname);
                    textBox1.Text = rtstr;
                    string hx = hexrotate == 0 || hexrotate == 10 ? _HCV[hexcodenum] : _HCV[hexcodenum]; //MessageBox.Show(hexcodenum +"=="+hexcodes.Length);
                    string[] wrds = hx.Split(new[] { "," }, StringSplitOptions.None);
                    serverrequest = 0; rtstr = ""; hexcodesent = 1; lasthxname = wrds[1].Trim();
                    WriteData(wrds[0].Trim());
                    hexcodenum += 1;
                }
                catch (Exception ex) { }

            }
            else if (hexcodenum >= (hexrotatelen))
            {
                hexcodenum = 0; hexcodenumr = 0; rtbDisplay.Text = ""; hexrotate += 1;
            }
           /* else if (hexcodenum >= (hexcodes.Length-1))
            {
                try { 
                hexcodenum = 0; hexcodenumr = 0;
                String ustrings = "";
                for (int j = 0; j < hexcodes.Length; j++)
                {
                    string hx = hexcodes[j];
                    string[] wrds = hx.Split(new[] { "," }, StringSplitOptions.None);
                    ustrings += hexcodesreturn[j] +","+ wrds[1] + "," + wrds[2] + "," + wrds[3] + "," + wrds[4]  + "|";
                }
               // MessageBox.Show(ustrings);

                textBox1.Text = ustrings;
                }
                catch (Exception ex) { }
                try
                {
                  //  MyWebRequest myRequest = new MyWebRequest("http://denyoapp.stridecdev.com/currentstatus1.php?gid=" + gid, "POST", "data=" + textBox1.Text);
                   // textBox1.Text = myRequest.GetResponse();
                }
                catch (WebException webex) { }
                rtbDisplay.Text = "";
            }*/
        }
        private void funcall(string tx)
        {
            try
            {
                // MyWebRequest myRequest = new MyWebRequest("http://denyoapp.stridecdev.com/currentstatus1.php?gid=" + gid, "POST", "data=" + tx);
               //  textBox1.Text = myRequest.GetResponse();
               // MessageBox.Show(tx);
                try { object y = browsor.Document.InvokeScript("posdata", new string[] { tx }); }
                catch (Exception ex) {  }
            }
            catch (WebException webex) { }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            
            if (timer1.Enabled) { timer1.Enabled = false; button1.Text = "ON"; }
            else { timer1.Enabled = true; button1.Text = "Off"; }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            
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
                }
                else
                {
                    throw new Exception("Invalid Method Type");
                }
            }

            public MyWebRequest(string url, string method, string data)
                : this(url, method)
            {
                try
                {
                    // Create POST data and convert it to a byte array.
                    string postData = data;
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                    // Set the ContentType property of the WebRequest.
                    request.ContentType = "application/x-www-form-urlencoded";

                    // Set the ContentLength property of the WebRequest.
                    request.ContentLength = byteArray.Length;

                    // Get the request stream.
                    dataStream = request.GetRequestStream();

                    // Write the data to the request stream.
                    dataStream.Write(byteArray, 0, byteArray.Length);

                    // Close the Stream object.
                    dataStream.Close();
                }catch(WebException e){}
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

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (timer1.Enabled) { timer1.Enabled = false; button1.Text = "ON"; }
            else { timer1.Enabled = true; button1.Text = "Off"; }
        }

        
        
    }
}