using System;
using System.Collections.Generic;

namespace Unity.XR.XREAL.Samples.NetWork
{
    /// <summary> A message packer. </summary>
    public class MessagePacker
    {
        /// <summary> The bytes. </summary>
        private List<byte> bytes = new List<byte>();

        /// <summary> Gets the package. </summary>
        /// <value> The package. </value>
        public byte[] Package
        {
            get { return bytes.ToArray(); }
        }

        /// <summary> Adds value. </summary>
        /// <param name="data"> The data to add.</param>
        /// <returns> A MessagePacker. </returns>
        public MessagePacker Add(byte[] data)
        {
            bytes.AddRange(data);
            return this;
        }

        /// <summary> Adds value. </summary>
        /// <param name="value"> The value to add.</param>
        /// <returns> A MessagePacker. </returns>
        public MessagePacker Add(ushort value)
        {
            byte[] data = BitConverter.GetBytes(value);
            bytes.AddRange(data);
            return this;
        }

        /// <summary> Adds value. </summary>
        /// <param name="value"> The value to add.</param>
        /// <returns> A MessagePacker. </returns>
        public MessagePacker Add(uint value)
        {
            byte[] data = BitConverter.GetBytes(value);
            bytes.AddRange(data);
            return this;
        }

        /// <summary> Adds value. </summary>
        /// <param name="value"> The value to add.</param>
        /// <returns> A MessagePacker. </returns>
        public MessagePacker Add(ulong value)
        {
            byte[] data = BitConverter.GetBytes(value);
            bytes.AddRange(data);
            return this;
        }
    }
}
