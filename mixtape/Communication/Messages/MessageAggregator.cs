using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Mixtape.Communication;

public class MessageAggregator : IMessageAggregator
{
  readonly ConcurrentBag<IMessageSubscription> Subscription = [];
  readonly IServiceProvider ServiceProvider;


  public MessageAggregator(IServiceProvider serviceProvider)
  {
    ServiceProvider = serviceProvider;
  }


  /// <inheritdoc />
  public async Task Publish<TMessage>(TMessage message) where TMessage : class, IMessage
  {
    IEnumerable<IMessageSubscription> subscriptions = Subscription.Where(s => s.CanDeliver<TMessage>());

    foreach (IMessageSubscription subscription in subscriptions)
    {
      await subscription.Deliver(ServiceProvider, message);
    }
  }


  /// <inheritdoc />
  public void Subscribe<TMessage>(Expression<Func<TMessage, Task>> handle) where TMessage : class, IMessage
  {
    Subscription.Add(new InlineMessageSubscription<TMessage>(handle));
  }


  /// <inheritdoc />
  public void Activate<TMessage, TMessageHandler>()
    where TMessage : class, IMessage
    where TMessageHandler : IMessageHandler<TMessage>
  {
    Subscription.Add(new MessageSubscription<TMessage, TMessageHandler>());
  }
}


public interface IMessageAggregator
{
  /// <summary>
  /// Publishes the specified message, invoking any handlers subscribed to the message.
  /// </summary>
  Task Publish<TMessage>(TMessage message) where TMessage : class, IMessage;

  /// <summary>
  /// Subscribe to a message
  /// </summary>
  void Subscribe<TMessage>(Expression<Func<TMessage, Task>> handle) where TMessage : class, IMessage;

  /// <summary>
  /// Subscribes the specified handler to the spified message type
  /// </summary>
  void Activate<TMessage, TMessageHandler>()
    where TMessage : class, IMessage
    where TMessageHandler : IMessageHandler<TMessage>;
}
