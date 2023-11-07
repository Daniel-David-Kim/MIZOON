using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

// 클라이언트 연결 로직 통합 성공. : 파일 선택 다운 성공. + 텍스트 파일도
namespace TCPIP2t
{
    delegate void SetTextDelegate(string s);
    public partial class MainForm : Form
    {
        private const string BASEPATH = "E:\\Mark12\\testArchive\\sockclient2\\";
        //private const string BASEPATH = "D:\\WS\\testArchive\\sockclient\\";
        private const string IPADDR = "127.0.0.1";
        private const int PORT = 9000;
        private const int STRING = 1001;
        private const int TXT = 1002;
        private const int IMG = 1003;
        private const int MP = 1004;
        private const int FILEPATH = 1005;
        private const int TXT_REQ = 1006;
        private const int IMG_REQ = 1007;
        private const int NUM = 1008;
        private string readpath = "";
        private Socket client;

        private Thread seeker;
        static public string nicname;
        static public string ServerLabel;
        public MainForm()
        {
            InitializeComponent();
            this.chattingBox1.KeyDown += sendKeyDown;
        }

        //초기서버 선택 시 포트연결
        public int setServer(string ServerLabel)
        {
            switch (ServerLabel)
            {
                case "서버1":
                    {
                        return 9000;
                    }
                case "서버2":
                    {
                        return 9000;
                    }
                default:
                    {
                        return -1;
                    }

            }
        }
        
        //서버와의 채팅을 실행
        //ChatHandler chatHandler = new ChatHandler();

        //해당 서버로 접속
        private void Server_Join(string ServerLabel)
        {
            try
            {   //소켓 connect
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //ip설정
                IPAddress ip = IPAddress.Parse("127.0.0.1");

                //port설정
                int port = setServer(ServerLabel);

                client.Connect(ip, port);

                //Thread chatThread = new Thread(new ThreadStart(chatHandler.ChatProcess));
                //chatThread.Start();

                Console.WriteLine("클라이언트 연결 성공!");
                seeker = new Thread(WaitBroadcastEcho);
                seeker.Start();

                string firstMsg = "\r\n[안내 메시지] " + nicname + " 님께서 입장 하셨습니다.";
                byte[] dataArr = Encoding.UTF8.GetBytes(firstMsg);
                SendHeader(client, STRING, dataArr.Length);
                //
                SendString(client, firstMsg);


            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Server 오류 또는 Start 되지 않았거나\n\n" + ex.Message, "Client");
                Application.Exit();
            }
        }












        private void MainForm_Load(object sender, EventArgs e)                                      ////////////////////////////////////////////////////////////////////////////////////////
        {
            NicNameChForm frm2 = new NicNameChForm();
            DialogResult dialogResult = frm2.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                nickNameBox.Text = nicname;
                memberCount.Text = "접속 인원 수 : " + 0;
                servername.Text = ServerLabel;
                Server_Join(ServerLabel);
            }
            else
            {
                this.Close();
            }
        }

        //채팅 엔터키 작동
        private void sendKeyDown(object sender, KeyEventArgs e)           
        {
            if(e.KeyCode == Keys.Enter)
            {
                sendKey();
                //이벤트 처리 중지, KeyUp or Click등
                e.Handled = true;
            }
        }



