namespace VContainer.Internal
{
    sealed partial class CompositeDisposable
    {
        public void Clear()
        {
            lock (disposables)
            {
                disposables.Clear();
            }
        }
    }
}