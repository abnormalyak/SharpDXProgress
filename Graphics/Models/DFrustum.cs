using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXPractice.Graphics
{
    public class DFrustum
    {
        private Plane[] _planes = new Plane[6];
        
        /// <summary>
        /// Called every frame; calculates the matrix of the view frustum
        /// at that frame (using input params).
        /// The new frustum matrix is then used to calculate the six planes
        /// that form the view frustum.
        /// </summary>
        /// <param name="screenDepth"></param>
        /// <param name="projection"></param>
        /// <param name="view"></param>
        public void ConstructFrustum(float screenDepth, Matrix projection, Matrix view)
        {
            // Calculate the minimum Z distance in the frustum
            float zMinimum = -projection.M43 / projection.M33;
            float r = screenDepth / (screenDepth - zMinimum);
            projection.M33 = r;
            projection.M43 = -r * zMinimum;

            // Create the frustum matrix from the view matrix
            // and updated projection matrix
            Matrix matrix = view * projection;

            // Calculate near plane of frustum
            _planes[0] = new Plane(
                matrix.M14 + matrix.M13,
                matrix.M24 + matrix.M23,
                matrix.M34 + matrix.M33,
                matrix.M44 + matrix.M43);
            _planes[0].Normalize();

            // Calculate far plane of frustum
            _planes[1] = new Plane(
                matrix.M14 - matrix.M13,
                matrix.M24 - matrix.M23,
                matrix.M34 - matrix.M33,
                matrix.M44 - matrix.M43);
            _planes[0].Normalize();

            // Calculate left plane of frustum
            _planes[2] = new Plane(
                matrix.M14 + matrix.M11,
                matrix.M24 + matrix.M21,
                matrix.M34 + matrix.M31,
                matrix.M44 + matrix.M41);
            _planes[2].Normalize();

            // Calculate right plane of frustum
            _planes[3] = new Plane(
                matrix.M14 - matrix.M11,
                matrix.M24 - matrix.M21,
                matrix.M34 - matrix.M31,
                matrix.M44 - matrix.M41);
            _planes[3].Normalize();

            // Calculate top plane of frustum
            _planes[4] = new Plane(
                matrix.M14 - matrix.M12,
                matrix.M24 - matrix.M22,
                matrix.M34 - matrix.M32,
                matrix.M44 - matrix.M42);
            _planes[4].Normalize();

            // Calculate bottom plane of frustum
            _planes[5]=new Plane(
                matrix.M14 + matrix.M12,
                matrix.M24 + matrix.M22,
                matrix.M34 + matrix.M32,
                matrix.M44 + matrix.M42);
            _planes[5].Normalize();
        }

        public bool CheckSphere(Vector3 center, float radius)
        {
            // Check if the radius of the sphere is inside all six planes of the view frustum
            for (int i = 0; i < 6; i++)
            {
                if (Plane.DotCoordinate(_planes[i], center) < -radius)
                    return false;
            }

            return true;
        }
    }
}
