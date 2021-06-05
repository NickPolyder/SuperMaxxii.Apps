using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Edge.SeleniumTools;
using Microsoft.Extensions.Configuration;
using SuperMaxxii.TikTokCounter.SoundPlayer;

namespace SuperMaxxii.TikTokCounter
{
	class Program
	{
		private static CancellationTokenSource _cancellationTokenSource;
		private static int? _currentCount;
		static async Task Main(string[] args)
		{
			Console.WriteLine("Starting... ");
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddCommandLine(args)
				.Build();

			var appSettings = GetAppSettings(configuration);
			_cancellationTokenSource = new CancellationTokenSource();

			Console.CancelKeyPress += Console_CancelKeyPress;

			var driver = GetWebDriver(appSettings);

			try
			{
				using (var soundWrapper = new SoundPlayerWrapper())
				{
					do
					{
						driver.Navigate();
						await Task.Delay(TimeSpan.FromSeconds(appSettings.PageLoadDelay), _cancellationTokenSource.Token);

						if (_cancellationTokenSource.IsCancellationRequested)
						{
							break;
						}

						var mainCounter = driver.FindElementByClassName(appSettings.FindClassName);
						var sanitised = Regex.Replace(mainCounter.Text, "\\D", "");

						var counter = int.Parse(sanitised);

						Console.WriteLine(counter);
						if (_currentCount == null)
						{
							_currentCount = counter;
						}
						else if(counter > _currentCount.Value )
						{
							_currentCount = counter;
							await soundWrapper.PlaySound(appSettings.NotificationSoundPath);
						}
						
						await Task.Delay(TimeSpan.FromSeconds(appSettings.ReloadDelay), _cancellationTokenSource.Token);

					} while (!_cancellationTokenSource.IsCancellationRequested);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}
			Console.WriteLine("Exiting....");
			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		private static TikTokCounterSettings GetAppSettings(IConfigurationRoot configuration)
		{
			var path = configuration.GetValue<string>("WebDriver:Path") ?? GetDefaultWebDriverPath();
			var webDriverName = configuration.GetValue<string>("WebDriver:FileName") ?? "msedgedriver.exe";

			var url = configuration.GetValue<string>("TikTokUrl");
			return new TikTokCounterSettings
			{
				PageLoadDelay = configuration.GetValue<int>("PageLoadDelay"),
				FindClassName = configuration.GetValue<string>("FindByClassName"),
				TikTokUrl = configuration.GetValue<string>("TikTokUrl"),
				WebDriver = (path, webDriverName),
				NotificationSoundPath = configuration.GetValue<string>("NotificationSoundPath")
			};
		}

		private static EdgeDriver GetWebDriver(TikTokCounterSettings settings)
		{
			var edgeService = EdgeDriverService.CreateChromiumService(settings.WebDriver.Path, settings.WebDriver.Name);
			edgeService.SuppressInitialDiagnosticInformation = true;
			edgeService.HideCommandPromptWindow = true;

			var edgeOptions = new EdgeOptions
			{
				UseChromium = true,
				LeaveBrowserRunning = true
			};

			edgeOptions.AddArgument("headless");

			return new EdgeDriver(edgeService, edgeOptions)
			{
				Url = settings.TikTokUrl
			};
		}

		private static string GetDefaultWebDriverPath()
		{
			return Path.Combine(Directory.GetCurrentDirectory(), "WebDriver");
		}

		private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			_cancellationTokenSource.Cancel();
		}
	}
}
