//----------------------------------------------------------------------- 
// <copyright file="GenerateMidiCode.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using MidiSharp;
using MidiSharp.CodeGeneration;
using System;
using System.IO;

namespace GenerateMidiCode
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1) {
                Console.WriteLine("Usage: {0} filename.mid");
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
                CSharpCodeGenerator.Write(sequence, "GeneratedCode", "Example", Console.Out);
            }
            catch (Exception exc) {
                Console.Error.WriteLine("Error: {0}", exc.Message);
            }
        }
    }
}