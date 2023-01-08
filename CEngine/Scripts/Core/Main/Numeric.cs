namespace CYM
{
    public class Numeric
    {
        public float Value => (Base + FixedAdd) * (1 + PercentAdd);
        public float Base { get; private set; }
        public float FixedAdd { get; private set; }
        public float PercentAdd { get; private set; }

        public void Reset()
        {
            Base = FixedAdd = PercentAdd = 0;
        }
        public float SetBase(float value)
        {
            Base = value;
            return Base;
        }
        public float Add(float value)
        {
            FixedAdd += value;
            return FixedAdd;
        }
        public float PctAdd(float value)
        {
            PercentAdd += value;
            return PercentAdd;
        }
    }
}