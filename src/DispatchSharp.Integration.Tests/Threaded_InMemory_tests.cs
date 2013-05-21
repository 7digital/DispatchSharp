﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DispatchSharp.QueueTypes;
using NUnit.Framework;

namespace DispatchSharp.Integration.Tests
{
	[TestFixture]
	public class Threaded_InMemory_tests : behaviours
	{
		[SetUp]
		public override void setup()
		{
			_output = new List<string>();
			_subject = new Dispatch<string>(new InMemoryWorkQueue<string>(), new ThreadedWorkerPool<string>("Test", 8));
		}
		
		[Test]
		public void stopping_the_dispatcher_completes_all_current_actions_before_stopping()
		{
			_subject.AddConsumer(s =>
			{
				_output.Add("Start");
				Thread.Sleep(2000);
				_output.Add("End");
			});
			_subject.Start();

			for (int i = 0; i < 100; i++) { _subject.AddWork(""); }

			Thread.Sleep(1500);

			_subject.Stop();

			Assert.That(_output.Count(), Is.GreaterThan(0));
			Assert.That(_output.Count(s => s == "Start"), Is.EqualTo(_output.Count(s => s == "End"))
				, "Mismatch between started and ended consumers");
		}

		[Test]
		public void can_repeatedly_start_and_stop_a_dispatcher ()
		{
			_subject.AddConsumer(s =>
			{
				_output.Add("Start");
				Thread.Sleep(2000);
				_output.Add("End");
			});

			for (int i = 0; i < 5; i++)
			{
				_subject.Stop();
				_subject.Start();
			}

			for (int i = 0; i < 100; i++) { _subject.AddWork(""); }

			Thread.Sleep(1500);
			
			for (int i = 0; i < 5; i++)
			{
				_subject.Start();
				Thread.Sleep(150);
				_subject.Stop();
			}

			Assert.That(_output.Count(), Is.GreaterThan(0));
			Assert.That(_output.Count(s => s == "Start"), Is.EqualTo(_output.Count(s => s == "End"))
				, "Mismatch between started and ended consumers");
		}

		
		[Test]
		[TestCase(1)]
		[TestCase(2)]
		[TestCase(4)]
		[TestCase(6)]
		public void maximum_inflight_is_respected (int limit)
		{
			var counts = new List<int>();
			_subject.AddConsumer(s => counts.Add(_subject.CurrentInflight()));

			_subject.MaximumInflight =limit;
			_subject.Start();

			for (int i = 0; i < 100; i++) { _subject.AddWork(""); }

			Thread.Sleep(1500);

			_subject.Stop();

			Assert.That(counts.Count(), Is.GreaterThan(0), "No actions ran");
			Assert.That(counts.Min(), Is.GreaterThan(0), "Inflight count is invalid");
			Assert.That(counts.Max(), Is.LessThanOrEqualTo(limit));
		}
	}
}