using System;
using Melville.CSJ2K.Icc;

namespace Melville.CSJ2K.Icc.Tags
{
    internal class ICCDataType : ICCTag
    {
        new public int type;
        public int reserved;
        public int dataFlag;
        //byte[] Data;

        /// <summary> Construct this tag from its constituant parts</summary>
        /// <param name="signature">tag id</param>
        /// <param name="data">array of bytes</param>
        /// <param name="offset">to data in the data array</param>
        /// <param name="length">of data in the data array</param>
        protected internal ICCDataType(int signature, byte[] data, int offset, int length)
            : base(signature, data, offset, offset + 2 * ICCProfile.int_size)
        {
            type = ICCProfile.getInt(data, offset);
            reserved = ICCProfile.getInt(data, offset + ICCProfile.int_size);
            dataFlag = ICCProfile.getInt(data, offset + ICCProfile.int_size);
            //Data = ICCProfile.getString(data, offset + ICCProfile.int_size, length, true);
        }
    }
}