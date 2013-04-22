using System.Threading;

namespace DispatchSharp
{
	class CrossThreadWait : IWaitHandle
	{
		readonly AutoResetEvent _base;

		public CrossThreadWait(bool initialSetting)
		{
			_base = new AutoResetEvent(initialSetting);
		}
		public bool WaitOne()
		{
			return _base.WaitOne();
		}

		public void Set()
		{
			_base.Set();
		}

		public void Reset()
		{
			_base.Reset();
		}

		public bool IsSet()
		{
			return _base.WaitOne(0);
		}
	}
}