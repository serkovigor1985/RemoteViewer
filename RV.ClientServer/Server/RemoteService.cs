using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading;

using RV.ClientServer.Services;
using RV.ClientServer.Update;

namespace RV.ClientServer.Server
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
	public class RemoteService : IRemoteService
	{
		private string _name;
		private Guid label;
		private readonly ScreenSigleton _capture = ScreenSigleton.UniqueInstance;

		public static ObservableCollection<string> UserCollection = new ObservableCollection<string>();

		public bool Login(string name)
		{
			const bool login = true;
			try
			{
				_name = name;
				if (Monitor.TryEnter(UserCollection))
				{
					try
					{
						UserCollection.Add(name);
					}
					finally
					{
						Monitor.Exit(UserCollection);
					}
				}
			}
			catch(Exception exc)
			{
				throw new FaultException<ServiceException>(new ServiceException(exc, ServiceExceptionType.Login));
			}
			return login;
		}

		public WindowData UpdateScreenImage()
		{
			try
			{
				if (Monitor.TryEnter(_capture))
				{
					WindowData screenImage = null;
					try
					{
						screenImage = _capture.GetNewImage(ref label);
					}
					finally
					{
						Monitor.Exit(_capture);
					}

					return screenImage;
				}
				return null;
			}
			catch (Exception exc)
			{
				throw new FaultException<ServiceException>(new ServiceException(exc, ServiceExceptionType.UpdateScreen));
			}
		}

		public CursorData UpdateCursorImage()
		{
			try
			{
				if (Monitor.TryEnter(_capture))
				{
					CursorData cursorData = null;
					try
					{
						cursorData = _capture.CursorImage;
					}
					finally
					{
						Monitor.Exit(_capture);
					}
					return cursorData;
				}
				return null;
			}
			catch (Exception exc)
			{
				throw new FaultException<ServiceException>(new ServiceException(exc, ServiceExceptionType.UpdateCursor));
			}
		}

		public Dictionary<string,string> GetDllsForUpdate()
		{
			Dictionary<string,string> result;
			try
			{
				var directory = new DirectoryHash(
					Properties.Settings.Default.Mask,
				                                  Utils.ConvertToArray(Properties.Settings.Default.FilesExclude));
				result = directory.GetFilesHash(Utils.ApplicationPath);
			}
			catch (Exception exc)
			{
				throw new FaultException<ServiceException>(new ServiceException(exc, ServiceExceptionType.CheckDllVersion));
			}
			return result;
		}

		public byte[] DllData(string name)
		{
			byte[] result = null;
			try
			{
				var path = Path.Combine(Utils.ApplicationPath, name);
				using(var stream = File.OpenRead(path))
				using (var binaryReader = new BinaryReader(stream))
				{
					result = binaryReader.ReadBytes((int) stream.Length);
				}
			}
			catch (Exception exc)
			{
				throw new FaultException<ServiceException>(new ServiceException(exc, ServiceExceptionType.DllUpdate));
			}
			return result;
		}

		public void Logout()
		{
			try
			{
				if(Monitor.TryEnter(UserCollection))
				{
					try
					{
						UserCollection.Remove(_name);
					}
					finally
					{
						Monitor.Exit(UserCollection);
					}
				}
			}
			catch (Exception exc)
			{
				throw new FaultException<ServiceException>(new ServiceException(exc, ServiceExceptionType.Logout));
			}
		}
	}

	[DataContract]
	public class ServiceException : IExtensibleDataObject
	{
		public ServiceException(Exception innerException, ServiceExceptionType exceptionType)
		{
			InnerException = innerException;
			ExceptionType = exceptionType;
		}

		[DataMember(Order = 0)]
		public Exception InnerException { get; private set; }

		[DataMember(Order = 1)]
		public ServiceExceptionType ExceptionType { get; private set; }

		public ExtensionDataObject ExtensionData { get; set; }
	}

	[DataContract]
	public class WindowData : IExtensibleDataObject
	{
		[DataMember(Order = 0)]
		public int Top { get; set; }

		[DataMember(Order = 1)]
		public int Bottom { get; set; }

		[DataMember(Order = 2)]
		public int Left { get; set; }

		[DataMember(Order = 3)]
		public int Right { get; set; }

		[DataMember(Order = 4)]
		public byte[] Image { get; set; }

		public ExtensionDataObject ExtensionData { get; set; }
	}

	[DataContract]
	public class CursorData : IExtensibleDataObject
	{
		[DataMember(Order = 0)]
		public int X { get; set; }

		[DataMember(Order = 1)]
		public int Y { get; set; }

		[DataMember(Order = 2)]
		public byte[] Image { get; set; }

		public ExtensionDataObject ExtensionData { get; set; }
	}
}
