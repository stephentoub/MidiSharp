//----------------------------------------------------------------------- 
// <copyright file="SeparateTracks.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using MidiSharp;
using System;
using System.IO;

namespace SeparateTracks
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1) {
                Console.WriteLine("Usage: SeparateTracks.exe filename.mid");
                Console.WriteLine("    filename.mid = MIDI file for which to generate code");
                Console.WriteLine();
                return;
            }

            if (!File.Exists(args[0])) {
                Console.WriteLine("Error: file {0} not found", args[0]);
                return;
            }

            try {
                MidiSequence sequence;
                using (Stream inputStream = File.OpenRead(args[0])) {
                    sequence = MidiSequence.Open(inputStream);
                }

                for (int i = 0; i < sequence.Tracks.Count; i++) {
                    MidiSequence newSequence = new MidiSequence(Format.Zero, sequence.Division);
                    newSequence.Tracks.Add(sequence.Tracks[i]);
                    using (Stream outputStream = File.OpenWrite(args[0] + "." + i + ".mid")) {
                        newSequence.Save(outputStream);
                    }
                }
            }
            catch (Exception exc) {
                Console.Error.WriteLine("Error: {0}", exc.Message);
            }
        }
    }
}