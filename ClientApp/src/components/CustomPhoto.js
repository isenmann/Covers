import React from 'react';
import PropTypes from 'prop-types';

const imgWithClick = { cursor: 'pointer' };

const CustomPhoto = ({ index, onClick, photo, margin, direction, top, left, key }) => {
  const imgStyle = { /*margin: margin,*/ display: 'block' };
  if (direction === 'column') {
    imgStyle.position = 'absolute';
    imgStyle.left = left;
    imgStyle.top = top;
  }

  const handleClick = event => {
    onClick(event, { photo, index });
  };

  return (
    <div className="gallery-tile" onClick={onClick ? handleClick : null}>
        <div className="picture-info">
            <h2>text</h2>
            {/*<p>{this.props.desc}</p>*/}
        </div>
        <img
        className="tile-image"
        key={key}
        style={onClick ? { ...imgStyle, ...imgWithClick } : imgStyle}
        {...photo}
        
        />
    </div>
  );
};

export const photoPropType = PropTypes.shape({
  key: PropTypes.string,
  src: PropTypes.string.isRequired,
  width: PropTypes.number.isRequired,
  height: PropTypes.number.isRequired,
  alt: PropTypes.string,
  title: PropTypes.string,
  srcSet: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
  sizes: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
});

CustomPhoto.propTypes = {
  index: PropTypes.number.isRequired,
  onClick: PropTypes.func,
  photo: photoPropType.isRequired,
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

export default CustomPhoto;