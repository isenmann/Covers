import React, { Component } from 'react';
import Gallery from 'react-photo-gallery';
import Modal from 'react-modal';
import { CoverModal } from './CoverModal';
import OverviewCover from './OverviewCover';

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
      coverIdForModal: -1 };
  }

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

  openCoverModal(albumId, frontCoverId, backCoverId) {
      this.setState({ 
        isCoverModalOpen: true,
        albumIdForModal: albumId,
        frontCoverIdForModal: frontCoverId,
        backCoverIdForModal: backCoverId});
  }

  hideModal = () => {
    this.setState({ 
      isCoverModalOpen: false,
      albumIdForModal: -1,
      frontCoverIdForModal: -1,
      backCoverIdForModal: -1});
  };

  render () {
    return (
      <div>
        <div className={!this.state.isCoverModalOpen ? "OverViewFadeIn" : "OverViewFadeOut"}>
          <Gallery renderImage={OverviewCover} photos={this.state.albums} onClick={(event, photo) => {this.openCoverModal(photo.photo.albumId, photo.photo.frontCoverId, photo.photo.backCoverId)}} />
        </div> 

        <Modal
          isOpen={this.state.isCoverModalOpen}
          onRequestClose={this.hideModal}
          contentLabel="My dialog"
          // className="coverModal"
          overlayClassName="coverModalOverlay"
          closeTimeoutMS={500}>
            <CoverModal albumId={this.state.albumIdForModal} frontCoverId={this.state.frontCoverIdForModal} backCoverId={this.state.backCoverIdForModal} hideModal={this.hideModal}/>
        </Modal>
      </div>
    );
  }
}