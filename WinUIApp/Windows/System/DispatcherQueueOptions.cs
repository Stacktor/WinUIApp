namespace Windows.System
{
    internal class DispatcherQueueOptions
    {
        public object apartmentType { get; set; }
        public object threadType { get; set; }
        public int flags { get; set; }
    }
}