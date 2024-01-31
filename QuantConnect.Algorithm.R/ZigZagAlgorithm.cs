using QuantConnect.Data;
using QuantConnect.Data.Consolidators;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using QuantConnect.Securities;
using System;
using System.Globalization;

namespace QuantConnect.Algorithm.R
{
    public class ZigZagPaAlgorithm : QCAlgorithm
    {
        // this is the security we're trading
        public Security Security;

        private ZigZagIndicator _ZigZag;
        private CustomData2Consolidator _Consolidator;
        public override void Initialize()
        {
            SetStartDate(2024, 1, 24);  //Set Start Date
            SetEndDate(2024, 1, 26);    //Set End Date
            SetCash("USD", 10000);
            Security = AddData<CustomData2>("USDJPY1", Resolution.Minute);
            _ZigZag = new ZigZagIndicator(6);
            _Consolidator = new CustomData2Consolidator(TimeSpan.FromMinutes(6));
            _Consolidator.DataConsolidated += _Consolidator_DataConsolidated;
        }

        private void _ZigZag_Updated()
        {
            if (_ZigZag.NonZeroValue.Count < 5)
                return;

            //var x = _ZigZag.Current.Value > 0 ? _ZigZag.NonZeroValue[4] : 0;
            //var a = _ZigZag.Current.Value > 0 ? _ZigZag.NonZeroValue[3] : 0;
            //var b = _ZigZag.Current.Value > 0 ? _ZigZag.NonZeroValue[2] : 0;
            //var c = _ZigZag.Current.Value > 0 ? _ZigZag.NonZeroValue[1] : 0;
            //var d = _ZigZag.Current.Value > 0 ? _ZigZag.NonZeroValue[0] : 0;
            var x = _ZigZag.NonZeroValue[4];
            var a = _ZigZag.NonZeroValue[3];
            var b = _ZigZag.NonZeroValue[2];
            var c = _ZigZag.NonZeroValue[1];
            var d = _ZigZag.NonZeroValue[0];

            var ab = Math.Abs(a - b);
            var xa = Math.Abs(x - a);
            var ad = Math.Abs(a - d);
            var bc = Math.Abs(b - c);
            var cd = Math.Abs(c - d);

            if (xa == 0 || ab == 0 || bc == 0)
                return;

            var xab = ab / xa;
            var xad = ad / xa;
            var abc = bc / ab;
            var bcd = cd / bc;

            var showPattern = false;
            bool isBat(bool mode)
            {
                var r =
                    xab >= (decimal)0.382 && xab <= (decimal)0.500 &&
                    abc >= (decimal)0.382 && abc <= (decimal)0.886 &&
                    bcd >= (decimal)1.618 && bcd <= (decimal)2.618 &&
                    xad <= (decimal)0.618 && xad <= (decimal)1.000;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " Bat");
                return r;
            };

            bool isAntiBat(bool mode)
            {
                var r =
                    xab >= (decimal)0.500 && xab <= (decimal)0.886 &&
                    abc >= (decimal)1.000 && abc <= (decimal)2.618 &&
                    bcd >= (decimal)1.618 && bcd <= (decimal)2.618 &&
                    xad >= (decimal)0.886 && xad <= (decimal)1.000;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " AntiBat");
                return r;
            };

