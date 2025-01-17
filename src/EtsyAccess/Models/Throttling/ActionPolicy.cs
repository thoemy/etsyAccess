﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using EtsyAccess.Exceptions;
using Polly;

namespace EtsyAccess.Models.Throttling
{
	public class ActionPolicy
	{
		private readonly int _retryAttempts;

		public ActionPolicy( int attempts )
		{
			Condition.Requires( attempts ).IsGreaterThan( 0 );

			_retryAttempts = attempts;
		}

		/// <summary>
		///	Retries function until it succeed or failed
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="funcToThrottle"></param>
		/// <param name="onRetryAttempt">Retry attempts</param>
		/// <param name="extraLogInfo"></param>
		/// <param name="onException"></param>
		/// <returns></returns>
		public Task< TResult > ExecuteAsync< TResult >( Func< Task< TResult > > funcToThrottle, Action< TimeSpan, int > onRetryAttempt, Func< string > extraLogInfo, Action< Exception > onException )
		{
			return Policy.Handle< EtsyNetworkException >()
				.WaitAndRetryAsync( _retryAttempts,
					retryCount => TimeSpan.FromSeconds( GetDelayBeforeNextAttempt(retryCount) ),
					( entityRaw, timeSpan, retryCount, context ) =>
					{
						onRetryAttempt?.Invoke( timeSpan, retryCount );
					})
				.ExecuteAsync( async () =>
				{
					try
					{
						return await funcToThrottle().ConfigureAwait( false );
					}
					catch ( Exception exception )
					{
						EtsyException etsyException = null;

						var exceptionDetails = string.Empty;

						if ( extraLogInfo != null )
							exceptionDetails = extraLogInfo();

						if ( exception is HttpRequestException 
							|| exception is EtsyInvalidSignatureException )
							etsyException = new EtsyNetworkException( exceptionDetails, exception );
						else
							etsyException = new EtsyException( exceptionDetails, exception );

						throw etsyException;
					}
					
				});
		}

		public static int GetDelayBeforeNextAttempt( int retryCount )
		{
			return 5 + 20 * retryCount;
		}
	}
}
