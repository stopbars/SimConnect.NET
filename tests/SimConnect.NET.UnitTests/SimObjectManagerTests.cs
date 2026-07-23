using System.Diagnostics;
using SimConnect.NET.AI;
using Xunit;

namespace SimConnect.NET.UnitTests;

/// <summary>
/// Tests AI object creation request correlation and cleanup.
/// </summary>
public sealed class SimObjectManagerTests
{
    /// <summary>
    /// Verifies a native packet ID resolves and immediately fails its different client request ID.
    /// </summary>
    /// <param name="error">The server-side object creation error to preserve.</param>
    [Theory]
    [InlineData(SimConnectError.CreateObjectFailed)]
    [InlineData(SimConnectError.ObjectContainer)]
    [InlineData(SimConnectError.ObjectAi)]
    public async Task DifferentNativeSendIdFailsCorrectPendingCreationImmediately(SimConnectError error)
    {
        const uint nativeSendId = 17;
        uint clientRequestId = 0;
        var nativeCallCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var client = new SimConnectClient("Unit test");
        using var manager = new SimObjectManager(
            client,
            (title, livery, position, requestId, registerPacketId, cancellationToken) =>
            {
                clientRequestId = requestId;
                registerPacketId(nativeSendId, requestId);
                nativeCallCompleted.SetResult();
                return Task.FromResult((int)SimConnectError.None);
            },
            TimeSpan.FromSeconds(10));

        var creation = manager.CreateObjectAsync("BARS_Light_21", default);
        await nativeCallCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.NotEqual(nativeSendId, clientRequestId);
        Assert.True(manager.TryResolveRequestId(nativeSendId, out var resolvedRequestId));
        Assert.Equal(clientRequestId, resolvedRequestId);

        var stopwatch = Stopwatch.StartNew();
        manager.ProcessObjectCreationFailed(resolvedRequestId, error, nativeSendId, 1);
        var exception = await Assert.ThrowsAsync<SimConnectException>(() => creation);

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1));
        Assert.Equal(error, exception.ErrorCode);
        Assert.Contains("BARS_Light_21", exception.Message, StringComparison.Ordinal);
        Assert.Contains("sendId=17", exception.Message, StringComparison.Ordinal);
        Assert.False(manager.TryResolveRequestId(nativeSendId, out _));
    }

    /// <summary>
    /// Verifies timeout cleanup removes the native packet mapping.
    /// </summary>
    [Fact]
    public async Task TimeoutRemovesNativePacketMapping()
    {
        const uint nativeSendId = 23;
        using var client = new SimConnectClient("Unit test");
        using var manager = new SimObjectManager(
            client,
            (title, livery, position, requestId, registerPacketId, cancellationToken) =>
            {
                registerPacketId(nativeSendId, requestId);
                return Task.FromResult((int)SimConnectError.None);
            },
            TimeSpan.FromMilliseconds(20));

        await Assert.ThrowsAsync<TimeoutException>(() => manager.CreateObjectAsync("CoffeeCup", default));

        Assert.False(manager.TryResolveRequestId(nativeSendId, out _));
    }

    /// <summary>
    /// Verifies successful creation removes the native packet mapping.
    /// </summary>
    [Fact]
    public async Task SuccessRemovesNativePacketMapping()
    {
        const uint nativeSendId = 29;
        uint clientRequestId = 0;
        var nativeCallCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var client = new SimConnectClient("Unit test");
        using var manager = new SimObjectManager(
            client,
            (title, livery, position, requestId, registerPacketId, cancellationToken) =>
            {
                clientRequestId = requestId;
                registerPacketId(nativeSendId, requestId);
                nativeCallCompleted.SetResult();
                return Task.FromResult((int)SimConnectError.None);
            },
            TimeSpan.FromSeconds(10));

        var creation = manager.CreateObjectAsync("CoffeeCup", default);
        await nativeCallCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        manager.ProcessObjectCreated(clientRequestId, 123, string.Empty, default);

        var created = await creation;
        Assert.Equal(123u, created.ObjectId);
        Assert.False(manager.TryResolveRequestId(nativeSendId, out _));
    }

    /// <summary>
    /// Verifies creation can complete when native packet correlation is unavailable.
    /// </summary>
    [Fact]
    public async Task SuccessWithoutPacketMappingStillCompletes()
    {
        uint clientRequestId = 0;
        var nativeCallCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var client = new SimConnectClient("Unit test");
        using var manager = new SimObjectManager(
            client,
            (title, livery, position, requestId, registerPacketId, cancellationToken) =>
            {
                clientRequestId = requestId;
                nativeCallCompleted.SetResult();
                return Task.FromResult((int)SimConnectError.None);
            },
            TimeSpan.FromSeconds(10));

        var creation = manager.CreateObjectAsync("CoffeeCup", default);
        await nativeCallCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        manager.ProcessObjectCreated(clientRequestId, 123, string.Empty, default);

        var created = await creation;
        Assert.Equal(123u, created.ObjectId);
    }
}
