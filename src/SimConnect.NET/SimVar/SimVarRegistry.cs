// <copyright file="SimVarRegistry.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimConnect.NET.SimVar
{
    /// <summary>
    /// Registry containing definitions for commonly used SimVars.
    /// </summary>
    public static class SimVarRegistry
    {
        private static readonly Dictionary<string, SimVarDefinition> Registry = new();
        private static readonly Dictionary<string, string> UpperCaseCache = new();

        static SimVarRegistry()
        {
            InitializeCommonSimVars();
        }

        /// <summary>
        /// Gets all registered SimVar definitions.
        /// </summary>
        public static IReadOnlyDictionary<string, SimVarDefinition> All => new ReadOnlyDictionary<string, SimVarDefinition>(Registry);

        /// <summary>
        /// Registers a new SimVar definition.
        /// </summary>
        /// <param name="definition">The SimVar definition to register.</param>
        public static void Register(SimVarDefinition definition)
        {
            ArgumentNullException.ThrowIfNull(definition);
            var upperKey = definition.Name.ToUpperInvariant();
            Registry[upperKey] = definition;
            UpperCaseCache[definition.Name] = upperKey; // Cache the conversion
        }

        /// <summary>
        /// Gets a SimVar definition by name.
        /// </summary>
        /// <param name="name">The SimVar name (case-insensitive).</param>
        /// <returns>The SimVar definition if found; otherwise null.</returns>
        public static SimVarDefinition? Get(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            // Try cache first for exact matches
            if (UpperCaseCache.TryGetValue(name, out var cachedKey))
            {
                Registry.TryGetValue(cachedKey, out var definition);
                return definition;
            }

            // Fall back to ToUpperInvariant for new strings
            var upperKey = name.ToUpperInvariant();
            UpperCaseCache[name] = upperKey; // Cache for future lookups
            Registry.TryGetValue(upperKey, out var result);
            return result;
        }

        /// <summary>
        /// Checks if a SimVar is registered.
        /// </summary>
        /// <param name="name">The SimVar name (case-insensitive).</param>
        /// <returns>True if the SimVar is registered; otherwise false.</returns>
        public static bool IsRegistered(string name)
        {
            return Get(name) != null;
        }

        private static void InitializeCommonSimVars()
        {
            // Aircraft Position & Orientation
            Register(new SimVarDefinition("PLANE LATITUDE", "degrees", SimConnectDataType.FloatDouble, true, "Aircraft latitude"));
            Register(new SimVarDefinition("PLANE LONGITUDE", "degrees", SimConnectDataType.FloatDouble, true, "Aircraft longitude"));
            Register(new SimVarDefinition("PLANE ALTITUDE", "feet", SimConnectDataType.FloatDouble, true, "Aircraft altitude above sea level"));
            Register(new SimVarDefinition("PLANE ALT ABOVE GROUND", "feet", SimConnectDataType.FloatDouble, false, "Aircraft altitude above ground"));
            Register(new SimVarDefinition("PLANE HEADING DEGREES TRUE", "degrees", SimConnectDataType.FloatDouble, true, "Aircraft true heading"));
            Register(new SimVarDefinition("PLANE HEADING DEGREES MAGNETIC", "degrees", SimConnectDataType.FloatDouble, true, "Aircraft magnetic heading"));
            Register(new SimVarDefinition("PLANE PITCH DEGREES", "degrees", SimConnectDataType.FloatDouble, true, "Aircraft pitch"));
            Register(new SimVarDefinition("PLANE BANK DEGREES", "degrees", SimConnectDataType.FloatDouble, true, "Aircraft bank"));

            // Aircraft Motion
            Register(new SimVarDefinition("AIRSPEED INDICATED", "knots", SimConnectDataType.FloatDouble, false, "Indicated airspeed"));
            Register(new SimVarDefinition("AIRSPEED TRUE", "knots", SimConnectDataType.FloatDouble, false, "True airspeed"));
            Register(new SimVarDefinition("GROUND VELOCITY", "knots", SimConnectDataType.FloatDouble, false, "Ground speed"));
            Register(new SimVarDefinition("VERTICAL SPEED", "feet per minute", SimConnectDataType.FloatDouble, false, "Vertical speed"));

            // Engine
            Register(new SimVarDefinition("ENGINE TYPE", "enum", SimConnectDataType.Integer32, false, "Engine type"));
            Register(new SimVarDefinition("NUMBER OF ENGINES", "number", SimConnectDataType.Integer32, false, "Number of engines"));
            Register(new SimVarDefinition("GENERAL ENG THROTTLE LEVER POSITION:1", "percent", SimConnectDataType.FloatDouble, true, "Engine 1 throttle position"));
            Register(new SimVarDefinition("GENERAL ENG RPM:1", "rpm", SimConnectDataType.FloatDouble, false, "Engine 1 RPM"));
            Register(new SimVarDefinition("TURB ENG N1:1", "percent", SimConnectDataType.FloatDouble, false, "Engine 1 N1"));
            Register(new SimVarDefinition("TURB ENG N2:1", "percent", SimConnectDataType.FloatDouble, false, "Engine 1 N2"));

            // Fuel
            Register(new SimVarDefinition("FUEL TOTAL QUANTITY", "gallons", SimConnectDataType.FloatDouble, true, "Total fuel quantity"));
            Register(new SimVarDefinition("FUEL TOTAL QUANTITY WEIGHT", "pounds", SimConnectDataType.FloatDouble, false, "Total fuel weight"));

            // Navigation
            Register(new SimVarDefinition("GPS GROUND SPEED", "meters per second", SimConnectDataType.FloatDouble, false, "GPS ground speed"));
            Register(new SimVarDefinition("GPS GROUND TRUE TRACK", "degrees", SimConnectDataType.FloatDouble, false, "GPS track"));
            Register(new SimVarDefinition("AUTOPILOT MASTER", "bool", SimConnectDataType.Integer32, true, "Autopilot master switch"));
            Register(new SimVarDefinition("AUTOPILOT ALTITUDE LOCK", "bool", SimConnectDataType.Integer32, true, "Autopilot altitude hold"));
            Register(new SimVarDefinition("AUTOPILOT HEADING LOCK", "bool", SimConnectDataType.Integer32, true, "Autopilot heading hold"));

            // Weather
            Register(new SimVarDefinition("AMBIENT WIND VELOCITY", "knots", SimConnectDataType.FloatDouble, false, "Wind speed"));
            Register(new SimVarDefinition("AMBIENT WIND DIRECTION", "degrees", SimConnectDataType.FloatDouble, false, "Wind direction"));
            Register(new SimVarDefinition("AMBIENT TEMPERATURE", "celsius", SimConnectDataType.FloatDouble, false, "Outside air temperature"));
            Register(new SimVarDefinition("BAROMETER PRESSURE", "millibars", SimConnectDataType.FloatDouble, false, "Barometric pressure"));

            // Aircraft Systems
            Register(new SimVarDefinition("ELECTRICAL MASTER BATTERY", "bool", SimConnectDataType.Integer32, true, "Master battery switch"));
            Register(new SimVarDefinition("GENERAL ENG MASTER ALTERNATOR", "bool", SimConnectDataType.Integer32, true, "Alternator master switch"));
            Register(new SimVarDefinition("LIGHT LANDING", "bool", SimConnectDataType.Integer32, true, "Landing lights"));
            Register(new SimVarDefinition("LIGHT STROBE", "bool", SimConnectDataType.Integer32, true, "Strobe lights"));
            Register(new SimVarDefinition("LIGHT NAV", "bool", SimConnectDataType.Integer32, true, "Navigation lights"));

            // Flight Controls
            Register(new SimVarDefinition("ELEVATOR POSITION", "position", SimConnectDataType.FloatDouble, true, "Elevator position"));
            Register(new SimVarDefinition("AILERON POSITION", "position", SimConnectDataType.FloatDouble, true, "Aileron position"));
            Register(new SimVarDefinition("RUDDER POSITION", "position", SimConnectDataType.FloatDouble, true, "Rudder position"));
            Register(new SimVarDefinition("FLAPS HANDLE PERCENT", "percent", SimConnectDataType.FloatDouble, true, "Flaps handle position"));
            Register(new SimVarDefinition("GEAR HANDLE POSITION", "bool", SimConnectDataType.Integer32, true, "Landing gear handle position"));

            // Radio
            Register(new SimVarDefinition("COM ACTIVE FREQUENCY:1", "frequency BCD16", SimConnectDataType.Integer32, true, "COM1 active frequency"));
            Register(new SimVarDefinition("COM STANDBY FREQUENCY:1", "frequency BCD16", SimConnectDataType.Integer32, true, "COM1 standby frequency"));
            Register(new SimVarDefinition("NAV ACTIVE FREQUENCY:1", "frequency BCD16", SimConnectDataType.Integer32, true, "NAV1 active frequency"));
            Register(new SimVarDefinition("NAV STANDBY FREQUENCY:1", "frequency BCD16", SimConnectDataType.Integer32, true, "NAV1 standby frequency"));

            // Simulation
            Register(new SimVarDefinition("SIM ON GROUND", "bool", SimConnectDataType.Integer32, false, "Aircraft on ground"));
            Register(new SimVarDefinition("PLANE IN PARKING STATE", "bool", SimConnectDataType.Integer32, false, "Aircraft in parking"));
            Register(new SimVarDefinition("SIMULATION RATE", "number", SimConnectDataType.Integer32, true, "Simulation rate"));
            Register(new SimVarDefinition("TITLE", "string", SimConnectDataType.String256, false, "Aircraft title"));
        }
    }
}
