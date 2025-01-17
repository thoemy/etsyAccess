﻿using System;
using System.Collections.Generic;
using System.Text;
using EtsyAccess.Models.Throttling;

namespace EtsyAccess.Exceptions
{
	public class EtsyException : Exception
	{
		public EtsyException( string message, Exception exception ) : base( message, exception ) { }
		public EtsyException( string message ) : this ( message, null ) { }
	}

	/// <summary>
	///	Request to Etsy servers failed due to connection issues or specific server behavior.
	///	Normally operation can be reattempted after receiving this exception
	/// </summary>
	public class EtsyNetworkException : EtsyException
	{
		public EtsyNetworkException( string message, Exception exception ) : base( message, exception ) { }
		public EtsyNetworkException( string message ) : this( message, null ) { }
	}

	/// <summary>
	///	Etsy server exception (they can't be reattempted)
	/// </summary>
	public class EtsyServerException : EtsyException
	{
		/// <summary>
		///	Http response status code
		/// </summary>
		public int Code { get; private set; }

		public EtsyServerException( string message, int code, Exception exception ) : base( message, exception )
		{
			Code = code;
		}

		public EtsyServerException( string message, int code ) : this ( message, code, null ) { }
	}

	/// <summary>
	///	OAuth request signature is invalid. The remedy is resend request again (normal behavior from Etsy backend)
	/// </summary>
	public class EtsyInvalidSignatureException : EtsyServerException
	{
		public EtsyInvalidSignatureException( string message ) : base( message, 403 ) { }
	}

	/// <summary>
	///	Etsy's API usage exceeded, normally Etsy allows 10 requests per second
	/// </summary>
	public class EtsyApiLimitsExceeded : EtsyServerException
	{
		public EtsyLimits Limits { get; private set; }

		public EtsyApiLimitsExceeded( EtsyLimits limits, string message ) : base(message, 403 )
		{
			Limits = limits;
		}
	}
}
