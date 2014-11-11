//----------------------------------------------------------------------- 
// <copyright file="Utils.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace MidiSharp.Tests
{
    /// <summary>Additional test utilities.</summary>
    internal static class Utils
    {
        /// <summary>Saves the sequence to a temporary file and returns the path to the file.</summary>
        /// <param name="sequence">The sequence to save.</param>
        /// <returns>The path to the temporary file where the sequence was saved.</returns>
        public static string SaveToTempFile(MidiSequence sequence)
        {
            string tmpPath = Path.GetTempFileName();
            using (var s = File.OpenWrite(tmpPath)) {
                sequence.Save(s);
            }
            return tmpPath;
        }

        /// <summary>Invokes the specified action and asserts that an exception of the generic type was thrown.</summary>
        /// <typeparam name="TException">Specifies the type of exception to expect.</typeparam>
        /// <param name="action">The action to invoke.</param>
        public static void AssertThrows<TException>(Action action) where TException : Exception
        {
            try { action(); }
            catch (Exception exc) {
                if (typeof(TException).IsInstanceOfType(exc)) {
                    return;
                }
                Assert.Fail("Incorrect exception type thrown.");
            }
            Assert.Fail("No exception thrown.");
        }

        /// <summary>Asserts that two sequences have equal content.</summary>
        /// <param name="left">The first sequence.</param>
        /// <param name="right">the second sequence.</param>
        public static void AssertAreEqual(MidiSequence left, MidiSequence right)
        {
            Assert.AreEqual(left.Format, right.Format);
            Assert.AreEqual(left.Division, right.Division);
            Assert.AreEqual(left.DivisionType, right.DivisionType);
            Assert.AreEqual(left.Tracks.Count, right.Tracks.Count);

            for (int i = 0; i < left.Tracks.Count; i++) {
                AssertAreEqual(left.Tracks[i], right.Tracks[i]);
            }
        }

        /// <summary>Asserts that two tracks have equal content.</summary>
        /// <param name="left">The first track.</param>
        /// <param name="right">the second track.</param>
        public static void AssertAreEqual(MidiTrack left, MidiTrack right)
        {
            Assert.AreEqual(left.Events.Count, right.Events.Count);
            for (int j = 0; j < left.Events.Count; j++) {
                Assert.AreEqual(left.Events[j].ToString(), right.Events[j].ToString());
            }
        }

    }
}
