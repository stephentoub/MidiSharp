//----------------------------------------------------------------------- 
// <copyright file="TransposeMidi.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using MidiSharp;
using System;
using System.IO;

namespace TransposeMidi
{
    /// <summary>
    /// Reads in a MIDI sequence, transposes it by a user-provided number of half-steps, and writes the result out to a new MIDI file.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2) {
                Console.WriteLine("Usage: TransposeMidi.exe filename.mid steps");
                Console.WriteLine("    filename.mid = MIDI file to be transposed");
                Console.WriteLine("    steps = Number of steps to transpose, positive or negative");
                Console.WriteLine();
                return;
            }

            if (!File.Exists(args[0])) {
                Console.WriteLine("Error: file {0} not found", args[0]);
                return;
            }

            int steps;
            if (!int.TryParse(args[1], out steps)) {
                Console.WriteLine("Error: invalid number of steps {0}", args[1]);
                return;
            }

            try {
                MidiSequence sequence;
                using (Stream inputStream = File.OpenRead(args[0])) {
                    sequence = MidiSequence.Open(inputStream);
                }

                sequence.Transpose(steps);

                string outputPath = args[0] + ".transposed.mid";
                using (Stream outputStream = File.OpenWrite(outputPath)) {
                    sequence.Save(outputStream);
                }
                Console.WriteLine("Transposed MIDI written to: {0}", outputPath);
            }
            catch (Exception exc) {
                Console.Error.WriteLine("Error: {0}", exc.Message);
            }
        }
    }
}