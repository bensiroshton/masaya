
using UnityEngine;

namespace Siroshton.Masaya.Entity
{
    public interface IDeathHandler
    {
        public string handlerName { get; set; }
        public void HandleDeath(Entity o);
    }
}
