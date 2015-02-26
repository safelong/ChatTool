using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatServer
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class ChatServer : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private int listenport = 5555;             //�˿ں�
		private TcpListener listener;              //TCP����ͻ��˼���
		private System.Windows.Forms.ListBox lbClients;
		private ArrayList clients;                 //�ͻ��˶�̬����
		private Thread processor;                  //�߳�processor
		private Socket clientsocket;
		private System.Windows.Forms.Button button1;  //�׽���clientsocket
		private Thread clientservice;              //�߳�clientservice

		public ChatServer()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			clients = new ArrayList();             //�����ͻ��˶�̬����
			processor = new Thread(new ThreadStart(StartListening));  //�������̣߳�ִ��StartListening
			processor.Start();                     //�������̣߳���ʼ��������
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
			this.lbClients = new System.Windows.Forms.ListBox();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lbClients
			// 
			this.lbClients.BackColor = System.Drawing.SystemColors.WindowFrame;
			this.lbClients.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.lbClients.Font = new System.Drawing.Font("Tahoma", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lbClients.ForeColor = System.Drawing.Color.Yellow;
			this.lbClients.ItemHeight = 17;
			this.lbClients.Location = new System.Drawing.Point(8, 8);
			this.lbClients.Name = "lbClients";
			this.lbClients.Size = new System.Drawing.Size(352, 255);
			this.lbClients.TabIndex = 0;
			// 
			// button1
			// 
			this.button1.ForeColor = System.Drawing.SystemColors.Window;
			this.button1.Location = new System.Drawing.Point(120, 272);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(112, 24);
			this.button1.TabIndex = 1;
			this.button1.Text = "�رշ�����";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// ChatServer
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.BackColor = System.Drawing.SystemColors.Desktop;
			this.ClientSize = new System.Drawing.Size(368, 302);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.lbClients);
			this.Name = "ChatServer";
			this.Text = "���������";
			this.ResumeLayout(false);

		}
		#endregion
		protected override void OnClosed(EventArgs e)
		{
			try
			{
				for(int n=0; n<clients.Count; n++)    //��ֹȫ���ͻ���
				{
					Client cl = (Client)clients[n];   //�ͻ����û�
					SendToClient(cl, "QUIT#");        //֪ͨ���û��������˳�
					cl.Sock.Close();                  //�Ͽ��ÿͻ��˵�Sock����
					cl.CLThread.Abort();              //��ֹ�ÿͻ��˵Ľ���
				}
				listener.Stop();                      //ֹͣ����
				if(processor != null)
					processor.Abort();                //��ֹ��������
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString() );
			}
			base.OnClosed(e);                        //ִ�л���رշ����رմ���
		}
		private void StartListening()                //��������
		{
			listener = new TcpListener(listenport); //������������  
			listener.Start();                        //��ʼ����
			while (true) {
				try
				{
					Socket s = listener.AcceptSocket();   //�����������󣬲�����һ���������ݴ��䣨���պͷ��ͣ���Socket
					clientsocket = s;
					clientservice = new Thread(new ThreadStart(ServiceClient));  //Ϊ�� Socket ����һ���µ��̣߳�ִ�� ServiceClient����
					clientservice.Start();                //�����߳�
				}
				catch(Exception e)
				{
					Console.WriteLine(e.ToString() );
				}
			}
		}
		private void ServiceClient()             //�������
		{
			Socket client = clientsocket;
			bool keepalive = true;               //���ֻ״̬

			while (keepalive)
			{
				Byte[] buffer = new Byte[2048];
				client.Receive(buffer);          //�ӵ�ǰ�ͻ���client�������ݣ�����buffer�� ���������ֽ�����
				string clientcommand = System.Text.Encoding.Unicode.GetString(buffer);  //��buffer�е�����ת��ΪUnicode�룬����clientcommand��

				string[] tokens = clientcommand.Split(new Char[]{'#'});   //��'#'�ֽ�clientcommand�е��������Ϣ
				//Console.WriteLine(clientcommand);
				Client cl;

				switch(tokens[0])
				{
					case   "CONN":                                 //����� �������� ������¿ͻ����û�
						for(int n=0; n<clients.Count; n++) 
						{
							cl = (Client)clients[n];
							SendToClient(cl, "JOIN#" + tokens[1]);           //֪ͨ����ÿһ���û��������û�����
						}
						EndPoint ep = client.RemoteEndPoint;                 //��ȡ�ͻ���client��Զ�̶˵㣨IP��Point��
						cl = new Client(tokens[1], ep, clientservice, client);   //��Client�࣬��ʽ�����µĿͻ����û����û������˿ںš��̡߳��׽��֣�
						clients.Add(cl);                                      //���û���������һ���û�
						string message = "LIST#" + GetChatterList();
						SendToClient(cl, message);                            //֪ͨ���¿ͻ������������е������û��б�

						lbClients.Items.Add(cl);                              //���б�����ʾ�µĿͻ�����Ϣ
						break;
					case   "CHAT":                                 //����� �������� �򽫽��յ�����Ϣ���͸�ȫ���û�
						for(int n=0; n<clients.Count; n++)
						{
							cl = (Client)clients[n];
							SendToClient(cl, clientcommand);
						}
						break;
					case    "PRIV":                               //����� ˽������ �򽫽��յ�����Ϣ���͸�ָ���û�����tokens[3]�У�
						for(int n=0; n<clients.Count; n++) 
						{
							cl = (Client)clients[n];
							if(cl.Name.CompareTo(tokens[3]) == 0)
								SendToClient(cl, clientcommand);           //�����շ�������Ϣ
							if(cl.Name.CompareTo(tokens[1]) == 0)
								SendToClient(cl, clientcommand);           //�����ͷ�������Ϣ
						}
						break;
					case    "GONE":                              //����� �������� ����û�����ɾ�����û�
						int remove = 0;
						bool found = false;
						for(int n=0; n<clients.Count; n++)
						{
							cl = (Client)clients[n];
							SendToClient(cl, clientcommand);
							if(cl.Name.CompareTo(tokens[1]) == 0)     //����ҵ����û����ڷ������б���ɾ��
							{
								remove = n;
								found = true;
								lbClients.Items.Remove(cl);
								cl.Sock.Close();                       //�ر��׽���
								//cl.CLThread.Abort();
							}
						}
						if(found)
						{
							clients.RemoveAt(remove);                  //����ҵ����û������û��б���ɾ��
						}
						keepalive = false;                             //���ٱ��ֻ
						break;
				}
			} 
		}
		private void SendToClient(Client cl, string message)     //��ͻ��� cl ������Ϣ message
		{
			try
			{
				byte[] buffer = System.Text.Encoding.Unicode.GetBytes(message.ToCharArray());    //ASCIIת��Ϊ�ֽ���
				cl.Sock.Send(buffer,buffer.Length,0);                 //��������
			}
			catch                         //����ʧ�ܴ���
			{
				cl.Sock.Close();          //�ر���cl��Sock
				cl.CLThread.Abort();      //��ֹ���� cl ���߳�
				clients.Remove(cl);       //ɾ���û� cl
				lbClients.Items.Remove(cl);      //�ӷ��������û��б���ɾ�� cl
				MessageBox.Show("û���ҵ� " + cl.Name + " - ����","����",
				      MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		private string GetChatterList()             //��ȡ�û��б�
		{
			string chatters = "";
			for(int n=0; n<clients.Count; n++)
			{
				Client cl = (Client)clients[n];     //ȡ�õ�n���û�
				chatters +=cl.Name;
				chatters += "#"	;					//�û���֮����'#'����
 			}
			//chatters.Trim('#');                   //ȥ��β��'#'
			return chatters;
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new ChatServer());
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
