import React, { Component } from 'react';
import Gallery from 'react-photo-gallery';
import Modal from 'react-modal';
import { CoverModal } from './CoverModal';
import OverviewCover from './OverviewCover';
import AudioPlayer, {RHAP_UI} from 'react-h5-audio-player';
import 'react-h5-audio-player/lib/styles.css';
import CoversService from '../services/CoversHubService'
import ReactTooltip from 'react-tooltip';
import axios from 'axios';


Modal.setAppElement("#root");

export class Home extends Component {
  static displayName = Home.name;
  constructor(props) {
    super(props);
    this.state = { 
      albums: [], 
      loading: true,
      isCoverModalOpen: false,
      albumIdForModal: -1,
      coverIdForModal: -1,
      trackIdToPlay: -1,
      spotifyUriToPlay: "",
      albumToPlay: null,
      playerCover: "",
      processedText: "",
      spotifyToken: "",
      spotifyDeviceId: "",
      playerPaused: true,
      volume: 1,
      spotifyTrackTotalTime: "--:--",
      spotifyTrackCurrentTime: "00:00",
      spotifyTrackCurrentProgressInPercent: 0
    }

    this.handleLoadSuccess = this.handleLoadSuccess.bind(this);
    this.handleLoadFailure = this.handleLoadSuccess.bind(this);
    this.cb = this.cb.bind(this);
    this.timeoutHandle = '';
    this.progressBarTimeoutHandle = '';

    CoversService.registerAlbumUpdates(() => {
        this.fetchAlbumData();
    });

    CoversService.registerProcessing((text) => {
      this.setState({processedText: text});
    });

    CoversService.registerSpotifyTokenRefresh((token) => {
      this.setState({spotifyToken: token});
      this.handleLoadSuccess();
    });
  }

  footerStyle = {
    backgroundColor: "#E7E7E7CC",
    fontSize: "20px",
    color: "black",
    borderTop: "1px solid #E7E7E7",
    textAlign: "center",
    position: "fixed",
    left: "0",
    bottom: "0",
    height: "60px",
    width: "100%",
    zIndex: 100
  };

  checkSpotifyLogin(){
    axios.get('Spotify/Login').then((response) => {
      if(response.data.startsWith("https://accounts.spotify.com")){
        window.location.href = response.data;
      }else{
          this.setState({ spotifyToken: response.data});
      }
    });
  }

  componentDidMount() {
    this.checkSpotifyLogin();

    if (!window.onSpotifyWebPlaybackSDKReady) {
      window.onSpotifyWebPlaybackSDKReady = this.handleLoadSuccess;
    } else {
      this.handleLoadSuccess();
    }
    this.loadSpotifyPlayer();

    this.fetchAlbumData();
  }

  loadSpotifyPlayer() {   
      const scriptTag = document.getElementById('spotify-player');
  
      if (!scriptTag) {
        const script = document.createElement('script');
  
        script.id = 'spotify-player';
        script.type = 'text/javascript';
        script.async = false;
        script.defer = true;
        script.src = 'https://sdk.scdn.co/spotify-player.js';
        // script.onload = () => resolve();
        // script.onerror = () => reject(new Error(`loadScript: ${error.message}`));
  
        document.head.appendChild(script);
      }
  }

  handleLoadSuccess() {
    this.setState({ scriptLoaded: true });
    console.log("Script loaded");
    const token = this.state.spotifyToken;
    const player = new window.Spotify.Player({
      name: 'Covers',
      getOAuthToken: cb => { cb(token); }
    });
    console.log(player);
    
    // Error handling
    player.addListener('initialization_error', ({ message }) => { console.error(message); });
    player.addListener('authentication_error', ({ message }) => { console.error(message); });
    player.addListener('account_error', ({ message }) => { console.error(message); });
    player.addListener('playback_error', ({ message }) => { console.error(message); });

    // Playback status updates
    player.addListener('player_state_changed', state => { 
      if(!this.state.playerPaused){
          // end of track reached, take the next track from album
          if (state.position === 0 &&
              this.state.spotifyUriToPlay === state.track_window.current_track.uri &&
              state.paused) {
          this.nextTrack();
        }
      }
      console.log(state); 
    });

    // Ready
    player.addListener('ready', ({ device_id }) => {
      console.log('Ready with Device ID', device_id);
      this.setState({spotifyDeviceId: device_id});
    });

    // Not Ready
    player.addListener('not_ready', ({ device_id }) => {
      console.log('Device ID has gone offline', device_id);
    });

    // Connect to the player!
    player.connect();
    player.setVolume(this.state.volume).then(() => {
      console.log('Volume updated!');
    });
  }

