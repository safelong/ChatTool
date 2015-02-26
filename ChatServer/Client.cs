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
			name = _name;                  //�û���
			endpoint = _endpoint;        //�˿ں�
			clthread = _thread;            //�߳�
			sock = _sock;                    //�׽���sock
		}
		public override string ToString()   //��дToString()
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