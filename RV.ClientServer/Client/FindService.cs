using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Discovery;

using RV.ClientServer.Server;

namespace RV.ClientServer.Client
{
	public static class FindService
	{
		public static ObservableCollection<EndpointAddress> ServicesAddressList()
		{
			var discoveryClient =
				new DiscoveryClient(new UdpDiscoveryEndpoint());

			var viewerServices =
				discoveryClient.Find(new FindCriteria(typeof(IRemoteService)));

			discoveryClient.Close();

			var findServiceList =
				new ObservableCollection<EndpointAddress>(viewerServices.Endpoints.Select(endpoint => endpoint.Address).ToList());

			return findServiceList;
		}
	}
}
