using QuantConnect.Data.Market;
using QuantConnect.Indicators;


namespace QuantConnect.Algorithm.R
{
    public class ZigZagIndicator : WindowIndicator<IBaseDataBar>
    {
        //private readonly int _period;
        private readonly Maximum _maximum;
        private readonly Minimum _minimum;
        private readonly RollingWindow<int> _direction;
        private readonly RollingWindow<decimal> _zigzag;
        private readonly int _period;
        public RollingWindow<decimal> NonZeroValue => _zigzag;
        public ZigZagIndicator(int period)
            : base($"ZZ({period})", period)
        {
            Window.Size = 100;
            _period = period;
            _maximum = new Maximum(period);
            _minimum = new Minimum(period);
            _direction = new RollingWindow<int>(period);
            _zigzag = new RollingWindow<decimal>(period);
        }

        protected override decimal ComputeNextValue(IReadOnlyWindow<IBaseDataBar> window, IBaseDataBar input)
        {            

            var input1 = window.Count > 1 ? window[1] : null;
            if (input1 == null)
            {
                Window.Reset();
                if (input.IsUp())
                {
                    _direction.Add(1);
                    return input.High;
                }
                else if (input.IsDown())
                {
                    _direction.Add(-1);
                    return input.Low;
                }
            }
            var max = Math.Max(input.High, input1.High);
            var min = Math.Min(input.Low, input1.Low);
            var isUp0 = input.Close >= input.Open;
            var isDown0 = input.Close <= input.Open;
            var isUp1 = input1.Close >= input1.Open;
            var isDown1 = input1.Close <= input1.Open;
            var direction1 = _direction.Count > 0 ? _direction[0] : 0;
            var direction0 = isUp1 && isDown0 ? -1 : isDown1 && isUp0 ? 1 : direction1;
            _direction.Add(direction0);
            var zigzag = 0m;

            if (isUp1 && isDown0 && direction1 != -1)
            {
                if (this[0].Value > 0)
                    this[0].Value = max;
                zigzag = input.Low;
            }

            if (isDown1 && isUp0 && direction1 != 1)
            {
                if (this[0].Value > 0)
                    this[0].Value = min;
                zigzag = input.High;
            }
            //_zigzag = _isUp[1] and _isDown and _direction[1] != -1 ? highest(2) : _isDown[1] and _isUp and _direction[1] != 1 ? lowest(2) : na
            //var zigzag = isUp1 && isDown0 && direction1 != -1 ? max : isDown1 && isUp0 && direction1 != 1 ? min : 0;
            if (zigzag > 0)
                this._zigzag.Add(zigzag);
            //input.EndTime = input.Time;
            return zigzag;
        }

        protected override IndicatorResult ValidateAndComputeNextValue(IBaseDataBar input)
        {
            // default implementation always returns IndicatorStatus.Success
            var res = ComputeNextValue(input);
            return new IndicatorResult(res, res >= 0 ? IndicatorStatus.Success : IndicatorStatus.InvalidInput);
        }
    }

    public static class HelperClass
    {
        public static bool IsUp(this IBaseDataBar data)
        {
            return data.Close >= data.Open;
        }

        public static bool IsDown(this IBaseDataBar data)
        {
            return data.Close <= data.Open;
        }
    }
}

