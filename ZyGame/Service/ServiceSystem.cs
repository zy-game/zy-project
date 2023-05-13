using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZyGame.Service
{
    internal class ServiceSystem : Singleton<ServiceSystem>
    {
        private Dictionary<Type, IService> services = new Dictionary<Type, IService>();
        public Task Startup<T>() where T : IService => Startup(typeof(T));

        public Task Startup(Type type)
        {
            if (GetService(type) is not null)
            {
                return Task.CompletedTask;
            }
            IService service = Activator.CreateInstance(type) as IService;
            services.Add(type, service);
            return service.Startup();
        }

        public Task Shutdown<T>() where T : IService => Shutdown(typeof(T));

        public Task Shutdown(Type type)
        {
            IService service = GetService(type);
            if (service is null)
            {
                return Task.CompletedTask;
            }
            services.Remove(type);
            return service.Shutdown();
        }

        public T GetService<T>() where T : IService => (T)GetService(typeof(T));

        public IService GetService(Type type)
        {
            if (services.TryGetValue(type, out IService service))
            {
                return service;
            }
            return default;
        }
    }

    public interface IService
    {
        Task Startup();

        Task Shutdown();
    }

    public abstract class ServiceHandle : IService
    {
        public virtual Task Shutdown()
        {
        }

        public virtual Task Startup()
        {
        }
    }
}
