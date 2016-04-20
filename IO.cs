namespace UnitySocket
{
    public class IO
    {


        public static Socket Socket(string url, int port, Emitter.Listener connectCallback)
        {
            Socket newSocket = new Socket(url, port, connectCallback);
            return newSocket;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


    }
}
