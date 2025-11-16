// <copyright file="FacilityDataResponse.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace SimConnect.NET.Facilities
{
    /// <summary>
    /// Represents the result of a facility data request. Contains one or more payloads describing the
    /// requested facility and any child objects (runways, approaches, etc.).
    /// </summary>
    public sealed class FacilityDataResponse
    {
        internal FacilityDataResponse(uint requestId, IReadOnlyList<FacilityDataResult> results)
        {
            this.RequestId = requestId;
            this.Results = results;
        }

        /// <summary>
        /// Gets the client supplied request identifier.
        /// </summary>
        public uint RequestId { get; }

        /// <summary>
        /// Gets the collection of data packets returned by SimConnect.
        /// </summary>
        public IReadOnlyList<FacilityDataResult> Results { get; }

        /// <summary>
        /// Finds the first payload matching the specified <see cref="SimConnectFacilityDataType"/>.
        /// </summary>
        /// <param name="dataType">The desired data type.</param>
        /// <returns>The payload if present; otherwise <see langword="null"/>.</returns>
        public FacilityDataResult? Find(SimConnectFacilityDataType dataType)
        {
            foreach (var result in this.Results)
            {
                if (result.DataType == dataType)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
