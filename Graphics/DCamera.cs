using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXPractice.Graphics
{
    /// <summary>
    /// Allows DirectX to know where and how we are viewing 
    /// a scene.
    /// Tracks where the camera is and its current rotation.
    /// </summary>
    public class DCamera
    {
        #region Properties
        // Position information
        private float PosX { get; set; }
        private float PosY { get; set; }
        private float PosZ { get; set; }
        
        // Rotation information
        private float RotX { get; set; }
        private float RotY { get; set; }
        private float RotZ { get; set; }
       
        public Matrix ViewMatrix { get; private set; }
        #endregion Properties
        public DCamera() { }

        public Vector3 GetPosition()
        {
            return new Vector3(PosX, PosY, PosZ);
        }

        public void SetPosition(float x, float y, float z)
        {
            PosX = x;
            PosY = y;
            PosZ = z;
        }

        public void SetRotation(float x, float y, float z)
        {
            RotX = x;
            RotY = y;
            RotZ = z;
        }

        /// <summary>
        /// Uses the position and rotation of the camera to build
        /// and update the view matrix.
        /// </summary>
        public void Render()
        {
            float yaw, pitch, roll;
            // Set up the position of the camera in the world
            Vector3 position = new Vector3(PosX, PosY, PosZ);

            // Set up where the camera is looking
            Vector3 lookAt = new Vector3(0, 0, 1);

            // Set the yaw (Y axis), pitch (X axis), and roll (Z axis) rotations in radians
            yaw = RotY * 0.0174532925f;
            pitch = RotX * 0.0174532925f;
            roll = RotZ * 0.0174532925f;

            // Create rotation matrix from yaw, pitch, roll values
            Matrix rotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);

            // Transform the lookAt and up vector by the rotation matrix so the view is correctly rotated at the origin
            Vector3.TransformCoordinate(lookAt, rotationMatrix);
            Vector3 up = Vector3.TransformCoordinate(Vector3.UnitY, rotationMatrix);

            // Translate the rotated camera position to the location of the viewer
            lookAt = position + lookAt;

            // Create the view matrix from the three updated vectors
            ViewMatrix = Matrix.LookAtLH(position, lookAt, up);
        }
    }
}
