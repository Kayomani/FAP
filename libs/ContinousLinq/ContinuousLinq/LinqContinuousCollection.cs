namespace ContinuousLinq
{
    /// <summary>
    /// Augments ContinuousCollection for CLINQ-specific functionality.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class LinqContinuousCollection<T> : ReadOnlyContinuousCollection<T>
    {
        public IViewAdapter SourceAdapter { get; set; }
    }
}
