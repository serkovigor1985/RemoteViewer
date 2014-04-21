using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace RV.AddIn
{
	[Serializable]
	public class NeedUpdateEventArgs : EventArgs
	{
		#region Public props

		public string Endpoint { get; private set; }
		public string Mask { get; set; }
		public string UpdatePath { get; set; }
		public string BackupPath { get; set; }
		public string[] FilesExclude { get; set; }

		#endregion

		public NeedUpdateEventArgs(string endpoint = null)
		{
			Endpoint = endpoint;
		}
	}
}
