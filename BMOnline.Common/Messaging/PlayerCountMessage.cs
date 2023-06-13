using System;

namespace BMOnline.Common.Messaging
{
    public class PlayerCountMessage : Message
    {
        public uint Secret { get; set; }
        public ushort[] CourseCounts { get; set; }
        public byte[] StageCounts { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            int pointer = 4;
            CourseCounts = new ushort[Definitions.CourseIds.Count];
            for (int i = 0; i < CourseCounts.Length; i++)
            {
                CourseCounts[i] = BitConverter.ToUInt16(data, pointer);
                pointer += 2;
            }
            StageCounts = new byte[Definitions.StageIds.Count];
            for (int i = 0;i < StageCounts.Length; i++)
            {
                StageCounts[i] = data[pointer++];
            }
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[4 + (CourseCounts.Length * 2) + StageCounts.Length];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            int pointer = 4;
            for (int i = 0; i < CourseCounts.Length; i++)
            {
                BitConverter.GetBytes(CourseCounts[i]).CopyTo(output, pointer);
                pointer += 2;
            }
            for (int i = 0; i < StageCounts.Length; i++)
            {
                output[pointer++] = StageCounts[i];
            }
            return output;
        }
    }
}
