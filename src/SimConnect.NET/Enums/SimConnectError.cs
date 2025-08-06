// <copyright file="SimConnectError.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the various errors that can occur in SimConnect operations.
    /// </summary>
    public enum SimConnectError
    {
        /// <summary>
        /// Specifies that there has not been an error. This value is not currently used.
        /// </summary>
        None,

        /// <summary>
        /// An unspecific error has occurred. This can be from incorrect flag settings, null or incorrect parameters, among other reasons.
        /// </summary>
        Error,

        /// <summary>
        /// Specifies the size of the data provided does not match the size required.
        /// </summary>
        SizeMismatch,

        /// <summary>
        /// Specifies that the client event, request ID, data definition ID, or object ID was not recognized.
        /// </summary>
        UnrecognizedId,

        /// <summary>
        /// Specifies that communication with the SimConnect server has not been opened. This error is not currently used.
        /// </summary>
        Unopened,

        /// <summary>
        /// Specifies a versioning error has occurred. Typically occurs when a client built on a newer version of the SimConnect client DLL attempts to work with an older version of the SimConnect server.
        /// </summary>
        VersionMismatch,

        /// <summary>
        /// Specifies that the maximum number of groups allowed has been reached. The maximum is 20.
        /// </summary>
        TooManyGroups,

        /// <summary>
        /// Specifies that the simulation event name is not recognized.
        /// </summary>
        NameUnrecognized,

        /// <summary>
        /// Specifies that the maximum number of event names allowed has been reached. The maximum is 1000.
        /// </summary>
        TooManyEventNames,

        /// <summary>
        /// Specifies that the event ID has been used already.
        /// </summary>
        EventIdDuplicate,

        /// <summary>
        /// Specifies that the maximum number of mappings allowed has been reached. The maximum is 20.
        /// </summary>
        TooManyMaps,

        /// <summary>
        /// Specifies that the maximum number of objects allowed has been reached. The maximum is 1000.
        /// </summary>
        TooManyObjects,

        /// <summary>
        /// Specifies that the maximum number of requests allowed has been reached. The maximum is 1000.
        /// </summary>
        TooManyRequests,

        /// <summary>
        /// Specifies an invalid port number was requested. This is a legacy exception and no longer used in the simulation.
        /// </summary>
        WeatherInvalidPort,

        /// <summary>
        /// Specifies that the METAR data supplied did not match the required format. This is a legacy exception and no longer used in the simulation.
        /// </summary>
        WeatherInvalidMetar,

        /// <summary>
        /// Specifies that the weather observation requested was not available. This is a legacy exception and no longer used in the simulation.
        /// </summary>
        WeatherUnableToGetObservation,

        /// <summary>
        /// Specifies that the weather station could not be created. This is a legacy exception and no longer used in the simulation.
        /// </summary>
        WeatherUnableToCreateStation,

        /// <summary>
        /// Specifies that the weather station could not be removed. This is a legacy exception and no longer used in the simulation.
        /// </summary>
        WeatherUnableToRemoveStation,

        /// <summary>
        /// Specifies that the data type requested does not apply to the type of data requested.
        /// </summary>
        InvalidDataType,

        /// <summary>
        /// Specifies that the size of the data provided is not what is expected.
        /// </summary>
        InvalidDataSize,

        /// <summary>
        /// Specifies a generic data error.
        /// </summary>
        DataError,

        /// <summary>
        /// Specifies an invalid array has been sent to the SimConnect_SetDataOnSimObject function.
        /// </summary>
        InvalidArray,

        /// <summary>
        /// Specifies that the attempt to create an AI object failed.
        /// </summary>
        CreateObjectFailed,

        /// <summary>
        /// Specifies that the specified flight plan could not be found, or did not load correctly.
        /// </summary>
        LoadFlightplanFailed,

        /// <summary>
        /// Specifies that the operation requested does not apply to the object type.
        /// </summary>
        OperationInvalidForObjectType,

        /// <summary>
        /// Specifies that the AI operation requested cannot be completed.
        /// </summary>
        IllegalOperation,

        /// <summary>
        /// Specifies that the client has already subscribed to that event.
        /// </summary>
        AlreadySubscribed,

        /// <summary>
        /// Specifies that the member of the enumeration provided was not valid.
        /// </summary>
        InvalidEnum,

        /// <summary>
        /// Specifies that there is a problem with a data definition.
        /// </summary>
        DefinitionError,

        /// <summary>
        /// Specifies that the ID has already been used.
        /// </summary>
        DuplicateId,

        /// <summary>
        /// Specifies that the datum ID is not recognized.
        /// </summary>
        DatumId,

        /// <summary>
        /// Specifies that the radius given was outside the acceptable range.
        /// </summary>
        OutOfBounds,

        /// <summary>
        /// Specifies that a client data area with the name requested has already been created by another addon.
        /// </summary>
        AlreadyCreated,

        /// <summary>
        /// Specifies that an attempt to create an ATC controlled AI object failed because the location of the object is outside the reality bubble.
        /// </summary>
        ObjectOutsideRealityBubble,

        /// <summary>
        /// Specifies that an attempt to create an AI object failed because of an error with the container system for the object.
        /// </summary>
        ObjectContainer,

        /// <summary>
        /// Specifies that an attempt to create an AI object failed because of an error with the AI system for the object.
        /// </summary>
        ObjectAi,

        /// <summary>
        /// Specifies that an attempt to create an AI object failed because of an error with the ATC system for the object.
        /// </summary>
        ObjectAtc,

        /// <summary>
        /// Specifies that an attempt to create an AI object failed because of a scheduling problem.
        /// </summary>
        ObjectSchedule,

        /// <summary>
        /// Specifies that an attempt to retrieve jetway data has caused an exception.
        /// </summary>
        JetwayData,

        /// <summary>
        /// Specifies that the given action cannot be found when using the SimConnect_ExecuteAction function.
        /// </summary>
        ActionNotFound,

        /// <summary>
        /// Specifies that the given action does not exist when using the SimConnect_ExecuteAction function.
        /// </summary>
        NotAnAction,

        /// <summary>
        /// Specifies that the wrong parameters have been given to the function SimConnect_ExecuteAction.
        /// </summary>
        IncorrectActionParams,

        /// <summary>
        /// This means that the wrong name/hash has been passed to the SimConnect_GetInputEvent function.
        /// </summary>
        GetInputEventFailed,

        /// <summary>
        /// This means that the wrong name/hash has been passed to the SimConnect_SetInputEvent function.
        /// </summary>
        SetInputEventFailed,
    }
}
