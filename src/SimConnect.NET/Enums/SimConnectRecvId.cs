// <copyright file="SimConnectRecvId.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SIMCONNECT_RECV_ID enumeration type is used within the SIMCONNECT_RECV structure to indicate which type of structure has been returned.
    /// </summary>
    public enum SimConnectRecvId
    {
        /// <summary>
        /// Specifies that nothing useful has been returned.
        /// </summary>
        Null,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EXCEPTION structure has been received.
        /// </summary>
        Exception,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_OPEN structure has been received.
        /// </summary>
        Open,

        /// <summary>
        /// Specifies that the user has exited from Microsoft Flight Simulator.
        /// </summary>
        Quit,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT structure has been received.
        /// </summary>
        Event,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_OBJECT_ADDREMOVE structure has been received.
        /// </summary>
        EventObjectAddRemove,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_FILENAME structure has been received.
        /// </summary>
        EventFilename,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_FRAME structure has been received.
        /// </summary>
        EventFrame,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_SIMOBJECT_DATA structure has been received.
        /// </summary>
        SimobjectData,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE structure has been received.
        /// </summary>
        SimobjectDataByType,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_WEATHER_OBSERVATION structure has been received.
        /// </summary>
        WeatherObservation,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_CLOUD_STATE structure has been received.
        /// </summary>
        CloudState,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_ASSIGNED_OBJECT_ID structure has been received.
        /// </summary>
        AssignedObjectId,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_RESERVED_KEY structure has been received.
        /// </summary>
        ReservedKey,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_CUSTOM_ACTION structure has been received.
        /// </summary>
        CustomAction,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_SYSTEM_STATE structure has been received.
        /// </summary>
        SystemState,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_CLIENT_DATA structure has been received.
        /// </summary>
        ClientData,

        /// <summary>
        /// Specifies that the dwData parameter will contain one value of the SIMCONNECT_WEATHER_MODE enumeration.
        /// </summary>
        EventWeatherMode,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_AIRPORT_LIST structure has been received.
        /// </summary>
        AirportList,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_VOR_LIST structure has been received.
        /// </summary>
        VorList,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_NDB_LIST structure has been received.
        /// </summary>
        NdbList,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_WAYPOINT_LIST structure has been received.
        /// </summary>
        WaypointList,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_MULTIPLAYER_SERVER_STARTED structure has been received.
        /// </summary>
        EventMultiplayerServerStarted,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_MULTIPLAYER_CLIENT_STARTED structure has been received.
        /// </summary>
        EventMultiplayerClientStarted,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_MULTIPLAYER_SESSION_ENDED structure has been received.
        /// </summary>
        EventMultiplayerSessionEnded,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_RACE_END structure has been received.
        /// </summary>
        EventRaceEnd,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_RACE_LAP structure has been received.
        /// </summary>
        EventRaceLap,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_PICK structure has been received.
        /// </summary>
        Pick,

        /// <summary>
        /// Specifies that the SIMCONNECT_RECV_EVENT_EX1 structure has been received. Can be triggered by various functions, e.g., trigger_key_event_EX1 or SimConnect_TransmitClientEvent_EX1.
        /// </summary>
        EventEx1,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_FACILITY_DATA structure has been received.
        /// </summary>
        FacilityData,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_FACILITY_DATA_END structure has been received.
        /// </summary>
        FacilityDataEnd,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_FACILITIES_MINIMAL structure has been received.
        /// </summary>
        FacilityMinimalList,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_JETWAY_DATA structure has been received.
        /// </summary>
        JetwayData,

        /// <summary>
        /// Specifies that the callback has been created by the SimConnect_EnumerateControllers function.
        /// </summary>
        ControllersList,

        /// <summary>
        /// Specifies that the callback has been created by the SimConnect_ExecuteAction function.
        /// </summary>
        ActionCallback,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_ENUMERATE_INPUT_EVENTS structure has been received.
        /// </summary>
        EnumerateInputEvents,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_GET_INPUT_EVENT structure has been received.
        /// </summary>
        GetInputEvent,

        /// <summary>
        /// Specifies that an input event has been subscribed to using the SimConnect_SubscribeInputEvent function.
        /// </summary>
        SubscribeInputEvent,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_ENUMERATE_INPUT_EVENT_PARAMS structure has been received.
        /// </summary>
        EnumerateInputEventParams,
    }
}
