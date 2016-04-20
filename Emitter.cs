using System;
namespace UnitySocket
{
    public class Emitter
    {
        public class Listener
        {
            Action<byte[]> action;
            public Listener(Action<byte[]> handleStuff)
            {
                this.action = handleStuff;
            }
            public void call(byte[] args)
            {
                action(args);
            }
        }
    }

}

