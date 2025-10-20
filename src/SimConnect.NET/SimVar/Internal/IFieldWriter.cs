// <copyright file="IFieldWriter.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.SimVar.Internal
{
    /// <summary>
    /// Writes a single annotated field from a struct into a contiguous unmanaged buffer.
    /// </summary>
    /// <typeparam name="T">Struct type containing SimVar-annotated fields.</typeparam>
    internal interface IFieldWriter<T>
        where T : struct
    {
    /// <summary>Gets or sets the byte offset of this field's payload within the packed buffer.</summary>
        int OffsetBytes { get; set; }

    /// <summary>Gets or sets the size in bytes of this field's payload in the packed buffer.</summary>
        int Size { get; set; }

    /// <summary>Gets or sets the effective SimConnect data type used for marshaling this field.</summary>
        SimConnectDataType DataType { get; set; }

        /// <summary>
        /// Writes the field value from the given struct into the buffer at OffsetBytes.
        /// </summary>
        /// <param name="source">Struct source value.</param>
        /// <param name="basePtr">Base pointer to the packed buffer.</param>
        void WriteFrom(in T source, IntPtr basePtr);
    }
}