        private void sendKey()
        {
            if (!chattingBox1.Text.Equals(""))
            {
                string msg = "\r\n" + "[ " + ServerLabel + ", " + nicname + " ] : " + chattingBox1.Text;

                //서버로 문자열 전송
                byte[] dataArr = Encoding.UTF8.GetBytes(msg);

                // 시도 : 성공! 
                //SendHeader(client, STRING, message.Length + 1);
                SendHeader(client, STRING, dataArr.Length);
                //
                SendString(client, msg);
                //
                //RecvEcho(client);
                //

                chattingBox1.Text = null;   
            }
            else MessageBox.Show("텍스트를 입력해주세요.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 접속 종료 로직
            SendHeader(client, -1, -1);
            seeker.Interrupt();
            client.Close();
            Console.WriteLine("클라이언트 연결 해제!");
            Application.Exit();
        }

        //
        void SendHeader(Socket client, int type, int length, string path = null)
        {
            //Console.WriteLine("Header send ready.....");
            Header header = new Header();
            header.type = type;
            header.length = length;
            if (path != null) header.path = path;
            byte[] buf = new byte[Marshal.SizeOf(header)];
            unsafe
            {
                fixed (byte* p = buf)
                {
                    Marshal.StructureToPtr(header, (IntPtr)p, false);
                }
            }
            int retval = client.Send(buf, Marshal.SizeOf(header), SocketFlags.None);
            //Console.WriteLine("{0} 헤더 전송!", header.length);
        }
        void SendString(Socket client, string msg)
        {
            byte[] dataArr = Encoding.UTF8.GetBytes(msg);
            //
            int retval = client.Send(dataArr, SocketFlags.None);
            Console.WriteLine("메시지 : {0}바이트를 전송했습니다.\n=============================", retval);
        }
        void SendTxt(Socket client, FileStream fs, int type)
        {
            StreamReader sr = new StreamReader(fs);
            string read;
            int temp;
            int sum = 0;
            while ((read = sr.ReadLine()) != null)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(read + "\n");
                temp = buffer.Length;
                SendHeader(client, type, temp);
                int bytes = client.Send(buffer);
                sum += temp;
            }
            sr.Close();
            // 추가! : 텍스트파일 내용 다 전송하면 헤더에 길이 -1을 넣어서 종료 신호로 삼음.
            SendHeader(client, type, -1);
            Console.WriteLine("TXT : {0}바이트를 전송했습니다!\n=============================", sum);
        }
        void SendFile(Socket client, FileStream fs, int type)
        {
            int sum = 0;
            StreamReader sr = new StreamReader(fs);
            byte[] buf = new byte[512];
            int temp;
            do
            {
                temp = fs.Read(buf, 0, buf.Length);
                //Console.WriteLine("리드 : {0}바이트 읽음.", temp);
                if (temp == 0) break;
                SendHeader(client, type, temp);
                int bytes = client.Send(buf);
                //Console.WriteLine(Encoding.UTF8.GetString(buf));
                //Console.WriteLine("바이트 : {0}바이트를 전송했습니다.", bytes);
                sum += temp;
            } while (temp != 0);
            //fs.Flush();
            // 추가! : 텍스트파일 내용 다 전송하면 헤더에 길이 -1을 넣어서 종료 신호로 삼음.
            //Console.WriteLine("loop escape");
            fs.Close();
            SendHeader(client, type, -1);
            Console.WriteLine("파일 : {0}바이트를 전송했습니다!\n=====================", sum);
        }
        Header RecvHeader(Socket client)
        {
            /*Header header = new Header();
            byte[] buffer = new byte[Marshal.SizeOf(header)];
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            int retval = client.Receive(buffer, SocketFlags.None);
            Console.WriteLine("{0}바이트를 전송받았습니다.", retval);

            Marshal.Copy(buffer, 0, ptr, Marshal.SizeOf(header));
            header = (Header)Marshal.PtrToStructure(ptr, typeof(Header));*/

            Header header = new Header();
            try
            {
                header = new Header();
                byte[] buffer = new byte[Marshal.SizeOf(header)];
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
                int retval = client.Receive(buffer, SocketFlags.None);
                //Console.WriteLine("{0}바이트를 전송받았습니다.", retval);

                //
                Marshal.Copy(buffer, 0, ptr, Marshal.SizeOf(header));

                header = (Header)Marshal.PtrToStructure(ptr, typeof(Header));
                //      
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
                Header failHeader = new Header();
                failHeader.length = -1;
                return failHeader;
            }


            return header;
        }

