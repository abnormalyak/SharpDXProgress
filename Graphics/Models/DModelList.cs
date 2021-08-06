using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXPractice.Graphics
{
    /// <summary>
    /// Maintains information about all the models in the scene.
    /// Currently only maintains the size and color of sphere models,
    /// as that is the only model type it'll be used with for now.
    /// </summary>
    public class DModelList
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct ModelInfoType
        {
            public Vector4 color;
            public Vector3 position;
        }

        private ModelInfoType[] _modelInfoList;

        public int ModelCount { get; private set; }

        public bool Initialize(int numModels)
        {
            // Store the number of models, and create an array of that size
            ModelCount = numModels;
            _modelInfoList = new ModelInfoType[ModelCount];

            // Seed the random number generator with the current time
            Random rand = new Random(DateTime.Now.TimeOfDay.Seconds);

            // Generate the model colour and position for each model
            for (int i = 0; i < ModelCount; i++)
            {
                // Generate a random colour for the model
                float red = (float)rand.Next() / int.MaxValue;
                float green = (float)rand.Next() / int.MaxValue;
                float blue = (float)rand.Next() / int.MaxValue;
                _modelInfoList[i].color = new Vector4(red, green, blue, 1);

                // Generate a random position in front of the viewer for the model
                _modelInfoList[i].position = new Vector3(
                    (float)(rand.Next() - rand.Next()) / int.MaxValue * 10,
                    (float)(rand.Next() - rand.Next()) / int.MaxValue * 10,
                    (float)((rand.Next() - rand.Next()) / int.MaxValue * 10) + 5);
            }

            return true;
        }

        public void Shutdown()
        {
            _modelInfoList = null;
        }

        public void GetData(int index, out Vector3 position, out Vector4 color)
        {
            ModelInfoType modelInfo = _modelInfoList[index];
            position = modelInfo.position;
            color = modelInfo.color;
        }
    }
}
