//----------------------------------------------------------------------- 
// <copyright file="Tonality.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

namespace MidiSharp
{
    /// <summary>The tonality of the key signature (major or minor).</summary>
    public enum Tonality : byte
    {
        /// <summary>The minimum valid value for this enum.</summary>
        MinValue = Major,
        /// <summary>Key is major.</summary>
        Major = 0,
        /// <summary>Key is minor.</summary>
        Minor = 1,
        /// <summary>The maximum valid value for this enum.</summary>
        MaxValue = Minor
    }
}