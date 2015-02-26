
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
	public class ChatServer : System.Windows.Forms.Form
	{
		private System.ComponentModel.Container components = null;
		private int listenport = 5555;             //�˿ں�
		private TcpListener listener;              //TCP����ͻ��˼���
		private System.Windows.Forms.ListBox lbClients;
		private ArrayList clients;                 //�ͻ��˶�̬����
		private Thread processor;                  //�߳�processor
		private Socket clientsocket;
		private System.Windows.Forms.Button button1; //�׽���clientsocket
		private Thread clientservice;              //�߳�clientservice

		public ChatServer()
		{
			InitializeComponent();
			clients = new ArrayList();             //�����ͻ��˶�̬����
			//�������̣߳�ִ��StartListening
			processor = new Thread(new ThreadStart(StartListening));  
			processor.Start();                     //�������̣߳���ʼ��������
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
					if (components != null) components.Dispose();
			}
			base.Dispose( disposing );
		}

		protected override void OnClosed(EventArgs e)
		{
			try	
			{
				for(int n=0; n<clients.Count; n++)    //��ֹȫ���ͻ���
				{
						Client cl = (Client)clients[n];   //�ͻ����û�
					SendToClient(cl, "QUIT#");        //֪ͨ���û��������˳�
					cl.Sock.Close();                  //�Ͽ��ÿͻ��˵�Sock����
					cl.CLThread.Abort();              //��ֹ�ÿͻ��˵��߳�
				}
				listener.Stop();                      //ֹͣ����
				if(processor != null)processor.Abort();  //��ֹ�����߳�
			}
			catch(Exception ex)
			{
					Console.WriteLine(ex.ToString() );
			}
			base.OnClosed(e);                        //ִ�л���رշ����رմ���
		}
		private void StartListening()                   //��������
		{
			listener =  new TcpListener(listenport); //������������  
			listener.Start();                        //��ʼ����
			while (true) 
			{
				try
				{	//�����������󣬲�����һ���������ݴ��䣨���պͷ��ͣ���Socket
					Socket s = listener.AcceptSocket();   
					clientsocket = s;
					//����һ���µ��̣߳�ִ��ServiceClient
					clientservice = new Thread(new ThreadStart(ServiceClient)); 
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
				client.Receive(buffer);      //�ӵ�ǰ�ͻ���client�������ݣ�����buffer��
				//��buffer�е�����ת��ΪUnicode��
				string clientcommand = System.Text.Encoding.Unicode.GetString(buffer); 
				//��'#'�ֽ�clientcommand�е��������Ϣ
				string[] tokens = clientcommand.Split(new Char[]{'#'});  
				Client cl;
				switch(tokens[0])
				{
					case   "CONN":                          //����� �������� ������¿ͻ����û�
						for(int n=0; n<clients.Count; n++) 
						{
								cl = (Client)clients[n];
							SendToClient(cl, "JOIN#" + tokens[1]); //֪ͨ����ÿһ���û��������û�����
						}
						EndPoint ep = client.RemoteEndPoint;  //��ȡ�ͻ���client��Զ�̶˵㣨IP��Point��
						//��Client�࣬��ʽ�����µĿͻ����û�
						cl = new Client(tokens[1], ep, clientservice, client); 
						clients.Add(cl);                //���û���������һ���û�
						string message = "LIST#" + GetChatterList();
						SendToClient(cl, message);      //֪ͨ���¿ͻ������������е������û��б�
						lbClients.Items.Add(cl);        //���б�����ʾ�µĿͻ�����Ϣ
						break;
					case   "CHAT":                      //����� �������� �򽫽��յ�����Ϣ���͸�ȫ���û�
						for(int n=0; n<clients.Count; n++)
						{
								cl = (Client)clients[n];
							SendToClient(cl, clientcommand);
						}
						break;
					case    "PRIV":         //����� ˽������ �򽫽��յ�����Ϣ���͸�ָ���û���tokens[3]�У�
						for(int n=0; n<clients.Count; n++) 
						{
								cl = (Client)clients[n];
							if(cl.Name.CompareTo(tokens[3]) == 0)
								SendToClient(cl, clientcommand);     //�����շ�������Ϣ
							if(cl.Name.CompareTo(tokens[1]) == 0)
								SendToClient(cl, clientcommand);     //�����ͷ�������Ϣ
						}
						break;
					case    "GONE":                        //����� �������� ����û�����ɾ�����û�
						int remove = 0;
						bool found = false;
						for(int n=0; n<clients.Count; n++)
						{
								cl = (Client)clients[n];
							SendToClient(cl, clientcommand);
							if(cl.Name.CompareTo(tokens[1]) == 0) //����ҵ����û����ڷ������б���ɾ��
							{
									remove = n;
								found = true;
								lbClients.Items.Remove(cl);
								cl.Sock.Close(); 		//�ر��׽���
							}
						}
						if(found)	clients.RemoveAt(remove);     //����ҵ����û������û��б���ɾ��
						keepalive = false; 		 //���ٱ��ֻ
						break;
				}
			} 
		}
		private void SendToClient(Client cl, string message)		//��ͻ��� cl ������Ϣ message
		{
			try	
			{
				byte[] buffer = System.Text.Encoding.Unicode.GetBytes(message.ToCharArray());
				cl.Sock.Send(buffer,buffer.Length,0); 		//��������
			}
			catch 
			{
				cl.Sock.Close(); 		//�ر���cl��Sock
				cl.CLThread.Abort(); 		//��ֹ���� cl ���߳�
				clients.Remove(cl); 		//ɾ���û� cl
				lbClients.Items.Remove(cl);	//�ӷ��������û��б���ɾ�� cl
				MessageBox.Show("û���ҵ� " + cl.Name ,"����",MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		private string GetChatterList() 			//��ȡ�û��б�
		{
			string chatters = "";
			for(int n=0; n<clients.Count; n++)
			{
					Client cl = (Client)clients[n];		//ȡ�õ�n���û�
				chatters +=cl.Name;
				chatters += "#"	;			//�û���֮����'#'����
			}
			return chatters;
		}
		[STAThread]
		static void Main() 
		{
			Application.Run(new ChatServer());
		}
		private void button1_Click(object sender, System.EventArgs e)
		{
				this.Close();
		}

		private void InitializeComponent()
		{
			// 
			// ChatServer
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Name = "ChatServer";
			this.Load += new System.EventHandler(this.ChatServer_Load);

		}

		private void ChatServer_Load(object sender, System.EventArgs e)
		{
		
		}
	}
}