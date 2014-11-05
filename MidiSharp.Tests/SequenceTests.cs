//----------------------------------------------------------------------- 
// <copyright file="SequenceTests.cs.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using Microsoft.VisualStudio.TestTools.UnitTesting;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Voice.Note;
using System.IO;
using System.Linq;

namespace MidiSharp.Tests
{
    [TestClass]
    public sealed class SequenceTests
    {
        [TestMethod]
        public void RoundtripEvents()
        {
            MidiSequence seq1 = CreateScaleSequence();

            string tmpPath = Path.GetTempFileName();
            using (var s = File.OpenWrite(tmpPath)) {
                seq1.Save(s);
            }

            MidiSequence seq2;
            using (var s = File.OpenRead(tmpPath)) {
                seq2 = MidiSequence.Open(s);
            }

            AssertAreEqual(seq1, seq2);

            File.Delete(tmpPath);
        }

        [TestMethod]
        public void CloneEvents()
        {
            MidiSequence seq1 = CreateScaleSequence();
            MidiSequence seq2 = new MidiSequence(seq1);

            Assert.AreNotSame(seq1, seq2);
            AssertAreEqual(seq1, seq2);
        }

        [TestMethod]
        public void Transpose()
        {
            for (int steps = -7; steps <= 7; steps++) {
                MidiSequence seq1 = CreateScaleSequence();
                MidiSequence seq2 = new MidiSequence(seq1);
                seq2.Transpose(steps);

                var onEvents1 = seq1.SelectMany(t => t.Events).OfType<OnNoteVoiceMidiEvent>().ToArray();
                var offEvents1 = seq1.SelectMany(t => t.Events).OfType<OffNoteVoiceMidiEvent>().ToArray();
                
                var onEvents2 = seq2.SelectMany(t => t.Events).OfType<OnNoteVoiceMidiEvent>().ToArray();
                var offEvents2 = seq2.SelectMany(t => t.Events).OfType<OffNoteVoiceMidiEvent>().ToArray();

                Assert.AreEqual(onEvents1.Length, onEvents2.Length);
                Assert.AreEqual(offEvents1.Length, offEvents2.Length);
                Assert.AreEqual(onEvents1.Length, offEvents1.Length);

                for (int i = 0; i < onEvents1.Length; i++) {
                    Assert.AreEqual(onEvents1[i].Note + steps, onEvents2[i].Note);
                    Assert.AreEqual(offEvents1[i].Note + steps, offEvents2[i].Note);
                }
            }
        }

        static MidiSequence CreateScaleSequence()
        {
            var sequence = new MidiSequence();
            var events = sequence.AddTrack().Events;

            string[] notes = new[] { "C5", "D5", "E5", "F5", "G5", "A5", "B5", "C6", "C6", "B5", "A5", "G5", "F5", "E5", "D5", "C5" };
            events.AddRange(notes.SelectMany(note => NoteVoiceMidiEvent.Complete(100, 0, note, 127, 100)));
            events.Add(new EndOfTrackMetaMidiEvent(notes.Length * 100));

            return sequence;
        }

        static void AssertAreEqual(MidiSequence sequence1, MidiSequence sequence2)
        {
            Assert.AreEqual(sequence1.Format, sequence2.Format);
            Assert.AreEqual(sequence1.Division, sequence2.Division);
            Assert.AreEqual(sequence1.DivisionType, sequence2.DivisionType);
            Assert.AreEqual(sequence1.TrackCount, sequence2.TrackCount);

            for (int i = 0; i < sequence1.TrackCount; i++) {
                AssertAreEqual(sequence1[i], sequence2[i]);
            }
        }

        static void AssertAreEqual(MidiTrack track1, MidiTrack track2)
        {
            Assert.AreEqual(track1.Events.Count, track2.Events.Count);
            for (int j = 0; j < track1.Events.Count; j++) {
                Assert.AreEqual(track1.Events[j].ToString(), track2.Events[j].ToString());
            }
        }
    }
}