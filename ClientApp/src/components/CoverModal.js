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

  nextTrack() {
    let trackArrayIndex = this.state.albumData.tracks.findIndex(t => t.trackId === this.state.trackIdToPlay);
    if(this.state.albumData.tracks.length > trackArrayIndex + 1){
      this.play(this.state.albumData.tracks[trackArrayIndex + 1].trackId);
    }
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : (
        <table className='table table-striped table-hover' aria-labelledby="tabelLabel">
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
      <div className="container-fluid h-100">
        <div className="row h-80 flexwrapOff">
          <div className="col-6 coverImageModalDialog" 
               style={{backgroundImage: `url('${frontCover}')`}} />
          <div className="col-6 tracklist">
            {contents}
          </div>
        </div>
        <div className="row h-20">
          <div className="col-12 d-flex align-self-end">
            <AudioPlayer layout="horizontal"
              customAdditionalControls={[]}
              src={`Track/${this.state.trackIdToPlay}`}
              onEnded={e => this.nextTrack()} />
          </div>
        </div> 
      </div>
    );
  }
}