  cb(token) {
    return(token);
  }

  handleScriptCreate() {
    this.setState({ scriptLoaded: false });
    console.log("Script created");
  }

  handleScriptError() {
    this.setState({ scriptError: true });
    console.log("Script error");
  }

  handleScriptLoad() {
    this.setState({ scriptLoaded: true});
    console.log("Script loaded");
  }

  async fetchAlbumData() {
    const response = await fetch('Album/Overview');
    const data = await response.json();
    const covers = [];
    let i = 0;

    data.albums.forEach(element => {
      let coverSrc = "placeholder.png";
      let placeholderLazyImage = "placeholder.png";
      if(element.frontCoverId > 0)
      {
        coverSrc = `/Cover/${element.frontCoverId}?size=500`;
        placeholderLazyImage = `/Cover/${element.frontCoverId}?size=100`;
      }

      covers.push({
        key: (i++).toString(),
        src: coverSrc,
        placeholder: placeholderLazyImage,
        width: 1,
        height: 1,
        frontCoverId: element.frontCoverId,
        backCoverId: element.backCoverId,
        albumId: element.albumId,
        albumName: element.albumName,
        artistName: element.artistName
      })
    });

    this.setState({ albums: covers, loading: false });
  }

  toggleCoverModal(albumId, frontCoverId, backCoverId, fromPlayer) {
    if(this.state.isCoverModalOpen){
      this.hideModal();
    }else{
      if(fromPlayer){
        let albumForThumbnailPlayer = this.state.albums.find(album => album.albumId === this.state.albumToPlay.albumId);

        this.setState({ 
          isCoverModalOpen: true,
          albumIdForModal: albumForThumbnailPlayer.albumId,
          frontCoverIdForModal: albumForThumbnailPlayer.frontCoverId,
          backCoverIdForModal: albumForThumbnailPlayer.backCoverId});
        }else{
          this.setState({ 
            isCoverModalOpen: true,
            albumIdForModal: albumId,
            frontCoverIdForModal: frontCoverId,
            backCoverIdForModal: backCoverId});
        }
    }
  }
  
  hideModal = () => {
    this.setState({ 
      isCoverModalOpen: false
    });
  };

  play = (trackId, spotifyUri, album) => {
    this.setState({playerPaused: false, trackIdToPlay: trackId, spotifyUriToPlay: spotifyUri, albumToPlay: album, playerCover: `Cover/${this.state.frontCoverIdForModal}`});
    if(spotifyUri){
      axios.post('Spotify/Play', {
        SpotifyTrackUri: spotifyUri,
        DeviceId: this.state.spotifyDeviceId
      });

      const that = this;
      function fetchCurrentSpotifyTrackInfo() {
        axios.get('Spotify/PlaybackState?deviceId=' + that.state.spotifyDeviceId).then(function (response) {
          that.setState({spotifyTrackTotalTime: response.data.totalTime, spotifyTrackCurrentTime: response.data.currentTime, spotifyTrackCurrentProgressInPercent: response.data.currentProgressInPercent});
          console.log(response);
        });
      }
      clearInterval(this.progressBarTimeoutHandle);
      this.progressBarTimeoutHandle = setInterval(fetchCurrentSpotifyTrackInfo, 1000);
     }else{
      this.pauseSpotify();
     }
  }

  pauseSpotify = () => {
    this.setState({playerPaused: true});
    axios.post('Spotify/Pause?deviceId=' + this.state.spotifyDeviceId);
    clearInterval(this.progressBarTimeoutHandle);
  }

