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

		private string clientname;        //用户名
		private NetworkStream ns;         //网络流
		private string serveraddress;     //服务器地址
		private int serverport;           //端口号
		private TcpClient clientsocket;   //TCP连接
		private IPEndPoint ipendpoint;    //服务器端点
		private Thread receive = null;    //线程

		private bool connected = false;   //是否已连接到服务器
		private bool privatemode = false; //是否进行私聊
		private bool logging = false;     //是否进行日志读写

		//private StreamReader sr;
		private StreamWriter logwriter;   //日志读写
		string serverresponse;            // 服务器应答信息
		Byte[] buffer = new Byte[2048];   // 接收数据缓冲区
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
			btnDisconnect.Enabled = false;     //断开按钮不可用
			btnSend.Enabled = false;           //发送按钮不可用

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
			this.cbPrivate.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.cbPrivate.ForeColor = System.Drawing.SystemColors.Window;
			this.cbPrivate.Location = new System.Drawing.Point(488, 248);
			this.cbPrivate.Name = "cbPrivate";
			this.cbPrivate.Size = new System.Drawing.Size(56, 25);
			this.cbPrivate.TabIndex = 10;
			this.cbPrivate.Text = "私聊";
			this.cbPrivate.CheckStateChanged += new System.EventHandler(this.cbPrivate_CheckedChanged);
			// 
			// btnLog
			// 
			this.btnLog.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.btnLog.ForeColor = System.Drawing.SystemColors.Window;
			this.btnLog.Location = new System.Drawing.Point(456, 208);
			this.btnLog.Name = "btnLog";
			this.btnLog.Size = new System.Drawing.Size(104, 26);
			this.btnLog.TabIndex = 9;
			this.btnLog.Text = "日志记录";
			this.btnLog.Click += new System.EventHandler(this.btnLog_Click);
			// 
			// btnSend
			// 
			this.btnSend.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.btnSend.ForeColor = System.Drawing.SystemColors.Window;
			this.btnSend.Location = new System.Drawing.Point(336, 250);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(96, 26);
			this.btnSend.TabIndex = 5;
			this.btnSend.Text = "发送";
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// lbChatters
			// 
			this.lbChatters.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.lbChatters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lbChatters.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
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
			this.btnConnect.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.btnConnect.ForeColor = System.Drawing.SystemColors.Window;
			this.btnConnect.Location = new System.Drawing.Point(456, 120);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(104, 26);
			this.btnConnect.TabIndex = 4;
			this.btnConnect.Text = "连接";
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// rtbChatIn
			// 
			this.rtbChatIn.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.rtbChatIn.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.rtbChatIn.Location = new System.Drawing.Point(10, 32);
			this.rtbChatIn.Name = "rtbChatIn";
			this.rtbChatIn.ReadOnly = true;
			this.rtbChatIn.Size = new System.Drawing.Size(302, 208);
			this.rtbChatIn.TabIndex = 6;
			this.rtbChatIn.Text = "";
			// 
			// btnDisconnect
			// 
			this.btnDisconnect.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.btnDisconnect.ForeColor = System.Drawing.SystemColors.Window;
			this.btnDisconnect.Location = new System.Drawing.Point(456, 152);
			this.btnDisconnect.Name = "btnDisconnect";
			this.btnDisconnect.Size = new System.Drawing.Size(104, 26);
			this.btnDisconnect.TabIndex = 8;
			this.btnDisconnect.Text = "断开";
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
			this.label4.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.label4.ForeColor = System.Drawing.SystemColors.Window;
			this.label4.Location = new System.Drawing.Point(464, 64);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(80, 16);
			this.label4.TabIndex = 13;
			this.label4.Text = "昵称";
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.label1.ForeColor = System.Drawing.SystemColors.Window;
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 16);
			this.label1.TabIndex = 15;
			this.label1.Text = "聊天信息";
			// 
			// label2
			// 
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.label2.ForeColor = System.Drawing.SystemColors.Window;
			this.label2.Location = new System.Drawing.Point(328, 16);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(104, 16);
			this.label2.TabIndex = 16;
			this.label2.Text = "聊天用户";
			// 
			// label3
			// 
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.label3.ForeColor = System.Drawing.SystemColors.Window;
			this.label3.Location = new System.Drawing.Point(464, 16);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 16);
			this.label3.TabIndex = 17;
			this.label3.Text = "服务器";
			// 
			// statusBar1
			// 
			this.statusBar1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.statusBar1.ForeColor = System.Drawing.SystemColors.Window;
			this.statusBar1.Location = new System.Drawing.Point(8, 296);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(560, 16);
			this.statusBar1.TabIndex = 18;
			this.statusBar1.Text = "断开";
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
			this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "ChatClientForm";
			this.Text = "聊天客户端";
			this.Load += new System.EventHandler(this.ChatClientForm_Load);
			this.ResumeLayout(false);

		}
		#endregion
		protected override void OnClosed(EventArgs e)    //重写关闭方法
		{
			QuitChat();                                  //退出聊天
			if(receive != null && receive.IsAlive)       //如果有活动线程则不关闭线程
				receive.Abort();
			
			base.OnClosed(e);                            //执行基类的不关闭方法
		}

		private void ServerConnection()                  //连接到服务器方法
		{
			statusBar1.Text = "连接到服务器";
			try
			{
				//把IP.Text输入的IP地址字符串转换为IPAddress格式;
				IPAddress myIP=IPAddress.Parse (IP.Text); 
				serveraddress=IP.Text;
				ipendpoint=new IPEndPoint(myIP,serverport);   //服务器端点
				clientsocket = new TcpClient(ipendpoint);     //建立TCP连接
			}
			catch
			{
				clientsocket = new TcpClient();
				clientsocket.Connect(serveraddress,serverport);     //建立TCP连接
//				clientsocket = new TcpClient(serveraddress,serverport);
//				MessageBox.Show ("您输入的IP地址格式不正确!");
//				return;
			}

			try 
			{
				ns = clientsocket.GetStream();          // 通过clientsocket(TcpClient)与服务器传输信息
				connected = true;                           // 已连接上
			}
			catch
			{
				MessageBox.Show("不能连接到服务器","错误",
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				statusBar1.Text = "断开";
			}
		}
		private void RegisterWithServer()                    //用户注册到服务器
		{
			try 
			{
				string command = "CONN#" + clientname+"#";         // 连接命令  
				Byte[] outbytes = System.Text.Encoding.Unicode.GetBytes(command.ToCharArray());
				ns.Write(outbytes,0,outbytes.Length);             // 通过ns（socket）向服务器发送信息（字节流）

				ns.Read(buffer,0,buffer.Length);                     //  通过ns（socket）从服务器读取信息到buffer中
				serverresponse = System.Text.Encoding.Unicode.GetString(buffer);  // 转换为Unicode格式

				string[] tokens = serverresponse.Split(new Char[]{'#'});         // 将serverresponse以'#'分解成多个字符串
				if(tokens[0] == "LIST")                                  // 如果获取到用户列表则表示登陆成功，显示用户表
				{
					statusBar1.Text = "已登陆";
					btnDisconnect.Enabled = true;
					for(int n=1; n<tokens.Length-1; n++)         // 显示用户表
						lbChatters.Items.Add(tokens[n]);
				}
				this.Text = clientname + ": 登陆到服务器";         //设置窗体标题
			}
			catch
			{
				MessageBox.Show("登陆错误","错误",
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		
		private void ReceiveChat()                        //接收聊天信息
		{
			bool keepalive = true;                          // 保持活动状态
			while (keepalive) 
			{
				try
				{
					ns.Read(buffer,0,buffer.Length);       // 通过ns从服务器读取信息到buffer中
					serverresponse = System.Text.Encoding.Unicode.GetString(buffer);   //转换为Unicode格式
					string[] tokens = serverresponse.Split(new Char[]{'#'});                         // 将 serverresponse 按'#' 分解成多个字符串
					if(logging)
					{  string time=System.DateTime.Now.ToString();               // 取得系统时间   
						logwriter.WriteLine(time);                            // 记录系统时间到文件中
					}
					switch(tokens[0])                    // 判断信息中的状态及命令
					{   case "CHAT":                        //聊天
							rtbChatIn.SelectionColor=Color.Blue;    //设置插入点为蓝色
							rtbChatIn.AppendText(tokens[1]);     // 在聊天信息框中添加发言者用户名
							rtbChatIn.AppendText("：");
							rtbChatIn.SelectionColor=Color.Black;   //开始显示黑色字符
							rtbChatIn.AppendText(tokens[2]);     // 在聊天信息框中增加聊天信息
							rtbChatIn.AppendText("\r\n");           // 换行
							if(logging)
								logwriter.WriteLine(tokens[1]+"："+tokens[2]);   // 如果需要记录日志则保存到文件
							break;
						case  "PRIV":                       //私聊
							rtbChatIn.SelectionColor=Color.DarkRed;   //设置插入点为暗红色
							rtbChatIn.AppendText(tokens[1]);
							rtbChatIn.AppendText(" 私聊：");
							rtbChatIn.SelectionColor=Color.Black;
							rtbChatIn.AppendText(tokens[2]);
							rtbChatIn.AppendText("\r\n");
							if(logging)
								logwriter.WriteLine(tokens[1]+" 与 "+tokens[3]+" 私聊："+tokens[2]);
							break;
						case   "JOIN":                      //上线
							rtbChatIn.SelectionColor=Color.Red;
							rtbChatIn.AppendText(tokens[1]);
							rtbChatIn.AppendText(" 进入聊天室\r\n");
							string newguy = tokens[1];
							lbChatters.Items.Add(newguy);                    // 在用户列表中增加一个新用户
							if(logging)
								logwriter.WriteLine(tokens[1]+" 加入聊天室");
							break;
						case    "GONE":                     //离线
							rtbChatIn.SelectionColor=Color.Red;
							rtbChatIn.AppendText(tokens[1]);
							rtbChatIn.AppendText(" 离开聊天室\r\n");
							if(logging)
								logwriter.WriteLine(tokens[1]+" 离开聊天室");
							lbChatters.Items.Remove(tokens[1]);                // 在用户列表中删除该用户
							break;
						case    "QUIT":                    // 服务器关闭
							ns.Close();                    // 关闭 ns（socket）
							clientsocket.Close();          // 关闭 clientsocket(TcpClient)
							keepalive = false;             // 不再活动
							statusBar1.Text = "服务器关闭";
							connected= false;              // 无连接
							btnSend.Enabled = false;       // “发送”按钮无效
							btnDisconnect.Enabled = false;  // “离线”按钮无效
							lbChatters.Items.Clear();      // 清除用户列表
							if(logging)
								logwriter.WriteLine("服务器关闭");
							break;
					}
				}
				catch(Exception ex){
//					MessageBox.Show(ex.ToString(),"错误",
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
					ns.Write(outbytes,0,outbytes.Length);            //通知服务器该用户要离线
				}
				catch(Exception ex){
					MessageBox.Show(ex.ToString(),"错误",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			if(logging)
			{
				StartStopLogging();                    // 停止日志记录
				logwriter.Close();                     // 关闭 logwriter
			}
			this.Text = "聊天室";                     // 设置窗体标题
		}
		private void StartStopLogging() 
		{
			if(!logging){
				if(!Directory.Exists("logs"))              // 如果不存在 logs 目录则建立该目录
					Directory.CreateDirectory("logs");
				string fname = "logs\\" + DateTime.Now.ToString("ddMMyyHHmm") + ".txt";   // 以时间命名日志文件名
				logwriter = new StreamWriter(new FileStream(fname, FileMode.OpenOrCreate,
					FileAccess.Write));                     // 创建日志文件，初始化文件流 logwriter
				logging = true;                             // 日志记录标志
				btnLog.Text = "终止日志跟踪";               // 更改日志按钮标题
				statusBar1.Text = "连接 - 日志跟踪";        // 更改状态信息
			}
			else{
				logwriter.Close();                          // 关闭文件流 logwriter
				logging = false;
				btnLog.Text = "日志跟踪";
				statusBar1.Text = "连接 - 日志关闭";
			}

		}
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(String[] args)                    // 可以在运行时带参数，存于 args 中
		{
			ChatClientForm cf = new ChatClientForm();      // 产生新的窗体
			if(args.Length == 0)                           // 如果运行时没带参数则默认为本机
				cf.serveraddress = "localhost";                      
			else
				cf.serveraddress = args[0];                // 如果运行时带有参数则第一个参数就是服务器DNS
			cf.IP.Text=cf.serveraddress;
			Application.Run(cf);                           // 启动进程，运行程序
		}

		private void btnConnect_Click(object sender, System.EventArgs e)
		{
			if(User.Text.Trim() == "")                           // 如果用户名不能为空
			{
				MessageBox.Show("请输入你的昵称","错误",
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			clientname = User.Text.Trim();
			clientname = clientname.Trim(new char[]{'\r','\n'});
			
			ServerConnection();                // 连接到服务器 （TcpClient）
			
			if(connected)                      // 如果连接成功
			{
				RegisterWithServer();          // 用户登陆
				receive = new Thread(new ThreadStart(ReceiveChat));    // 创建新线程 receive
				receive.Start();               // 启动新线程 receive
				btnSend.Enabled = true;        // 允许发送
				btnConnect.Enabled = false;    // 不允许电击“连接”按钮
				btnDisconnect.Enabled=true;    // 允许电击“离线”按钮
				ChatOut.Text = "";             // 清除 ChatOut
				ChatOut.Focus();               // 设置 ChatOut 为当前输入焦点
			}
		}

		private void btnSend_Click(object sender, System.EventArgs e)      // 信息发送
		{
			if (ChatOut.Text.Trim()=="")
				return;
			try{
				string command;
				if(!privatemode){                      // 如果不是私聊模式则构造聊天信息字符串
					command = "CHAT#" + clientname +"#"+ChatOut.Text + "#";
				}
				else{
					if(lbChatters.SelectedIndex == -1){      // 如果没有选择用户
						MessageBox.Show("请选择一个聊天用户","错误",MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return;
					}
					string destclient = lbChatters.SelectedItem.ToString();
					command = "PRIV#" + clientname + "#" + ChatOut.Text + "#" + destclient + "#";   //构造私聊信息字符串
				}
					Byte[] outbytes = System.Text.Encoding.Unicode.GetBytes(command.ToCharArray());  // 转换为字节流
					ns.Write(outbytes,0,outbytes.Length);                      // 发送给服务器
					ChatOut.Text = "";
					ChatOut.Focus();
			}
			catch(Exception ex)                    // 信息发送失败则离线
			{
				MessageBox.Show(ex.ToString(),"错误",	MessageBoxButtons.OK, MessageBoxIcon.Error);
				ns.Close();                        // 关闭ns
				clientsocket.Close();
				if(receive != null && receive.IsAlive)     // 终止该线程
					receive.Abort();
				connected = false;
				statusBar1.Text = "离线";
			}
		}

		private void btnDisconnect_Click(object sender, System.EventArgs e)   // 离线
		{
			QuitChat();
			ns.Close();
			clientsocket.Close();
			if(receive != null && receive.IsAlive)    // 如果有活动进程则关闭它
				receive.Abort();
			btnDisconnect.Enabled = false;
			btnConnect.Enabled = true;
			btnSend.Enabled = false;
			connected = false;
			lbChatters.Items.Clear();
			statusBar1.Text = "离线";
		}

		private void ChatOut_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)   // 定义热键
			{
			if(e.KeyChar == '\r')     // 如果是回车键并且网络已经连接则自动点击 btnSend 按钮
				if(connected)
					btnSend_Click(sender, e);
		}

		private void btnLog_Click(object sender, System.EventArgs e)     // 日志记录按钮
		{
			StartStopLogging();        
		}

		private void cbPrivate_CheckedChanged(object sender, System.EventArgs e)    // 私聊选项
		{
			if(cbPrivate.Checked)                  
			{	privatemode = true;                   // 设置为私聊模式
				lbChatters.SelectionMode = System.Windows.Forms.SelectionMode.One;   // 用户列表中可以选择一项
			}
			else{
				privatemode = false;                  // 取消私聊模式
				lbChatters.SelectionMode = System.Windows.Forms.SelectionMode.None;  // 不能选取用户
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
