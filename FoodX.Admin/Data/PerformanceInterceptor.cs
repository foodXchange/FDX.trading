using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Diagnostics;

namespace FoodX.Admin.Data;

public class PerformanceInterceptor : DbCommandInterceptor
{
    private readonly ILogger<PerformanceInterceptor> _logger;
    private readonly int _slowQueryThresholdMs;

    public PerformanceInterceptor(ILogger<PerformanceInterceptor> logger, int slowQueryThresholdMs = 1000)
    {
        _logger = logger;
        _slowQueryThresholdMs = slowQueryThresholdMs;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        command.CommandTimeout = 60; // Ensure consistent timeout
        return base.ReaderExecuting(command, eventData, result);
    }

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        command.CommandTimeout = 60; // Ensure consistent timeout
        return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        LogPerformance(eventData);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogPerformance(eventData);
        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        command.CommandTimeout = 60; // Ensure consistent timeout
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        command.CommandTimeout = 60; // Ensure consistent timeout
        return await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        LogPerformance(eventData);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        LogPerformance(eventData);
        return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        command.CommandTimeout = 60; // Ensure consistent timeout
        return base.ScalarExecuting(command, eventData, result);
    }

    public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        command.CommandTimeout = 60; // Ensure consistent timeout
        return await base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        LogPerformance(eventData);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override async ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        LogPerformance(eventData);
        return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void LogPerformance(CommandExecutedEventData eventData)
    {
        var duration = eventData.Duration;

        if (duration.TotalMilliseconds > _slowQueryThresholdMs)
        {
            _logger.LogWarning(
                "Slow query detected. Duration: {Duration}ms, Command: {Command}",
                duration.TotalMilliseconds,
                eventData.Command.CommandText);
        }
        else if (duration.TotalMilliseconds > 100)
        {
            _logger.LogDebug(
                "Query executed. Duration: {Duration}ms",
                duration.TotalMilliseconds);
        }
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        _logger.LogError(
            eventData.Exception,
            "Database command failed. Duration: {Duration}ms, Command: {Command}",
            eventData.Duration.TotalMilliseconds,
            command.CommandText);

        base.CommandFailed(command, eventData);
    }

    public override async Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogError(
            eventData.Exception,
            "Database command failed. Duration: {Duration}ms, Command: {Command}",
            eventData.Duration.TotalMilliseconds,
            command.CommandText);

        await base.CommandFailedAsync(command, eventData, cancellationToken);
    }
}