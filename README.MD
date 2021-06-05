# SuperMaxxii.Apps

## TikTokCounter

Prequesites: 
- [.NET 5 Runtime](https://dotnet.microsoft.com/download/dotnet/5.0)


### How to Run

Unzip the app. Update the `NotificationSoundPath` in the `appsettings.json`
to the sound you want to hear every time you get a new follower.

```JSON
{
	"TikTokUrl": "https://tiktokcounter.com/?user=supermaxxii",
	"FindByClassName": "mainOdometer",
	"PageLoadDelay": 5,
	"ReloadDelay": 300,
	"NotificationSoundPath": "M:\\MaxxiiSounds\\NewTikTokFollower.mp3"
}
```

 Run the `SuperMaxxii.TikTokCounter.exe`