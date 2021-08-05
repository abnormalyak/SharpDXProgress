using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DirectSound;
using SharpDX.Multimedia;

namespace SharpDXPractice.Sound
{
    public class DSound
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct WaveHeaderType
        {
            public string chunkId;
            public int chunkSize;
            public string format;
            public string subChunkId;
            public int subChunkSize;
            public WaveFormatEncoding audioFormat;
            public short numChannels;
            public int sampleRate;
            public int bytesPerSecond;
            public short blockAlign;
            public short bitsPerSample;
            public string dataChunkId;
            public int dataSize;
        }

        public DirectSound DirectSound { get; set; }
        private PrimarySoundBuffer PrimaryBuffer { get; set; }
        private SecondarySoundBuffer SecondaryBuffer { get; set; }
        private string AudioFileName;

        public DSound(string filename)
        {
            AudioFileName = @"C:\Users\Wing\Documents\Practice\SharpDXPractice\Sound\" + filename;
        }

        public bool Initialize(IntPtr windowHandle)
        {
            if (!InitializeDirectSound(windowHandle))
                return false;

            if (!LoadWavFile())
                return false;

            return PlayWavFile(5);
        }

        public void Shutdown()
        {
            ShutdownWavFile();

            ShutdownDirectSound();
        }

        private bool InitializeDirectSound(IntPtr windowHandle)
        {
            DirectSound = new DirectSound();

            DirectSound.SetCooperativeLevel(windowHandle, CooperativeLevel.Priority);

            SoundBufferDescription primaryBufferDesc = new SoundBufferDescription()
            {
                Flags = BufferFlags.PrimaryBuffer | BufferFlags.ControlVolume,
                AlgorithmFor3D = Guid.Empty
            };

            PrimaryBuffer = new PrimarySoundBuffer(DirectSound, primaryBufferDesc);

            PrimaryBuffer.Format = new WaveFormat(44100, 16, 2);

            return true;
        }

        private void ShutdownDirectSound()
        {
            PrimaryBuffer?.Dispose();
            PrimaryBuffer = null;

            DirectSound?.Dispose();
            DirectSound = null;
        }

        private bool LoadWavFile()
        {
            // Read in the .wav file
            BinaryReader reader = new BinaryReader(File.OpenRead(AudioFileName));
            WaveHeaderType waveHeader = new WaveHeaderType()
            {
                chunkId = new string(reader.ReadChars(4)),
                chunkSize = reader.ReadInt32(),
                format = new string(reader.ReadChars(4)),
                subChunkId = new string(reader.ReadChars(4)),
                subChunkSize = reader.ReadInt32(),
                audioFormat = (WaveFormatEncoding)reader.ReadInt16(),
                numChannels = reader.ReadInt16(),
                sampleRate = reader.ReadInt32(),
                bytesPerSecond = reader.ReadInt32(),
                blockAlign = reader.ReadInt16(),
                bitsPerSample = reader.ReadInt16(),
                dataChunkId = new string(reader.ReadChars(4)),
                dataSize = reader.ReadInt32()
            };

            // Check the .wav file is in the correct format
            if (waveHeader.chunkId != "RIFF"                        ||
                waveHeader.format != "WAVE"                         ||
                waveHeader.subChunkId.Trim() != "fmt"               ||
                waveHeader.audioFormat != WaveFormatEncoding.Pcm    ||
                waveHeader.numChannels != 2                         ||
                waveHeader.sampleRate != 44100                      ||
                waveHeader.bitsPerSample != 16                      ||
                waveHeader.dataChunkId != "data")
                return false;

            // Create the description for the secondary sound buffer
            SoundBufferDescription secondaryBufferDesc = new SoundBufferDescription()
            {
                Flags = BufferFlags.ControlVolume,
                BufferBytes = waveHeader.dataSize,
                Format = new WaveFormat(44100, 16, 2),
                AlgorithmFor3D = Guid.Empty
            };

            // Create the secondary sound buffer
            SecondaryBuffer = new SecondarySoundBuffer(DirectSound, secondaryBufferDesc);

            // Read the wav file data into the temporary buffer
            byte[] wavData = reader.ReadBytes(waveHeader.dataSize);

            // Close the file reader
            reader.Close();

            // Lock the secondary buffer to write wav data into it
            DataStream wavBufferData2;
            DataStream wavBufferData1 = SecondaryBuffer.Lock(0, waveHeader.dataSize, LockFlags.None, out wavBufferData2);

            // Copy the wav data into the buffer
            wavBufferData1.Write(wavData, 0, waveHeader.dataSize);

            // Unlock the secondary buffer
            SecondaryBuffer.Unlock(wavBufferData1, wavBufferData2);

            return true;
        }

        private void ShutdownWavFile()
        {
            SecondaryBuffer?.Dispose();
            SecondaryBuffer = null;
        }

        public bool PlayWavFile(int volume)
        {
            SecondaryBuffer.CurrentPosition = 0;

            //SecondaryBuffer.Volume = volume; // Causes crash? "parameter is incorrect"

            SecondaryBuffer.Play(0, PlayFlags.Looping);

            return true;
        }
    }
}
