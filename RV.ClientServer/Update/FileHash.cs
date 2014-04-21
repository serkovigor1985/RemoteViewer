using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RV.ClientServer.Update
{
	internal class FileHash
	{
		private readonly MD5 _md5Hash;

		internal FileHash()
		{
			_md5Hash = MD5.Create();
		}

		internal string ComputeHash(string path)
		{
			var builder = new StringBuilder();
			using (var stream = File.OpenRead(path))
			{
				var data = _md5Hash.ComputeHash(stream);
				for (var index = 0; index < data.Length; index++)
					builder.Append(data[index].ToString("x2"));
			}

			return builder.ToString();
		}

		internal bool CheckHash(string path, string hash)
		{
			var hashOfFile = ComputeHash(path);

			var comparer = StringComparer.OrdinalIgnoreCase;
			return comparer.Compare(hash, hashOfFile) == 0;
		}
	}
}
