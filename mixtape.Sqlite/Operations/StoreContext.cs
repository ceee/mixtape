using System;
using Mixtape.Communication;
using Mixtape.Configuration;
using Mixtape.Context;

namespace Mixtape.Sqlite;

public class StoreContext(IMixtapeStore store, IMixtapeContext context, IServiceProvider serviceProvider, IMessageAggregator messages)
{
  public IMixtapeStore Store { get; private set; } = store;
  
  public IMixtapeContext Context { get; private set; } = context;

  public IMixtapeOptions Options { get; private set; } = context.Options;

  public IServiceProvider Services { get; private set; } = serviceProvider;

  public IMessageAggregator Messages { get; private set; } = messages;
}