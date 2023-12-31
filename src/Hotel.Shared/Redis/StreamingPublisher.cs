﻿using Hotel.Shared.Handlers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Hotel.Shared.Redis;

internal class StreamingPublisher : IStreamingPublisher
{
    private readonly ISubscriber _subscriber;
    private readonly ILogger<StreamingPublisher> _logger;
    public StreamingPublisher(
        ILogger<StreamingPublisher> logger,
        IConnectionMultiplexer connection)
    {
        _logger = logger;
        _subscriber = connection.GetSubscriber();
    }

    public async Task PublishAsync<TCommand>(string topic, TCommand command) where TCommand : ICommand
    {
        _logger.LogInformation($"publish a message {command.GetType().Name} at topic {topic}_{command.GetType().Name}");
        var data = JsonConvert.SerializeObject(command);
        // var channel = $"{topic}_{command.GetType().Name}";
        await _subscriber.PublishAsync(topic, data);
    }
}
