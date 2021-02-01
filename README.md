# Covers

Covers is a browser based network music player for album art lovers. 
Written in C# (Web API in .NET 5) for the backend and ReactJS for the browser UI.

## Features
* network music player for your local music library
* play your music on any device with a browser
* no user limitations, every user has its own session
* Album cover art as main navigation
* especially for album lovers, no shuffle over the whole music library, just the album
* automatically album cover art import from different sources (using the [Album Art Downloader](https://sourceforge.net/projects/album-art/)) or importing them from the ID3 tags of your files
* the local music library won't be altered in any way

## Screenshots
tbd


## Installation

* unzip to a folder of your choice
* adjust the appsettings.json file to your needs, especially:
  * "MusicDirectory": "C:\\Users\\ise\\Music",
  * "aadExecutable": "C:\\Users\\ise\\Downloads\\AlbumArtDownloaderXUI-1.05\\aad.exe"
    - if you don't want to use the Album Art Downloader (which you have to download separately) then leave this option empty, but then no cover will be fetched from the internet and only covers from the ID3 tags are imported if they exists
* make sure that you have adjusted and set those two options in the appsettings file before you start Covers
* start covers.exe
* point your browser to the address shown in the command line

## Usage
tbd

## Roadmap

* Spotify integration
* Back cover song chooser (make the back cover interactable to choose the tracks instead of a simple table)
* UI improvements for smaller displays (especially the album modal with track listing)

## Thanks

Special thanks to Stephan for late night discussions about UI related stuff and overall brainstorming.
