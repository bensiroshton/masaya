using System.Collections.Generic;

namespace Siroshton.Masaya.Util
{
    public class ObjectPool<T>
    {
        public delegate T CreateObjectFunc();

        private Queue<T> _pool = new Queue<T>();
        private CreateObjectFunc _createObject;

        public ObjectPool()
        {
            _createObject = () => { return default(T); };
        }

        public ObjectPool(CreateObjectFunc createFunction)
        {
            _createObject = createFunction;
        }

        public T GetObject()
        {
            T o;
            if( !_pool.TryDequeue(out o) )
            {
                o = _createObject();
            }
            return o;
        }

        public void ReturnObject(T o)
        {
            _pool.Enqueue(o);
        }
    }

}