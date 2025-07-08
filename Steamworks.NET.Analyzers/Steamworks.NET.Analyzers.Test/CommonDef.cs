namespace Steamworks
{
	[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class SteamHasAsyncCallResultAttribute : System.Attribute
	{
		private System.Type callbackType; // for vs project ignore suggestion IDE0044, I'm not sure if Unity supports readonly field

		// See the attribute guidelines at 
		//  http://go.microsoft.com/fwlink/?LinkId=85236

		internal SteamHasAsyncCallResultAttribute(System.Type callbackType)
		{
			this.callbackType = callbackType;
		}

		/// <summary>
		/// Result type of the async operation.
		/// </summary>
		public System.Type CallbackType { get { return callbackType; } }
	}

	/// <summary>
	/// Inform invokers use <see cref="Callback{T}"/> to receive async result.
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class SteamHasAsyncCallbackAttribute : System.Attribute
	{
		private System.Type callbackType; // for vs project ignore suggestion IDE0044, I'm not sure if Unity supports readonly field

		// See the attribute guidelines at 
		//  http://go.microsoft.com/fwlink/?LinkId=85236

		internal SteamHasAsyncCallbackAttribute(System.Type callbackType)
		{
			this.callbackType = callbackType;
		}

		/// <summary>
		/// Result type of the async operation.
		/// </summary>
		public System.Type CallbackType { get { return callbackType; } }
	}

	// essential definition(mostly public API) of callback system

	public sealed class CallResult<T>
	{
		public delegate void APIDispatchDelegate(T param, bool bIOFailure);

		public static CallResult<T> Create(APIDispatchDelegate func = null)
		{
			return new CallResult<T>();
		}

		public void Set(SteamAPICall_t hAPICall, APIDispatchDelegate func = null)
		{

		}
	}

	public sealed class Callback<T>
	{
		public delegate void DispatchDelegate(T param);

		public static Callback<T> Create(DispatchDelegate func = null)
		{
			return new Callback<T>();
		}

		public static Callback<T> CreateGameServer(DispatchDelegate func)
		{
			return new Callback<T>();
		}

		public Callback(DispatchDelegate func, bool bGameServer = false)
		{
			m_bGameServer = bGameServer;
			Register(func);
		}

		public void Register(DispatchDelegate func)
		{

		}
	}

	public struct SteamAPICall_t { }

	public struct AsyncCallResult_t { }
	public struct AsyncCallback_t { }

	public static class SteamAsyncTest
	{
		[SteamHasAsyncCallResult(typeof(AsyncCallResult_t))]
		public static SteamAPICall_t TestCallResult() { return new SteamAPICall_t(); }

		[SteamHasAsyncCallback(typeof(AsyncCallback_t))]
		public static void TestCallback() { }
	}
}