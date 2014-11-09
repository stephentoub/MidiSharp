//----------------------------------------------------------------------- 
// <copyright file="CSharpMidiGenerator.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using MidiSharp.Events;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Meta.Text;
using MidiSharp.Events.Voice;
using MidiSharp.Events.Voice.Note;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MidiSharp.CodeGeneration
{
    /// <summary>Generator to render C# code to generate a given MIDI file.</summary>
    public static class CSharpCodeGenerator
    {
        /// <summary>Creates code from a MIDI sequence.</summary>
        /// <param name="sequence">The sequence for which code should be generated.</param>
        /// <param name="namespaceName">The namspace of the type to be generated.</param>
        /// <param name="typeName">The name of the type to be generated.</param>
        /// <param name="writer">The writer to which the text is written.</param>
        public static void Write(MidiSequence sequence, string namespaceName, string typeName, TextWriter writer)
        {
            Validate.NonNull("sequence", sequence);
            Validate.NonNull("namespaceName", namespaceName);
            Validate.NonNull("typeName", typeName);
            Validate.NonNull("writer", writer);

            new Generator { m_writer = writer }.Generate(sequence, namespaceName, typeName);
        }

        /// <summary>
        /// Provides the actual generation logic.  This is separated out to make it easier to share state amongst method
        /// calls via instance fields.
        /// </summary>
        sealed class Generator
        {
            /// <summary>The writer to which all output should be written.</summary>
            internal TextWriter m_writer;
            /// <summary>The current indentation level.</summary>
            int m_indentationLevel;
            /// <summary>The name of the events list to which events should be added.</summary>
            const string EventsListName = "events";

            /// <summary>Generates a C# code file that when compiled and executed will create the specified MidiSequence.</summary>
            /// <param name="sequence">The sequence to be codified.</param>
            /// <param name="namespaceName">The namespace to be used in the generated code.</param>
            /// <param name="typeName">The type name to be used in the generated code.</param>
            public void Generate(MidiSequence sequence, string namespaceName, string typeName)
            {
                string trackName = sequence.Select(t => t.TrackName).Where(s => s != null).FirstOrDefault();

                Ln("using MidiSharp;");
                Ln("using MidiSharp.Events;");
                Ln("using MidiSharp.Events.Meta;");
                Ln("using MidiSharp.Events.Meta.Text;");
                Ln("using MidiSharp.Events.Voice;");
                Ln("using MidiSharp.Events.Voice.Note;");
                Ln("using System.Collections.Generic;");
                Ln();
                Ln("namespace ", namespaceName);
                using (Braces()) {
                    Ln("/// <summary>Provides a method for creating the ", (trackName != null ? TextString(trackName) : typeName), " MIDI sequence.</summary>");
                    Ln("public class ", typeName);
                    using (Braces()) {
                        Ln("/// <summary>Creates the MIDI sequence.</summary>");
                        Ln("public static MidiSequence CreateSequence()");
                        using (Braces()) {
                            Ln("MidiSequence sequence = new MidiSequence(MidiSharp.Format.", sequence.Format, ", ", sequence.Division, ");");
                            for (int i = 0; i < sequence.Tracks.Count; i++) {
                                Ln("sequence.Tracks.Add(CreateTrack", i, "());");
                            }
                            Ln("return sequence;");
                        }
                        Ln();
                        for (int i = 0; i < sequence.Tracks.Count; i++) {
                            if (i != 0) {
                                Ln();
                            }
                            Ln("/// <summary>Creates track #", i, " in the MIDI sequence.</summary>");
                            Ln("private static MidiTrack CreateTrack", i, "()");
                            using (Braces()) {
                                Ln("MidiTrack track = new MidiTrack();");
                                Ln("MidiEventCollection ", EventsListName, " = track.Events;");
                                foreach (MidiEvent ev in sequence.Tracks[i].Events) {
                                    GenerateAddEvent(ev);
                                }
                                Ln("return track;");
                            }
                        }
                    }
                }
            }

            /// <summary>Generates a line of code to add the specified event to the "events" list.</summary>
            /// <param name="ev">The event to be added.</param>
            void GenerateAddEvent(MidiEvent ev)
            {
                string eventName = ev.GetType().Name;
                string eventParams =
                    Case<BaseTextMetaMidiEvent>(ev, e => Commas(e.DeltaTime, TextString(e.Text))) ??
                    Case<NoteVoiceMidiEvent>(ev, e =>
                        e.Channel == (byte)SpecialChannel.Percussion && Enum.IsDefined(typeof(GeneralMidiPercussion), e.Note) ?
                            Commas(e.DeltaTime, "GeneralMidiPercussion." + (GeneralMidiPercussion)e.Note, e.Parameter2) :
                            Commas(e.DeltaTime, e.Channel, "\"" + MidiEvent.GetNoteName(e.Note) + "\"", e.Parameter2)) ??
                    Case<ProgramChangeVoiceMidiEvent>(ev, e =>
                        Enum.IsDefined(typeof(GeneralMidiInstrument), e.Number) ?
                            Commas(e.DeltaTime, e.Channel, "GeneralMidiInstrument." + (GeneralMidiInstrument)e.Number) :
                            Commas(e.DeltaTime, e.Channel, e.Number)) ??
                    Case<SystemExclusiveMidiEvent>(ev, e => Commas(e.DeltaTime, ByteArrayCreationString(e.Data))) ??
                    Case<ChannelPrefixMetaMidiEvent>(ev, e => Commas(e.DeltaTime, e.Prefix)) ??
                    Case<EndOfTrackMetaMidiEvent>(ev, e => Commas(e.DeltaTime)) ??
                    Case<KeySignatureMetaMidiEvent>(ev, e => Commas(e.DeltaTime, "Key." + e.Key, "Tonality." + e.Tonality)) ??
                    Case<MidiPortMetaMidiEvent>(ev, e => Commas(e.DeltaTime, e.Port)) ??
                    Case<ProprietaryMetaMidiEvent>(ev, e => Commas(e.DeltaTime, ByteArrayCreationString(e.Data))) ??
                    Case<SequenceNumberMetaMidiEvent>(ev, e => Commas(e.DeltaTime, e.Number)) ??
                    Case<SMPTEOffsetMetaMidiEvent>(ev, e => Commas(e.DeltaTime, e.Hours, e.Minutes, e.Seconds, e.Frames, e.FractionalFrames)) ??
                    Case<TempoMetaMidiEvent>(ev, e => Commas(e.DeltaTime, e.Value)) ??
                    Case<TimeSignatureMetaMidiEvent>(ev, e => Commas(e.DeltaTime, e.Numerator, e.Denominator, e.MidiClocksPerClick, e.NumberOfNotated32nds)) ??
                    Case<UnknownMetaMidiEvent>(ev, e => Commas(e.DeltaTime, ByteArrayCreationString(e.Data))) ??
                    Case<ChannelPressureVoiceMidiEvent>(ev, e => Commas(e.DeltaTime, e.Channel, e.Pressure)) ??
                    Case<ControllerVoiceMidiEvent>(ev, e => Commas(e.DeltaTime, e.Channel, "Controller." + (Controller)e.Number, e.Value)) ??
                    Case<PitchWheelVoiceMidiEvent>(ev, e => Commas(e.DeltaTime, e.Channel, "PitchWheelStep." + (PitchWheelStep)e.Position)) ??
                    null;
                if (eventParams == null) {
                    throw new ArgumentException("Unknown MidiEvent");
                }
                Ln(EventsListName, ".Add(new ", eventName, "(", eventParams, "));");
            }

            /// <summary>Writes to the writer a line with each of the values, prefixed with the current indentation.</summary>
            /// <param name="values">The values to write.</param>
            private void Ln(params object[] values)
            {
                if (m_indentationLevel > 0) {
                    m_writer.Write(string.Concat(Enumerable.Repeat("    ", m_indentationLevel)));
                }
                foreach (object o in values) {
                    m_writer.Write(string.Format(CultureInfo.InvariantCulture, "{0}", o));
                }
                m_writer.WriteLine();
            }

            /// <summary>
            /// Outputs a line with a brace and increases the indentation level.
            /// When the returned IDisposable is disposed, a line with a closing brace will be output and the indentation level decreased.
            /// </summary>
            /// <returns>An IDisposable that will output a closing brace and decrease the indentation level.</returns>
            private EndBrace Braces()
            {
                Ln("{");
                m_indentationLevel++;
                return new EndBrace { Parent = this };
            }

            /// <summary>An IDisposable that, when disposed, will decrease the indentation level and output a closing brace.</summary>
            private struct EndBrace : IDisposable
            {
                internal Generator Parent;
                public void Dispose()
                {
                    Parent.m_indentationLevel--;
                    Parent.Ln("}");
                }
            }

            /// <summary>
            /// Checks whether the specified MidiEvent is of the specified type.
            /// If it is the func is evaluated and its resulting string is returned.
            /// Otherwise, returns null.
            /// </summary>
            /// <typeparam name="T">Specifies the type of the event desired.</typeparam>
            /// <param name="ev">The event to process.</param>
            /// <param name="func">The function to execute with the event if the event is of the specified type.</param>
            /// <returns>The result of running the function over the event if the event is of the right type; otherwise, null.</returns>
            private static string Case<T>(MidiEvent ev, Func<T, string> func) where T : MidiEvent
            {
                T castEvent = ev as T;
                return castEvent != null ? func(castEvent) : null;
            }

            /// <summary>Create a string of C# code for creating a byte array containing the specified data.</summary>
            /// <param name="data">The array of data.</param>
            /// <returns>A string of C# code for allocating a byte array containing the specified data.</returns>
            private static string ByteArrayCreationString(byte[] data)
            {
                var sb = new StringBuilder();
                sb.AppendFormat(CultureInfo.InvariantCulture, "new byte[{0}]{{", data.Length);
                for (int i = 0; i < data.Length; i++) {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", i > 0 ? "," : "", data[i]);
                }
                sb.Append("}");
                return sb.ToString();
            }

            [ThreadStatic]
            private static StringBuilder t_cachedBuilder;

            /// <summary>
            /// Create a C# string from text, escaping some characters as unicode to make sure it renders reasonably in the C# code while
            /// still resulting in the same output string.
            /// </summary>
            /// <param name="text">The text to output.</param>
            /// <returns>The text put into a string.</returns>
            private static string TextString(string text)
            {
                bool acceptable = true;
                foreach (char c in text) {
                    if (!IsValidInTextString(c)) {
                        acceptable = false;
                        break;
                    }
                }
                if (acceptable) {
                    return "\"" + text + "\"";
                }

                StringBuilder sb = t_cachedBuilder ?? (t_cachedBuilder = new StringBuilder(text.Length * 2 + 2));
                sb.Append('\"');
                foreach (char c in text) {
                    if (IsValidInTextString(c)) {
                        sb.Append(c);
                    }
                    else {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:X4}", (int)c);
                    }
                }
                sb.Append('\"');
                string result = sb.ToString();
                sb.Clear();
                return result;
            }

            /// <summary>
            /// Gets whether a character is ok to render in a C# quoted string.
            /// Letters and digits are ok.  A space is fine, but whitespace like new lines could
            /// cause problems for a string, since it's not rendered as a verbatim string.
            /// Punctuation is generally ok, but certainly punctuation has special meaning inside
            /// a C# string and is not ok.
            /// </summary>
            /// <param name="c">The character to examine.</param>
            /// <returns>true if the character is valid; otherwise, false.</returns>
            private static bool IsValidInTextString(char c)
            {
                return
                    Char.IsLetterOrDigit(c) || c == ' ' ||
                    (Char.IsPunctuation(c) && c != '\\' && c != '\"' && c != '{');
            }

            /// <summary>Creates a string of the specified parts comma-separated.</summary>
            /// <param name="values">The parts to process.</param>
            /// <returns>The comma-delimited string of values.</returns>
            private static string Commas(params object[] values)
            {
                Validate.NonNull("values", values);
                return 
                    values.Length == 0 ? string.Empty :
                    values.Length == 1 ? values[0] as string ?? string.Format(CultureInfo.InvariantCulture, "{0}", values[0]) :
                    string.Join(", ", values.Select(o => string.Format(CultureInfo.InvariantCulture, "{0}", o)));
            }
        }
    }
}