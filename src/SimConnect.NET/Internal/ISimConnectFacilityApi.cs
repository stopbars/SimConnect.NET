// <copyright file="ISimConnectFacilityApi.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.Internal
{
    /// <summary>
    /// Abstraction over the SimConnect facility P/Invoke surface.
    /// </summary>
    internal interface ISimConnectFacilityApi
    {
        /// <summary>Adds a field to a facility definition.</summary>
        /// <param name="handle">Active SimConnect handle.</param>
        /// <param name="definitionId">Target facility definition identifier.</param>
        /// <param name="fieldName">The field path to add.</param>
        /// <returns>The raw SimConnect result.</returns>
        int AddToFacilityDefinition(IntPtr handle, uint definitionId, string fieldName);

        /// <summary>Requests facility data using a definition.</summary>
        /// <param name="handle">Active SimConnect handle.</param>
        /// <param name="definitionId">Definition identifier.</param>
        /// <param name="requestId">Client request identifier.</param>
        /// <param name="icao">Facility ICAO code.</param>
        /// <param name="region">Optional region code.</param>
        /// <returns>The raw SimConnect result.</returns>
        int RequestFacilityData(IntPtr handle, uint definitionId, uint requestId, string icao, string region);

        /// <summary>Requests a facility list using the legacy API.</summary>
        /// <param name="handle">Active SimConnect handle.</param>
        /// <param name="type">Facility list type.</param>
        /// <param name="requestId">Client request identifier.</param>
        /// <returns>The raw SimConnect result.</returns>
        int RequestFacilitiesList(IntPtr handle, uint type, uint requestId);

        /// <summary>Requests a facility list using the EX1 API.</summary>
        /// <param name="handle">Active SimConnect handle.</param>
        /// <param name="type">Facility list type.</param>
        /// <param name="requestId">Client request identifier.</param>
        /// <returns>The raw SimConnect result.</returns>
        int RequestFacilitiesListEx1(IntPtr handle, uint type, uint requestId);

        /// <summary>Subscribes to facilities entering/exiting range.</summary>
        /// <param name="handle">Active SimConnect handle.</param>
        /// <param name="type">Facility list type.</param>
        /// <param name="newInRangeRequestId">Request ID for entries entering range.</param>
        /// <param name="oldOutRangeRequestId">Request ID for entries leaving range.</param>
        /// <returns>The raw SimConnect result.</returns>
        int SubscribeToFacilitiesEx1(IntPtr handle, uint type, uint newInRangeRequestId, uint oldOutRangeRequestId);

        /// <summary>Unsubscribes from facility notifications.</summary>
        /// <param name="handle">Active SimConnect handle.</param>
        /// <param name="type">Facility list type.</param>
        /// <param name="unsubscribeNewInRange">Whether to unsubscribe from new-in-range events.</param>
        /// <param name="unsubscribeOldOutRange">Whether to unsubscribe from out-of-range events.</param>
        /// <returns>The raw SimConnect result.</returns>
        int UnsubscribeFromFacilitiesEx1(IntPtr handle, uint type, bool unsubscribeNewInRange, bool unsubscribeOldOutRange);
    }

    /// <summary>
    /// Default implementation that forwards calls to <see cref="SimConnectNative"/>.
    /// </summary>
    internal sealed class SimConnectFacilityApi : ISimConnectFacilityApi
    {
        private SimConnectFacilityApi()
        {
        }

        /// <summary>Gets the shared singleton instance.</summary>
        public static ISimConnectFacilityApi Instance { get; } = new SimConnectFacilityApi();

        /// <summary>Adds a field to a facility definition.</summary>
        /// <param name="handle">Active SimConnect handle.</param>
        /// <param name="definitionId">Target facility definition identifier.</param>
        /// <param name="fieldName">The field path to add.</param>
        /// <returns>The raw SimConnect result.</returns>
        public int AddToFacilityDefinition(IntPtr handle, uint definitionId, string fieldName)
            => SimConnectNative.SimConnect_AddToFacilityDefinition(handle, definitionId, fieldName);

        /// <summary>Requests facility data using a definition.</summary>
        /// <param name="handle">Active SimConnect handle.</param>
        /// <param name="definitionId">Definition identifier.</param>
        /// <param name="requestId">Client request identifier.</param>
        /// <param name="icao">Facility ICAO code.</param>
        /// <param name="region">Optional region code.</param>
        /// <returns>The raw SimConnect result.</returns>
        public int RequestFacilityData(IntPtr handle, uint definitionId, uint requestId, string icao, string region)
            => SimConnectNative.SimConnect_RequestFacilityData(handle, definitionId, requestId, icao, region ?? string.Empty);

        /// <summary>Requests a facility list using the legacy API.</summary>
        /// <param name="handle">Active SimConnect handle.</param>
        /// <param name="type">Facility list type.</param>
        /// <param name="requestId">Client request identifier.</param>
        /// <returns>The raw SimConnect result.</returns>
        public int RequestFacilitiesList(IntPtr handle, uint type, uint requestId)
            => SimConnectNative.SimConnect_RequestFacilitiesList(handle, type, requestId);

        /// <summary>Requests a facility list using the EX1 API.</summary>
        /// <param name="handle">Active SimConnect handle.</param>
        /// <param name="type">Facility list type.</param>
        /// <param name="requestId">Client request identifier.</param>
        /// <returns>The raw SimConnect result.</returns>
        public int RequestFacilitiesListEx1(IntPtr handle, uint type, uint requestId)
            => SimConnectNative.SimConnect_RequestFacilitiesList_EX1(handle, type, requestId);

        /// <summary>Subscribes to facilities entering/exiting range.</summary>
        /// <param name="handle">Active SimConnect handle.</param>
        /// <param name="type">Facility list type.</param>
        /// <param name="newInRangeRequestId">Request ID for entries entering range.</param>
        /// <param name="oldOutRangeRequestId">Request ID for entries leaving range.</param>
        /// <returns>The raw SimConnect result.</returns>
        public int SubscribeToFacilitiesEx1(IntPtr handle, uint type, uint newInRangeRequestId, uint oldOutRangeRequestId)
            => SimConnectNative.SimConnect_SubscribeToFacilities_EX1(handle, type, newInRangeRequestId, oldOutRangeRequestId);

        /// <summary>Unsubscribes from facility notifications.</summary>
        /// <param name="handle">Active SimConnect handle.</param>
        /// <param name="type">Facility list type.</param>
        /// <param name="unsubscribeNewInRange">Whether to unsubscribe from new-in-range events.</param>
        /// <param name="unsubscribeOldOutRange">Whether to unsubscribe from out-of-range events.</param>
        /// <returns>The raw SimConnect result.</returns>
        public int UnsubscribeFromFacilitiesEx1(IntPtr handle, uint type, bool unsubscribeNewInRange, bool unsubscribeOldOutRange)
            => SimConnectNative.SimConnect_UnsubscribeToFacilities_EX1(handle, type, unsubscribeNewInRange, unsubscribeOldOutRange);
    }
}
