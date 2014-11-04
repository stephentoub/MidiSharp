//----------------------------------------------------------------------- 
// <copyright file="MThdChunkHeader.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System;
using System.IO;

namespace MidiSharp.Headers
{
    /// <summary>"MThd" header for writing out MIDI files.</summary>
    internal struct MThdChunkHeader
    {
        /// <summary>Additional chunk header data.</summary>
        private readonly ChunkHeader m_header;
        /// <summary>The format for the MIDI file (0, 1, or 2).</summary>
        private readonly int m_format;
        /// <summary>The number of tracks in the MIDI sequence.</summary>
        private readonly int m_numTracks;
        /// <summary>Specifies the meaning of the delta-times</summary>
        private readonly int m_division;

        /// <summary>Initialize the MThd chunk header.</summary>
        /// <param name="format">
        /// The format for the MIDI file (0, 1, or 2).
        /// 0 - a single multi-channel track
        /// 1 - one or more simultaneous tracks
        /// 2 - one or more sequentially independent single-track patterns
        /// </param>
        /// <param name="numTracks">The number of tracks in the MIDI file.</param>
        /// <param name="division">
        /// The meaning of the delta-times in the file.
        /// If the number is zero or positive, then bits 14 thru 0 represent the number of delta-time 
        /// ticks which make up a quarter-note. If number is negative, then bits 14 through 0 represent
        /// subdivisions of a second, in a way consistent with SMPTE and MIDI time code.
        /// </param>
        public MThdChunkHeader(int format, int numTracks, int division)
        {
            // Verify the parameters
            Validate.InRange("format", format, 0, 2);
            Validate.InRange("numTracks", numTracks, 1, int.MaxValue);
            Validate.InRange("division", division, 1, int.MaxValue);

            m_header = new ChunkHeader(
                MThdID, // 0x4d546864 = "MThd"
                6);	// 2 bytes for each of the format, num tracks, and division == 6
            m_format = format;
            m_numTracks = numTracks;
            m_division = division;
        }

        /// <summary>Gets additional chunk header data.</summary>
        public ChunkHeader Header { get { return m_header; } }
        /// <summary>Gets the format for the MIDI file (0, 1, or 2).</summary>
        public int Format { get { return m_format; } }
        /// <summary>Gets the number of tracks in the MIDI sequence.</summary>
        public int NumberOfTracks { get { return m_numTracks; } }
        /// <summary>Gets the meaning of the delta-times</summary>
        public int Division { get { return m_division; } }
        /// <summary>Gets the id for an MThd header.</summary>
        private static byte[] MThdID { get { return new byte[] { 0x4d, 0x54, 0x68, 0x64 }; } }

        /// <summary>Validates that a header is correct as an MThd header.</summary>
        /// <param name="header">The header to be validated.</param>
        private static void ValidateHeader(ChunkHeader header)
        {
            byte[] validHeader = MThdID;
            for (int i = 0; i < 4; i++) {
                if (header.ID[i] != validHeader[i]) throw new InvalidOperationException("Invalid MThd header.");
            }
            if (header.Length != 6) throw new InvalidOperationException("The length of the MThd header is incorrect.");
        }

        /// <summary>Writes the MThd header out to the stream.</summary>
        /// <param name="outputStream">The stream to which the header should be written.</param>
        public void Write(Stream outputStream)
        {
            Validate.NonNull("outputStream", outputStream);

            // Write out the main header
            m_header.Write(outputStream);

            // Add format
            outputStream.WriteByte((byte)((m_format & 0xFF00) >> 8));
            outputStream.WriteByte((byte)(m_format & 0x00FF));

            // Add numTracks
            outputStream.WriteByte((byte)((m_numTracks & 0xFF00) >> 8));
            outputStream.WriteByte((byte)(m_numTracks & 0x00FF));

            // Add division
            outputStream.WriteByte((byte)((m_division & 0xFF00) >> 8));
            outputStream.WriteByte((byte)(m_division & 0x00FF));
        }

        /// <summary>Read in an MThd chunk from the stream.</summary>
        /// <param name="inputStream">The stream from which to read the MThd chunk.</param>
        /// <returns>The MThd chunk read.</returns>
        public static MThdChunkHeader Read(Stream inputStream)
        {
            Validate.NonNull("inputStream", inputStream);

            // Read in a header from the stream and validate it
            ChunkHeader header = ChunkHeader.Read(inputStream);
            ValidateHeader(header);

            // Read in the format
            int format = 0;
            for (int i = 0; i < 2; i++) {
                int val = inputStream.ReadByte();
                if (val < 0) throw new InvalidOperationException("The stream is invalid.");
                format <<= 8;
                format |= val;
            }

            // Read in the number of tracks
            int numTracks = 0;
            for (int i = 0; i < 2; i++) {
                int val = inputStream.ReadByte();
                if (val < 0) throw new InvalidOperationException("The stream is invalid.");
                numTracks <<= 8;
                numTracks |= val;
            }

            // Read in the division
            int division = 0;
            for (int i = 0; i < 2; i++) {
                int val = inputStream.ReadByte();
                if (val < 0) throw new InvalidOperationException("The stream is invalid.");
                division <<= 8;
                division |= val;
            }

            // Create a new MThd header and return it
            return new MThdChunkHeader(format, numTracks, division);
        }
    }
}
