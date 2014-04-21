using System;
using System.ServiceModel;

namespace RV.UserInterface.Events
{
	public class ConnectEventArgs : EventArgs
	{
		public EndpointAddress Endpoint { get; private set; }

		public ConnectEventArgs(EndpointAddress endpoint)
		{
			Endpoint = endpoint;
		}
	}
}
