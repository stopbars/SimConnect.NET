// <copyright file="AircraftDataManager.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using SimConnect.NET.SimVar;

namespace SimConnect.NET.Aircraft
{
    /// <summary>
    /// Provides high-level access to common aircraft data and operations.
    /// </summary>
    public sealed class AircraftDataManager
    {
        private readonly SimVarManager simVarManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftDataManager"/> class.
        /// </summary>
        /// <param name="simVarManager">The SimVar manager instance.</param>
        public AircraftDataManager(SimVarManager simVarManager)
        {
            this.simVarManager = simVarManager ?? throw new ArgumentNullException(nameof(simVarManager));
        }

        /// <summary>
        /// Gets the complete aircraft position and orientation.
        /// </summary>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous get operation.</returns>
        public async Task<AircraftPosition> GetPositionAsync(uint objectId = 0, CancellationToken cancellationToken = default)
        {
            // Start all requests concurrently to minimize total latency
            var latitudeTask = this.simVarManager.GetAsync<double>("PLANE LATITUDE", "degrees", objectId, cancellationToken);
            var longitudeTask = this.simVarManager.GetAsync<double>("PLANE LONGITUDE", "degrees", objectId, cancellationToken);
            var altitudeTask = this.simVarManager.GetAsync<double>("PLANE ALTITUDE", "feet", objectId, cancellationToken);
            var altAglTask = this.simVarManager.GetAsync<double>("PLANE ALT ABOVE GROUND", "feet", objectId, cancellationToken);
            var trueHeadingTask = this.simVarManager.GetAsync<double>("PLANE HEADING DEGREES TRUE", "degrees", objectId, cancellationToken);
            var magHeadingTask = this.simVarManager.GetAsync<double>("PLANE HEADING DEGREES MAGNETIC", "degrees", objectId, cancellationToken);
            var pitchTask = this.simVarManager.GetAsync<double>("PLANE PITCH DEGREES", "degrees", objectId, cancellationToken);
            var bankTask = this.simVarManager.GetAsync<double>("PLANE BANK DEGREES", "degrees", objectId, cancellationToken);

            // Await all concurrently
            await Task.WhenAll(latitudeTask, longitudeTask, altitudeTask, altAglTask, trueHeadingTask, magHeadingTask, pitchTask, bankTask).ConfigureAwait(false);

            return new AircraftPosition
            {
                Latitude = latitudeTask.Result,
                Longitude = longitudeTask.Result,
                Altitude = altitudeTask.Result,
                AltitudeAboveGround = altAglTask.Result,
                TrueHeading = trueHeadingTask.Result,
                MagneticHeading = magHeadingTask.Result,
                Pitch = pitchTask.Result,
                Bank = bankTask.Result,
            };
        }

        /// <summary>
        /// Sets the aircraft position.
        /// </summary>
        /// <param name="position">The new position.</param>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous set operation.</returns>
        public async Task SetPositionAsync(AircraftPosition position, uint objectId = 0, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(position);

            var tasks = new[]
            {
                this.simVarManager.SetAsync("PLANE LATITUDE", "degrees", position.Latitude, objectId, cancellationToken),
                this.simVarManager.SetAsync("PLANE LONGITUDE", "degrees", position.Longitude, objectId, cancellationToken),
                this.simVarManager.SetAsync("PLANE ALTITUDE", "feet", position.Altitude, objectId, cancellationToken),
                this.simVarManager.SetAsync("PLANE HEADING DEGREES TRUE", "degrees", position.TrueHeading, objectId, cancellationToken),
                this.simVarManager.SetAsync("PLANE PITCH DEGREES", "degrees", position.Pitch, objectId, cancellationToken),
                this.simVarManager.SetAsync("PLANE BANK DEGREES", "degrees", position.Bank, objectId, cancellationToken),
            };

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the aircraft motion data.
        /// </summary>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous get operation.</returns>
        public async Task<AircraftMotion> GetMotionAsync(uint objectId = 0, CancellationToken cancellationToken = default)
        {
            // Start all requests concurrently
            var indicatedAirspeedTask = this.simVarManager.GetAsync<double>("AIRSPEED INDICATED", "knots", objectId, cancellationToken);
            var trueAirspeedTask = this.simVarManager.GetAsync<double>("AIRSPEED TRUE", "knots", objectId, cancellationToken);
            var groundVelocityTask = this.simVarManager.GetAsync<double>("GROUND VELOCITY", "knots", objectId, cancellationToken);
            var verticalSpeedTask = this.simVarManager.GetAsync<double>("VERTICAL SPEED", "feet per minute", objectId, cancellationToken);
            var gpsGroundSpeedTask = this.simVarManager.GetAsync<double>("GPS GROUND SPEED", "meters per second", objectId, cancellationToken);
            var gpsTrackTask = this.simVarManager.GetAsync<double>("GPS GROUND TRUE TRACK", "degrees", objectId, cancellationToken);

            // Await all concurrently
            await Task.WhenAll(indicatedAirspeedTask, trueAirspeedTask, groundVelocityTask, verticalSpeedTask, gpsGroundSpeedTask, gpsTrackTask).ConfigureAwait(false);

            return new AircraftMotion
            {
                IndicatedAirspeed = indicatedAirspeedTask.Result,
                TrueAirspeed = trueAirspeedTask.Result,
                GroundSpeed = groundVelocityTask.Result,
                VerticalSpeed = verticalSpeedTask.Result,
                GpsGroundSpeed = gpsGroundSpeedTask.Result,
                GpsTrack = gpsTrackTask.Result,
            };
        }

        /// <summary>
        /// Gets engine data for a specific engine.
        /// </summary>
        /// <param name="engineNumber">The engine number (1-based).</param>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous get operation.</returns>
        public async Task<AircraftEngine> GetEngineAsync(int engineNumber, uint objectId = 0, CancellationToken cancellationToken = default)
        {
            if (engineNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(engineNumber), "Engine number must be 1 or greater");
            }

            // Start all requests concurrently
            var throttleTask = this.simVarManager.GetAsync<double>($"GENERAL ENG THROTTLE LEVER POSITION:{engineNumber}", "percent", objectId, cancellationToken);
            var rpmTask = this.simVarManager.GetAsync<double>($"GENERAL ENG RPM:{engineNumber}", "rpm", objectId, cancellationToken);
            var n1Task = this.simVarManager.GetAsync<double>($"TURB ENG N1:{engineNumber}", "percent", objectId, cancellationToken);
            var n2Task = this.simVarManager.GetAsync<double>($"TURB ENG N2:{engineNumber}", "percent", objectId, cancellationToken);

            // Await all concurrently
            await Task.WhenAll(throttleTask, rpmTask, n1Task, n2Task).ConfigureAwait(false);

            return new AircraftEngine
            {
                EngineNumber = engineNumber,
                ThrottlePosition = throttleTask.Result,
                Rpm = rpmTask.Result,
                N1 = n1Task.Result,
                N2 = n2Task.Result,
                IsRunning = rpmTask.Result > 0, // Simple check based on RPM
            };
        }

