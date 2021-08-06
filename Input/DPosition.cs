using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXPractice.Input
{
    public class DPosition
    {
        private float _leftTurnSpeed, _rightTurnSpeed;

        public float FrameTime { private get; set; }
        public float RotationY { get; private set; }

        public DPosition()
        {
            RotationY = 0;
            FrameTime = 0;
            _leftTurnSpeed = _rightTurnSpeed = 0.0f;
        }

        public void TurnLeft(bool keyDown)
        {
            // If the key is pressed, increase the speed at which the camera turns left
            if (keyDown)
            {
                _leftTurnSpeed = Math.Min(_leftTurnSpeed + (FrameTime * 0.01f), 0.15f);
            }
            // Otherwise, decrease the speed
            else
            {
                _leftTurnSpeed = Math.Max(_leftTurnSpeed - (FrameTime * 0.005f), 0);
            }

            RotationY -= _leftTurnSpeed;
            RotationY = RotationY < 0 ? (RotationY + 360) : RotationY;
        }
        
        public void TurnRight(bool keyDown)
        {
            // If the key is pressed, increase the speed at which the camera turns right
            if (keyDown)
            {
                _rightTurnSpeed = Math.Min(_rightTurnSpeed + FrameTime * 0.01f, 0.15f);
            }
            // Otherwise, decrease the speed
            else
            {
                _rightTurnSpeed = Math.Max(_rightTurnSpeed - FrameTime * 0.005f, 0);
            }

            RotationY += _rightTurnSpeed;
            RotationY = RotationY > 360 ? (RotationY - 360) : RotationY;
        }
    }
}
