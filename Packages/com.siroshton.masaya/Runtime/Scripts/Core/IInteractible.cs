namespace Siroshton.Masaya.Core
{

    public interface IInteractible
    {
        public bool isReadyForInteraction { get; }
        public void TriggerInteraction();
    }

}