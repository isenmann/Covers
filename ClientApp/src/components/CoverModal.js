import React, { Component } from 'react';
import AudioPlayer from 'react-h5-audio-player';
import 'react-h5-audio-player/lib/styles.css';

export class CoverModal extends Component {
  static displayName = CoverModal.name;

  constructor(props) {
    super(props);
    this.state = { albumId: props.albumId, coverId: props.coverId, albumData: [], loading: true, trackIdToPlay: -1 };
  }

  componentDidMount() {
    this.populateAlbumData();
  }

  async populateAlbumData() {
    const response = await fetch(`Album/${this.state.albumId}`);
    const albumData = await response.json();
    const album = {
        name : albumData.name,
        artist : albumData.artist,
        tracks : []
    };
    let i = 0;

    albumData.tracks.forEach(track => {
      album.tracks.push({
        key: (i++).toString(),
        trackId: track.trackId,
        number: track.number,
        name: track.name,
        artist: track.artist
      })
    });

    this.setState({ albumData: album, loading: false });
  }

  play(trackId) {
    this.setState({trackIdToPlay: trackId});
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : (
        <table className='table table-striped' aria-labelledby="tabelLabel">
          <thead>
            <tr>
              <th>Number</th>
              <th>Name</th>
            </tr>
          </thead>
          <tbody>
            {this.state.albumData.tracks.map(track =>
              <tr key={track.trackId} onClick={() => this.play(track.trackId)}>
                <td>{track.number}</td>
                <td>{track.name}</td>
              </tr>
            )}
          </tbody>
        </table>
      );

    let frontCover = "placeholder.png";
    let backCover = "placeholder.png"
    if(this.state.coverId > 0){
        frontCover = `Cover/${this.state.coverId}/front`;
        backCover = `Cover/${this.state.coverId}/back`;
    }

    return (
      <div>
          <h1 id="albumName">{this.state.albumData.name}</h1>
          <img src={frontCover}/>
          <img src={backCover}/>
          <p></p>
          <AudioPlayer
            //autoPlay
            src={`Track/${this.state.trackIdToPlay}`}
            // onPlay={e => console.log("onPlay")}
            // other props here
            />
        <h3 id="tabelLabel" >Tracks</h3>
        {contents}
      </div>
    );
  }
}