        /// <summary>
        /// Sets the throttle position for a specific engine.
        /// </summary>
        /// <param name="engineNumber">The engine number (1-based).</param>
        /// <param name="throttlePercent">The throttle position as a percentage (0-100).</param>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous set operation.</returns>
        public async Task SetThrottleAsync(int engineNumber, double throttlePercent, uint objectId = 0, CancellationToken cancellationToken = default)
        {
            if (engineNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(engineNumber), "Engine number must be 1 or greater");
            }

            if (throttlePercent < 0 || throttlePercent > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(throttlePercent), "Throttle percentage must be between 0 and 100");
            }

            await this.simVarManager.SetAsync($"GENERAL ENG THROTTLE LEVER POSITION:{engineNumber}", "percent", throttlePercent, objectId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the aircraft's current latitude.
        /// </summary>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous get operation.</returns>
        public async Task<double> GetLatitudeAsync(uint objectId = 0, CancellationToken cancellationToken = default)
        {
            return await this.simVarManager.GetAsync<double>("PLANE LATITUDE", "degrees", objectId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the aircraft's current longitude.
        /// </summary>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous get operation.</returns>
        public async Task<double> GetLongitudeAsync(uint objectId = 0, CancellationToken cancellationToken = default)
        {
            return await this.simVarManager.GetAsync<double>("PLANE LONGITUDE", "degrees", objectId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the aircraft's current altitude above sea level.
        /// </summary>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous get operation.</returns>
        public async Task<double> GetAltitudeAsync(uint objectId = 0, CancellationToken cancellationToken = default)
        {
            return await this.simVarManager.GetAsync<double>("PLANE ALTITUDE", "feet", objectId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the aircraft's indicated airspeed.
        /// </summary>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous get operation.</returns>
        public async Task<double> GetIndicatedAirspeedAsync(uint objectId = 0, CancellationToken cancellationToken = default)
        {
            return await this.simVarManager.GetAsync<double>("AIRSPEED INDICATED", "knots", objectId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the aircraft's true heading.
        /// </summary>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous get operation.</returns>
        public async Task<double> GetHeadingAsync(uint objectId = 0, CancellationToken cancellationToken = default)
        {
            return await this.simVarManager.GetAsync<double>("PLANE HEADING DEGREES TRUE", "degrees", objectId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets whether the aircraft is on the ground.
        /// </summary>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous get operation.</returns>
        public async Task<bool> IsOnGroundAsync(uint objectId = 0, CancellationToken cancellationToken = default)
        {
            var result = await this.simVarManager.GetAsync<int>("SIM ON GROUND", "bool", objectId, cancellationToken).ConfigureAwait(false);
            return result != 0;
        }

        /// <summary>
        /// Gets the total fuel quantity.
        /// </summary>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous get operation.</returns>
        public async Task<double> GetFuelQuantityAsync(uint objectId = 0, CancellationToken cancellationToken = default)
        {
            return await this.simVarManager.GetAsync<double>("FUEL TOTAL QUANTITY", "gallons", objectId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the total fuel quantity.
        /// </summary>
        /// <param name="gallons">The fuel quantity in gallons.</param>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous set operation.</returns>
        public async Task SetFuelQuantityAsync(double gallons, uint objectId = 0, CancellationToken cancellationToken = default)
        {
            if (gallons < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gallons), "Fuel quantity cannot be negative");
            }

            await this.simVarManager.SetAsync("FUEL TOTAL QUANTITY", "gallons", gallons, objectId, cancellationToken).ConfigureAwait(false);
        }
    }
}
