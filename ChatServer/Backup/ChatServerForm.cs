
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
		private int listenport = 5555;             //端口号
		private TcpListener listener;              //TCP网络客户端监听
		private System.Windows.Forms.ListBox lbClients;
		private ArrayList clients;                 //客户端动态数组
		private Thread processor;                  //线程processor
		private Socket clientsocket;
		private System.Windows.Forms.Button button1; //套接字clientsocket
		private Thread clientservice;              //线程clientservice

		public ChatServer()
		{
			InitializeComponent();
			clients = new ArrayList();             //建立客户端动态数组
			//建立新线程，执行StartListening
			processor = new Thread(new ThreadStart(StartListening));  
			processor.Start();                     //启动新线程，开始监听网络
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
				for(int n=0; n<clients.Count; n++)    //终止全部客户端
				{
						Client cl = (Client)clients[n];   //客户端用户
					SendToClient(cl, "QUIT#");        //通知该用户服务器退出
					cl.Sock.Close();                  //断开该客户端的Sock连接
					cl.CLThread.Abort();              //终止该客户端的线程
				}
				listener.Stop();                      //停止监听
				if(processor != null)processor.Abort();  //终止监听线程
			}
			catch(Exception ex)
			{
					Console.WriteLine(ex.ToString() );
			}
			base.OnClosed(e);                        //执行基类关闭方法关闭窗体
		}
		private void StartListening()                   //监听网络
		{
			listener =  new TcpListener(listenport); //建立监听连接  
			listener.Start();                        //开始监听
			while (true) 
			{
				try
				{	//接受连接请求，并返回一个用于数据传输（接收和发送）的Socket
					Socket s = listener.AcceptSocket();   
					clientsocket = s;
					//产生一个新的线程，执行ServiceClient
					clientservice = new Thread(new ThreadStart(ServiceClient)); 
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
				client.Receive(buffer);      //从当前客户端client接收数据，存于buffer中
				//将buffer中的数据转换为Unicode码
				string clientcommand = System.Text.Encoding.Unicode.GetString(buffer); 
				//用'#'分解clientcommand中的命令和信息
				string[] tokens = clientcommand.Split(new Char[]{'#'});  
				Client cl;
				switch(tokens[0])
				{
					case   "CONN":                          //如果是 连接请求 则产生新客户端用户
						for(int n=0; n<clients.Count; n++) 
						{
								cl = (Client)clients[n];
							SendToClient(cl, "JOIN#" + tokens[1]); //通知现有每一个用户，有新用户加入
						}
						EndPoint ep = client.RemoteEndPoint;  //获取客户端client的远程端点（IP和Point）
						//用Client类，正式建立新的客户端用户
						cl = new Client(tokens[1], ep, clientservice, client); 
						clients.Add(cl);                //在用户表中增加一个用户
						string message = "LIST#" + GetChatterList();
						SendToClient(cl, message);      //通知该新客户端网络内现有的聊天用户列表
						lbClients.Items.Add(cl);        //在列表中显示新的客户端信息
						break;
					case   "CHAT":                      //如果是 聊天请求 则将接收到的信息发送给全部用户
						for(int n=0; n<clients.Count; n++)
						{
								cl = (Client)clients[n];
							SendToClient(cl, clientcommand);
						}
						break;
					case    "PRIV":         //如果是 私聊请求 则将接收到的信息发送给指定用户（tokens[3]中）
						for(int n=0; n<clients.Count; n++) 
						{
								cl = (Client)clients[n];
							if(cl.Name.CompareTo(tokens[3]) == 0)
								SendToClient(cl, clientcommand);     //给接收方发送信息
							if(cl.Name.CompareTo(tokens[1]) == 0)
								SendToClient(cl, clientcommand);     //给发送方发送信息
						}
						break;
					case    "GONE":                        //如果是 离线请求 则从用户表中删除该用户
						int remove = 0;
						bool found = false;
						for(int n=0; n<clients.Count; n++)
						{
								cl = (Client)clients[n];
							SendToClient(cl, clientcommand);
							if(cl.Name.CompareTo(tokens[1]) == 0) //如果找到该用户则在服务器列表中删除
							{
									remove = n;
								found = true;
								lbClients.Items.Remove(cl);
								cl.Sock.Close(); 		//关闭套接字
							}
						}
						if(found)	clients.RemoveAt(remove);     //如果找到该用户则在用户列表中删除
						keepalive = false; 		 //不再保持活动
						break;
				}
			} 
		}
		private void SendToClient(Client cl, string message)		//向客户端 cl 发送信息 message
		{
			try	
			{
				byte[] buffer = System.Text.Encoding.Unicode.GetBytes(message.ToCharArray());
				cl.Sock.Send(buffer,buffer.Length,0); 		//发送数据
			}
			catch 
			{
				cl.Sock.Close(); 		//关闭与cl的Sock
				cl.CLThread.Abort(); 		//终止处理 cl 的线程
				clients.Remove(cl); 		//删除用户 cl
				lbClients.Items.Remove(cl);	//从服务器的用户列表中删除 cl
				MessageBox.Show("没有找到 " + cl.Name ,"错误",MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		private string GetChatterList() 			//获取用户列表
		{
			string chatters = "";
			for(int n=0; n<clients.Count; n++)
			{
					Client cl = (Client)clients[n];		//取得第n个用户
				chatters +=cl.Name;
				chatters += "#"	;			//用户名之间用'#'隔开
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
