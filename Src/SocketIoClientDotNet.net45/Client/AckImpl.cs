using System;

namespace Quobject.SocketIoClientDotNet.Client
{
    public class AckImpl : IAck
    {
        private readonly Action fn0;
        private readonly Action<object> fn1;
        private readonly Action<object, object> fn2;
        private readonly Action<object, object, object> fn3;
        private readonly Action<object, object, object, object> fn4;

        public AckImpl(Action fn)
        {
            fn0 = fn;
        }

        public AckImpl(Action<object> fn)
        {
            fn1 = fn;
        }

        public AckImpl(Action<object, object> fn)
        {
            fn2 = fn;
        }

        public AckImpl(Action<object, object, object> fn)
        {
            fn3 = fn;
        }

        public AckImpl(Action<object, object, object, object> fn)
        {
            fn4 = fn;
        }

        public void Call(params object[] args)
        {
            if (fn0 != null)
            {
                fn0();
            }
            else if (fn1 != null)
            {
                var arg0 = args.Length > 0 ? args[0] : null;
                fn1(arg0);
            }
            else if (fn2 != null)
            {
                var arg0 = args.Length > 0 ? args[0] : null;
                var arg1 = args.Length > 1 ? args[1] : null;
                fn2(arg0, arg1);
            }
            else if (fn3 != null)
            {
                var arg0 = args.Length > 0 ? args[0] : null;
                var arg1 = args.Length > 1 ? args[1] : null;
                var arg2 = args.Length > 2 ? args[2] : null;
                fn3(arg0, arg1, arg2);
            }
            else if (fn4 != null)
            {
                var arg0 = args.Length > 0 ? args[0] : null;
                var arg1 = args.Length > 1 ? args[1] : null;
                var arg2 = args.Length > 2 ? args[2] : null;
                var arg3 = args.Length > 3 ? args[3] : null;
                fn4(arg0, arg1, arg2, arg3);
            }
        }
    }
}