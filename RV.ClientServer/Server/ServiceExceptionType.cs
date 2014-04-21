namespace RV.ClientServer.Server
{
	public enum ServiceExceptionType
	{
		None = 0,
		Login = 1,
		UpdateScreen = 2,
		UpdateCursor = 3,
		Logout = 4,
		CheckDllVersion = 5,
		DllUpdate = 6,
		Default = None
	}
}