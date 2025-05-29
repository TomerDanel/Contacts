namespace DAL.Factory.Interface;

public interface IContextFactory<out TContext>
{
    /// <summary>
    /// Create new context instance
    /// </summary>
    /// <returns>The context instance</returns>
    TContext CreateContext();
}