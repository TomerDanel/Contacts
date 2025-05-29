using DAL.Factory.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Factory;

/// <summary>
/// Provide context from provider delegate.
/// </summary>
/// <typeparam name="TContext">The type of context object to create</typeparam>
public class ContextFactory<TContext> : IContextFactory<TContext>
{
    #region Class Members

    private readonly Func<TContext> _contextProvider;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of ContextFactory class which is a factory for creating <typeparamref name="TContext"/> objects. 
    /// </summary>
    /// <param name="contextProvider">A delegate that returns a <typeparamref name="TContext"/> object.</param>
    public ContextFactory(Func<TContext> contextProvider)
    {
        _contextProvider = contextProvider;
    }

    #endregion

    #region Class Methods

    /// <summary>
    /// Create new context instance
    /// </summary>
    /// <returns>The context instance</returns>
    public TContext CreateContext()
    {
        return _contextProvider();
    }

    #endregion 
}