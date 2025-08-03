// <copyright file="SimConnectFacilityDataType.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectFacilityDataType enumeration type is used within the SIMCONNECT_RECV_FACILITY_DATA return to give the type of data that is being received.
    /// </summary>
    public enum SimConnectFacilityDataType
    {
        /// <summary>Contents of the parent struct are for an airport.</summary>
        Airport,

        /// <summary>Contents of the parent struct are for a runway.</summary>
        Runway,

        /// <summary>Contents of the parent struct are for defining an airport start position.</summary>
        Start,

        /// <summary>Contents of the parent struct are for frequencies.</summary>
        Frequency,

        /// <summary>Contents of the parent struct are for a helipad.</summary>
        Helipad,

        /// <summary>Contents of the parent struct are for an approach.</summary>
        Approach,

        /// <summary>Contents of the parent struct are for an approach transition.</summary>
        ApproachTransition,

        /// <summary>Contents of the parent struct are for an approach leg.</summary>
        ApproachLeg,

        /// <summary>Contents of the parent struct are for a final approach leg.</summary>
        FinalApproachLeg,

        /// <summary>Contents of the parent struct are for a missed approach leg.</summary>
        MissedApproachLeg,

        /// <summary>Contents of the parent struct are for a departure.</summary>
        Departure,

        /// <summary>Contents of the parent struct are for an arrival.</summary>
        Arrival,

        /// <summary>Contents of the parent struct are for a runway transition.</summary>
        RunwayTransition,

        /// <summary>Contents of the parent struct are for a route transition.</summary>
        EnrouteTransition,

        /// <summary>Contents of the parent struct are for a taxiway point.</summary>
        TaxiPoint,

        /// <summary>Contents of the parent struct are for a taxiway parking spot.</summary>
        TaxiParking,

        /// <summary>Contents of the parent struct are for a taxiway path.</summary>
        TaxiPath,

        /// <summary>Contents of the parent struct are for a taxi name.</summary>
        TaxiName,

        /// <summary>Contents of the parent struct are for a jetway.</summary>
        Jetway,

        /// <summary>Contents of the parent struct are for a VOR station.</summary>
        Vor,

        /// <summary>Contents of the parent struct are for an NDB station.</summary>
        Ndb,

        /// <summary>Contents of the parent struct are for a waypoint.</summary>
        Waypoint,

        /// <summary>Contents of the parent struct are for a route.</summary>
        Route,

        /// <summary>Contents of the parent struct are for a pavement element.</summary>
        Pavement,

        /// <summary>Contents of the parent struct are for the runway approach lights.</summary>
        ApproachLights,

        /// <summary>Contents of the parent struct are for VASI information.</summary>
        Vasi,
    }
}
