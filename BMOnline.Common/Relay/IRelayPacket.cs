namespace BMOnline.Common.Relay
{
    public interface IRelayPacket
    {
        /// <summary>
        /// Sets the fields in this packet based on the array of bytes provided.
        /// </summary>
        /// <param name="data">The received data to decode.</param>
        void Decode(byte[] data);

        /// <summary>
        /// Creates an array of bytes based on the fields in this packet.
        /// </summary>
        /// <returns>The encoded data to send.</returns>
        byte[] Encode();
    }
}
