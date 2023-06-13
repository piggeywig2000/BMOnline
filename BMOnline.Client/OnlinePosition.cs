
namespace BMOnline.Client
{
    public readonly struct OnlinePosition
    {
        public readonly float PosX;
        public readonly float PosY;
        public readonly float PosZ;

        public readonly float AngVeloX;
        public readonly float AngVeloY;
        public readonly float AngVeloZ;

        public OnlinePosition(float posX, float posY, float posZ, float angVeloX, float angVeloY, float angVeloZ)
        {
            PosX = posX;
            PosY = posY;
            PosZ = posZ;
            AngVeloX = angVeloX;
            AngVeloY = angVeloY;
            AngVeloZ = angVeloZ;
        }

        public static OnlinePosition Lerp(OnlinePosition from, OnlinePosition to, float t) => new OnlinePosition(
            from.PosX + ((to.PosX - from.PosX) * t),
            from.PosY + ((to.PosY - from.PosY) * t),
            from.PosZ + ((to.PosZ - from.PosZ) * t),
            from.AngVeloX + ((to.AngVeloX - from.AngVeloX) * t),
            from.AngVeloY + ((to.AngVeloY - from.AngVeloY) * t),
            from.AngVeloZ + ((to.AngVeloZ - from.AngVeloZ) * t)
            );

        public override string ToString()
        {
            return $"Position: {PosX:N3}, {PosY:N3}, {PosZ:N3}    Angular Velocity: {AngVeloX:N3}, {AngVeloY:N3}, {AngVeloZ:N3}";
        }
    }
}
