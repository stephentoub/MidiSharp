//----------------------------------------------------------------------- 
// <copyright file="DumpMidi.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using MidiSharp;
using System;
using System.IO;

namespace DumpMidi
{
    /// <summary>
    /// Displays the contents of a MIDI sequence in a human-readable list of events.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1) {
                Console.WriteLine("Usage: DumpMidi.exe filename.mid");
                Console.WriteLine("    filename.mid = MIDI file from which to extract lyric events text");
                Console.WriteLine();
                return;
            }

            if (!File.Exists(args[0])) {
                Console.WriteLine("Error: file {0} not found", args[0]);
                return;
            }

            try {
                using (Stream inputStream = File.OpenRead(args[0])) {
                    Console.WriteLine(MidiSequence.Open(inputStream));
                }
            }
            catch (Exception exc) {
                Console.Error.WriteLine("Error: {0}", exc.Message);
            }
        }
    }
}
