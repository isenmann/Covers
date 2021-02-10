import React, { Component } from 'react';
import Gallery from 'react-photo-gallery';
import Modal from 'react-modal';
import { CoverModal } from './CoverModal';
import OverviewCover from './OverviewCover';
import AudioPlayer, {RHAP_UI} from 'react-h5-audio-player';
import 'react-h5-audio-player/lib/styles.css';
import CoversService from '../services/CoversHubService'
import ReactTooltip from 'react-tooltip';

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
      albumToPlay: null,
      playerCover: "",
      processedText: "",
      spotifyToken: "" };

    CoversService.registerAlbumUpdates(() => {
        this.fetchAlbumData();
    });

    CoversService.registerProcessing((text) => {
      this.setState({processedText: text});
    });

    CoversService.registerSpotifyTokenRefresh((token) => {
      this.setState({spotifyToken: token});
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

  componentDidMount() {
    this.fetchAlbumData();
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

  play = (trackId, album) => {
    this.setState({trackIdToPlay: trackId, albumToPlay: album, playerCover: `Cover/${this.state.frontCoverIdForModal}`});
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

  nextTrack() {
    let trackArrayIndex = this.state.albumToPlay.tracks.findIndex(t => t.trackId === this.state.trackIdToPlay);
    if(this.state.albumToPlay.tracks.length > trackArrayIndex + 1){
        this.play(this.state.albumToPlay.tracks[trackArrayIndex + 1].trackId, this.state.albumToPlay);
    }
  }

  previousTrack() {
    let trackArrayIndex = this.state.albumToPlay.tracks.findIndex(t => t.trackId === this.state.trackIdToPlay);
    if(trackArrayIndex - 1 >= 0){
        this.play(this.state.albumToPlay.tracks[trackArrayIndex - 1].trackId, this.state.albumToPlay);
    }else{
      this.play(this.state.albumToPlay.tracks[trackArrayIndex].trackId, this.state.albumToPlay);
    }
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
              src={`Track/${this.state.trackIdToPlay}`}
              onEnded={e => this.nextTrack()}
              onClickNext={e => this.nextTrack()}
              onClickPrevious={e => this.previousTrack()}
              customVolumeControls={[thumbCover, RHAP_UI.VOLUME]} 
              showSkipControls={true}/>
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