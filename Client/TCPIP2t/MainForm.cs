using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

// Ŭ���̾�Ʈ ���� ���� ���� ����. : ���� ���� �ٿ� ����. + �ؽ�Ʈ ���ϵ�
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

        //�ʱ⼭�� ���� �� ��Ʈ����
        public int setServer(string ServerLabel)
        {
            switch (ServerLabel)
            {
                case "����1":
                    {
                        return 9000;
                    }
                case "����2":
                    {
                        return 9000;
                    }
                default:
                    {
                        return -1;
                    }

            }
        }
        
        //�������� ä���� ����
        //ChatHandler chatHandler = new ChatHandler();

        //�ش� ������ ����
        private void Server_Join(string ServerLabel)
        {
            try
            {   //���� connect
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //ip����
                IPAddress ip = IPAddress.Parse("127.0.0.1");

                //port����
                int port = setServer(ServerLabel);

                client.Connect(ip, port);

                //Thread chatThread = new Thread(new ThreadStart(chatHandler.ChatProcess));
                //chatThread.Start();

                Console.WriteLine("Ŭ���̾�Ʈ ���� ����!");
                seeker = new Thread(WaitBroadcastEcho);
                seeker.Start();

                string firstMsg = "\r\n[�ȳ� �޽���] " + nicname + " �Բ��� ���� �ϼ̽��ϴ�.";
                byte[] dataArr = Encoding.UTF8.GetBytes(firstMsg);
                SendHeader(client, STRING, dataArr.Length);
                //
                SendString(client, firstMsg);


            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Server ���� �Ǵ� Start ���� �ʾҰų�\n\n" + ex.Message, "Client");
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
                memberCount.Text = "���� �ο� �� : " + 0;
                servername.Text = ServerLabel;
                Server_Join(ServerLabel);
            }
            else
            {
                this.Close();
            }
        }

        //ä�� ����Ű �۵�
        private void sendKeyDown(object sender, KeyEventArgs e)           
        {
            if(e.KeyCode == Keys.Enter)
            {
                sendKey();
                //�̺�Ʈ ó�� ����, KeyUp or Click��
                e.Handled = true;
            }
        }



        private void sendKey()
        {
            if (!chattingBox1.Text.Equals(""))
            {
                string msg = "\r\n" + "[ " + ServerLabel + ", " + nicname + " ] : " + chattingBox1.Text;

                //������ ���ڿ� ����
                byte[] dataArr = Encoding.UTF8.GetBytes(msg);

                // �õ� : ����! 
                //SendHeader(client, STRING, message.Length + 1);
                SendHeader(client, STRING, dataArr.Length);
                //
                SendString(client, msg);
                //
                //RecvEcho(client);
                //

                chattingBox1.Text = null;   
            }
            else MessageBox.Show("�ؽ�Ʈ�� �Է����ּ���.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // ���� ���� ����
            SendHeader(client, -1, -1);
            seeker.Interrupt();
            client.Close();
            Console.WriteLine("Ŭ���̾�Ʈ ���� ����!");
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
            //Console.WriteLine("{0} ��� ����!", header.length);
        }
        void SendString(Socket client, string msg)
        {
            byte[] dataArr = Encoding.UTF8.GetBytes(msg);
            //
            int retval = client.Send(dataArr, SocketFlags.None);
            Console.WriteLine("�޽��� : {0}����Ʈ�� �����߽��ϴ�.\n=============================", retval);
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
            // �߰�! : �ؽ�Ʈ���� ���� �� �����ϸ� ����� ���� -1�� �־ ���� ��ȣ�� ����.
            SendHeader(client, type, -1);
            Console.WriteLine("TXT : {0}����Ʈ�� �����߽��ϴ�!\n=============================", sum);
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
                //Console.WriteLine("���� : {0}����Ʈ ����.", temp);
                if (temp == 0) break;
                SendHeader(client, type, temp);
                int bytes = client.Send(buf);
                //Console.WriteLine(Encoding.UTF8.GetString(buf));
                //Console.WriteLine("����Ʈ : {0}����Ʈ�� �����߽��ϴ�.", bytes);
                sum += temp;
            } while (temp != 0);
            //fs.Flush();
            // �߰�! : �ؽ�Ʈ���� ���� �� �����ϸ� ����� ���� -1�� �־ ���� ��ȣ�� ����.
            //Console.WriteLine("loop escape");
            fs.Close();
            SendHeader(client, type, -1);
            Console.WriteLine("���� : {0}����Ʈ�� �����߽��ϴ�!\n=====================", sum);
        }
        Header RecvHeader(Socket client)
        {
            /*Header header = new Header();
            byte[] buffer = new byte[Marshal.SizeOf(header)];
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            int retval = client.Receive(buffer, SocketFlags.None);
            Console.WriteLine("{0}����Ʈ�� ���۹޾ҽ��ϴ�.", retval);

            Marshal.Copy(buffer, 0, ptr, Marshal.SizeOf(header));
            header = (Header)Marshal.PtrToStructure(ptr, typeof(Header));*/

            Header header = new Header();
            try
            {
                header = new Header();
                byte[] buffer = new byte[Marshal.SizeOf(header)];
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
                int retval = client.Receive(buffer, SocketFlags.None);
                //Console.WriteLine("{0}����Ʈ�� ���۹޾ҽ��ϴ�.", retval);

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
            //////////////////////////// Ŭ���̾�Ʈ ���� //////////////////////////////////////
            // 0. ��� ���� : ����
            int retval = -1;
            int sum;
            Header hd = RecvHeader(client);
            Console.WriteLine("������ ��� ����\ntype : {0}\nlength : {1}\npath : {2}", hd.type, hd.length, hd.path);

            switch (hd.type)
            {
                case STRING: // ���ڿ� ���� : ����!
                    byte[] buffer = new byte[hd.length];
                    client.Receive(buffer, hd.length, SocketFlags.None);
                    string msgstr = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine("[����] : {0}", msgstr);
                    // ���� �޽����� ä��â�� �߰�
                    chattingBox2.Invoke( // �ٸ� ��ǻ�Ϳ��� ó���Ϸ��� �ʼ�!
                        (MethodInvoker)delegate () {
                            chattingBox2.Text += msgstr;
                            //chattingBox2.Text += "\r\n";
                            chattingBox1.Clear();
                        }
                    );
                    // �̰� �߰� �����ָ� �ؽ�Ʈ�ڽ����� ���� ���� �ȵ�! msgstr + "\r\n"; �ص� �ȵ�!

                    //chat.AppendText(msg + Environment.NewLine);
                    //AppendChat();
                    //
                    break;
                case FILEPATH:
                    // ���� ��θ� ����Ʈ�� �߰�
                    lb1.Invoke( // �ٸ� ��ǻ�Ϳ��� ó���Ϸ��� �ʼ�!
                       (MethodInvoker)delegate () {
                           lb1.Items.Add(hd.path);
                       }
                   );
                    break;
                case NUM:
                    memberCount.Text = $"���� �ο� �� : {hd.length}";
                    break;
                default: // �ؽ�Ʈ ����, �̹��� ����, ������ ����, ���� ���� ���� : ����!
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
                        // �õ� : �Ϻθ� �޾��� �� ��� �ޱ�
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
                    Console.WriteLine("{0}����Ʈ�� �����߽��ϴ�!\n", retval);
                    break;
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////
        }


        private void connect_Click(object sender, EventArgs e)
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(IPAddress.Parse(IPADDR), PORT);
            Console.WriteLine("Ŭ���̾�Ʈ ���� ����!");
            seeker = new Thread(WaitBroadcastEcho);
            seeker.Start();
        }

        private void WaitBroadcastEcho()
        {
            while (client.Connected)
            {
                Console.WriteLine("��Ŀ : ��ε�ĳ��Ʈ �޽��� ��� ��.....");
                RecvEcho(client);
            }
            Console.WriteLine("������ ����");
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
                MessageBox.Show("������ �������ּ���.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int id = readpath.LastIndexOf(".");
            string cut = readpath.Substring(id + 1);
            FileStream fs = new FileStream(readpath, FileMode.Open);
            FileInfo target = new FileInfo(readpath);
            long sz = target.Exists ? target.Length : -1;
            Console.WriteLine("���� ������ : {0}", (int)sz);
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
            if (lb1.SelectedItems.Count == 0) MessageBox.Show("�ٿ�ε��� ������ �������ּ���.");
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