// <copyright file="SimObjectType.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET.AI
{
    /// <summary>
    /// Defines common simulation object types with their container titles.
    /// These are commonly available objects in Microsoft Flight Simulator.
    /// </summary>
    public static class SimObjectType
    {
        /// <summary>
        /// Gets all available container titles as a collection.
        /// </summary>
        /// <returns>An enumerable of all known container titles.</returns>
        public static IEnumerable<string> GetAllContainerTitles()
        {
            var types = new[]
            {
                typeof(GroundVehicles),
                typeof(GroundSupport),
                typeof(Marine),
                typeof(Aircraft),
                typeof(Special),
            };

            foreach (var type in types)
            {
                var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(string) && field.GetValue(null) is string value)
                    {
                        yield return value;
                    }
                }
            }
        }

        /// <summary>
        /// Ground vehicles.
        /// </summary>
        public static class GroundVehicles
        {
            /// <summary>
            /// Generic car object.
            /// </summary>
            public const string Car = "Car";

            /// <summary>
            /// Truck object.
            /// </summary>
            public const string Truck = "Truck";

            /// <summary>
            /// Bus object.
            /// </summary>
            public const string Bus = "Bus";

            /// <summary>
            /// Emergency vehicle.
            /// </summary>
            public const string Emergency = "Emergency";

            /// <summary>
            /// Fire truck.
            /// </summary>
            public const string FireTruck = "FireTruck";

            /// <summary>
            /// Police car.
            /// </summary>
            public const string Police = "Police";

            /// <summary>
            /// Ambulance.
            /// </summary>
            public const string Ambulance = "Ambulance";
        }

        /// <summary>
        /// Airport ground support equipment.
        /// </summary>
        public static class GroundSupport
        {
            /// <summary>
            /// Fuel truck for aircraft refueling.
            /// </summary>
            public const string FuelTruck = "FuelTruck";

            /// <summary>
            /// Baggage cart.
            /// </summary>
            public const string BaggageCart = "BaggageCart";

            /// <summary>
            /// Ground power unit.
            /// </summary>
            public const string GroundPowerUnit = "GroundPowerUnit";

            /// <summary>
            /// Aircraft pushback tug.
            /// </summary>
            public const string PushbackTug = "PushbackTug";

            /// <summary>
            /// Catering truck.
            /// </summary>
            public const string CateringTruck = "CateringTruck";

            /// <summary>
            /// Aircraft stairs.
            /// </summary>
            public const string AircraftStairs = "AircraftStairs";
        }

        /// <summary>
        /// Marine vehicles.
        /// </summary>
        public static class Marine
        {
            /// <summary>
            /// Generic boat object.
            /// </summary>
            public const string Boat = "Boat";

            /// <summary>
            /// Sailboat object.
            /// </summary>
            public const string Sailboat = "Sailboat";

            /// <summary>
            /// Yacht object.
            /// </summary>
            public const string Yacht = "Yacht";

            /// <summary>
            /// Speedboat object.
            /// </summary>
            public const string Speedboat = "Speedboat";

            /// <summary>
            /// Cargo ship.
            /// </summary>
            public const string CargoShip = "CargoShip";

            /// <summary>
            /// Cruise ship.
            /// </summary>
            public const string CruiseShip = "CruiseShip";
        }

        /// <summary>
        /// Aircraft objects (stationary display aircraft).
        /// </summary>
        public static class Aircraft
        {
            /// <summary>
            /// Generic static aircraft for display.
            /// </summary>
            public const string StaticAircraft = "StaticAircraft";

            /// <summary>
            /// Military aircraft display.
            /// </summary>
            public const string MilitaryAircraft = "MilitaryAircraft";

            /// <summary>
            /// Vintage aircraft display.
            /// </summary>
            public const string VintageAircraft = "VintageAircraft";
        }

        /// <summary>
        /// Special objects and animals.
        /// </summary>
        public static class Special
        {
            /// <summary>
            /// Hot air balloon.
            /// </summary>
            public const string HotAirBalloon = "HotAirBalloon";

            /// <summary>
            /// Humpback whale (for oceanic scenes).
            /// </summary>
            public const string HumpbackWhale = "HumpbackWhale";

            /// <summary>
            /// Wildlife - deer.
            /// </summary>
            public const string Deer = "Deer";

            /// <summary>
            /// Wildlife - bird flock.
            /// </summary>
            public const string BirdFlock = "BirdFlock";

            /// <summary>
            /// Lighthouse object.
            /// </summary>
            public const string Lighthouse = "Lighthouse";

            /// <summary>
            /// Wind turbine.
            /// </summary>
            public const string WindTurbine = "WindTurbine";
        }
    }
}
