namespace TedToolkit.Fluent;

/// <summary>
///     The type of the <see cref="Fluent{TTarget}" />
/// </summary>
public enum FluentType : byte
{
    /// <summary>
    ///     Do it asap.
    /// </summary>
    Immediate,

    /// <summary>
    ///     Wait for calling result or dispose to do it
    ///     <remarks>
    ///         <para>
    ///             <c>⚠ WARNING:</c> Don't forget to call the <see cref="IDisposable.Dispose" /> or
    ///             <see cref="Fluent{TTarget}.Result" />.
    ///         </para>
    ///     </remarks>
    /// </summary>
    Lazy
}