        void RecvEcho(Socket client)
        {
            //////////////////////////// 클라이언트 수신 //////////////////////////////////////
            // 0. 헤더 수신 : 성공
            int retval = -1;
            int sum;
            Header hd = RecvHeader(client);
            Console.WriteLine("수신한 헤더 정보\ntype : {0}\nlength : {1}\npath : {2}", hd.type, hd.length, hd.path);

            switch (hd.type)
            {
                case STRING: // 문자열 전송 : 성공!
                    byte[] buffer = new byte[hd.length];
                    client.Receive(buffer, hd.length, SocketFlags.None);
                    string msgstr = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine("[서버] : {0}", msgstr);
                    // 받은 메시지를 채팅창에 추가
                    chattingBox2.Invoke( // 다른 컴퓨터에서 처리하려면 필수!
                        (MethodInvoker)delegate () {
                            chattingBox2.Text += msgstr;
                            //chattingBox2.Text += "\r\n";
                            chattingBox1.Clear();
                        }
                    );
                    // 이거 추가 안해주면 텍스트박스에서 개행 절대 안됨! msgstr + "\r\n"; 해도 안됨!

                    //chat.AppendText(msg + Environment.NewLine);
                    //AppendChat();
                    //
                    break;
                case FILEPATH:
                    // 파일 경로를 리스트에 추가
                    lb1.Invoke( // 다른 컴퓨터에서 처리하려면 필수!
                       (MethodInvoker)delegate () {
                           lb1.Items.Add(hd.path);
                       }
                   );
                    break;
                case NUM:
                    memberCount.Text = $"접속 인원 수 : {hd.length}";
                    break;
                default: // 텍스트 파일, 이미지 파일, 동영상 파일, 음악 파일 전송 : 성공!
                    string path = hd.path;
                    string cut = path.Substring(path.LastIndexOf("\\") + 1);
                    string merge = string.Concat(BASEPATH, cut);
                    Console.WriteLine("path : {0}\nmerge : {1}", path, merge);
                    FileStream fs = new FileStream(merge, FileMode.Create);
                    try
                    {
                        /*
                        byte[] buffer2 = new byte[hd.length];
                        retval = client.Receive(buffer2, buffer2.Length, SocketFlags.None);
                        fs.Write(buffer2, 0, hd.length);
                        fs.Flush();
                        */
                        // 시도 : 일부만 받았을 때 모두 받기
                        int summ = 0;
                        do
                        {
                            byte[] buffer2 = new byte[hd.length];
                            retval = client.Receive(buffer2, buffer2.Length, SocketFlags.None);
                            fs.Write(buffer2, 0, hd.length);
                            fs.Flush();
                            summ += retval;
                        } while(!((retval == 0)||(summ >= hd.length)));
                        //
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }
                    fs.Flush();
                    fs.Close();
                    Console.WriteLine("{0}바이트를 수신했습니다!\n", retval);
                    break;
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////
        }


        private void connect_Click(object sender, EventArgs e)
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(IPAddress.Parse(IPADDR), PORT);
            Console.WriteLine("클라이언트 연결 성공!");
            seeker = new Thread(WaitBroadcastEcho);
            seeker.Start();
        }

        private void WaitBroadcastEcho()
        {
            while (client.Connected)
            {
                Console.WriteLine("시커 : 브로드캐스트 메시지 대기 중.....");
                RecvEcho(client);
            }
            Console.WriteLine("스레드 종료");
        }

        private void fileUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "all files (*.*)|*.*|text files (*.txt)|*.txt";
            ofd.Multiselect = false;
            DialogResult res = ofd.ShowDialog();
            if (res == DialogResult.OK)
            {
                readpath = ofd.FileName;
                path.Text = readpath;
            }
        }

        private void filesend_Click(object sender, EventArgs e)
        {
            if (readpath.Equals("") || readpath == null)
            {
                MessageBox.Show("파일을 선택해주세요.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int id = readpath.LastIndexOf(".");
            string cut = readpath.Substring(id + 1);
            FileStream fs = new FileStream(readpath, FileMode.Open);
            FileInfo target = new FileInfo(readpath);
            long sz = target.Exists ? target.Length : -1;
            Console.WriteLine("파일 사이즈 : {0}", (int)sz);
            if (sz == -1) return;

            if (cut.Equals("txt"))
            {
                Console.WriteLine("format txt");
                SendHeader(client, TXT, (int)sz, readpath);
                SendTxt(client, fs, TXT);
            }
            else
            {
                SendHeader(client, IMG, (int)sz, readpath);
                SendFile(client, fs, IMG);
            }
            //
            //RecvEcho(client);
            //

            readpath = "";
            path.Text = "";
        }

        private void sendChat_Click(object sender, EventArgs e)
        {
            sendKey();
        }

        private void download_Click(object sender, EventArgs e)
        {
            if (lb1.SelectedItems.Count == 0) MessageBox.Show("다운로드할 파일을 선택해주세요.");
            else
            {
                string path = lb1.SelectedItem.ToString();
                int idx = path.LastIndexOf(".");
                string cut = path.Substring(idx);
                MessageBox.Show(cut);
                if(cut.Equals(".txt")) SendHeader(client, TXT_REQ, 0, path);
                else SendHeader(client, IMG_REQ, 0, path);
            }
        }
    }

    public struct Header
    {
        [MarshalAs(UnmanagedType.I4)]
        public int type;
        [MarshalAs(UnmanagedType.I4)]
        public int length;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string userName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string path;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256 - 128 - 4 - 4 - 9)]
        public string dummy;
    }

}