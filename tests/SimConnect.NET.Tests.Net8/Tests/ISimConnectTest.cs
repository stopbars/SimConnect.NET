// <copyright file="ISimConnectTest.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

using SimConnect.NET;

namespace SimConnect.NET.Tests.Net8.Tests
{
    /// <summary>
    /// Interface for SimConnect test cases.
    /// </summary>
    public interface ISimConnectTest
    {
        /// <summary>
        /// Gets the name of the test.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of what this test validates.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the category of the test.
        /// </summary>
        string Category { get; }

        /// <summary>
        /// Runs the test asynchronously.
        /// </summary>
        /// <param name="client">The connected SimConnect client.</param>
        /// <param name="cancellationToken">Cancellation token for the test.</param>
        /// <returns>A task that represents the test result. True if passed, false if failed.</returns>
        Task<bool> RunAsync(SimConnectClient client, CancellationToken cancellationToken = default);
    }
}
