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
		private int listenport = 5555;             //端口号
		private TcpListener listener;              //TCP网络客户端监听
		private System.Windows.Forms.ListBox lbClients;
		private ArrayList clients;                 //客户端动态数组
		private Thread processor;                  //线程processor
		private Socket clientsocket;
		private System.Windows.Forms.Button button1;  //套接字clientsocket
		private Thread clientservice;              //线程clientservice

		public ChatServer()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			clients = new ArrayList();             //建立客户端动态数组
			processor = new Thread(new ThreadStart(StartListening));  //建立新线程，执行StartListening
			processor.Start();                     //启动新线程，开始监听网络
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
			this.button1.Text = "关闭服务器";
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
			this.Text = "聊天服务器";
			this.ResumeLayout(false);

		}
		#endregion
		protected override void OnClosed(EventArgs e)
		{
			try
			{
				for(int n=0; n<clients.Count; n++)    //终止全部客户端
				{
					Client cl = (Client)clients[n];   //客户端用户
					SendToClient(cl, "QUIT#");        //通知该用户服务器退出
					cl.Sock.Close();                  //断开该客户端的Sock连接
					cl.CLThread.Abort();              //终止该客户端的进程
				}
				listener.Stop();                      //停止监听
				if(processor != null)
					processor.Abort();                //终止监听进程
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString() );
			}
			base.OnClosed(e);                        //执行基类关闭方法关闭窗体
		}
		private void StartListening()                //监听网络
		{
			listener = new TcpListener(listenport); //建立监听连接  
			listener.Start();                        //开始监听
			while (true) {
				try
				{
					Socket s = listener.AcceptSocket();   //接受连接请求，并返回一个用于数据传输（接收和发送）的Socket
					clientsocket = s;
					clientservice = new Thread(new ThreadStart(ServiceClient));  //为该 Socket 产生一个新的线程，执行 ServiceClient服务
					clientservice.Start();                //启动线程
				}
				catch(Exception e)
				{
					Console.WriteLine(e.ToString() );
				}
			}
		}
		private void ServiceClient()             //聊天服务
		{
			Socket client = clientsocket;
			bool keepalive = true;               //保持活动状态

			while (keepalive)
			{
				Byte[] buffer = new Byte[2048];
				client.Receive(buffer);          //从当前客户端client接收数据，存于buffer中 （二进制字节流）
				string clientcommand = System.Text.Encoding.Unicode.GetString(buffer);  //将buffer中的数据转换为Unicode码，存于clientcommand中

				string[] tokens = clientcommand.Split(new Char[]{'#'});   //用'#'分解clientcommand中的命令和信息
				//Console.WriteLine(clientcommand);
				Client cl;

				switch(tokens[0])
				{
					case   "CONN":                                 //如果是 连接请求 则产生新客户端用户
						for(int n=0; n<clients.Count; n++) 
						{
							cl = (Client)clients[n];
							SendToClient(cl, "JOIN#" + tokens[1]);           //通知现有每一个用户，有新用户加入
						}
						EndPoint ep = client.RemoteEndPoint;                 //获取客户端client的远程端点（IP和Point）
						cl = new Client(tokens[1], ep, clientservice, client);   //用Client类，正式建立新的客户端用户（用户名、端口号、线程、套接字）
						clients.Add(cl);                                      //在用户表中增加一个用户
						string message = "LIST#" + GetChatterList();
						SendToClient(cl, message);                            //通知该新客户端网络内现有的聊天用户列表

						lbClients.Items.Add(cl);                              //在列表中显示新的客户端信息
						break;
					case   "CHAT":                                 //如果是 聊天请求 则将接收到的信息发送给全部用户
						for(int n=0; n<clients.Count; n++)
						{
							cl = (Client)clients[n];
							SendToClient(cl, clientcommand);
						}
						break;
					case    "PRIV":                               //如果是 私聊请求 则将接收到的信息发送给指定用户（在tokens[3]中）
						for(int n=0; n<clients.Count; n++) 
						{
							cl = (Client)clients[n];
							if(cl.Name.CompareTo(tokens[3]) == 0)
								SendToClient(cl, clientcommand);           //给接收方发送信息
							if(cl.Name.CompareTo(tokens[1]) == 0)
								SendToClient(cl, clientcommand);           //给发送方发送信息
						}
						break;
					case    "GONE":                              //如果是 离线请求 则从用户表中删除该用户
						int remove = 0;
						bool found = false;
						for(int n=0; n<clients.Count; n++)
						{
							cl = (Client)clients[n];
							SendToClient(cl, clientcommand);
							if(cl.Name.CompareTo(tokens[1]) == 0)     //如果找到该用户则在服务器列表中删除
							{
								remove = n;
								found = true;
								lbClients.Items.Remove(cl);
								cl.Sock.Close();                       //关闭套接字
								//cl.CLThread.Abort();
							}
						}
						if(found)
						{
							clients.RemoveAt(remove);                  //如果找到该用户则在用户列表中删除
						}
						keepalive = false;                             //不再保持活动
						break;
				}
			} 
		}
		private void SendToClient(Client cl, string message)     //向客户端 cl 发送信息 message
		{
			try
			{
				byte[] buffer = System.Text.Encoding.Unicode.GetBytes(message.ToCharArray());    //ASCII转换为字节流
				cl.Sock.Send(buffer,buffer.Length,0);                 //发送数据
			}
			catch                         //发送失败处理
			{
				cl.Sock.Close();          //关闭与cl的Sock
				cl.CLThread.Abort();      //终止处理 cl 的线程
				clients.Remove(cl);       //删除用户 cl
				lbClients.Items.Remove(cl);      //从服务器的用户列表中删除 cl
				MessageBox.Show("没有找到 " + cl.Name + " - 离线","错误",
				      MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		private string GetChatterList()             //获取用户列表
		{
			string chatters = "";
			for(int n=0; n<clients.Count; n++)
			{
				Client cl = (Client)clients[n];     //取得第n个用户
				chatters +=cl.Name;
				chatters += "#"	;					//用户名之间用'#'隔开
 			}
			//chatters.Trim('#');                   //去掉尾部'#'
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
