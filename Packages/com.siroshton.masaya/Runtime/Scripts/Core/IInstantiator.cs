namespace Siroshton.Masaya.Core
{
    public interface IInstantiator<T>
    {
        public T Instantiate();
    }
}