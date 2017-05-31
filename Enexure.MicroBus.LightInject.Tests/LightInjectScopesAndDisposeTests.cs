﻿using System;
using System.Threading.Tasks;
using Enexure.MicroBus.Annotations;
using FluentAssertions;
using LightInject;
using Xunit;

namespace Enexure.MicroBus.LightInject.Tests
{
	public class LightInjectScopesAndDisposeTests
	{
		[UsedImplicitly]
		private class DisposableObject : IDisposable
		{
			public bool IsDisposed { get; set; }

			public void Dispose()
			{
				IsDisposed = true;
			}
		}

		class Command : ICommand { }

		[UsedImplicitly]
		private class CommandHandler : ICommandHandler<Command>
		{
			private readonly DisposableObject disposable;

			public CommandHandler(DisposableObject disposable)
			{
				this.disposable = disposable;
			}

			public async Task Handle(Command command)
			{
				await Task.Delay(1);

				disposable.IsDisposed.Should().BeFalse("");
			}
		}

		class Event : IEvent { }

		[UsedImplicitly]
		private class EventHandler : IEventHandler<Event>
		{
			private readonly DisposableObject disposable;

			public EventHandler(DisposableObject disposable)
			{
				this.disposable = disposable;
			}

			public async Task Handle(Event Event)
			{
				await Task.Delay(1);

				disposable.IsDisposed.Should().BeFalse();
			}
		}

		class QueryAsync : IQuery<QueryAsync, Result> { }

		private class Result { }

		[UsedImplicitly]
		private class QueryHandler : IQueryHandler<QueryAsync, Result>
		{
			private readonly DisposableObject disposable;

			public QueryHandler(DisposableObject disposable)
			{
				this.disposable = disposable;
			}

			public async Task<Result> Handle(QueryAsync QueryAsync)
			{
				await Task.Delay(1);

				disposable.IsDisposed.Should().BeFalse();

				return new Result();
			}
		}

		[Fact]
		public async Task InTheDefaultAutofacScopeCommandHandlersShouldFinishBeforeTheScopeIsDisposed()
		{
			var containerBuilder = new ServiceContainer();

			var busBuilder = new BusBuilder()
				.RegisterCommandHandler<Command, CommandHandler>();

			containerBuilder.RegisterMicroBus(busBuilder);
			containerBuilder.Register<DisposableObject>();
			//var container = containerBuilder.Build();

		    var bus = containerBuilder.GetInstance<IMicroBus>();

            await bus.SendAsync(new Command());
		}

		[Fact]
		public async Task InTheDefaultAutofacScopeEventHandlersShouldFinishBeforeTheScopeIsDisposed()
		{
			var containerBuilder = new ServiceContainer();

			var busBuilder = new BusBuilder()
				.RegisterEventHandler<Event, EventHandler>();

			containerBuilder.RegisterMicroBus(busBuilder);
			containerBuilder.Register<DisposableObject>();
			//var container = containerBuilder.Build();

		    var bus = containerBuilder.GetInstance<IMicroBus>();

            await bus.PublishAsync(new Event());
		}

		[Fact]
		public async Task InTheDefaultAutofacScopeQueryHandlersShouldFinishBeforeTheScopeIsDisposed()
		{
			var containerBuilder = new ServiceContainer();

			var busBuilder = new BusBuilder()
				.RegisterQueryHandler<QueryAsync, Result, QueryHandler>();

			containerBuilder.RegisterMicroBus(busBuilder);
			containerBuilder.Register<DisposableObject>();

			var bus = containerBuilder.GetInstance<IMicroBus>();

			await bus.QueryAsync(new QueryAsync());
		}
	}
}