using Quobject.EngineIoClientDotNet.ComponentEmitter;
using System;

namespace Quobject.SocketIoClientDotNet.Client
{
    public class On
    {
        private On() { }

        public static IHandle Create(Emitter obj, string ev, IListener fn)
        {
            obj.On(ev, fn);
            return new HandleImpl(obj,ev,fn);
        }

        public class HandleImpl : IHandle
        {
            private Emitter obj;
            private string ev;
            private IListener fn;

            public HandleImpl(Emitter obj, string ev, IListener fn)
            {
                this.obj = obj;
                this.ev = ev;
                this.fn = fn;
            }

            public void Destroy()
            {
                obj.Off(ev, fn);
            }
        }

        public class ActionHandleImpl : IHandle
        {
            private Action fn;

            public ActionHandleImpl(Action fn)
            {
                this.fn = fn;
            }

            public void Destroy()
            {
                fn();
            }
        }

        public interface IHandle
        {
            void Destroy();
        }
     
    }

}
