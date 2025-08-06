// <copyright file="SimConnectJetwayData.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectJetwayData struct is used to return information on a single jetway.
    /// </summary>
    public struct SimConnectJetwayData
    {
        /// <summary>
        /// Gets or sets the ICAO code of the airport.
        /// </summary>
        public string AirportIcao { get; set; }

        /// <summary>
        /// Gets or sets the index of the parking space linked to this jetway.
        /// </summary>
        public int ParkingIndex { get; set; }

        /// <summary>
        /// Gets or sets the latitude, longitude, and altitude of the jetway.
        /// </summary>
        public SimConnectDataLatLonAlt Lla { get; set; }

        /// <summary>
        /// Gets or sets the pitch, bank, and heading of the jetway.
        /// </summary>
        public SimConnectDataPbh Pbh { get; set; }

        /// <summary>
        /// Gets or sets the status of the jetway.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the index of the door attached to the jetway.
        /// </summary>
        public int Door { get; set; }

        /// <summary>
        /// Gets or sets the door position relative to the aircraft.
        /// </summary>
        public SimConnectDataXyz ExitDoorRelativePos { get; set; }

        /// <summary>
        /// Gets or sets the relative position of the main handle.
        /// </summary>
        public SimConnectDataXyz MainHandlePos { get; set; }

        /// <summary>
        /// Gets or sets the relative position of the secondary handle.
        /// </summary>
        public SimConnectDataXyz SecondaryHandle { get; set; }

        /// <summary>
        /// Gets or sets the relative position of the wheel ground lock.
        /// </summary>
        public SimConnectDataXyz WheelGroundLock { get; set; }

        /// <summary>
        /// Gets or sets the object ID of the jetway.
        /// </summary>
        public uint JetwayObjectId { get; set; }

        /// <summary>
        /// Gets or sets the object ID of the object attached to the jetway.
        /// </summary>
        public uint AttachedObjectId { get; set; }
    }
}
