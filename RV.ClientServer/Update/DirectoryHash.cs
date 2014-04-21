using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace RV.ClientServer.Update
{
	internal class DirectoryHash
	{
		#region Private props
		private readonly string _mask;
		private readonly string[] _filesExclude;
		private readonly FileHash _fileHash = new FileHash();
		#endregion

		internal DirectoryHash(string mask, string[] filesExclude)
		{
			_mask = mask;
			_filesExclude = filesExclude;
		}

		/// <summary>
		/// Создает 
		/// </summary>
		internal Dictionary<string,string> GetFilesHash(string path)
		{
			var files = Directory.GetFiles(path, _mask);

			var dictionary = new Dictionary<string, string>();
			foreach (string filePath in files)
			{
				string fileName = Path.GetFileName(filePath);
				if (!_filesExclude.Contains(fileName))
					dictionary.Add(fileName, _fileHash.ComputeHash(filePath));
			}
			return dictionary;
		}

		internal string[] NeedUpdate(Dictionary<string,string> files, string path)
		{
			var directoryFiles = Directory.GetFiles(path, _mask);
			var directory= directoryFiles.Select(Path.GetFileName).ToList();

			foreach (var fileName in _filesExclude.Where(directory.Contains))
			{
				directory.Remove(fileName);
			}


			return (from filePath in directory let fileName = Path.GetFileName(filePath) where files.ContainsKey(fileName) let file = files[fileName] let comparer = StringComparer.OrdinalIgnoreCase where comparer.Compare(file, _fileHash.ComputeHash(filePath)) != 0 select fileName).ToArray();
		}
	}
}
