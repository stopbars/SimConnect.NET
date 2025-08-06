// <copyright file="SimConnectRecvId.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
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
        Null = 0,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EXCEPTION structure has been received.
        /// </summary>
        Exception = 1,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_OPEN structure has been received.
        /// </summary>
        Open = 2,

        /// <summary>
        /// Specifies that the user has exited from Microsoft Flight Simulator.
        /// </summary>
        Quit = 3,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT structure has been received.
        /// </summary>
        Event = 4,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_OBJECT_ADDREMOVE structure has been received.
        /// </summary>
        EventObjectAddRemove = 5,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_FILENAME structure has been received.
        /// </summary>
        EventFilename = 6,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_FRAME structure has been received.
        /// </summary>
        EventFrame = 7,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_SIMOBJECT_DATA structure has been received.
        /// </summary>
        SimobjectData = 8,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE structure has been received.
        /// </summary>
        SimobjectDataByType = 9,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_WEATHER_OBSERVATION structure has been received.
        /// </summary>
        WeatherObservation = 10,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_CLOUD_STATE structure has been received.
        /// </summary>
        CloudState = 11,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_ASSIGNED_OBJECT_ID structure has been received.
        /// </summary>
        AssignedObjectId = 12,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_RESERVED_KEY structure has been received.
        /// </summary>
        ReservedKey = 13,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_CUSTOM_ACTION structure has been received.
        /// </summary>
        CustomAction = 14,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_SYSTEM_STATE structure has been received.
        /// </summary>
        SystemState = 15,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_CLIENT_DATA structure has been received.
        /// </summary>
        ClientData = 16,

        /// <summary>
        /// Specifies that the dwData parameter will contain one value of the SIMCONNECT_WEATHER_MODE enumeration.
        /// </summary>
        EventWeatherMode = 17,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_AIRPORT_LIST structure has been received.
        /// </summary>
        AirportList = 18,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_VOR_LIST structure has been received.
        /// </summary>
        VorList = 19,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_NDB_LIST structure has been received.
        /// </summary>
        NdbList = 20,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_WAYPOINT_LIST structure has been received.
        /// </summary>
        WaypointList = 21,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_MULTIPLAYER_SERVER_STARTED structure has been received.
        /// </summary>
        EventMultiplayerServerStarted = 22,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_MULTIPLAYER_CLIENT_STARTED structure has been received.
        /// </summary>
        EventMultiplayerClientStarted = 23,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_MULTIPLAYER_SESSION_ENDED structure has been received.
        /// </summary>
        EventMultiplayerSessionEnded = 24,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_RACE_END structure has been received.
        /// </summary>
        EventRaceEnd = 25,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_EVENT_RACE_LAP structure has been received.
        /// </summary>
        EventRaceLap = 26,

        /// <summary>
        /// Specifies that the SIMCONNECT_RECV_EVENT_EX1 structure has been received. Can be triggered by various functions, e.g., trigger_key_event_EX1 or SimConnect_TransmitClientEvent_EX1.
        /// </summary>
        EventEx1 = 27,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_FACILITY_DATA structure has been received.
        /// </summary>
        FacilityData = 28,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_FACILITY_DATA_END structure has been received.
        /// </summary>
        FacilityDataEnd = 29,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_FACILITIES_MINIMAL structure has been received.
        /// </summary>
        FacilityMinimalList = 30,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_JETWAY_DATA structure has been received.
        /// </summary>
        JetwayData = 31,

        /// <summary>
        /// Specifies that the callback has been created by the SimConnect_EnumerateControllers function.
        /// </summary>
        ControllersList = 32,

        /// <summary>
        /// Specifies that the callback has been created by the SimConnect_ExecuteAction function.
        /// </summary>
        ActionCallback = 33,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_ENUMERATE_INPUT_EVENTS structure has been received.
        /// </summary>
        EnumerateInputEvents = 34,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_GET_INPUT_EVENT structure has been received.
        /// </summary>
        GetInputEvent = 35,

        /// <summary>
        /// Specifies that an input event has been subscribed to using the SimConnect_SubscribeInputEvent function.
        /// </summary>
        SubscribeInputEvent = 36,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_ENUMERATE_INPUT_EVENT_PARAMS structure has been received.
        /// </summary>
        EnumerateInputEventParams = 37,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_ENUMERATE_SIMOBJECT_AND_LIVERY_LIST structure has been received.
        /// </summary>
        EnumerateSimobjectAndLiveryList = 38,

        /// <summary>
        /// Specifies that a SIMCONNECT_RECV_FLOW_EVENT structure has been received.
        /// </summary>
        FlowEvent = 39,
    }
}
