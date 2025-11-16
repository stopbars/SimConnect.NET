// <copyright file="FacilitySubscription.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace SimConnect.NET.Facilities
{
    /// <summary>
    /// Represents an active subscription created through <see cref="FacilityManager.SubscribeToMinimalFacilities"/>.
    /// </summary>
    public sealed class FacilitySubscription : IDisposable
    {
        private readonly FacilityManager manager;
        private readonly Action<IReadOnlyList<SimConnectFacilityMinimal>> onEntered;
        private readonly Action<IReadOnlyList<SimConnectFacilityMinimal>>? onExited;
        private bool disposed;

        internal FacilitySubscription(
            FacilityManager manager,
            SimConnectFacilityListType type,
            uint enteredRequestId,
            uint exitedRequestId,
            Action<IReadOnlyList<SimConnectFacilityMinimal>> onEntered,
            Action<IReadOnlyList<SimConnectFacilityMinimal>>? onExited)
        {
            this.manager = manager;
            this.Type = type;
            this.EnteredRequestId = enteredRequestId;
            this.ExitedRequestId = exitedRequestId;
            this.onEntered = onEntered ?? throw new ArgumentNullException(nameof(onEntered));
            this.onExited = onExited;
        }

        /// <summary>
        /// Gets the facility type represented by the subscription.
        /// </summary>
        public SimConnectFacilityListType Type { get; }

        internal uint EnteredRequestId { get; }

        internal uint ExitedRequestId { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.manager.RemoveSubscription(this);
            GC.SuppressFinalize(this);
        }

        internal void Dispatch(uint requestId, IReadOnlyList<SimConnectFacilityMinimal> facilities)
        {
            if (requestId == this.EnteredRequestId)
            {
                this.onEntered(facilities);
            }
            else if (requestId == this.ExitedRequestId)
            {
                this.onExited?.Invoke(facilities);
            }
        }
    }
}
