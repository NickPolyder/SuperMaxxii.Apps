using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SuperMaxxii.TikTokCounter.SoundPlayer
{
	public class SoundPlayerWrapper: IDisposable
	{
		private readonly Process _cmd;
		private const string _FFPlayCommand = ".\\SoundPlayer\\ffplay.exe {0} -nodisp -autoexit -nostats";

		public SoundPlayerWrapper()
		{
			_cmd = new Process
			{
				StartInfo =
				{
					FileName = "cmd.exe",
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					UseShellExecute = false
				}
			};
			_cmd.Start();
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
			_cmd.WaitForExit(5000);
			_cmd.Dispose();
		}
	}
}