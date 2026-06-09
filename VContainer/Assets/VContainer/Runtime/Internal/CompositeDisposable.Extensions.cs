namespace VContainer.Internal
{
    sealed partial class CompositeDisposable
    {
        public void Clear()
        {
            disposables.Clear();
        }
    }
}