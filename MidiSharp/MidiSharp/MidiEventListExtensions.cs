//----------------------------------------------------------------------- 
// <copyright file="MidiEventListExtensions.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using MidiSharp.Events;
using System.Collections.Generic;

namespace MidiSharp
{
    internal static class MidiEventListExtensions
    {
        /// <summary>Converts the delta times on all events to from delta times to total times.</summary>
        internal static void ConvertDeltasToTotals(this List<MidiEvent> events)
        {
            long total = events[0].DeltaTime;
            for (int i = 1; i < events.Count; i++) {
                total += events[i].DeltaTime;
                events[i].DeltaTime = total;
            }
        }

        /// <summary>Converts the delta times on all events from total times back to delta times.</summary>
        internal static void ConvertTotalsToDeltas(this List<MidiEvent> events)
        {
            long lastValue = 0;
            for (int i = 0; i < events.Count; i++) {
                long tempTime = events[i].DeltaTime;
                events[i].DeltaTime -= lastValue;
                lastValue = tempTime;
            }
        }
    }
}
