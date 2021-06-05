using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SuperMaxxii.TikTokCounter.SoundPlayer
{
	public class SoundPlayerWrapper : IDisposable
	{
		private readonly Process _cmd;
		private const string _FFPlayCommand = ".\\SoundPlayer\\ffplay.exe \"{0}\" -nodisp -autoexit -nostats -loglevel error";

		public SoundPlayerWrapper()
		{
			_cmd = new Process
			{
				StartInfo =
				{
					FileName = "cmd.exe",
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true,
					UseShellExecute = false,

				},
			};
			_cmd.ErrorDataReceived += ErrorDataReceived;
			_cmd.Start();
			_cmd.BeginErrorReadLine();
		}

		private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(e.Data) && !e.Data.EndsWith("Failed to send close message"))
			{
				Console.Error.WriteLine(e.Data);
			}
		}

		public async Task PlaySound(string pathToSoundFile)
		{
			if (string.IsNullOrWhiteSpace(pathToSoundFile))
			{
				return;
			}
			await _cmd.StandardInput.WriteLineAsync(string.Format(_FFPlayCommand, pathToSoundFile));
			await _cmd.StandardInput.FlushAsync();
		}

		public void Dispose()
		{
			_cmd.ErrorDataReceived -= ErrorDataReceived;
			_cmd.CancelErrorRead();
			_cmd.WaitForExit(5000);
			_cmd.Dispose();
		}
	}
}