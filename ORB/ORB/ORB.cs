using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None, AddIndicators = true)]
    public class ORB : Robot
    {
        [Parameter("Range Start Time", DefaultValue = "13:30", Group = "Times")]
        public string RangeStartTime { get; set; }

        [Parameter("Zone Color", DefaultValue = "Gray", Group = "Colors")]
        public Color ZoneColor { get; set; }

        [Parameter("Lots", DefaultValue = 1, Group = "Trading")]
        public double Volume { get; set; }

        [Parameter("SL pips", DefaultValue = 20, Group = "Trading")]
        public double StopLoss { get; set; }

        [Parameter("TP pips", DefaultValue = 20, Group = "Trading")]
        public double TakeProfit { get; set; }

        [Parameter("Use Loss Multiplier", DefaultValue = true, Group = "Martingale")]
        public bool UseMultiplier { get; set; }

        [Parameter("Loss Multiplier", DefaultValue = 2, Group = "Martingale")]
        public double Exponent { get; set; }

        private TimeOnly _rangeStartTime;
        private ORZ _orz = new();
        private double _volume;
        private bool tradeOccured = false;

        protected override void OnStart()
        {
            _rangeStartTime = ConvertToTime(RangeStartTime);
            _volume = Symbol.QuantityToVolumeInUnits(Volume);
            InitializeZones();
        }

        protected override void OnTick()
        {
            DrawZone();
            EnterTrade();
        }

        protected override void OnBar()
        {
            InitializeZones();
            Martingale();
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }

        #region Times

        private TimeOnly ConvertToTime(string timeString)
        {
            try
            {
                TimeOnly time = TimeOnly.ParseExact(timeString, "HH:mm");
                return time;
            }
            catch (Exception ex)
            {
                Print($"Error: {ex.Message} Please enter a valid time.");
                Stop();
                return TimeOnly.MinValue;
            }
        }

        #endregion

        public class ORZ
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public double High { get; set; }
            public double Low { get; set; }
        }

        private void InitializeZones()
        {
            if (Server.Time.Hour < _rangeStartTime.Hour)
            {
                _orz = new ORZ
                {
                    StartTime = new DateTime(Server.Time.Year, Server.Time.Month, Server.Time.Day, _rangeStartTime.Hour, _rangeStartTime.Minute, _rangeStartTime.Second),
                    EndTime = new DateTime(Server.Time.Year, Server.Time.Month, Server.Time.Day, _rangeStartTime.Hour, _rangeStartTime.AddMinutes(15).Minute, _rangeStartTime.Second)
                };

            }
        }

        private bool IsRangeTime()=> Server.Time.Hour == _rangeStartTime.Hour 
            && Server.Time.Minute <= _rangeStartTime.AddMinutes(15).Minute
            && Server.Time.Minute >= 30;

        private bool IsTradingTime() => Server.Time >= _orz.EndTime.AddMinutes(1);

        public (double High, double Low) FindHighLow(int startIndex, int endIndex)
        {
            double highestPrice = double.MinValue;
            double lowestPrice = double.MaxValue;

            for (int i = startIndex; i <= endIndex; i++)
            {
                var high = Bars.HighPrices[i];
                var low = Bars.LowPrices[i];

                highestPrice = Math.Max(highestPrice, high);
                lowestPrice = Math.Min(lowestPrice, low);
            }

            return (highestPrice, lowestPrice);
        }

        private void DrawZone()
        {
            if (IsRangeTime())
            {
                var startIndex = Bars.OpenTimes.GetIndexByExactTime(_orz.StartTime);
                var endIndex = Bars.OpenTimes.GetIndexByExactTime(Bars.OpenTimes.LastValue);

                _orz.High = FindHighLow(startIndex, endIndex).High;
                _orz.Low = FindHighLow(startIndex, endIndex).Low;

                Chart.DrawRectangle($"{_orz.StartTime} Zone", _orz.StartTime, _orz.Low, _orz.EndTime, _orz.High, ZoneColor).IsFilled = true;

                tradeOccured = false;
            }
        }

        private void EnterTrade()
        {
            if (!IsTradingTime())
                return;
            var currentPrice = Bars.ClosePrices.LastValue;
            var currentAsk = Symbol.Ask;
            var currentBid = Symbol.Bid;

            if (_orz.High == 0 || _orz.Low == 0)
                return;

            if (currentPrice < _orz.Low && !tradeOccured)
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, _volume, $"ORB_Sell", StopLoss, TakeProfit);
                tradeOccured = true;
            }
            else if (currentPrice > _orz.High && !tradeOccured)
            {
                Print(_orz.High);
                ExecuteMarketOrder(TradeType.Buy, SymbolName, _volume, $"ORB_Sell", StopLoss, TakeProfit);
                tradeOccured = true;
            }
        }

        private int lastLength = 0;

        private void Martingale()
        {
            if (!UseMultiplier)
                return;

            var newList = History.Where(p => p.Label.StartsWith("ORB"));

            if (!newList.Any())
                return;

            if (newList.Count() > lastLength)
                lastLength = newList.Count();
            else
                return;

            if (newList.Last().Pips < 0)
            {
                _volume = Math.Round(_volume * Exponent, 1);
            }
            else
                _volume = Symbol.QuantityToVolumeInUnits(Volume);
        }
    }
}