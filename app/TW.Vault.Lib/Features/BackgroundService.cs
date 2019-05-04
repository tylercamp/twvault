using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TW.Vault.Features
{
    // Copyright (c) .NET Foundation. Licensed under the Apache License, Version 2.0.
    /// <summary>
    /// Base class for implementing a long running <see cref="IHostedService"/>.
    /// </summary>
    public abstract class BackgroundService : IHostedService, IDisposable
    {
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts =
                                                       new CancellationTokenSource();

        protected ILogger logger;
        private IServiceScopeFactory scopeFactory;

        public BackgroundService(IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger(this.GetType());
            this.scopeFactory = scopeFactory;
        }

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        public Task ExecutingTask => _executingTask;

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            // Store the task we're executing
            _executingTask = ExecuteAsync(_stoppingCts.Token);

            // If the task is completed then return it,
            // this will bubble cancellation and failure to the caller
            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            // Otherwise it's running
            return Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite,
                                                              cancellationToken));
            }
        }

        public virtual void Dispose()
        {
            _stoppingCts.Cancel();
        }

        public async Task WithVaultContext(Func<Scaffold.VaultContext, Task> action)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<Scaffold.VaultContext>())
                {
                    await action(context);
                }
            }
        }

        public async Task<T> WithVaultContext<T>(Func<Scaffold.VaultContext, Task<T>> action)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<Scaffold.VaultContext>())
                {
                    return await action(context);
                }
            }
        }
    }

}