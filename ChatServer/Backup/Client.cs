using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace ChatServer
{
	public class Client
	{
		private Thread clthread;
		private EndPoint endpoint;
		private string name;
		private Socket sock;

		public Client(string _name, EndPoint _endpoint, Thread _thread, Socket _sock)
		{
			name = _name;                  //用户名
			endpoint = _endpoint;        //端口号
			clthread = _thread;            //线程
			sock = _sock;                    //套接字sock
		}
		public override string ToString()   //重写ToString()
		{	return endpoint.ToString()+" : "+name;  
		}
		public Thread CLThread
		{
			get{return clthread;}
			set{clthread = value;}
		}
		public EndPoint Host
		{
			get{return endpoint;}
			set{endpoint = value;}
		}
		public string Name
		{
			get{return name;}
			set{name = value;}
		}
		public Socket Sock
		{
			get{return sock;}
			set{sock = value;}
		}
	}
}
