// <copyright file="SimConnectRecvReservedKey.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvReservedKey structure is used with the SimConnect_RequestReservedKey function to return the reserved key combination.
    /// </summary>
    public struct SimConnectRecvReservedKey
    {
        /// <summary>
        /// Gets or sets the total size of the returned structure in bytes.
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// Gets or sets the version number of the SimConnect server.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Gets or sets the ID of the returned structure.
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Gets or sets a null-terminated string containing the key that has been reserved.
        /// This will be identical to the string entered as one of the choices for the SimConnect_RequestReservedKey function.
        /// </summary>
        public string ChoiceReserved { get; set; }

        /// <summary>
        /// Gets or sets a null-terminated string containing the reserved key combination.
        /// This will be an uppercase string containing all the modifiers that apply.
        /// For example, if the client program requests "q", and the choice is accepted, then this parameter will contain "TAB+Q".
        /// If the client program requests "Q", then this parameter will contain "SHIFT+TAB+Q".
        /// This string could then appear, for example, in a dialog from the client application, informing a user of the appropriate help key.
        /// </summary>
        public string ReservedKey { get; set; }
    }
}
