import React, { Component } from 'react';
import axios from 'axios';
import Rectangle from './Rectangle';
import Elli from './Ellipse';
import Measure from 'react-measure';
import { Stage, Layer } from 'react-konva';
import Modal from 'react-modal';
import { EditorModal } from './EditorModal';
import { ColorExtractor } from 'react-color-extractor'

export class CoverModal extends Component {
  static displayName = CoverModal.name;

  constructor(props) {
    super(props);
    this.state = { 
      albumId: props.albumId, 
      frontCoverId: props.frontCoverId, 
      backCoverId: props.backCoverId,
      albumData: [],
      backgroundColors: [],
      trackIdToPlay: props.trackIdToPlay,
      loading: true,
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

  componentDidUpdate(prevProps, prevState, snapshot) {
    if (this.props.trackIdToPlay !== prevProps.trackIdToPlay) {
      this.setState({ trackIdToPlay: this.props.trackIdToPlay });
    }
  }

  componentDidMount() {
    this.populateAlbumData();
  }

  async populateAlbumData() {
    const response = await fetch(`Album/${this.state.albumId}`);
    const albumData = await response.json();
    const album = {
        albumId: albumData.albumId,
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
        artist: track.artist,
        spotifyUri: track.spotifyUri
      })
    });

    this.setState({ albumData: album, loading: false });
  }

  setFrontCoverId(coverId){
    this.setState({frontCoverId: coverId});
    this.props.onFrontCoverUpdated(this.state.albumId, coverId);
  }

  onChangeFrontCoverHandler=event=>{
    const data = new FormData() ;
    data.append('albumid', this.state.albumId);
    data.append('cover', event.target.files[0]);
    axios.post("Album/FrontCover", data, {
    }).then(response => this.setFrontCoverId(response.data.coverId));
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

  hideEditorModal = () => {
    this.setState({ 
      openEditor: false});
  };

  getColors = (colors) => {
    this.setState({backgroundColors: colors});
    console.log(colors);
  };

  render() {
    let frontCover = "placeholder.png";
    let backCover = "placeholder.png"
    if(this.state.frontCoverId > 0){
        frontCover = `Cover/${this.state.frontCoverId}`;
    }
    if(this.state.backCoverId > 0){
        backCover = `Cover/${this.state.backCoverId}`;
    }

    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : (
        <table className='table table-striped table-hover table-borderless' aria-labelledby="tabelLabel">
          <thead>
            <tr>
              <th style={{color: this.state.backgroundColors[5]}}>#</th>
              <th style={{color: this.state.backgroundColors[5]}}>Track</th>
            </tr>
          </thead>
          <tbody>
            {this.state.albumData.tracks.map(track =>
              {
                if(track.trackId === this.state.trackIdToPlay){
                return <tr className="tablerow" style={{background: this.state.backgroundColors[4]}} 
                key={track.trackId}
                 onClick={() => { this.setState({trackIdToPlay: track.trackId}); this.props.onPlay(track.trackId, track.spotifyUri, this.state.albumData)}}>
                  <td style={{color: this.state.backgroundColors[5]}}>{track.number}</td>
                  <td style={{color: this.state.backgroundColors[5]}}>{track.name}</td>
                </tr>
                }else{
                  return <tr className="tablerow" key={track.trackId} onClick={() => { this.setState({trackIdToPlay: track.trackId}); this.props.onPlay(track.trackId, track.spotifyUri, this.state.albumData)}}>
                  <td style={{color: this.state.backgroundColors[5]}}>{track.number}</td>
                  <td style={{color: this.state.backgroundColors[5]}}>{track.name}</td>
                </tr>
                }
              }
            )}
          </tbody>
        </table>
      );

    return (
      <div className="container-fluid h-100" style={{background: this.state.backgroundColors[0]}}>
        <ColorExtractor
          src={frontCover}
          getColors={this.getColors}
        />
        <svg onClick={this.props.hideModal} className="modal-close-icon" style={{fill: this.state.backgroundColors[5], stroke:this.state.backgroundColors[1]}} viewBox="0 0 24 24">
            <path d="M12 2C6.47 2 2 6.47 2 12s4.47 10 10 10 10-4.47 10-10S17.53 2 12 2zm5 13.59L15.59 17 12 13.41 8.41 17 7 15.59 10.59 12 7 8.41 8.41 7 12 10.59 15.59 7 17 8.41 13.41 12 17 15.59z"/>
        </svg>

        <div className="row h-100 flexwrapOff">
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
          <div className="col-6 tracklist" style={{paddingTop: '15px'}}>
            <div className="row" style={{paddingLeft: '15px'}}>
              <h5 style={{color: this.state.backgroundColors[5]}}>{this.state.albumData.artist} - {this.state.albumData.name}</h5> 
              {/* edit button to open the editor modal
              <svg onClick={() => { this.setState({ openEditor : true}); }} className="open-editor-icon" viewBox="0 0 24 24">
                <path d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z"/>
              </svg> */}
              
            </div>
            {/* ...otherwise show the track table */}
            {contents}
          </div>
          }
          
        </div>

        <Modal
          isOpen={this.state.openEditor}
          onRequestClose={this.hideEditorModal}
          contentLabel="My dialog"
          overlayClassName="coverModalOverlay"
          closeTimeoutMS={500}>
            <EditorModal hideModal={this.hideEditorModal}></EditorModal>
        </Modal>
      </div>
    );
  }
}
