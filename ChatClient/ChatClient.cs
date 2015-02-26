using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text;

namespace ChatClient
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class ChatClientForm : System.Windows.Forms.Form
	{
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.TextBox ChatOut;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.ListBox lbChatters;
		private System.Windows.Forms.RichTextBox rtbChatIn;
		private System.Windows.Forms.Button btnDisconnect;
		private System.Windows.Forms.Button btnLog;
		private System.Windows.Forms.CheckBox cbPrivate;
		private System.Windows.Forms.TextBox User;
		private System.Windows.Forms.TextBox IP;

		private string clientname;        //�û���
		private NetworkStream ns;         //������
		private string serveraddress;     //��������ַ
		private int serverport;           //�˿ں�
		private TcpClient clientsocket;   //TCP����
		private IPEndPoint ipendpoint;    //�������˵�
		private Thread receive = null;    //�߳�

		private bool connected = false;   //�Ƿ������ӵ�������
		private bool privatemode = false; //�Ƿ����˽��
		private bool logging = false;     //�Ƿ������־��д

		//private StreamReader sr;
		private StreamWriter logwriter;   //��־��д
		string serverresponse;            // ������Ӧ����Ϣ
		Byte[] buffer = new Byte[2048];   // �������ݻ�����
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label statusBar1;
		private System.Windows.Forms.Label label3;

		public ChatClientForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			serverport = 5555;
			btnDisconnect.Enabled = false;     //�Ͽ���ť������
			btnSend.Enabled = false;           //���Ͱ�ť������

		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.cbPrivate = new System.Windows.Forms.CheckBox();
			this.btnLog = new System.Windows.Forms.Button();
			this.btnSend = new System.Windows.Forms.Button();
			this.lbChatters = new System.Windows.Forms.ListBox();
			this.ChatOut = new System.Windows.Forms.TextBox();
			this.btnConnect = new System.Windows.Forms.Button();
			this.rtbChatIn = new System.Windows.Forms.RichTextBox();
			this.btnDisconnect = new System.Windows.Forms.Button();
			this.IP = new System.Windows.Forms.TextBox();
			this.User = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.statusBar1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// cbPrivate
			// 
			this.cbPrivate.BackColor = System.Drawing.Color.Transparent;
			this.cbPrivate.Font = new System.Drawing.Font("����", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.cbPrivate.ForeColor = System.Drawing.SystemColors.Window;
			this.cbPrivate.Location = new System.Drawing.Point(488, 248);
			this.cbPrivate.Name = "cbPrivate";
			this.cbPrivate.Size = new System.Drawing.Size(56, 25);
			this.cbPrivate.TabIndex = 10;
			this.cbPrivate.Text = "˽��";
			this.cbPrivate.CheckStateChanged += new System.EventHandler(this.cbPrivate_CheckedChanged);
			// 
			// btnLog
			// 
			this.btnLog.Font = new System.Drawing.Font("����", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.btnLog.ForeColor = System.Drawing.SystemColors.Window;
			this.btnLog.Location = new System.Drawing.Point(456, 208);
			this.btnLog.Name = "btnLog";
			this.btnLog.Size = new System.Drawing.Size(104, 26);
			this.btnLog.TabIndex = 9;
			this.btnLog.Text = "��־��¼";
			this.btnLog.Click += new System.EventHandler(this.btnLog_Click);
			// 
			// btnSend
			// 
			this.btnSend.Font = new System.Drawing.Font("����", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.btnSend.ForeColor = System.Drawing.SystemColors.Window;
			this.btnSend.Location = new System.Drawing.Point(336, 250);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(96, 26);
			this.btnSend.TabIndex = 5;
			this.btnSend.Text = "����";
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// lbChatters
			// 
			this.lbChatters.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.lbChatters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lbChatters.Font = new System.Drawing.Font("����", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.lbChatters.ForeColor = System.Drawing.SystemColors.Window;
			this.lbChatters.ItemHeight = 14;
			this.lbChatters.Location = new System.Drawing.Point(326, 32);
			this.lbChatters.Name = "lbChatters";
			this.lbChatters.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.lbChatters.Size = new System.Drawing.Size(114, 198);
			this.lbChatters.TabIndex = 7;
			// 
			// ChatOut
			// 
			this.ChatOut.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.ChatOut.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.ChatOut.Location = new System.Drawing.Point(10, 250);
			this.ChatOut.Name = "ChatOut";
			this.ChatOut.Size = new System.Drawing.Size(302, 23);
			this.ChatOut.TabIndex = 2;
			this.ChatOut.Text = "";
			this.ChatOut.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ChatOut_KeyPress);
			// 
			// btnConnect
			// 
			this.btnConnect.Font = new System.Drawing.Font("����", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.btnConnect.ForeColor = System.Drawing.SystemColors.Window;
			this.btnConnect.Location = new System.Drawing.Point(456, 120);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(104, 26);
			this.btnConnect.TabIndex = 4;
			this.btnConnect.Text = "����";
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// rtbChatIn
			// 
			this.rtbChatIn.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.rtbChatIn.Font = new System.Drawing.Font("����", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.rtbChatIn.Location = new System.Drawing.Point(10, 32);
			this.rtbChatIn.Name = "rtbChatIn";
			this.rtbChatIn.ReadOnly = true;
			this.rtbChatIn.Size = new System.Drawing.Size(302, 208);
			this.rtbChatIn.TabIndex = 6;
			this.rtbChatIn.Text = "";
			// 
			// btnDisconnect
			// 
			this.btnDisconnect.Font = new System.Drawing.Font("����", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.btnDisconnect.ForeColor = System.Drawing.SystemColors.Window;
			this.btnDisconnect.Location = new System.Drawing.Point(456, 152);
			this.btnDisconnect.Name = "btnDisconnect";
			this.btnDisconnect.Size = new System.Drawing.Size(104, 26);
			this.btnDisconnect.TabIndex = 8;
			this.btnDisconnect.Text = "�Ͽ�";
			this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
			// 
			// IP
			// 
			this.IP.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.IP.Location = new System.Drawing.Point(456, 32);
			this.IP.Name = "IP";
			this.IP.Size = new System.Drawing.Size(104, 21);
			this.IP.TabIndex = 12;
			this.IP.Text = "localhost";
			this.IP.TextChanged += new System.EventHandler(this.IP_TextChanged);
			// 
			// User
			// 
			this.User.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.User.Location = new System.Drawing.Point(456, 80);
			this.User.Name = "User";
			this.User.Size = new System.Drawing.Size(104, 21);
			this.User.TabIndex = 14;
			this.User.Text = "";
			// 
			// label4
			// 
			this.label4.BackColor = System.Drawing.Color.Transparent;
			this.label4.Font = new System.Drawing.Font("����", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.label4.ForeColor = System.Drawing.SystemColors.Window;
			this.label4.Location = new System.Drawing.Point(464, 64);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(80, 16);
			this.label4.TabIndex = 13;
			this.label4.Text = "�ǳ�";
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Font = new System.Drawing.Font("����", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.label1.ForeColor = System.Drawing.SystemColors.Window;
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 16);
			this.label1.TabIndex = 15;
			this.label1.Text = "������Ϣ";
			// 
			// label2
			// 
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Font = new System.Drawing.Font("����", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.label2.ForeColor = System.Drawing.SystemColors.Window;
			this.label2.Location = new System.Drawing.Point(328, 16);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(104, 16);
			this.label2.TabIndex = 16;
			this.label2.Text = "�����û�";
			// 
			// label3
			// 
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.Font = new System.Drawing.Font("����", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.label3.ForeColor = System.Drawing.SystemColors.Window;
			this.label3.Location = new System.Drawing.Point(464, 16);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 16);
			this.label3.TabIndex = 17;
			this.label3.Text = "������";
			// 
			// statusBar1
			// 
			this.statusBar1.Font = new System.Drawing.Font("����", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.statusBar1.ForeColor = System.Drawing.SystemColors.Window;
			this.statusBar1.Location = new System.Drawing.Point(8, 296);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(560, 16);
			this.statusBar1.TabIndex = 18;
			this.statusBar1.Text = "�Ͽ�";
			// 
			// ChatClientForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.BackColor = System.Drawing.SystemColors.Desktop;
			this.ClientSize = new System.Drawing.Size(578, 312);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.User);
			this.Controls.Add(this.IP);
			this.Controls.Add(this.ChatOut);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.cbPrivate);
			this.Controls.Add(this.btnLog);
			this.Controls.Add(this.btnDisconnect);
			this.Controls.Add(this.lbChatters);
			this.Controls.Add(this.rtbChatIn);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.btnConnect);
			this.Font = new System.Drawing.Font("����", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "ChatClientForm";
			this.Text = "����ͻ���";
			this.Load += new System.EventHandler(this.ChatClientForm_Load);
			this.ResumeLayout(false);

		}
		#endregion
		protected override void OnClosed(EventArgs e)    //��д�رշ���
		{
			QuitChat();                                  //�˳�����
			if(receive != null && receive.IsAlive)       //����л�߳��򲻹ر��߳�
				receive.Abort();
			
			base.OnClosed(e);                            //ִ�л���Ĳ��رշ���
		}

		private void ServerConnection()                  //���ӵ�����������
		{
			statusBar1.Text = "���ӵ�������";
			try
			{
				//��IP.Text�����IP��ַ�ַ���ת��ΪIPAddress��ʽ;
				IPAddress myIP=IPAddress.Parse (IP.Text); 
				serveraddress=IP.Text;
				ipendpoint=new IPEndPoint(myIP,serverport);   //�������˵�
				clientsocket = new TcpClient(ipendpoint);     //����TCP����
			}
			catch
			{
				clientsocket = new TcpClient();
				clientsocket.Connect(serveraddress,serverport);     //����TCP����
//				clientsocket = new TcpClient(serveraddress,serverport);
//				MessageBox.Show ("�������IP��ַ��ʽ����ȷ!");
//				return;
			}

			try 
			{
				ns = clientsocket.GetStream();          // ͨ��clientsocket(TcpClient)�������������Ϣ
				connected = true;                           // ��������
			}
			catch
			{
				MessageBox.Show("�������ӵ�������","����",
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				statusBar1.Text = "�Ͽ�";
			}
		}
		private void RegisterWithServer()                    //�û�ע�ᵽ������
		{
			try 
			{
				string command = "CONN#" + clientname+"#";         // ��������  
				Byte[] outbytes = System.Text.Encoding.Unicode.GetBytes(command.ToCharArray());
				ns.Write(outbytes,0,outbytes.Length);             // ͨ��ns��socket���������������Ϣ���ֽ�����

				ns.Read(buffer,0,buffer.Length);                     //  ͨ��ns��socket���ӷ�������ȡ��Ϣ��buffer��
				serverresponse = System.Text.Encoding.Unicode.GetString(buffer);  // ת��ΪUnicode��ʽ

				string[] tokens = serverresponse.Split(new Char[]{'#'});         // ��serverresponse��'#'�ֽ�ɶ���ַ���
				if(tokens[0] == "LIST")                                  // �����ȡ���û��б����ʾ��½�ɹ�����ʾ�û���
				{
					statusBar1.Text = "�ѵ�½";
					btnDisconnect.Enabled = true;
					for(int n=1; n<tokens.Length-1; n++)         // ��ʾ�û���
						lbChatters.Items.Add(tokens[n]);
				}
				this.Text = clientname + ": ��½��������";         //���ô������
			}
			catch
			{
				MessageBox.Show("��½����","����",
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		
		private void ReceiveChat()                        //����������Ϣ
		{
			bool keepalive = true;                          // ���ֻ״̬
			while (keepalive) 
			{
				try
				{
					ns.Read(buffer,0,buffer.Length);       // ͨ��ns�ӷ�������ȡ��Ϣ��buffer��
					serverresponse = System.Text.Encoding.Unicode.GetString(buffer);   //ת��ΪUnicode��ʽ
					string[] tokens = serverresponse.Split(new Char[]{'#'});                         // �� serverresponse ��'#' �ֽ�ɶ���ַ���
					if(logging)
					{  string time=System.DateTime.Now.ToString();               // ȡ��ϵͳʱ��   
						logwriter.WriteLine(time);                            // ��¼ϵͳʱ�䵽�ļ���
					}
					switch(tokens[0])                    // �ж���Ϣ�е�״̬������
					{   case "CHAT":                        //����
							rtbChatIn.SelectionColor=Color.Blue;    //���ò����Ϊ��ɫ
							rtbChatIn.AppendText(tokens[1]);     // ��������Ϣ������ӷ������û���
							rtbChatIn.AppendText("��");
							rtbChatIn.SelectionColor=Color.Black;   //��ʼ��ʾ��ɫ�ַ�
							rtbChatIn.AppendText(tokens[2]);     // ��������Ϣ��������������Ϣ
							rtbChatIn.AppendText("\r\n");           // ����
							if(logging)
								logwriter.WriteLine(tokens[1]+"��"+tokens[2]);   // �����Ҫ��¼��־�򱣴浽�ļ�
							break;
						case  "PRIV":                       //˽��
							rtbChatIn.SelectionColor=Color.DarkRed;   //���ò����Ϊ����ɫ
							rtbChatIn.AppendText(tokens[1]);
							rtbChatIn.AppendText(" ˽�ģ�");
							rtbChatIn.SelectionColor=Color.Black;
							rtbChatIn.AppendText(tokens[2]);
							rtbChatIn.AppendText("\r\n");
							if(logging)
								logwriter.WriteLine(tokens[1]+" �� "+tokens[3]+" ˽�ģ�"+tokens[2]);
							break;
						case   "JOIN":                      //����
							rtbChatIn.SelectionColor=Color.Red;
							rtbChatIn.AppendText(tokens[1]);
							rtbChatIn.AppendText(" ����������\r\n");
							string newguy = tokens[1];
							lbChatters.Items.Add(newguy);                    // ���û��б�������һ�����û�
							if(logging)
								logwriter.WriteLine(tokens[1]+" ����������");
							break;
						case    "GONE":                     //����
							rtbChatIn.SelectionColor=Color.Red;
							rtbChatIn.AppendText(tokens[1]);
							rtbChatIn.AppendText(" �뿪������\r\n");
							if(logging)
								logwriter.WriteLine(tokens[1]+" �뿪������");
							lbChatters.Items.Remove(tokens[1]);                // ���û��б���ɾ�����û�
							break;
						case    "QUIT":                    // �������ر�
							ns.Close();                    // �ر� ns��socket��
							clientsocket.Close();          // �ر� clientsocket(TcpClient)
							keepalive = false;             // ���ٻ
							statusBar1.Text = "�������ر�";
							connected= false;              // ������
							btnSend.Enabled = false;       // �����͡���ť��Ч
							btnDisconnect.Enabled = false;  // �����ߡ���ť��Ч
							lbChatters.Items.Clear();      // ����û��б�
							if(logging)
								logwriter.WriteLine("�������ر�");
							break;
					}
				}
				catch(Exception ex){
//					MessageBox.Show(ex.ToString(),"����",
//						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
		private void QuitChat() 
		{
			if(connected) {
				try{
					string command = "GONE#" + clientname+"#";
					Byte[] outbytes = System.Text.Encoding.Unicode.GetBytes(command.ToCharArray());
					ns.Write(outbytes,0,outbytes.Length);            //֪ͨ���������û�Ҫ����
				}
				catch(Exception ex){
					MessageBox.Show(ex.ToString(),"����",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			if(logging)
			{
				StartStopLogging();                    // ֹͣ��־��¼
				logwriter.Close();                     // �ر� logwriter
			}
			this.Text = "������";                     // ���ô������
		}
		private void StartStopLogging() 
		{
			if(!logging){
				if(!Directory.Exists("logs"))              // ��������� logs Ŀ¼������Ŀ¼
					Directory.CreateDirectory("logs");
				string fname = "logs\\" + DateTime.Now.ToString("ddMMyyHHmm") + ".txt";   // ��ʱ��������־�ļ���
				logwriter = new StreamWriter(new FileStream(fname, FileMode.OpenOrCreate,
					FileAccess.Write));                     // ������־�ļ�����ʼ���ļ��� logwriter
				logging = true;                             // ��־��¼��־
				btnLog.Text = "��ֹ��־����";               // ������־��ť����
				statusBar1.Text = "���� - ��־����";        // ����״̬��Ϣ
			}
			else{
				logwriter.Close();                          // �ر��ļ��� logwriter
				logging = false;
				btnLog.Text = "��־����";
				statusBar1.Text = "���� - ��־�ر�";
			}

		}
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(String[] args)                    // ����������ʱ������������ args ��
		{
			ChatClientForm cf = new ChatClientForm();      // �����µĴ���
			if(args.Length == 0)                           // �������ʱû��������Ĭ��Ϊ����
				cf.serveraddress = "localhost";                      
			else
				cf.serveraddress = args[0];                // �������ʱ���в������һ���������Ƿ�����DNS
			cf.IP.Text=cf.serveraddress;
			Application.Run(cf);                           // �������̣����г���
		}

		private void btnConnect_Click(object sender, System.EventArgs e)
		{
			if(User.Text.Trim() == "")                           // ����û�������Ϊ��
			{
				MessageBox.Show("����������ǳ�","����",
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			clientname = User.Text.Trim();
			clientname = clientname.Trim(new char[]{'\r','\n'});
			
			ServerConnection();                // ���ӵ������� ��TcpClient��
			
			if(connected)                      // ������ӳɹ�
			{
				RegisterWithServer();          // �û���½
				receive = new Thread(new ThreadStart(ReceiveChat));    // �������߳� receive
				receive.Start();               // �������߳� receive
				btnSend.Enabled = true;        // ������
				btnConnect.Enabled = false;    // �������������ӡ���ť
				btnDisconnect.Enabled=true;    // �����������ߡ���ť
				ChatOut.Text = "";             // ��� ChatOut
				ChatOut.Focus();               // ���� ChatOut Ϊ��ǰ���뽹��
			}
		}

		private void btnSend_Click(object sender, System.EventArgs e)      // ��Ϣ����
		{
			if (ChatOut.Text.Trim()=="")
				return;
			try{
				string command;
				if(!privatemode){                      // �������˽��ģʽ����������Ϣ�ַ���
					command = "CHAT#" + clientname +"#"+ChatOut.Text + "#";
				}
				else{
					if(lbChatters.SelectedIndex == -1){      // ���û��ѡ���û�
						MessageBox.Show("��ѡ��һ�������û�","����",MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return;
					}
					string destclient = lbChatters.SelectedItem.ToString();
					command = "PRIV#" + clientname + "#" + ChatOut.Text + "#" + destclient + "#";   //����˽����Ϣ�ַ���
				}
					Byte[] outbytes = System.Text.Encoding.Unicode.GetBytes(command.ToCharArray());  // ת��Ϊ�ֽ���
					ns.Write(outbytes,0,outbytes.Length);                      // ���͸�������
					ChatOut.Text = "";
					ChatOut.Focus();
			}
			catch(Exception ex)                    // ��Ϣ����ʧ��������
			{
				MessageBox.Show(ex.ToString(),"����",	MessageBoxButtons.OK, MessageBoxIcon.Error);
				ns.Close();                        // �ر�ns
				clientsocket.Close();
				if(receive != null && receive.IsAlive)     // ��ֹ���߳�
					receive.Abort();
				connected = false;
				statusBar1.Text = "����";
			}
		}

		private void btnDisconnect_Click(object sender, System.EventArgs e)   // ����
		{
			QuitChat();
			ns.Close();
			clientsocket.Close();
			if(receive != null && receive.IsAlive)    // ����л������ر���
				receive.Abort();
			btnDisconnect.Enabled = false;
			btnConnect.Enabled = true;
			btnSend.Enabled = false;
			connected = false;
			lbChatters.Items.Clear();
			statusBar1.Text = "����";
		}

		private void ChatOut_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)   // �����ȼ�
			{
			if(e.KeyChar == '\r')     // ����ǻس������������Ѿ��������Զ���� btnSend ��ť
				if(connected)
					btnSend_Click(sender, e);
		}

		private void btnLog_Click(object sender, System.EventArgs e)     // ��־��¼��ť
		{
			StartStopLogging();        
		}

		private void cbPrivate_CheckedChanged(object sender, System.EventArgs e)    // ˽��ѡ��
		{
			if(cbPrivate.Checked)                  
			{	privatemode = true;                   // ����Ϊ˽��ģʽ
				lbChatters.SelectionMode = System.Windows.Forms.SelectionMode.One;   // �û��б��п���ѡ��һ��
			}
			else{
				privatemode = false;                  // ȡ��˽��ģʽ
				lbChatters.SelectionMode = System.Windows.Forms.SelectionMode.None;  // ����ѡȡ�û�
			}
		}

		private void ChatClientForm_Load(object sender, System.EventArgs e)
		{
		
		}

		private void IP_TextChanged(object sender, System.EventArgs e)
		{
		
		}

	}
}
