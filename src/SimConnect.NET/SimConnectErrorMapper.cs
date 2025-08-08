// <copyright file="SimConnectErrorMapper.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET
{
    /// <summary>
    /// Maps SimConnectError codes to descriptive, actionable messages.
    /// </summary>
    internal static class SimConnectErrorMapper
    {
        /// <summary>
        /// Returns a human-friendly description for a SimConnect error code.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <returns>Descriptive text suitable for logs or exceptions.</returns>
        public static string Describe(SimConnectError code)
        {
            return code switch
            {
                SimConnectError.None => "No error.",
                SimConnectError.Error => "Unspecified SimConnect error. Check parameters and connection state.",
                SimConnectError.SizeMismatch => "Provided buffer size does not match expected size.",
                SimConnectError.UnrecognizedId => "Unknown ID (event/request/definition/object). Ensure IDs are created and cached properly.",
                SimConnectError.Unopened => "SimConnect handle is not opened.",
                SimConnectError.VersionMismatch => "Client/Server version mismatch. Ensure matching SimConnect.dll and sim runtime.",
                SimConnectError.TooManyGroups => "Group limit reached (max 20). Reuse or release groups.",
                SimConnectError.NameUnrecognized => "Event name or SimVar name is unrecognized.",
                SimConnectError.TooManyEventNames => "Event name limit reached (max 1000).",
                SimConnectError.EventIdDuplicate => "Event ID already used. Use unique IDs per client.",
                SimConnectError.TooManyMaps => "Mapping limit reached (max 20).",
                SimConnectError.TooManyObjects => "Object limit reached (max 1000).",
                SimConnectError.TooManyRequests => "Request limit reached (max 1000). Throttle or reuse.",
                SimConnectError.InvalidDataType => "Invalid data type for requested operation.",
                SimConnectError.InvalidDataSize => "Invalid data size for requested operation.",
                SimConnectError.DataError => "Data error returned by SimConnect.",
                SimConnectError.InvalidArray => "Invalid array passed to SetDataOnSimObject.",
                SimConnectError.CreateObjectFailed => "Failed to create AI object (reality bubble, container, ATC or schedule).",
                SimConnectError.LoadFlightplanFailed => "Failed to load flight plan (missing or invalid).",
                SimConnectError.OperationInvalidForObjectType => "Operation invalid for object type.",
                SimConnectError.IllegalOperation => "Illegal operation. Check API usage.",
                SimConnectError.AlreadySubscribed => "Already subscribed to this event.",
                SimConnectError.InvalidEnum => "Invalid enum value provided.",
                SimConnectError.DefinitionError => "Problem with a data definition.",
                SimConnectError.DuplicateId => "ID already used. Ensure unique counters per type.",
                SimConnectError.DatumId => "Datum ID not recognized.",
                SimConnectError.OutOfBounds => "Parameter out of acceptable range.",
                SimConnectError.AlreadyCreated => "Client data area already created by another addon.",
                SimConnectError.ObjectOutsideRealityBubble => "AI object outside reality bubble.",
                SimConnectError.ObjectContainer => "AI object container system error.",
                SimConnectError.ObjectAi => "AI system error for object.",
                SimConnectError.ObjectAtc => "ATC system error for object.",
                SimConnectError.ObjectSchedule => "Scheduling problem creating AI object.",
                SimConnectError.JetwayData => "Exception while retrieving jetway data.",
                SimConnectError.ActionNotFound => "Action not found for ExecuteAction.",
                SimConnectError.NotAnAction => "Name is not an action for ExecuteAction.",
                SimConnectError.IncorrectActionParams => "Incorrect parameters for ExecuteAction.",
                SimConnectError.GetInputEventFailed => "GetInputEvent failed: wrong name or hash.",
                SimConnectError.SetInputEventFailed => "SetInputEvent failed: wrong name or hash.",
                _ => $"SimConnect error: {code}",
            };
        }

        /// <summary>
        /// Creates a SimConnectException with a mapped message and optional inner exception.
        /// </summary>
        public static SimConnectException Wrap(string operation, SimConnectError code, Exception? inner = null)
        {
            var message = $"{operation} failed: {code} - {Describe(code)}";
            return inner == null ? new SimConnectException(message, code) : new SimConnectException(message, code, inner);
        }
    }
}
