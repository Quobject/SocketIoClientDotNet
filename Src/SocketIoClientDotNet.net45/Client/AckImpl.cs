using System;

namespace Quobject.SocketIoClientDotNet.Client
{
    public class AckImpl : IAck
    {
        private readonly Action fn;
        private readonly Action<object> fn1;
        private readonly Action<object, object> fn2;
        private readonly Action<object, object, object> fn3;

        public AckImpl(Action fn)
        {
            this.fn = fn;
        }

        public AckImpl(Action<object> fn)
        {
            this.fn1 = fn;
        }
        public AckImpl(Action<object,object> fn)
        {
            this.fn2 = fn;
        }

        public AckImpl(Action<object, object, object> fn)
        {
            this.fn3 = fn;
        }


        public void Call(params object[] args)
        {
            if (fn != null)
            {
                fn();
            }
            else if (fn1 != null)
            {
                var arg = args.Length > 0 ? args[0] : null;
                fn1(arg);
            }
            else if (fn2 != null)
            {
                var arg = args.Length > 0 ? args[0] : null;
                var arg1 = args.Length > 1 ? args[1] : null;
                fn2(arg, arg1);
            }
            else if (fn3 != null)
            {
                var arg = args.Length > 0 ? args[0] : null;
                var arg1 = args.Length > 1 ? args[1] : null;
                var arg2 = args.Length > 2 ? args[2] : null;
                fn3(arg, arg1, arg2);
            }
        }
    }
}