//----------------------------------------------------------------------- 
// <copyright file="ExtractLyrics.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using MidiSharp;
using MidiSharp.Events.Meta.Text;
using System;
using System.IO;
using System.Linq;

namespace ExtractLyrics
{
    /// <summary>
    /// Extracts and displays text from lyric events in a MIDI file.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1) {
                Console.WriteLine("Usage: ExtractLyrics.exe filename.mid");
                Console.WriteLine("    filename.mid = MIDI file from which to extract lyric events text");
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

                string[] lyrics = sequence.SelectMany(track => track.Events).OfType<LyricTextMetaMidiEvent>().Select(e => e.Text.Trim()).ToArray();
                Console.WriteLine(lyrics.Length > 0 ?
                    string.Join(" ", lyrics) :
                    "(no lyrics found)");
            }
            catch (Exception exc) {
                Console.Error.WriteLine("Error: {0}", exc.Message);
            }
        }
    }
}
