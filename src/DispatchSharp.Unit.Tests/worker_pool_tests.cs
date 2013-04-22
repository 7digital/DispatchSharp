﻿using System.Threading;
using NSubstitute;
using NUnit.Framework;

namespace DispatchSharp.Unit.Tests
{
	[TestFixture]
	public class worker_pool_tests
	{
		IDispatch<object> _dispatcher;
		IWorkerPool<object> _subject;
		IWorkQueue<object> _queue;

		[SetUp]
		public void setup ()
		{
			_dispatcher = Substitute.For<IDispatch<object>>();
			_queue = Substitute.For<IWorkQueue<object>>();
			_subject = new ThreadedWorkerPool<object>("name", 1);
			_subject.SetSource(_dispatcher, _queue);
		}

		[Test]
		public void worker_pool_does_nothing_if_not_started ()
		{
			Thread.Sleep(250);
			_queue.DidNotReceive().TryDequeue();
		}

		[Test]
		public void worker_pool_does_nothing_after_being_stopped ()
		{
			_subject.Start();
			_subject.Stop();
			_dispatcher.ClearReceivedCalls();
			_queue.ClearReceivedCalls();

			Thread.Sleep(250);
			
			_queue.DidNotReceive().TryDequeue();
		}

		[Test]
		public void if_available_flag_is_not_set_worker_waits ()
		{
			Available(false);
			Go();
			_queue.DidNotReceive().TryDequeue();
		}

		[Test]
		public void if_available_flag_is_set_worker_polls_for_work_item()
		{
			Available(true);
			Go();
			_queue.Received().TryDequeue();
		}

		[Test]
		public void when_no_work_items_are_available_worker_waits_for_next_available ()
		{
			ItemAvailable(false);
			Go();

			_queue.Received().TryDequeue();
			_dispatcher.DidNotReceive().WorkActions();
		}

		[Test]
		public void when_work_items_are_available_worker_reads_work_actions()
		{
			ItemAvailable(true);
			Go();

			_queue.Received().TryDequeue();
			_dispatcher.Received().WorkActions();
		}

		void Available(bool b)
		{
			if (b) _subject.Available.Set();
			else _subject.Available.Reset();
		}
		void Go()
		{
			_subject.Start();
			Thread.Sleep(20);
			_subject.Stop();
		}
		void ItemAvailable(bool yes)
		{
			var item = Substitute.For<IWorkQueueItem<object>>();
			item.HasItem.Returns(yes);
			_queue.TryDequeue().Returns(item);
		}
	}
}