            bool isAltBat(bool mode)
            {
                var r =
                    xab <= (decimal)0.382 &&
                    abc >= (decimal)0.382 && abc <= (decimal)0.886 &&
                    bcd >= (decimal)2.000 && bcd <= (decimal)3.618 &&
                    xad <= (decimal)1.130;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " AltBat");
                return r;
            };

            bool isButterfly(bool mode)
            {
                var r =
                    xab <= (decimal)0.786 &&
                    abc >= (decimal)0.382 && abc <= (decimal)0.886 &&
                    bcd >= (decimal)1.618 && bcd <= (decimal)2.618 &&
                    xad >= (decimal)1.270 && xad <= (decimal)1.618;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " Butterfly");
                return r;
            };

            bool isAntiButterfly(bool mode)
            {
                var r =
                    xab >= (decimal)0.236 && xab <= (decimal)0.886 &&
                    abc >= (decimal)1.130 && abc <= (decimal)2.618 &&
                    bcd >= (decimal)1.000 && bcd <= (decimal)1.382 &&
                    xad >= (decimal)0.500 && xad <= (decimal)0.886;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " AntiButterfly");
                return r;
            };

            bool isABCD(bool mode)
            {
                var r =
                    abc >= (decimal)0.382 && abc <= (decimal)0.886 &&
                    bcd >= (decimal)1.130 && bcd <= (decimal)2.618;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " ABCD");
                return r;
            };

            bool isGartley(bool mode)
            {
                var r =
                    xab >= (decimal)0.500 && xab <= (decimal)0.618 &&
                    abc >= (decimal)0.382 && abc <= (decimal)0.886 &&
                    bcd >= (decimal)1.130 && bcd <= (decimal)2.618 &&
                    xad >= (decimal)0.750 && xad <= (decimal)0.875;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " Gartley");
                return r;
            };

            bool isAntiGartley(bool mode)
            {
                var r =
                    xab >= (decimal)0.500 && xab <= (decimal)0.886 &&
                    abc >= (decimal)1.000 && abc <= (decimal)2.618 &&
                    bcd >= (decimal)1.500 && bcd <= (decimal)5.000 &&
                    xad >= (decimal)1.000 && xad <= (decimal)5.000;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " AntiGartley");
                return r;
            };

            bool isCrab(bool mode)
            {
                var r =
                    xab >= (decimal)0.500 && xab <= (decimal)0.875 &&
                    abc >= (decimal)0.382 && abc <= (decimal)0.886 &&
                    bcd >= (decimal)2.000 && bcd <= (decimal)5.000 &&
                    xad >= (decimal)1.382 && xad <= (decimal)5.000;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " Crab");
                return r;
            };

            bool isAntiCrab(bool mode)
            {
                var r =
                    xab >= (decimal)0.250 && xab <= (decimal)0.500 &&
                    abc >= (decimal)1.130 && abc <= (decimal)2.618 &&
                    bcd >= (decimal)1.618 && bcd <= (decimal)2.618 &&
                    xad >= (decimal)0.500 && xad <= (decimal)0.750;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " AntiCrab");
                return r;
            };

            bool isShark(bool mode)
            {
                var r =
                    xab >= (decimal)0.500 && xab <= (decimal)0.875 &&
                    abc >= (decimal)1.130 && abc <= (decimal)1.618 &&
                    bcd >= (decimal)1.270 && bcd <= (decimal)2.240 &&
                    xad >= (decimal)0.886 && xad <= (decimal)1.130;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " Shark");
                return r;
            };

            bool isAntiShark(bool mode)
            {
                var r =
                    xab >= (decimal)0.382 && xab <= (decimal)0.875 &&
                    abc >= (decimal)0.500 && abc <= (decimal)1.000 &&
                    bcd >= (decimal)1.250 && bcd <= (decimal)2.618 &&
                    xad >= (decimal)0.500 && xad <= (decimal)1.250;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " AntiShark");
                return r;
            };

            bool is5o(bool mode)
            {
                var r =
                    xab >= (decimal)1.130 && xab <= (decimal)1.618 &&
                    abc >= (decimal)1.618 && abc <= (decimal)2.240 &&
                    bcd >= (decimal)0.500 && bcd <= (decimal)0.625 &&
                    xad >= (decimal)0.000 && xad <= (decimal)0.236;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " 5o");
                return r;
            };

            bool isWolf(bool mode)
            {
                var r =
                    xab >= (decimal)1.270 && xab <= (decimal)1.618 &&
                    abc >= (decimal)0.000 && abc <= (decimal)5.000 &&
                    bcd >= (decimal)1.270 && bcd <= (decimal)1.618 &&
                    xad >= (decimal)0.000 && xad <= (decimal)5.000;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " Wolf");
                return r;
            };

            bool isHnS(bool mode)
            {
                var r =
                    xab >= (decimal)2.000 && xab <= (decimal)10.00 &&
                    abc >= (decimal)0.900 && abc <= (decimal)1.100 &&
                    bcd >= (decimal)0.236 && bcd <= (decimal)0.880 &&
                    xad >= (decimal)0.900 && xad <= (decimal)1.100;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " HnS");
                return r;
            };

            bool isConTria(bool mode)
            {
                var r =
                    xab >= (decimal)0.382 && xab <= (decimal)0.618 &&
                    abc >= (decimal)0.382 && abc <= (decimal)0.618 &&
                    bcd >= (decimal)0.382 && bcd <= (decimal)0.618 &&
                    xad >= (decimal)0.236 && xad <= (decimal)0.764;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " ConTria");
                return r;
            };

            bool isExpTria(bool mode)
            {
                var r =
                    xab >= (decimal)1.236 && xab <= (decimal)1.618 &&
                    abc >= (decimal)1.000 && abc <= (decimal)1.618 &&
                    bcd >= (decimal)1.236 && bcd <= (decimal)2.000 &&
                    xad >= (decimal)2.000 && xad <= (decimal)2.236;
                r &= mode ? d < c : d > c;
                if (r && showPattern) Debug(_Current.Time + " ExpTria");
                return r;
            };

            var fib_range = Math.Abs(d - c);

            decimal f_last_fib(decimal rate)
            {
                var r =
                 d > c ? d - (fib_range * rate) : d + (fib_range * rate);
                return r;
            };

            var buy_partterns_00 = isABCD(true) || isBat(true) || isAltBat(true) || isButterfly(true) || isGartley(true) || isCrab(true) || isShark(true) || isShark(true) || is5o(true) || isWolf(true) || isHnS(true) || isConTria(true) || isExpTria(true);
            var buy_partterns_01 = isAntiBat(true) || isAntiButterfly(true) || isAntiGartley(true) || isAntiCrab(true) || isAntiShark(true);
            var sel_partterns_00 = isABCD(false) || isBat(false) || isAltBat(false) || isButterfly(false) || isGartley(false) || isCrab(false) || isShark(false) || isShark(false) || is5o(false) || isWolf(false) || isHnS(false) || isConTria(false) || isExpTria(false);
            var sel_partterns_01 = isAntiBat(false) || isAntiButterfly(false) || isAntiGartley(false) || isAntiCrab(false) || isAntiShark(false);

            var target01_trade_size = (decimal)10000;
            var target01_ew_rate = (decimal)0.236;
            var target01_tp_rate = (decimal)0.618;
            var target01_sl_rate = (decimal)-0.236;

            var target01_buy_entry = (buy_partterns_00 || buy_partterns_01) && _Current.Close <= f_last_fib(target01_ew_rate);
            var target01_buy_close = _Current.High >= f_last_fib(target01_tp_rate) || _Current.Low <= f_last_fib(target01_sl_rate);
            var target01_sel_entry = (sel_partterns_00 || sel_partterns_01) && _Current.Close >= f_last_fib(target01_ew_rate);
            var target01_sel_close = _Current.Low <= f_last_fib(target01_tp_rate) || _Current.High >= f_last_fib(target01_sl_rate);

            if (target01_buy_entry)
            {
                var ticket = Buy(Security.Symbol, 2);
                Debug(_Current.Time + " Execute buy at" + _Current.Value);
            }
            if (!target01_buy_entry && target01_buy_close)
            {
                var r = Liquidate(Security.Symbol);
                if (r.Count > 0)
                    Debug(_Current.Time + " Close buy at" + _Current.Value);
            }
            if (target01_sel_entry)
            {
                Sell(Security.Symbol, 1);
            }
            if (target01_sel_close)
            {
                Liquidate(Security.Symbol);
            }
        }


        private void _Consolidator_DataConsolidated(object sender, TradeBar e)
        {
            _ZigZag.Update(e);
        }

        public override void OnData(Slice slice)
        {
            var data = slice.Get<CustomData2>();
            _Current = data[Security.Symbol];
            _Consolidator.Update(data[Security.Symbol]);
            //_ZigZag.Update(_Current);
            _ZigZag_Updated();
        }

        private CustomData2 _Current;
    }

    public class CustomData2Consolidator : TradeBarConsolidatorBase<CustomData2>
    {
        public CustomData2Consolidator(TimeSpan period)
            : base(period)
        {
        }
        protected override void AggregateBar(ref TradeBar workingBar, CustomData2 data)
        {
            if (workingBar == null)
            {
                workingBar = new TradeBar
                {
                    Symbol = data.Symbol,
                    Time = GetRoundedBarTime(data.Time),
                    Close = data.Close,
                    High = data.High,
                    Low = data.Low,
                    Open = data.Open,
                    DataType = data.DataType,
                    Value = data.Value,
                    Volume = data.Volume,
                };
            }
            else
            {
                //Aggregate the working bar
                workingBar.Close = data.Value;
                workingBar.Volume += data.Volume;
                if (data.Low < workingBar.Low) workingBar.Low = data.Low;
                if (data.High > workingBar.High) workingBar.High = data.High;
            }
        }
    }

    /// <summary>
    /// Custom data from local LEAN data
    /// </summary>
    public class CustomData2 : BaseData, IBaseDataBar
    {
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }

        public override DateTime EndTime
        {
            get { return Time + Period; }
            set { Time = value - Period; }
        }

        public TimeSpan Period
        {
            get { return QuantConnect.Time.OneMinute * 1; }
        }

        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            //var source = Path.Combine(Globals.DataFolder, "equity", "usa", config.Resolution.ToString().ToLower(), LeanData.GenerateZipFileName(config.Symbol, date, config.Resolution, config.TickType));
            var source = System.IO.Path.Combine(Globals.DataFolder, "forex", "MT4", $"{config.Symbol.Value.ToLowerInvariant()}.csv");
            return new SubscriptionDataSource(source, SubscriptionTransportMedium.LocalFile, FileFormat.Csv);
        }

        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            var csv = line.ToCsv();

            var custom = new CustomData2
            {
                Symbol = config.Symbol,
                Time = DateTime.ParseExact(csv[0] + "-" + csv[1], "yyyy.MM.dd-HH:mm", CultureInfo.InvariantCulture),
                Open = csv[2].ToDecimal(),
                High = csv[3].ToDecimal(),
                Low = csv[4].ToDecimal(),
                Close = csv[5].ToDecimal(),
                Value = csv[5].ToDecimal(),
                Volume = csv[6].ToDecimal(),
            };
            return custom;
        }

        public override bool RequiresMapping()
        {
            return false;
        }
    }
}

