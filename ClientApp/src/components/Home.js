import React, { Component } from 'react';
import Gallery from 'react-photo-gallery';
import Modal from 'react-modal';
import { CoverModal } from './CoverModal';
import OverviewCover from './OverviewCover';
import AudioPlayer, {RHAP_UI} from 'react-h5-audio-player';
import 'react-h5-audio-player/lib/styles.css';
import ReactTooltip from "react-tooltip";

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
      playerCover: "" };
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
    this.populateAlbumData();
  }

  async populateAlbumData() {
    const response = await fetch('Album/Overview');
    const data = await response.json();
    const covers = [];
    let i = 0;

    data.albums.forEach(element => {
      let coverSrc = "placeholder.png";
      if(element.frontCoverId > 0)
      {
        coverSrc = `/Cover/${element.frontCoverId}?scaled=true`;
      }

      covers.push({
        key: (i++).toString(),
        src: coverSrc,
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

  toggleCoverModal(albumId, frontCoverId, backCoverId) {
    if(this.state.isCoverModalOpen){
      this.hideModal();
    }else{
      this.setState({ 
        isCoverModalOpen: true,
        albumIdForModal: albumId,
        frontCoverIdForModal: frontCoverId,
        backCoverIdForModal: backCoverId});
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
    album.src = `/Cover/${album.frontCoverId}?scaled=true`;
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
    let thumbCover = 
        <a data-tip data-for="registerTip">
          <div className="playerThumbCover" 
            style={{backgroundImage: `url('${this.state.playerCover}')`}} 
            onClick={() => this.toggleCoverModal(this.state.albumIdForModal, this.state.frontCoverIdForModal, this.state.backCoverIdForModal)}>
      
          </div>
        </a>;

    
    let tooltip= "";
    if(this.state.albumToPlay){
      let trackArrayIndex = this.state.albumToPlay.tracks.findIndex(t => t.trackId === this.state.trackIdToPlay);
      tooltip = (
      <div className="container-fluid h-100">
        <div className="row h-100">
        <div className="col-5">
          <div className="tooltipThumbCover" 
              style={{backgroundImage: `url('${this.state.playerCover}')`}}>
            </div>
        </div>
        <div className="col-2"></div>
        <div className="col-5">
          <div className="row h-33">
            <p>{this.state.albumToPlay.artist}</p>
          </div>
          <div className="row h-33">
            <p>{this.state.albumToPlay.name}</p>
          </div>
          <div className="row h-33">
            <p>{this.state.albumToPlay.tracks[trackArrayIndex].name}</p>
          </div>
        </div>
        </div>
      </div>
      );
    }

    let content = this.state.loading
    ? <p><em>Loading information from server, please wait...</em></p>
    : (
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
        <ReactTooltip id="registerTip" place="top" effect="solid"  
                      overridePosition={ (
                          { left, top },
                          currentEvent, currentTarget, node) => {
                          left = left - 25;
                          return { top, left }}}>
          {tooltip}
        </ReactTooltip>
      </div>
      );

    return (
      <div>
        
        {content}
      </div>
    );
  }
}