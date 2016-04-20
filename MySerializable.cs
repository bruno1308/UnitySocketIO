namespace UnitySocket
{
    public interface MySerializable<T>
    {

        byte[] ToByteArray();
        T FromByteArray(byte[] data);
    }
}