/*
 strategy(title='[STRATEGY][RS]ZigZag PA Strategy V4.1', shorttitle='S', overlay=true, pyramiding=0, initial_capital=100000, currency=currency.USD)
useHA = input(false, title='Use Heikken Ashi Candles')
useAltTF = input(true, title='Use Alt Timeframe')
tf = input('60', title='Alt Timeframe')
showPatterns = input(true, title='Show Patterns')
showFib0000 = input(title='Display Fibonacci 0.000:', type=bool, defval=true)
showFib0236 = input(title='Display Fibonacci 0.236:', type=bool, defval=true)
showFib0382 = input(title='Display Fibonacci 0.382:', type=bool, defval=true)
showFib0500 = input(title='Display Fibonacci 0.500:', type=bool, defval=true)
showFib0618 = input(title='Display Fibonacci 0.618:', type=bool, defval=true)
showFib0764 = input(title='Display Fibonacci 0.764:', type=bool, defval=true)
showFib1000 = input(title='Display Fibonacci 1.000:', type=bool, defval=true)
zigzag() =>
    _isUp = close >= open
    _isDown = close <= open
    _direction = _isUp[1] and _isDown ? -1 : _isDown[1] and _isUp ? 1 : nz(_direction[1])
    _zigzag = _isUp[1] and _isDown and _direction[1] != -1 ? highest(2) : _isDown[1] and _isUp and _direction[1] != 1 ? lowest(2) : na

_ticker = useHA ? heikenashi(tickerid) : tickerid
sz = useAltTF ? (change(time(tf)) != 0 ? security(_ticker, tf, zigzag()) : na) : zigzag()

plot(sz, title='zigzag', color=black, linewidth=1)

//  ||---   Pattern Recognition:

x = valuewhen(sz, sz, 4) 
a = valuewhen(sz, sz, 3) 
b = valuewhen(sz, sz, 2) 
c = valuewhen(sz, sz, 1) 
d = valuewhen(sz, sz, 0)

xab = (abs(b-a)/abs(x-a))
xad = (abs(a-d)/abs(x-a))
abc = (abs(b-c)/abs(a-b))
bcd = (abs(c-d)/abs(b-c))

//  ||-->   Functions:
isBat(_mode)=>
    _xab = xab >= 0.382 and xab <= 0.5
    _abc = abc >= 0.382 and abc <= 0.886
    _bcd = bcd >= 1.618 and bcd <= 2.618
    _xad = xad <= 0.618 and xad <= 1.000    // 0.886
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

isAntiBat(_mode)=>
    _xab = xab >= 0.500 and xab <= 0.886    // 0.618
    _abc = abc >= 1.000 and abc <= 2.618    // 1.13 --> 2.618
    _bcd = bcd >= 1.618 and bcd <= 2.618    // 2.0  --> 2.618
    _xad = xad >= 0.886 and xad <= 1.000    // 1.13
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

isAltBat(_mode)=>
    _xab = xab <= 0.382
    _abc = abc >= 0.382 and abc <= 0.886
    _bcd = bcd >= 2.0 and bcd <= 3.618
    _xad = xad <= 1.13
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

isButterfly(_mode)=>
    _xab = xab <= 0.786
    _abc = abc >= 0.382 and abc <= 0.886
    _bcd = bcd >= 1.618 and bcd <= 2.618
    _xad = xad >= 1.27 and xad <= 1.618
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

isAntiButterfly(_mode)=>
    _xab = xab >= 0.236 and xab <= 0.886    // 0.382 - 0.618
    _abc = abc >= 1.130 and abc <= 2.618    // 1.130 - 2.618
    _bcd = bcd >= 1.000 and bcd <= 1.382    // 1.27
    _xad = xad >= 0.500 and xad <= 0.886    // 0.618 - 0.786
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

isABCD(_mode)=>
    _abc = abc >= 0.382 and abc <= 0.886
    _bcd = bcd >= 1.13 and bcd <= 2.618
    _abc and _bcd and (_mode == 1 ? d < c : d > c)

isGartley(_mode)=>
    _xab = xab >= 0.5 and xab <= 0.618 // 0.618
    _abc = abc >= 0.382 and abc <= 0.886
    _bcd = bcd >= 1.13 and bcd <= 2.618
    _xad = xad >= 0.75 and xad <= 0.875 // 0.786
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

isAntiGartley(_mode)=>
    _xab = xab >= 0.500 and xab <= 0.886    // 0.618 -> 0.786
    _abc = abc >= 1.000 and abc <= 2.618    // 1.130 -> 2.618
    _bcd = bcd >= 1.500 and bcd <= 5.000    // 1.618
    _xad = xad >= 1.000 and xad <= 5.000    // 1.272
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

isCrab(_mode)=>
    _xab = xab >= 0.500 and xab <= 0.875    // 0.886
    _abc = abc >= 0.382 and abc <= 0.886    
    _bcd = bcd >= 2.000 and bcd <= 5.000    // 3.618
    _xad = xad >= 1.382 and xad <= 5.000    // 1.618
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

isAntiCrab(_mode)=>
    _xab = xab >= 0.250 and xab <= 0.500    // 0.276 -> 0.446
    _abc = abc >= 1.130 and abc <= 2.618    // 1.130 -> 2.618
    _bcd = bcd >= 1.618 and bcd <= 2.618    // 1.618 -> 2.618
    _xad = xad >= 0.500 and xad <= 0.750    // 0.618
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

isShark(_mode)=>
    _xab = xab >= 0.500 and xab <= 0.875    // 0.5 --> 0.886
    _abc = abc >= 1.130 and abc <= 1.618    //
    _bcd = bcd >= 1.270 and bcd <= 2.240    //
    _xad = xad >= 0.886 and xad <= 1.130    // 0.886 --> 1.13
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

isAntiShark(_mode)=>
    _xab = xab >= 0.382 and xab <= 0.875    // 0.446 --> 0.618
    _abc = abc >= 0.500 and abc <= 1.000    // 0.618 --> 0.886
    _bcd = bcd >= 1.250 and bcd <= 2.618    // 1.618 --> 2.618
    _xad = xad >= 0.500 and xad <= 1.250    // 1.130 --> 1.130
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

is5o(_mode)=>
    _xab = xab >= 1.13 and xab <= 1.618
    _abc = abc >= 1.618 and abc <= 2.24
    _bcd = bcd >= 0.5 and bcd <= 0.625 // 0.5
    _xad = xad >= 0.0 and xad <= 0.236 // negative?
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

isWolf(_mode)=>
    _xab = xab >= 1.27 and xab <= 1.618
    _abc = abc >= 0 and abc <= 5
    _bcd = bcd >= 1.27 and bcd <= 1.618
    _xad = xad >= 0.0 and xad <= 5
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

isHnS(_mode)=>
    _xab = xab >= 2.0 and xab <= 10
    _abc = abc >= 0.90 and abc <= 1.1
    _bcd = bcd >= 0.236 and bcd <= 0.88
    _xad = xad >= 0.90 and xad <= 1.1
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

isConTria(_mode)=>
    _xab = xab >= 0.382 and xab <= 0.618
    _abc = abc >= 0.382 and abc <= 0.618
    _bcd = bcd >= 0.382 and bcd <= 0.618
    _xad = xad >= 0.236 and xad <= 0.764
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

isExpTria(_mode)=>
    _xab = xab >= 1.236 and xab <= 1.618
    _abc = abc >= 1.000 and abc <= 1.618
    _bcd = bcd >= 1.236 and bcd <= 2.000
    _xad = xad >= 2.000 and xad <= 2.236
    _xab and _abc and _bcd and _xad and (_mode == 1 ? d < c : d > c)

// plotshape(not showPatterns ? na : isABCD(-1) and not isABCD(-1)[1], text="\nAB=CD", title='Bear ABCD', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0, offset=-2)
// plotshape(not showPatterns ? na : isBat(-1) and not isBat(-1)[1], text="Bat", title='Bear Bat', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0, offset=-2)
// plotshape(not showPatterns ? na : isAntiBat(-1) and not isAntiBat(-1)[1], text="Anti Bat", title='Bear Anti Bat', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0, offset=-2)
// plotshape(not showPatterns ? na : isAltBat(-1) and not isAltBat(-1)[1], text="Alt Bat", title='Bear Alt Bat', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0)
// plotshape(not showPatterns ? na : isButterfly(-1) and not isButterfly(-1)[1], text="Butterfly", title='Bear Butterfly', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0)
// plotshape(not showPatterns ? na : isAntiButterfly(-1) and not isAntiButterfly(-1)[1], text="Anti Butterfly", title='Bear Anti Butterfly', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0)
// plotshape(not showPatterns ? na : isGartley(-1) and not isGartley(-1)[1], text="Gartley", title='Bear Gartley', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0)
// plotshape(not showPatterns ? na : isAntiGartley(-1) and not isAntiGartley(-1)[1], text="Anti Gartley", title='Bear Anti Gartley', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0)
// plotshape(not showPatterns ? na : isCrab(-1) and not isCrab(-1)[1], text="Crab", title='Bear Crab', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0)
// plotshape(not showPatterns ? na : isAntiCrab(-1) and not isAntiCrab(-1)[1], text="Anti Crab", title='Bear Anti Crab', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0)
// plotshape(not showPatterns ? na : isShark(-1) and not isShark(-1)[1], text="Shark", title='Bear Shark', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0)
// plotshape(not showPatterns ? na : isAntiShark(-1) and not isAntiShark(-1)[1], text="Anti Shark", title='Bear Anti Shark', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0)
// plotshape(not showPatterns ? na : is5o(-1) and not is5o(-1)[1], text="5-O", title='Bear 5-O', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0)
// plotshape(not showPatterns ? na : isWolf(-1) and not isWolf(-1)[1], text="Wolf Wave", title='Bear Wolf Wave', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0)
// plotshape(not showPatterns ? na : isHnS(-1) and not isHnS(-1)[1], text="Head and Shoulders", title='Bear Head and Shoulders', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0)
// plotshape(not showPatterns ? na : isConTria(-1) and not isConTria(-1)[1], text="Contracting Triangle", title='Bear Contracting triangle', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0)
// plotshape(not showPatterns ? na : isExpTria(-1) and not isExpTria(-1)[1], text="Expanding Triangle", title='Bear Expanding Triangle', style=shape.labeldown, color=maroon, textcolor=white, location=location.top, transp=0)

// plotshape(not showPatterns ? na : isABCD(1) and not isABCD(1)[1], text="AB=CD\n", title='Bull ABCD', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isBat(1) and not isBat(1)[1], text="Bat", title='Bull Bat', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isAntiBat(1) and not isAntiBat(1)[1], text="Anti Bat", title='Bull Anti Bat', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isAltBat(1) and not isAltBat(1)[1], text="Alt Bat", title='Bull Alt Bat', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isButterfly(1) and not isButterfly(1)[1], text="Butterfly", title='Bull Butterfly', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isAntiButterfly(1) and not isAntiButterfly(1)[1], text="Anti Butterfly", title='Bull Anti Butterfly', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isGartley(1) and not isGartley(1)[1], text="Gartley", title='Bull Gartley', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isAntiGartley(1) and not isAntiGartley(1)[1], text="Anti Gartley", title='Bull Anti Gartley', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isCrab(1) and not isCrab(1)[1], text="Crab", title='Bull Crab', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isAntiCrab(1) and not isAntiCrab(1)[1], text="Anti Crab", title='Bull Anti Crab', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isShark(1) and not isShark(1)[1], text="Shark", title='Bull Shark', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isAntiShark(1) and not isAntiShark(1)[1], text="Anti Shark", title='Bull Anti Shark', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : is5o(1) and not is5o(1)[1], text="5-O", title='Bull 5-O', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isWolf(1) and not isWolf(1)[1], text="Wolf Wave", title='Bull Wolf Wave', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isHnS(1) and not isHnS(1)[1], text="Head and Shoulders", title='Bull Head and Shoulders', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isConTria(1) and not isConTria(1)[1], text="Contracting Triangle", title='Bull Contracting Triangle', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)
// plotshape(not showPatterns ? na : isExpTria(1) and not isExpTria(1)[1], text="Expanding Triangle", title='Bull Expanding Triangle', style=shape.labelup, color=green, textcolor=white, location=location.bottom, transp=0)

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
fib_range = abs(d-c)
fib_0000 = not showFib0000 ? na : d > c ? d-(fib_range*0.000):d+(fib_range*0.000)
fib_0236 = not showFib0236 ? na : d > c ? d-(fib_range*0.236):d+(fib_range*0.236)
fib_0382 = not showFib0382 ? na : d > c ? d-(fib_range*0.382):d+(fib_range*0.382)
fib_0500 = not showFib0500 ? na : d > c ? d-(fib_range*0.500):d+(fib_range*0.500)
fib_0618 = not showFib0618 ? na : d > c ? d-(fib_range*0.618):d+(fib_range*0.618)
fib_0764 = not showFib0764 ? na : d > c ? d-(fib_range*0.764):d+(fib_range*0.764)
fib_1000 = not showFib1000 ? na : d > c ? d-(fib_range*1.000):d+(fib_range*1.000)
// plot(title='Fib 0.000', series=fib_0000, color=fib_0000 != fib_0000[1] ? na : black)
// plot(title='Fib 0.236', series=fib_0236, color=fib_0236 != fib_0236[1] ? na : red)
// plot(title='Fib 0.382', series=fib_0382, color=fib_0382 != fib_0382[1] ? na : olive)
// plot(title='Fib 0.500', series=fib_0500, color=fib_0500 != fib_0500[1] ? na : lime)
// plot(title='Fib 0.618', series=fib_0618, color=fib_0618 != fib_0618[1] ? na : teal)
// plot(title='Fib 0.764', series=fib_0764, color=fib_0764 != fib_0764[1] ? na : blue)
// plot(title='Fib 1.000', series=fib_1000, color=fib_1000 != fib_1000[1] ? na : black)

bgcolor(not useAltTF ? na : change(time(tf))!=0?black:na)
f_last_fib(_rate)=>d > c ? d-(fib_range*_rate):d+(fib_range*_rate)

target01_trade_size = input(title='Target 1 - Trade size:', type=float, defval=10000.00)
target01_ew_rate = input(title='Target 1 - Fib. Rate to use for Entry Window:', type=float, defval=0.236)
target01_tp_rate = input(title='Target 1 - Fib. Rate to use for TP:', type=float, defval=0.618)
target01_sl_rate = input(title='Target 1 - Fib. Rate to use for SL:', type=float, defval=-0.236)
target02_active = input(title='Target 2 - Active?', type=bool, defval=false)
target02_trade_size = input(title='Target 2 - Trade size:', type=float, defval=10000.00)
target02_ew_rate = input(title='Target 2 - Fib. Rate to use for Entry Window:', type=float, defval=0.236)
target02_tp_rate = input(title='Target 2 - Fib. Rate to use for TP:', type=float, defval=1.618)
target02_sl_rate = input(title='Target 2 - Fib. Rate to use for SL:', type=float, defval=-0.236)

buy_patterns_00 = isABCD(1) or isBat(1) or isAltBat(1) or isButterfly(1) or isGartley(1) or isCrab(1) or isShark(1) or is5o(1) or isWolf(1) or isHnS(1) or isConTria(1) or isExpTria(1)
buy_patterns_01 = isAntiBat(1) or isAntiButterfly(1) or isAntiGartley(1) or isAntiCrab(1) or isAntiShark(1)
sel_patterns_00 = isABCD(-1) or isBat(-1) or isAltBat(-1) or isButterfly(-1) or isGartley(-1) or isCrab(-1) or isShark(-1) or is5o(-1) or isWolf(-1) or isHnS(-1) or isConTria(-1) or isExpTria(-1)
sel_patterns_01 = isAntiBat(-1) or isAntiButterfly(-1) or isAntiGartley(-1) or isAntiCrab(-1) or isAntiShark(-1)

target01_buy_entry = (buy_patterns_00 or buy_patterns_01) and close <= f_last_fib(target01_ew_rate)
target01_buy_close = high >= f_last_fib(target01_tp_rate) or low <= f_last_fib(target01_sl_rate)
target01_sel_entry = (sel_patterns_00 or sel_patterns_01) and close >= f_last_fib(target01_ew_rate)
target01_sel_close = low <= f_last_fib(target01_tp_rate) or high >= f_last_fib(target01_sl_rate)

strategy.entry('target01_buy', long=strategy.long, qty=target01_trade_size, comment='buy 01', when=target01_buy_entry)
strategy.close('target01_buy', when=target01_buy_close)
strategy.entry('target01_sell', long=strategy.short, qty=target01_trade_size, comment='sell 01', when=target01_sel_entry)
strategy.close('target01_sell', when=target01_sel_close)

target02_buy_entry = target02_active and (buy_patterns_00 or buy_patterns_01) and close <= f_last_fib(target02_ew_rate)
target02_buy_close = target02_active and high >= f_last_fib(target02_tp_rate) or low <= f_last_fib(target02_sl_rate)
target02_sel_entry = target02_active and (sel_patterns_00 or sel_patterns_01) and close >= f_last_fib(target02_ew_rate)
target02_sel_close = target02_active and low <= f_last_fib(target02_tp_rate) or high >= f_last_fib(target02_sl_rate)

strategy.entry('target02_buy', long=strategy.long, qty=target02_trade_size, comment='buy 02', when=target02_buy_entry)
strategy.close('target02_buy', when=target02_buy_close)
strategy.entry('target02_sell', long=strategy.short, qty=target02_trade_size, comment='sell 02', when=target02_sel_entry)
strategy.close('target02_sell', when=target02_sel_close)

 */

