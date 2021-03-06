using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Edge.SeleniumTools;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using SuperMaxxii.TikTokCounter.SoundPlayer;

namespace SuperMaxxii.TikTokCounter
{
	class Program
	{
		private static CancellationTokenSource _cancellationTokenSource;
		
		private static int? _currentCount;
		private static Thread _thread;
		private static EdgeDriver _driver;
		private static SoundPlayerWrapper _soundWrapper;

		static async Task Main(string[] args)
		{
			_cancellationTokenSource = new CancellationTokenSource();
			
			SetupResourcesCleaner();

			Console.WriteLine("Starting... ");

			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddCommandLine(args)
				.Build();

			var appSettings = GetAppSettings(configuration);
			
			PrintSettings(appSettings);

			Console.CancelKeyPress += Console_CancelKeyPress;

			using (_driver = GetWebDriver(appSettings))
			{
				try
				{
					_driver.Navigate();
					using (_soundWrapper = new SoundPlayerWrapper())
					{
						do
						{
							if (_cancellationTokenSource.IsCancellationRequested)
							{
								break;
							}

							var mainCounter = _driver.FindElementByClassName(appSettings.FindClassName);
							var counter = GetCounter(mainCounter);

							// double check values;
							await Task.Delay(TimeSpan.FromSeconds(appSettings.PageLoadDelay), _cancellationTokenSource.Token);

							counter = Math.Min(counter, GetCounter(mainCounter));

							if (_currentCount == null)
							{
								_currentCount = counter;
							}
							else if (counter > _currentCount.Value)
							{
								_currentCount = counter;
								await _soundWrapper.PlaySound(appSettings.NotificationSoundPath);
							}
							Console.WriteLine(_currentCount.Value);

							await Task.Delay(TimeSpan.FromSeconds(appSettings.ReloadDelay), _cancellationTokenSource.Token);

						} while (!_cancellationTokenSource.IsCancellationRequested);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.Message}");
				}
				_driver.Close();
			}
			
			Console.WriteLine("Exiting....");
			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		private static void SetupResourcesCleaner()
		{
			_thread = new Thread(new ThreadStart(() =>
				{
					WaitHandle.WaitAny(new[] { _cancellationTokenSource.Token.WaitHandle });
					_driver.Dispose();
					_soundWrapper.Dispose();
					
					Thread.Sleep(5000);
				}))
				{IsBackground = false};
			_thread.Start();
			AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => { _cancellationTokenSource.Cancel(); };
		}

		private static void PrintSettings(TikTokCounterSettings appSettings)
		{
			Console.WriteLine("Settings: ");
			foreach (var propertyInfo in appSettings.GetType().GetProperties())
			{
				Console.WriteLine($"\t - {propertyInfo.Name} => {propertyInfo.GetValue(appSettings)}");
			}
		}

		private static int GetCounter(IWebElement mainCounter)
		{
			var sanitised = Regex.Replace(mainCounter.Text, "\\D", "");

			var counter = int.Parse(sanitised);
			return counter;
		}

		private static TikTokCounterSettings GetAppSettings(IConfigurationRoot configuration)
		{
			var path = configuration.GetValue<string>("WebDriver:Path") ?? GetDefaultWebDriverPath();
			var webDriverName = configuration.GetValue<string>("WebDriver:FileName") ?? "msedgedriver.exe";

			var url = configuration.GetValue<string>("TikTokUrl");
			return new TikTokCounterSettings
			{
				PageLoadDelay = configuration.GetValue<int>("PageLoadDelay"),
				ReloadDelay = configuration.GetValue<int>("ReloadDelay"),
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
				LeaveBrowserRunning = false
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
