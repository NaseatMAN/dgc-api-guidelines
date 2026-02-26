using System.Linq.Expressions;

namespace DGC.Sample.Domain.Specifications;

public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    
    // Named Filter Toggling
    List<string> IgnoredNamedFilters { get; }
}
