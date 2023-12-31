﻿using Hotel.Shared.Dispatchers;
using Hotel.Shared.Exceptions;
using Hotel.Shared.Handlers;
using Hotel.Shared.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using StackExchange.Redis;

namespace Hotel.Shared.Redis;

internal class StreamingSubscriber : IStreamingSubscriber
{
    private readonly ILogger<StreamingSubscriber> _logger;
    private readonly ISubscriber _subscriber;
    private readonly IStreamingPublisher _publisher;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly RedisOptions _options;
    private readonly IMessagingChannel<ICommand> _messagingChannel;
    public StreamingSubscriber(
        WebApplication application)
    {
        // get redis option include retry policy and something related to polly
        _logger = application.Services.GetService<ILogger<StreamingSubscriber>>()!;
        _subscriber = application.Services.GetService<IConnectionMultiplexer>()!.GetSubscriber();
        _publisher = application.Services.GetService<IStreamingPublisher>()!;
        _commandDispatcher = application.Services.GetService<ICommandDispatcher>()!;
        _options = application.Services.GetService<IOptions<RedisOptions>>()!.Value;
        _messagingChannel = application.Services.GetService<IMessagingChannel<ICommand>>()!;
    }

    public IStreamingSubscriber SubscribeAsync<TCommand>(
        string topic, Func<TCommand, DomainException, IRejectedCommand>? onError = null)
        where TCommand : ICommand
    {
        _logger.LogInformation($"subscriber topic {topic}_{typeof(TCommand).Name}");
        // write handle async function with polly fault handling
        // string channel = $"{topic}_{typeof(TCommand).Name}";
        _subscriber.SubscribeAsync(topic, async (channel, data) =>
        {
            string parse = data!;

            // handle invoice expire
            if (!parse.Contains('{'))
            {
                // not handle that event
                if (!parse.StartsWith("payment"))
                {
                    return;
                }

                var elements = parse.Split(":");
                parse = "{" + "\"" + elements[0] + "\"" + ":" + "\"" + elements[1] + "\"" + "}";
            }

            var command = JsonConvert.DeserializeObject<TCommand>(parse);
            if (command == null)
            {
                return;
            }
            await HandleAsync(topic, command, () => _commandDispatcher.DispatchAsync(command), onError);
        });

        return this;
    }


    // pass handler and on error here
    private async Task HandleAsync<TCommand>(
        string topic,
        TCommand command,
        Func<Task> handler,
        Func<TCommand, DomainException, IRejectedCommand>? onError = null)
        where TCommand : ICommand
    {
        // design policy
        var policy = Policy
            .Handle<Exception>()
            .RetryAsync(_options.RetryPolicy);

        var messageName = command.GetType().Name;
        var currentPollyExecution = 0;
        await policy.ExecuteAsync(async () =>
        {
            try
            {
                _logger.LogInformation($"handling message {messageName} on topic {topic}");

                await handler();

                _logger.LogInformation($"handled message {messageName} on topic {topic}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                if (currentPollyExecution != 0)
                {
                    _logger.LogInformation($"retry handle message {messageName} on topic {topic} at {currentPollyExecution} time");
                }

                currentPollyExecution++;
                if (ex is DomainException domainException && onError != null)
                {
                    var rejectedCommand = onError(command, domainException)!;
                    await _publisher.PublishAsync(topic, rejectedCommand);
                    return Task.CompletedTask;
                }

                if (ex is DomainException domainException1)
                {
                    _logger.LogInformation($"Domain throw exception {domainException1.Message}");
                    return Task.CompletedTask;
                }

                throw new Exception($"unable to handle message {messageName} on topic {topic}");
            }
        });
    }
}
