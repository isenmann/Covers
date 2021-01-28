import React, { Component } from 'react';
import 'react-h5-audio-player/lib/styles.css';
import axios from 'axios';
import Rectangle from './Rectangle';
import Elli from './Ellipse';
import Measure from 'react-measure';
import { Stage, Layer } from 'react-konva';

export class EditorModal extends Component {
  static displayName = EditorModal.name;

  constructor(props) {
    super(props);
    this.state = { 
      albumId: props.albumId, 
      frontCoverId: props.frontCoverId, 
      backCoverId: props.backCoverId,
      albumData: [], 
      loading: true, 
      trackIdToPlay: -1,
      selectedFrontCover: null,
      selectedBackCover: null,
      dimensions: {
        width: -1,
        height: -1,
      },
    rectangles: [],
  selectedId: null,
  isRectangleAddingMode: false,
openEditor: false };
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

    // albumData.tracks.forEach(track => {
    //   album.tracks.push({
    //     key: (i++).toString(),
    //     trackId: track.trackId,
    //     number: track.number,
    //     name: track.name,
    //     artist: track.artist
    //   })
    // });

    //this.setState({ albumData: album, loading: false });
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

  onChangeFrontCoverHandler=event=>{
    const data = new FormData() ;
    data.append('albumid', this.state.albumId);
    data.append('cover', event.target.files[0]);
    axios.post("Album/FrontCover", data, {
    });
  }

  onChangeBackCoverHandler=event=>{
    const data = new FormData() ;
    data.append('albumid', this.state.albumId);
    data.append('cover', event.target.files[0]);
    axios.post("Album/BackCover", data, {
    });
  }

  checkDeselect = (e) => {
    // deselect when clicked on empty area
    const clickedOnEmpty = e.target === e.target.getStage();
    if (clickedOnEmpty) {
      this.setState({ selectedId :  null});
    }

    if(this.state.isRectangleAddingMode) {
      const newShapes = this.state.rectangles.slice();
        newShapes.push({
          x: e.evt.layerX,
          y: e.evt.layerY,
          width: 40,
          height: 40,
          fill: '#BBDEFB80',
          id: this.state.rectangles.length + 1,
          rotation: 0
        });

        this.setState({rectangles: newShapes});
    }
  };

  handleCheckboxChange = () => {
    // toggle drawing mode
    this.setState({
      isRectangleAddingMode: !this.state.isRectangleAddingMode,
    })
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : (
        <table className='table table-striped table-hover table-borderless' aria-labelledby="tabelLabel">
          <thead>
            <tr>
              <th>#</th>
              <th>Track</th>
            </tr>
          </thead>
          <tbody>
            {this.state.albumData.tracks.map(track =>
              {
                if(track.trackId === this.state.trackIdToPlay){
                return <tr className="tablerow"style={{background: 'gold'}} key={track.trackId} onClick={() => this.play(track.trackId)}>
                  <td>{track.number}</td>
                  <td>{track.name}</td>
                </tr>
                }else{
                  return <tr className="tablerow" key={track.trackId} onClick={() => this.play(track.trackId)}>
                  <td>{track.number}</td>
                  <td>{track.name}</td>
                </tr>
                }
              }
            )}
          </tbody>
        </table>
      );

    let frontCover = "placeholder.png";
    let backCover = "placeholder.png"
    if(this.state.frontCoverId > 0){
        frontCover = `Cover/${this.state.frontCoverId}`;
    }
    if(this.state.backCoverId > 0){
        backCover = `Cover/${this.state.backCoverId}`;
    }
   
    return (
      <div className="container-fluid h-100">
        
        <div className="row h-5">
            <h5>{this.state.albumData.artist} - {this.state.albumData.name}</h5>
            <svg onClick={this.props.hideModal} className="modal-close-icon" viewBox="0 0 24 24">
              <path d="M12 2C6.47 2 2 6.47 2 12s4.47 10 10 10 10-4.47 10-10S17.53 2 12 2zm5 13.59L15.59 17 12 13.41 8.41 17 7 15.59 10.59 12 7 8.41 8.41 7 12 10.59 15.59 7 17 8.41 13.41 12 17 15.59z"/>
            </svg>
        </div>

        <div className="row h-80 flexwrapOff">
          <input className="inputFile" style={{overflow: "hidden"}} type="file" name="frontCover" id="frontCover" onChange={this.onChangeFrontCoverHandler}/>
          <div className="col-6 coverImageModalDialog" 
               style={{backgroundImage: `url('${frontCover}')`}}>
              <label className="w-100 h-100 imageButton" htmlFor="frontCover"></label>
          </div>

          {/* if back cover exists then show the back cover... */}
          {this.state.backCoverId > 0 ? 
          <Measure
          bounds
          onResize={contentRect => {
            this.setState({ dimensions: contentRect.bounds });
          }}
        >
          {({ measureRef }) => (
            <div ref={measureRef} className="col-6 coverImageModalDialog" 
                style={{backgroundImage: `url('${backCover}')`}}>
                  <input type="checkbox" checked={this.state.isRectangleAddingMode} onChange={this.handleCheckboxChange}/>
                  <label>Rectangles</label>
                  <Stage
                    width={this.state.dimensions.width}
                    height={this.state.dimensions.height}
                    onMouseDown={this.checkDeselect}
                    onTouchStart={this.checkDeselect}
                    >
                      <Layer>
                          {this.state.rectangles.map((rect, i) => {
                          return (
                              <Rectangle
                              key={i}
                              shapeProps={rect}
                              isSelected={rect.id === this.state.selectedId}
                              onSelect={() => {
                                this.setState({ selectedId : rect.id});
                              }}
                              onChange={(newAttrs) => {
                                  const rects = this.state.rectangles.slice();
                                  rects[i] = newAttrs;
                                  this.setState({ rectangles : rects});
                              }}
                              />
                          );
                          })}
                      </Layer>
                   </Stage>
            </div>
            )}
            </Measure>
          :
          <div className="col-6 tracklist">
            {/* ...otherwise show the track table */}
            {contents}
          </div>
          }
          
        </div>
        <div className="row h-15">
          <div className="col-12 d-flex align-self-end">
            
          </div>
        </div>
      </div>
    );
  }
}
