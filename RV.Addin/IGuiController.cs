using System;
using System.ServiceModel;

namespace RV.AddIn
{
	public interface IGuiController
	{
		void Register();
		void Main();
		void Tray();
		void Restart(string endpoint);
		event EventHandler Closed;
		event EventHandler<NeedUpdateEventArgs> Update;
	}
}
