using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RV.Command
{
	internal class Update
	{
		#region Private props

		private readonly string _mask;
		private readonly string _updatePath;
		private readonly string _backupPath;
		private readonly string[] _filesExclude;
		#endregion

		public Update(
			string mask,
			string updatePath,
			string backupPath,
			string[] filesExclude
			)
		{
			_mask = mask;
			_updatePath = updatePath;
			_backupPath = backupPath;
			_filesExclude = filesExclude;
		}

		#region Public methods
		public void Start()
		{
			var backup = false;
			try
			{
				backup = Backup();
				Copy();
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc.Message);

				if (backup)
					Restore();
			}
		}
		#endregion

		#region Private methods
		private bool Backup()
		{
			return Copy(StartupPath, _backupPath, _mask);
		}

		private bool Copy()
		{
			return Copy(_updatePath, StartupPath, "*");
		}

		private bool Restore()
		{
			return Copy(_backupPath, StartupPath, "*");
		}

		/// <summary>
		/// Производит копирование файлов по маске из исходной папки в папку назначения.
		/// </summary>
		private bool Copy(string sourcePath, string destPath, string mask = "*")
		{
			var result = true;

			try
			{
				if (!Directory.Exists(destPath))
					Directory.CreateDirectory(destPath);

				var files = Directory.GetFiles(sourcePath, mask);

				foreach (var file in files)
				{
					var fileName = Path.GetFileName(file);
					if(_filesExclude.Contains(fileName))
						continue;

					var destinationFile = Path.Combine(destPath, fileName);
					File.Copy(file, destinationFile, true);
				}
			}
			catch(Exception exc)
			{
				Trace.WriteLine(exc.ToString());
				result = false;
			}
			return result;
		}

		private static string StartupPath
		{
			get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
		}
		#endregion
	}
}
