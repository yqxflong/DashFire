using System;
namespace SocketIOClient
{
	/// <summary>
	/// C# Socket.IO client interface
	/// </summary>
	interface IClient
	{
    EventHandler Opened { get; set; }
    EventHandler<MessageEventArgs> Message { get; set; }
    EventHandler SocketConnectionClosed { get; set; }
    EventHandler<ErrorEventArgs> Error { get; set; }

		SocketIOHandshake HandShake { get; }
		bool IsConnected { get; }
		WebSocket4Net.WebSocketState ReadyState { get; }

		void Connect();
		IEndPointClient Connect(string endPoint);

		void Close();
		void Dispose();

		void On(string eventName, Action<SocketIOClient.Messages.IMessage> action);
		void On(string eventName, string endPoint, Action<SocketIOClient.Messages.IMessage> action);

		void Emit(string eventName, Object payload);
		void Emit(string eventName, Object payload, string endPoint  , Action<Object>  callBack  );
		
		void Send(SocketIOClient.Messages.IMessage msg);
		//void Send(string rawEncodedMessageText);
	}
}
