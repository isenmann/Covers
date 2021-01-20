import React, { Component } from 'react';
import Gallery from 'react-photo-gallery';
import Modal from 'react-modal';
import { CoverModal } from './CoverModal';

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
      if(element.coverId > 0)
      {
        coverSrc = `/Cover/${element.coverId}/front?scaled=true`;
      }

      covers.push({
        key: (i++).toString(),
        src: coverSrc,
        width: 1,
        height: 1,
        coverId: element.coverId,
        albumId: element.albumId
      })
    });

    this.setState({ albums: covers, loading: false });
  }

  openCoverModal(albumId, coverId) {
      this.setState({ 
        isCoverModalOpen: true,
        albumIdForModal: albumId,
        coverIdForModal: coverId });
  }

  hideModal = () => {
    this.setState({ 
      isCoverModalOpen: false,
      albumIdForModal: -1,
      coverIdForModal: -1 });
  };

  render () {
    return (
      <div>
        <Gallery photos={this.state.albums} onClick={(event, photo) => {this.openCoverModal(photo.photo.albumId, photo.photo.coverId)}} />;
        <Modal
          isOpen={this.state.isCoverModalOpen}
          onRequestClose={this.hideModal}
          contentLabel="My dialog"
          // className="coverModal"
          overlayClassName="coverModalOverlay"
          closeTimeoutMS={500}>
            <CoverModal albumId={this.state.albumIdForModal} coverId={this.state.coverIdForModal}/>
        </Modal>
      </div>
    );
  }
}
