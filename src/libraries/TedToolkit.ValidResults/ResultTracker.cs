namespace TedToolkit.ValidResults;

public class ResultTracker<TValue>
{
    protected internal ResultTracker(
        TValue value,
        string callerInfo)
    {
        Value = value;
        CallerInfo = callerInfo;
    }

    public TValue Value { get; }
    public string CallerInfo { get; }
}