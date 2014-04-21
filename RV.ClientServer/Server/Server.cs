using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;

namespace RV.ClientServer.Server
{
	public class Server
	{
		#region Private props

		private Uri _serverAddress;
		private volatile ServiceHost _serviceHost;

		#endregion

		#region Public events

		public event EventHandler ServerStart;
		public event EventHandler ServerProblem;

		#endregion

		#region Public methods

		public void SetPort(string port)
		{
			var binding = Services.Utils.GetTcpBinding();

			_serverAddress = new Uri(string.Format("{0}://{1}:{2}/", binding.Scheme, System.Net.Dns.GetHostName(), port));
		}

		public void Start()
		{
			StartServer();
		}

		public void Stop()
		{
			if (_serviceHost != null)
				_serviceHost.Close();
		}

		public ObservableCollection<string> UserCollection
		{
			get { return RemoteService.UserCollection; }
		}

		#endregion

		#region Private methods

		private void StartServer()
		{
			try
			{
				_serviceHost = new ServiceHost(typeof(RemoteService), _serverAddress);
#if DEBUG
				var binding = Services.Utils.GetTcpBinding();
#else
			var binding = Services.Utils.GetTcpBinding();
#endif

				_serviceHost.AddServiceEndpoint(typeof(IRemoteService), binding, _serverAddress);
#if DEBUG
				/*var metad = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>() ?? new ServiceMetadataBehavior();
				metad.HttpGetEnabled = true;
				serviceHost.Description.Behaviors.Add(metad);
				serviceHost.AddServiceEndpoint(
					ServiceMetadataBehavior.MexContractName,
					MetadataExchangeBindings.CreateMexHttpBinding(),
					"mex"
					);*/
#endif
				var discoveryBehavior = new ServiceDiscoveryBehavior();
				_serviceHost.Description.Behaviors.Add(discoveryBehavior);
				discoveryBehavior.AnnouncementEndpoints.Add(
					new UdpAnnouncementEndpoint());
				_serviceHost.Description.Endpoints.Add(new UdpDiscoveryEndpoint());
				_serviceHost.Open();

				if (ServerStart != null)
					ServerStart(null, EventArgs.Empty);
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc.ToString());

				if (ServerProblem != null)
					ServerProblem(null, EventArgs.Empty);
			}
	}

	#endregion
	}
}
