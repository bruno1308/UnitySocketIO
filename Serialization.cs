
using System.Text;
namespace UnitySocket {
    public static class Serialization
    {
        public static byte[] StringToByteArray(string str)
        {
            //byte[] bytes = new byte[str.Length * sizeof(char)];
            //System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            UTF8Encoding utf8 = new UTF8Encoding();
            // Debug.Log("String cost: " + bytes.Length+ " size of char: "+ sizeof(char));
            return utf8.GetBytes(str);
        }

    }
}
