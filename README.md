# Covers

Covers is a browser based network music player for album art lovers. 
Written in C# (Web API in .NET 5) for the backend and ReactJS for the browser UI.

<img src="https://lh3.googleusercontent.com/pw/ACtC-3esfg585AU3HQkJWyIFUtjbh-x81DaCvlypm0UZcLEzvL0gFaO1M-d5WKDFfRLGlfVJG-ERutPAbmD2rCwshAy4P3p1tbZFfTmeJ462Q2hb1aWKr4i8eXWsMTOWPAr4a6vgs-IP7iHF6BXCcYKEm1Ie6Q=w1119-h815-no" />

<img src="https://lh3.googleusercontent.com/pw/ACtC-3cPOzMtaB8KCSA3rssPSKBt5ubW73ww5Tp72rdRJofRRqY236VoHZMauMNAMYFxWjt4KmsDmtmooXhexWCCauQYapJlJKAC8K-ijz26mDXfbeFZ87uB4FuC_T43Fp0XREy0WnXtwfMwLQaaDxAINtSDaQ=w1254-h746-no?authuser=0" />

## Features
* network music player for your local music library
* play your music on any device with a browser
* no user limitations, every user has its own session
* Album cover art as main navigation
* especially for album lovers, no shuffle over the whole music library, just the album
* automatically album cover art import from different sources (using the [Album Art Downloader](https://sourceforge.net/projects/album-art/) or [Smart Automatic Cover Art Downloader](https://github.com/desbma/sacad)) or importing them from the ID3 tags of your files
* the local music library won't be altered in any way

## Installation

* install one of the mentioned album cover downloader
* unzip the release package of Covers to a folder of your choice
* adjust the appsettings.json file to your needs, especially:
```
"MusicDirectory": "<your full path to your local mp3 files>",
"CoverDownloader": {
  // AAD = Album Art Downloader (https://sourceforge.net/projects/album-art/)
  // SACAD = Smart Automatic Cover Art Downloader (https://github.com/desbma/sacad)
  "Type": "<your preferred cover downloader from above>",
  "Executable": "<your full path to the executable>"
}
```
Example:
```
"MusicDirectory": "C:\\Music",
"CoverDownloader": {
  // AAD = Album Art Downloader (https://sourceforge.net/projects/album-art/)
  // SACAD = Smart Automatic Cover Art Downloader (https://github.com/desbma/sacad)
  "Type": "AAD",
  "Executable": "C:\\AlbumArtDownloaderXUI-1.05\\aad.exe"
}
```
* make sure that you have adjusted at least those two options in the appsettings file before you start Covers
* furthermore you can configure the listening port for Covers. If you want to change it, just change port 5000 with your preferred one:
```
"Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      }
    }
  }
  ```
* start Covers by double-click on the exe (Linux: make sure that Covers has the execution flag and then type ./Covers in a terminal of your choice)
* point your browser to the address shown in the command line

## Usage
### Listen to music
Using Covers and listen to music is very easy. Just click on a album cover art and the tracks of this album will be shown in a modal dialog. Click on a track and it will play. Click another track and this track will be played. Down below in the player you can scrap through the track with the slider or jump to next/previous track with the controls. Also you canjump -/+10 seconds with the forward and backward controls. Clicking the small cover thumbnail will open the album modal dialog again, clicking it again will close it.
The playback plays all title from top to bottom and will stop if it reaches the end of the album.

### Change album cover art
It can happen that the automatic album cover downloader downloads a wrong cover art, but you can change this afterwards. To do this just click on the overview page on an album and then click in the modal dialog the album cover image. Choose another album cover image from your disk and press 'Open'. The album cover will be replaced with the new one and automatically displayed.

## Roadmap

* Spotify integration
* Back cover song chooser (make the back cover interactable to choose the tracks instead of a simple table)
* UI improvements for smaller displays (especially the album modal with track listing)

## Thanks

Special thanks to Stephan for late night discussions about UI related stuff and overall brainstorming.
