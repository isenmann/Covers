import React, { Component, useCallback } from 'react';
import AudioPlayer from 'react-h5-audio-player';
import 'react-h5-audio-player/lib/styles.css';
import Gallery from 'react-photo-gallery';

export class Home extends Component {
  static displayName = Home.name;

  constructor(props) {
    super(props);
    this.state = { albums: [], loading: true };
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
debugger
  }

  render () {
    return (
      <div>
        {/* <AudioPlayer
          autoPlay
          src="https://localhost:5001/Track/201"
          onPlay={e => console.log("onPlay")}
          // other props here
        /> */}

        <Gallery photos={this.state.albums} onClick={(event, photo) => {this.openCoverModal(photo.photo.albumId, photo.photo.coverId)}} />;
       
      </div>
    );
  }
}
