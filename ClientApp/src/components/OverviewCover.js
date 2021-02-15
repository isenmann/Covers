import React from 'react';
import PropTypes from 'prop-types';
import { LazyImage } from "react-lazy-images";

const imgWithClick = { cursor: 'pointer' };

const OverviewCover = ({ index, onClick, photo: cover, margin, direction, top, left, key }) => {
  const imgStyle = { /*margin: margin,*/ display: 'block' };
  const tilestyle = { /*margin: margin,*/ display: 'block' };

  if (direction === 'column') {
    tilestyle.position = 'absolute';
    tilestyle.left = left;
    tilestyle.top = top;
  }

  const handleClick = event => {
    onClick(event, { cover, index });
  };

  return (
    <div key={key + "tile"} style={tilestyle}>
      <div key={key} className="gallery-tile" onClick={onClick ? handleClick : null}>
          <div className="picture-info">
                <h5>{cover.albumName}</h5>
                <p>{cover.artistName}</p>
                {cover.fromSpotify ?
                  <img width="25px" src="Spotify_Icon_RGB_Green.png"></img> 
                  : <div/>
                }
          </div>
          <LazyImage
            placeholder={({ imageProps, ref }) => (
              <img ref={ref} 
              style={onClick ? { ...imgStyle, ...imgWithClick } : imgStyle}
              {...cover} 
              src={cover.placeholder} />
            )}
            actual={({ imageProps }) => (
              <img
                key={key}
                style={onClick ? { ...imgStyle, ...imgWithClick } : imgStyle}
                {...cover}
              />
            )}
          />
      </div>
    </div>
  );
};

export const coverPropType = PropTypes.shape({
  key: PropTypes.string,
  src: PropTypes.string.isRequired,
  width: PropTypes.number.isRequired,
  height: PropTypes.number.isRequired,
  alt: PropTypes.string,
  title: PropTypes.string,
  srcSet: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
  sizes: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
});

OverviewCover.propTypes = {
  index: PropTypes.number.isRequired,
  onClick: PropTypes.func,
  cover: coverPropType.isRequired,
  margin: PropTypes.number,
  top: props => {
    if (props.direction === 'column' && typeof props.top !== 'number') {
      return new Error('top is a required number when direction is set to `column`');
    }
  },
  left: props => {
    if (props.direction === 'column' && typeof props.left !== 'number') {
      return new Error('left is a required number when direction is set to `column`');
    }
  },
  direction: PropTypes.string,
};

export default OverviewCover;