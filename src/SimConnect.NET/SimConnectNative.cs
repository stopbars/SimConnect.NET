// <copyright file="SimConnectNative.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments - ANSI marshaling is required for SimConnect

using System;
using System.Runtime.InteropServices;

namespace SimConnect.NET
{
    internal static class SimConnectNative
    {
        public delegate void SimConnectDispatchProc(IntPtr pData, uint cbData, IntPtr context);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_Open(
            out IntPtr phSimConnect,
            [MarshalAs(UnmanagedType.LPStr)] string szName,
            IntPtr hWnd,
            uint userEventWin32,
            IntPtr hEventHandle,
            uint configIndex);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_Close(IntPtr hSimConnect);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_CallDispatch(
            IntPtr hSimConnect,
            SimConnectDispatchProc callback,
            IntPtr context);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_GetNextDispatch(
            IntPtr hSimConnect,
            out IntPtr ppData,
            out uint pcbData);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RequestSystemState(
            IntPtr hSimConnect,
            uint requestId,
            [MarshalAs(UnmanagedType.LPStr)] string szState);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_MapClientEventToSimEvent(
            IntPtr hSimConnect,
            uint eventId,
            [MarshalAs(UnmanagedType.LPStr)] string eventName);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_SubscribeToSystemEvent(
            IntPtr hSimConnect,
            uint eventId,
            [MarshalAs(UnmanagedType.LPStr)] string systemEventName);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_SetSystemEventState(
            IntPtr hSimConnect,
            uint eventId,
            uint dwState);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_UnsubscribeFromSystemEvent(
            IntPtr hSimConnect,
            uint eventId);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_SetNotificationGroupPriority(
            IntPtr hSimConnect,
            uint groupId,
            uint uPriority);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RequestDataOnSimObject(
            IntPtr hSimConnect,
            uint requestId,
            uint defineId,
            uint objectId,
            uint period,
            uint flags = 0,
            uint origin = 0,
            uint interval = 0,
            uint limit = 0);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RequestDataOnSimObjectType(
            IntPtr hSimConnect,
            uint requestId,
            uint defineId,
            uint dwRadiusMeters,
            uint type);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_AddClientEventToNotificationGroup(
            IntPtr hSimConnect,
            uint groupId,
            uint eventId,
            bool bMaskable = false);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RemoveClientEvent(
            IntPtr hSimConnect,
            uint groupId,
            uint eventId);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_TransmitClientEvent(
            IntPtr hSimConnect,
            uint objectId,
            uint eventId,
            uint dwData,
            uint groupId,
            uint flags);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_TransmitClientEvent_EX1(
            IntPtr hSimConnect,
            uint objectId,
            uint eventId,
            uint groupId,
            uint flags,
            uint dwData0,
            uint dwData1,
            uint dwData2,
            uint dwData3,
            uint dwData4);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_MapClientDataNameToID(
            IntPtr hSimConnect,
            [MarshalAs(UnmanagedType.LPStr)] string szClientDataName,
            uint clientDataId);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RequestClientData(
            IntPtr hSimConnect,
            uint clientDataId,
            uint requestId,
            uint defineId,
            uint period = 0,
            uint flags = 0,
            uint origin = 0,
            uint interval = 0,
            uint limit = 0);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_CreateClientData(
            IntPtr hSimConnect,
            uint clientDataId,
            uint dwSize,
            uint flags);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_AddToClientDataDefinition(
            IntPtr hSimConnect,
            uint defineId,
            uint dwOffset,
            uint dwSizeOrType,
            float fEpsilon = 0,
            uint datumId = 0);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_AddToDataDefinition(
            IntPtr hSimConnect,
            uint defineId,
            [MarshalAs(UnmanagedType.LPStr)] string datumName,
            [MarshalAs(UnmanagedType.LPStr)] string unitsName,
            uint datumType = 4,
            float fEpsilon = 0,
            uint datumId = 0xFFFFFFFFu);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_SetClientData(
            IntPtr hSimConnect,
            uint clientDataId,
            uint defineId,
            uint flags,
            uint dwReserved,
            uint cbUnitSize,
            IntPtr pDataSet);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_SetDataOnSimObject(
            IntPtr hSimConnect,
            uint defineId,
            uint objectId,
            uint flags,
            uint arrayCount,
            uint cbUnitSize,
            IntPtr pDataSet);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_ClearClientDataDefinition(
            IntPtr hSimConnect,
            uint defineId);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_ClearDataDefinition(
            IntPtr hSimConnect,
            uint defineId);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RequestNotificationGroup(
            IntPtr hSimConnect,
            uint groupId,
            uint dwReserved = 0,
            uint flags = 0);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_ClearInputGroup(
            IntPtr hSimConnect,
            uint groupId);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_ClearNotificationGroup(
            IntPtr hSimConnect,
            uint groupId);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RequestReservedKey(
            IntPtr hSimConnect,
            uint eventId,
            [MarshalAs(UnmanagedType.LPStr)] string szKeyChoice1,
            [MarshalAs(UnmanagedType.LPStr)] string szKeyChoice2 = "",
            [MarshalAs(UnmanagedType.LPStr)] string szKeyChoice3 = "");

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_SetInputGroupPriority(
            IntPtr hSimConnect,
            uint groupId,
            uint uPriority);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_SetInputGroupState(
            IntPtr hSimConnect,
            uint groupId,
            uint dwState);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RemoveInputEvent(
            IntPtr hSimConnect,
            uint groupId,
            [MarshalAs(UnmanagedType.LPStr)] string pszInputDefinition);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_AICreateEnrouteATCAircraft(
            IntPtr hSimConnect,
            [MarshalAs(UnmanagedType.LPStr)] string szContainerTitle,
            [MarshalAs(UnmanagedType.LPStr)] string szTailNumber,
            int iFlightNumber,
            [MarshalAs(UnmanagedType.LPStr)] string szFlightPlanPath,
            double dFlightPlanPosition,
            bool bTouchAndGo,
            uint requestId);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_AICreateNonATCAircraft(
            IntPtr hSimConnect,
            [MarshalAs(UnmanagedType.LPStr)] string szContainerTitle,
            [MarshalAs(UnmanagedType.LPStr)] string szTailNumber,
            int initPos,
            uint requestId);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_AICreateParkedATCAircraft(
            IntPtr hSimConnect,
            [MarshalAs(UnmanagedType.LPStr)] string szContainerTitle,
            [MarshalAs(UnmanagedType.LPStr)] string szTailNumber,
            [MarshalAs(UnmanagedType.LPStr)] string szAirportID,
            uint requestId);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_AICreateSimulatedObject(
            IntPtr hSimConnect,
            [MarshalAs(UnmanagedType.LPStr)] string szContainerTitle,
            SimConnectDataInitPosition initPos,
            uint requestId);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_AIReleaseControl(
            IntPtr hSimConnect,
            uint objectId,
            uint requestId);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_AIRemoveObject(
            IntPtr hSimConnect,
            uint objectId,
            uint requestId);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_AISetAircraftFlightPlan(
            IntPtr hSimConnect,
            uint objectId,
            [MarshalAs(UnmanagedType.LPStr)] string szFlightPlanPath,
            uint requestId);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_FlightLoad(
            IntPtr hSimConnect,
            [MarshalAs(UnmanagedType.LPStr)] string szFileName);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_FlightSave(
            IntPtr hSimConnect,
            [MarshalAs(UnmanagedType.LPStr)] string szFileName,
            [MarshalAs(UnmanagedType.LPStr)] string szDescription,
            uint flags);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_FlightPlanLoad(
            IntPtr hSimConnect,
            [MarshalAs(UnmanagedType.LPStr)] string szFileName);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_GetLastSentPacketID(
            IntPtr hSimConnect,
            out uint pdwSendID);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RequestResponseTimes(
            IntPtr hSimConnect,
            uint nCount,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] float[] fElapsedSeconds);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_InsertString(
            IntPtr pDest,
            uint cbDest,
            ref IntPtr ppEnd,
            out uint pcbStringV,
            [MarshalAs(UnmanagedType.LPStr)] string pSource);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RetrieveString(
            IntPtr pData,
            uint cbData,
            IntPtr pStringV,
            out IntPtr ppszString,
            out uint pcbString);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_AddToFacilityDefinition(
            IntPtr hSimConnect,
            uint defineID,
            [MarshalAs(UnmanagedType.LPStr)] string fieldName);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_AddFacilityDataDefinitionFilter(
            IntPtr hSimConnect,
            uint defineID,
            [MarshalAs(UnmanagedType.LPStr)] string szFilterPath,
            uint cbFilterDataSize,
            IntPtr pFilterData);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_ClearAllFacilityDataDefinitionFilters(
            IntPtr hSimConnect,
            uint defineID);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RequestFacilitiesList(
            IntPtr hSimConnect,
            uint type,
            uint requestID);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RequestFacilitiesList_EX1(
            IntPtr hSimConnect,
            uint type,
            uint requestID);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RequestFacilityData(
            IntPtr hSimConnect,
            uint defineID,
            uint requestID,
            [MarshalAs(UnmanagedType.LPStr)] string icao,
            [MarshalAs(UnmanagedType.LPStr)] string region = "");

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RequestFacilityData_EX1(
            IntPtr hSimConnect,
            uint defineID,
            uint requestID,
            [MarshalAs(UnmanagedType.LPStr)] string icao,
            [MarshalAs(UnmanagedType.LPStr)] string region = "",
            char type = '\0');

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_RequestJetwayData(
            IntPtr hSimConnect,
            [MarshalAs(UnmanagedType.LPStr)] string szAirportIcao,
            uint dwArrayCount,
            IntPtr pIndexes);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_SubscribeToFacilities(
            IntPtr hSimConnect,
            uint type,
            uint requestID);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_SubscribeToFacilities_EX1(
            IntPtr hSimConnect,
            uint type,
            uint newElemInRangeRequestID,
            uint oldElemOutRangeRequestID);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_UnsubscribeToFacilities(
            IntPtr hSimConnect,
            uint type);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_UnsubscribeToFacilities_EX1(
            IntPtr hSimConnect,
            uint type,
            bool bUnsubscribeNewInRange,
            bool bUnsubscribeOldOutRange);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_EnumerateControllers(
            IntPtr hSimConnect);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_EnumerateInputEvents(
            IntPtr hSimConnect,
            uint requestID);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_EnumerateInputEventParams(
            IntPtr hSimConnect,
            uint requestID);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_GetInputEvent(
            IntPtr hSimConnect,
            uint requestID,
            ulong hash);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_MapInputEventToClientEvent_EX1(
            IntPtr hSimConnect,
            uint groupID,
            [MarshalAs(UnmanagedType.LPStr)] string pszInputDefinition,
            uint downEventID,
            uint downValue = 0,
            uint upEventID = uint.MaxValue, // SIMCONNECT_UNUSED
            uint upValue = 0,
            bool bMaskable = false);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_SetInputEvent(
            IntPtr hSimConnect,
            ulong hash,
            uint cbUnitSize,
            IntPtr value);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_SubscribeInputEvent(
            IntPtr hSimConnect,
            ulong hash);

        [DllImport("SimConnect.dll")]
        public static extern int SimConnect_UnsubscribeInputEvent(
            IntPtr hSimConnect,
            ulong hash);
    }
}