  resumeSpotify = () => {
    this.setState({playerPaused: false});
    axios.post('Spotify/Resume?deviceId=' + this.state.spotifyDeviceId);

    const that = this;
    function fetchCurrentSpotifyTrackInfo() {
      axios.get('Spotify/PlaybackState?deviceId=' + that.state.spotifyDeviceId).then(function (response) {
        that.setState({spotifyTrackTotalTime: response.data.totalTime, spotifyTrackCurrentTime: response.data.currentTime, spotifyTrackCurrentProgressInPercent: response.data.currentProgressInPercent});
        console.log(response);
      });
    }

    clearInterval(this.progressBarTimeoutHandle);
    this.progressBarTimeoutHandle = setInterval(fetchCurrentSpotifyTrackInfo, 1000);
  }

  nextTrack() {
    let trackArrayIndex = this.state.albumToPlay.tracks.findIndex(t => t.trackId === this.state.trackIdToPlay);
      if (this.state.albumToPlay.tracks.length > trackArrayIndex + 1) {
          this.play(this.state.albumToPlay.tracks[trackArrayIndex + 1].trackId, this.state.albumToPlay.tracks[trackArrayIndex + 1].spotifyUri, this.state.albumToPlay);
      } else {
          this.setState({playerPaused: true});
      }
  }

  previousTrack() {
    let trackArrayIndex = this.state.albumToPlay.tracks.findIndex(t => t.trackId === this.state.trackIdToPlay);
    if(trackArrayIndex - 1 >= 0){
        this.play(this.state.albumToPlay.tracks[trackArrayIndex - 1].trackId, this.state.albumToPlay.tracks[trackArrayIndex - 1].spotifyUri, this.state.albumToPlay);
    }else{
      this.play(this.state.albumToPlay.tracks[trackArrayIndex].trackId, this.state.albumToPlay.tracks[trackArrayIndex - 1].spotifyUri, this.state.albumToPlay);
    }
  }

  seekOffset = (offset) => {
    axios.post('Spotify/Step?deviceId=' + this.state.spotifyDeviceId + '&offset=' + offset);
  }

  volumeChanged = (event) => {
    this.setState({volume: event.target.volume});
    clearTimeout(this.timeoutHandle)
    this.timeoutHandle = setTimeout(this.changeVolume, 500);
  }

  changeVolume = () => {
    axios.post('Spotify/Volume?deviceId=' + this.state.spotifyDeviceId + '&volume=' + this.state.volume)
  }

  fetchCurrentSpotifyTrackInfo(deviceId) {
    axios.get('Spotify/PlaybackState?deviceId=' + deviceId).then(function (response) {
      this.setState({spotifyTrackTotalTime: response.data.TotalTime, spotifyTrackCurrentTime: response.data.CurrentTime, spotifyTrackCurrentProgressInPercent: response.data.CurrentProgressInPercent});
      console.log(response);
    });
  }

  frontCoverUpdated = (albumId, coverId) => {
    let album = this.state.albums.find(album => album.albumId === albumId);
    album.frontCoverId = coverId;
    album.src = `/Cover/${album.frontCoverId}?size=500`;
    this.setState({albums: this.state.albums});
  }

  backCoverUpdated = (albumId, coverId) => {
    let album = this.state.albums.find(album => album.albumId === albumId);
    album.backCoverId = coverId;
    this.setState({albums: this.state.albums});
  }

