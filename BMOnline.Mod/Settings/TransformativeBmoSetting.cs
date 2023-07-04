using System;

namespace BMOnline.Mod.Settings
{
    internal class TransformativeBmoSetting<T> : BmoSetting<T>
    {
        private Func<T, T> transformationFunction;

        public TransformativeBmoSetting(T value, Func<T, T> transformationFunction) : base(transformationFunction(value))
        {
            this.transformationFunction = transformationFunction;
        }

        public override void SetValue(T value)
        {
            base.SetValue(transformationFunction(value));
        }
    }
}
