using System.Collections.Generic;
using System.Linq;

namespace Neurotoxin.Contour.Core.Extensions
{
	public static class EnumerableExtensions
	{
		public static IList<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
		{
			IEnumerable<IEnumerable<T>> enumerables = new[] { Enumerable.Empty<T>() };
			return sequences.Aggregate(enumerables, ( accumulator, sequence) => 
				from accseq in accumulator
				from item in sequence
				select accseq.Concat(new[] { item })).ToList();
		}
	}
}