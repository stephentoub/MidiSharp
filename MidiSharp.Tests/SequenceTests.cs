//----------------------------------------------------------------------- 
// <copyright file="SequenceTests.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using Microsoft.VisualStudio.TestTools.UnitTesting;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Meta.Text;
using MidiSharp.Events.Voice.Note;
using System;
using System.IO;
using System.Linq;

namespace MidiSharp.Tests
{
    [TestClass]
    public sealed class SequenceTests
    {
        [TestMethod]
        public void SequenceArgValidation()
        {
            new MidiSequence();
            new MidiSequence(new MidiSequence());
            foreach (Format format in Enum.GetValues(typeof(Format))) {
                foreach (int division in new[] { 0, 1, 100, 1000, int.MaxValue }) {
                    new MidiSequence(format, division);
                }
            }

            Utils.AssertThrows<ArgumentNullException>(() => new MidiSequence(null));
            Utils.AssertThrows<ArgumentOutOfRangeException>(() => new MidiSequence((Format)(-1), 1));
            Utils.AssertThrows<ArgumentOutOfRangeException>(() => new MidiSequence((Format)(3), 1));
            Utils.AssertThrows<ArgumentOutOfRangeException>(() => new MidiSequence(Format.One, -1));
        }

        [TestMethod]
        public void TrackArgValidation()
        {
            new MidiTrack();
            new MidiTrack(new MidiTrack());
            Utils.AssertThrows<ArgumentNullException>(() => new MidiTrack(null));
        }

        [TestMethod]
        public void RoundtripEvents1()
        {
            RoundtripEventsCore(CreateScaleSequence());
        }

        [TestMethod]
        public void RoundtripEvents2()
        {
            RoundtripEventsCore(CreateAllInclusiveSequence());
        }

        private void RoundtripEventsCore(MidiSequence seq1)
        {
            string tmpPath = Utils.SaveToTempFile(seq1);
            using (var s = File.OpenRead(tmpPath)) {
                Utils.AssertAreEqual(seq1, MidiSequence.Open(s));
            }
            File.Delete(tmpPath);
        }

        [TestMethod]
        public void CloneEvents()
        {
            MidiSequence seq1 = CreateScaleSequence();
            MidiSequence seq2 = new MidiSequence(seq1);

            Assert.AreNotSame(seq1, seq2);
            Utils.AssertAreEqual(seq1, seq2);
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
            MidiSequence sequence = new MidiSequence();
            MidiEventCollection events = sequence.Tracks.AddNewTrack().Events;

            string[] notes = new[] { "C5", "D5", "E5", "F5", "G5", "A5", "B5", "C6", "C6", "B5", "A5", "G5", "F5", "E5", "D5", "C5" };
            events.AddRange(notes.SelectMany(note => NoteVoiceMidiEvent.Complete(100, 0, note, 127, 100)));
            events.Add(new EndOfTrackMetaMidiEvent(notes.Length * 100));

            return sequence;
        }

        static MidiSequence CreateAllInclusiveSequence()
        {
            MidiSequence sequence = new MidiSequence(Format.One, 480);
            MidiTrack track1 = sequence.Tracks.AddNewTrack();
            MidiTrack track2 = sequence.Tracks.AddNewTrack();

            track1.Events.Add(new CopyrightTextMetaMidiEvent(0, "Copyright"));
            track1.Events.Add(new CuePointTextMetaMidiEvent(1, "CuePoint"));
            track1.Events.Add(new DeviceNameTextMidiEvent(2, "DeviceName"));
            track1.Events.Add(new InstrumentTextMetaMidiEvent(3, "Instrument"));
            track1.Events.Add(new LyricTextMetaMidiEvent(4, "Lyric"));
            track1.Events.Add(new MarkerTextMetaMidiEvent(5, "Marker"));
            track1.Events.Add(new ProgramNameTextMetaMidiEvent(6, "ProgramName"));
            track1.Events.Add(new SequenceTrackNameTextMetaMidiEvent(7, "SequenceTrackName"));
            track1.Events.Add(new TextMetaMidiEvent(7, "Text"));
            track1.Events.Add(new EndOfTrackMetaMidiEvent(0));

            track2.Events.Add(new ChannelPrefixMetaMidiEvent(0, 17));
            track2.Events.Add(new KeySignatureMetaMidiEvent(1, Key.Flat4, Tonality.Minor));
            track2.Events.Add(new MidiPortMetaMidiEvent(2, 1));
            track2.Events.Add(new ProprietaryMetaMidiEvent(3, new byte[] { 0, 1, 2, 3 }));
            track2.Events.Add(new SequenceNumberMetaMidiEvent(4, 123));
            track2.Events.Add(new SMPTEOffsetMetaMidiEvent(0, 1, 2, 3, 4, 5));
            track2.Events.Add(new TempoMetaMidiEvent(0, 123));
            track2.Events.Add(new TimeSignatureMetaMidiEvent(0, 1, 2, 3, 4));
            track2.Events.Add(new UnknownMetaMidiEvent(0, 123, new byte[] { 1, 2, 3 }));

            track2.Events.Add(new EndOfTrackMetaMidiEvent(0));

            // TODO: Finish adding at least one of each event type
            
            return sequence;
        }
    }
}