namespace SuperMaxxii.TikTokCounter
{
	public class TikTokCounterSettings
	{
		/// <summary>
		/// Page Load Delay in Seconds
		/// </summary>
		public int PageLoadDelay { get; set; }

		/// <summary>
		/// Css Class to search for the reading
		/// </summary>
		public string FindClassName { get; set; }

		/// <summary>
		/// Read the page every <see cref="ReloadDelay"/>  in Seconds
		/// </summary>
		public int ReloadDelay { get; set; }

		/// <summary>
		/// Web Driver Path and Name
		/// </summary>
		public (string Path, string Name) WebDriver { get; set; }

		/// <summary>
		/// The Url to TikTok Counter
		/// </summary>
		public string TikTokUrl { get; set; }

		/// <summary>
		/// The File Path to the sound to be played
		/// </summary>
		public string NotificationSoundPath { get; set; }
	}
}