using System.Collections.Generic;
using System.ServiceModel;
using System.Collections.Specialized;

namespace RV.ClientServer.Server
{
	[ServiceContract(SessionMode = SessionMode.Required)]
	public interface IRemoteService
	{
		[OperationContract]
		[FaultContract(typeof(ServiceException))]
		bool Login(string name);

		[OperationContract]
		[FaultContract(typeof(ServiceException))]
		WindowData UpdateScreenImage();

		[OperationContract]
		[FaultContract(typeof(ServiceException))]
		CursorData UpdateCursorImage();

		[OperationContract]
		[FaultContract(typeof(ServiceException))]
		Dictionary<string,string> GetDllsForUpdate();

		[OperationContract]
		[FaultContract(typeof(ServiceException))]
		byte[] DllData(string name);

		[OperationContract]
		[FaultContract(typeof(ServiceException))]
		void Logout();
	}
}