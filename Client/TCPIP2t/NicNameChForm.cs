using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TCPIP2t
{
    public partial class NicNameChForm : Form
    {

        static public String nicname;
        public NicNameChForm()
        {
            InitializeComponent();
            textBox1.KeyDown += sendKeyDown;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.Text = nicname;
            textBox1.MaxLength = 8;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            EnterServer();
        }
        private void sendKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                EnterServer();
            }
        }

        private void EnterServer()
        {
            if (!textBox1.Text.Equals(""))
            {
                this.DialogResult = DialogResult.OK;
                MainForm.nicname = textBox1.Text;
                if (MainForm.ServerLabel != null) MainForm.ServerLabel = comboBox1.SelectedItem.ToString();
                else MainForm.ServerLabel = comboBox1.Items[0].ToString();
                this.Close(); 
            }
            else
            {
                MessageBox.Show("닉네임을 입력해주세요.", "에러!");
                textBox1.Focus();
            }
        }
    }
}