  render () {
    let thumbCover;
    if(this.state.albumToPlay){
      thumbCover = <a data-tip data-for="registerTip">
        <div className="playerThumbCover" 
          style={{backgroundImage: `url('${this.state.playerCover}')`}} 
          onClick={() => this.toggleCoverModal(this.state.albumIdForModal, this.state.frontCoverIdForModal, this.state.backCoverIdForModal, true)}>
        </div>
      </a>;
    }else{
      thumbCover = <div className="playerThumbCover" style={{cursor: 'default'}} />
    }

    let tooltip= "";
    if(this.state.albumToPlay){
      let trackArrayIndex = this.state.albumToPlay.tracks.findIndex(t => t.trackId === this.state.trackIdToPlay);
      tooltip = (
      <div className="container-fluid h-100">
        <div className="col-12">
            <div className="tooltipThumbCover">
                <img src={this.state.playerCover} width="200px" />
            </div>
            <div>
              {this.state.albumToPlay.artist}
            </div>
            <div>
              {this.state.albumToPlay.name}
            </div>
            <div>
              {this.state.albumToPlay.tracks[trackArrayIndex].name}
            </div>
        </div>
      </div>
      );
    }

    var customProgressBarSection = [RHAP_UI.CURRENT_TIME, RHAP_UI.PROGRESS_BAR, RHAP_UI.DURATION];
    var customControlsSection = [RHAP_UI.ADDITIONAL_CONTROLS, RHAP_UI.MAIN_CONTROLS, RHAP_UI.VOLUME_CONTROLS];
    var playTrackSrc = `Track/${this.state.trackIdToPlay}`;

    if(this.state.spotifyUriToPlay){
      var percentage = `${this.state.spotifyTrackCurrentProgressInPercent}%`;

      var spotifyProgressBar = (
        <div className="rhap_progress-section">
          <div className="rhap_time rhap_current-time">{this.state.spotifyTrackCurrentTime}</div>
              <div className="rhap_progress-container" style={{cursor: 'default'}} aria-label="Audio Progress Control" role="progressbar" aria-valuemin="0" aria-valuemax="100" aria-valuenow={this.state.spotifyTrackCurrentProgressInPercent} tabIndex="0">
                  <div className="rhap_progress-bar rhap_progress-bar-show-download">
                  <div className="rhap_progress-indicator" style={{left: percentage}}></div>
                  <div className="rhap_progress-filled" style={{width: percentage}}></div>
                  <div className="rhap_download-progress" style={{left: '0%', width: '100%', transitionDuration: '0'}}></div>
              </div>
          </div>
          <div className="rhap_time rhap_total-time">{this.state.spotifyTrackTotalTime}</div>
        </div>);

        var spotifyControls = (<div className="rhap_main-controls">
        <button aria-label="Previous" className="rhap_button-clear rhap_main-controls-button rhap_skip-button" type="button" onClick={() => this.previousTrack()}>
            <svg xmlns="http://www.w3.org/2000/svg" focusable="false" width="1em" height="1em" preserveAspectRatio="xMidYMid meet" viewBox="0 0 24 24" >
                <path d="M6 18V6h2v12H6m3.5-6L18 6v12l-8.5-6z" fill="currentColor"></path>
            </svg>
        </button>
        <button aria-label="Rewind" className="rhap_button-clear rhap_main-controls-button rhap_rewind-button" type="button" onClick={() => this.seekOffset(-10000)}>
            <svg xmlns="http://www.w3.org/2000/svg" focusable="false" width="1em" height="1em" preserveAspectRatio="xMidYMid meet" viewBox="0 0 24 24" >
                <path d="M11.5 12l8.5 6V6m-9 12V6l-8.5 6l8.5 6z" fill="currentColor"></path>
            </svg>
        </button>
        {this.state.playerPaused ?
          <button aria-label="Play" className="rhap_button-clear rhap_main-controls-button rhap_play-pause-button" type="button" onClick={() => this.resumeSpotify()}>
            <svg xmlns="http://www.w3.org/2000/svg" focusable="false" width="1em" height="1em" preserveAspectRatio="xMidYMid meet" viewBox="0 0 24 24">
                <path d="M10 16.5v-9l6 4.5M12 2A10 10 0 0 0 2 12a10 10 0 0 0 10 10a10 10 0 0 0 10-10A10 10 0 0 0 12 2z" fill="currentColor"></path>
            </svg>
          </button>
        :
          <button aria-label="Pause" className="rhap_button-clear rhap_main-controls-button rhap_play-pause-button" type="button" onClick={() => this.pauseSpotify()}>
            <svg xmlns="http://www.w3.org/2000/svg" focusable="false" width="1em" height="1em" preserveAspectRatio="xMidYMid meet" viewBox="0 0 24 24">
              <path d="M15 16h-2V8h2m-4 8H9V8h2m1-6A10 10 0 0 0 2 12a10 10 0 0 0 10 10a10 10 0 0 0 10-10A10 10 0 0 0 12 2z" fill="currentColor"></path>
            </svg>
          </button>
        }
        <button aria-label="Forward" className="rhap_button-clear rhap_main-controls-button rhap_forward-button" type="button" onClick={() => this.seekOffset(10000)}>
            <svg xmlns="http://www.w3.org/2000/svg" focusable="false" width="1em" height="1em" preserveAspectRatio="xMidYMid meet" viewBox="0 0 24 24" >
                <path d="M13 6v12l8.5-6M4 18l8.5-6L4 6v12z" fill="currentColor"></path>
            </svg>
        </button>
        <button aria-label="Skip" className="rhap_button-clear rhap_main-controls-button rhap_skip-button" type="button" onClick={() => this.nextTrack()}>
            <svg xmlns="http://www.w3.org/2000/svg" focusable="false" width="1em" height="1em" preserveAspectRatio="xMidYMid meet" viewBox="0 0 24 24" >
                <path d="M16 18h2V6h-2M6 18l8.5-6L6 6v12z" fill="currentColor"></path>
            </svg>
        </button>
        </div>);

      customProgressBarSection = [spotifyProgressBar];
      customControlsSection = [RHAP_UI.ADDITIONAL_CONTROLS, spotifyControls, RHAP_UI.VOLUME_CONTROLS];
      playTrackSrc = '';
    }

    let content = this.state.loading
    ? <p><em>Loading information from server, please wait...</em></p>
    : (
      this.state.albums.length > 0
      ?
      <div>
        <div className={!this.state.isCoverModalOpen ? "OverViewFadeIn" : "OverViewFadeOut"}>
        {/* <Gallery direction={"column"} columns="2" renderImage={OverviewCover} photos={this.state.albums} onClick={(event, photo) => {this.toggleCoverModal(photo.photo.albumId, photo.photo.frontCoverId, photo.photo.backCoverId)}} /> */}
          <Gallery direction={"column"} renderImage={OverviewCover} photos={this.state.albums} onClick={(event, photo) => {this.toggleCoverModal(photo.photo.albumId, photo.photo.frontCoverId, photo.photo.backCoverId)}} />
        </div> 

        <Modal
          isOpen={this.state.isCoverModalOpen}
          onRequestClose={this.hideModal}
          contentLabel="My dialog"
          overlayClassName="coverModalOverlay"
          className="coverModal"
          closeTimeoutMS={500}>
            <CoverModal albumId={this.state.albumIdForModal} 
            frontCoverId={this.state.frontCoverIdForModal} 
            backCoverId={this.state.backCoverIdForModal} 
            hideModal={this.hideModal}
            onPlay={this.play}
            onFrontCoverUpdated={this.frontCoverUpdated}
            onBackCoverUpdated={this.backCoverUpdated}
            trackIdToPlay={this.state.trackIdToPlay}/>
        </Modal>
  
        <div>
         <div style={this.footerStyle}>
              <AudioPlayer style={{backgroundColor: "transparent"}} layout="horizontal"
                  customAdditionalControls={[]}
                  src={playTrackSrc}
                  onEnded={e => this.nextTrack()}
                  onClickNext={e => this.nextTrack()}
                  onClickPrevious={e => this.previousTrack()}
                  customVolumeControls={[thumbCover, RHAP_UI.VOLUME]}
                  showSkipControls={true}
                  customProgressBarSection={customProgressBarSection}
                  customControlsSection={customControlsSection}
                  onVolumeChange={e => this.volumeChanged(e)} />
          </div>
        </div>
        {this.state.albumToPlay ?
        <ReactTooltip id="registerTip" place="top" effect="solid"
            overridePosition={(
                { left, top },
                currentEvent, currentTarget, node) => {
                left = left - 25;
                return { top, left }
            }}>
            {tooltip}
        </ReactTooltip>
        : ( <div/> )}
      </div>
      : (
        <div class="initialStartPage">
          <div class="center">
              <img src="placeholder.png" width="400px"/>
          </div>
          <div class="center">
              <p><em>No albums found...(yet)</em></p></div>
          <div class="center">
              <p><em>If you have started Covers for the first time, then it will take some time to process your music library</em></p>
          </div>
          <div class="center">
              <p><em>{this.state.processedText}</em></p>
          </div>
        </div>
      )
    );

    return (
      <div>
        {content}
      </div>
    );
  }
}