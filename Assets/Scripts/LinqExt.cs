using System;
using System.Collections.Generic;
using System.Linq;

public static class LinqExt {
	private static TElement Extreme <TElement, TValue> (
		this IEnumerable <TElement> source, Func <TElement, TValue> selector,
		int compareResult, out TValue extremeValue
	)
		where TValue : IComparable
	{
		if ( object.ReferenceEquals ( source, null ) )
			throw new ArgumentNullException ( "source" );

		var en = source.GetEnumerator ();

		if ( en.MoveNext () ) {
			var eExtreme = en.Current;
			var vExtreme = selector ( eExtreme );

			while ( en.MoveNext () ) {
				var e = en.Current;
				var v = selector ( e );

				if ( Math.Sign ( v.CompareTo ( vExtreme ) ) == compareResult ) {
					eExtreme = e;
					vExtreme = v;
				}
			}

			extremeValue = vExtreme;

			return	eExtreme;
		} else
			throw new InvalidOperationException ( "The source sequence is empty." );
	}

	public static TElement WithMin <TElement, TValue> (
		this IEnumerable <TElement> source, Func <TElement, TValue> selector,
		out TValue minValue
	)
		where TValue : IComparable
	{
		return	Extreme ( source, selector, -1, out minValue );
	}

	public static TElement WithMin <TElement, TValue> (
		this IEnumerable <TElement> source, Func <TElement, TValue> selector
	)
		where TValue : IComparable
	{
		TValue minValue;

		return	WithMin ( source, selector, out minValue );
	}

	public static TElement WithMax <TElement, TValue> (
		this IEnumerable <TElement> source, Func <TElement, TValue> selector,
		out TValue maxValue
	)
		where TValue : IComparable
	{
		return	Extreme ( source, selector, 1, out maxValue );
	}

	public static TElement WithMax <TElement, TValue> (
		this IEnumerable <TElement> source, Func <TElement, TValue> selector
	)
		where TValue : IComparable
	{
		TValue maxValue;

		return	WithMax ( source, selector, out maxValue );
	}

	public static string Join <TElement> ( string separator, IEnumerable <TElement> source ) {
		var stringElements = source.Select ( e => e + "" ).ToArray ();

		return	string.Join ( separator, stringElements );
	}
}
