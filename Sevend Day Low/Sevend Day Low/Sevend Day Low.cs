using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SevendDayLow : Robot
    {


        [Parameter("Risk %", DefaultValue = 0.02)]
        public double RiskPct { get; set; }


        private MovingAverage ema;
        private AverageTrueRange atr;
        public string botName;

        protected override void OnStart()
        {
            this.ema = Indicators.MovingAverage(Bars.ClosePrices, 200, MovingAverageType.Simple);
            atr = Indicators.AverageTrueRange(20, MovingAverageType.Exponential);
            botName = GetType().ToString();
            botName = botName.Substring(botName.LastIndexOf('.') + 1) + "_" + SymbolName;
        }

        protected override void OnBar()
        {
            var emaValue = this.ema.Result.Last(1);
            var isSevenDayLow = this.isSevenDayLow(Bars.Last(1).Close);
            var isSevenDayHigh = this.isSevenDayHigh(Bars.Last(1).Close);
            var position = Positions.Find(botName + "-position");
            if (position != null && isSevenDayHigh)
            {
                position.Close();
            }

            if (isSevenDayLow && Bars.Last(1).Close > emaValue && position == null)
            {

                var ATR = Math.Round(atr.Result.Last(0) / Symbol.PipSize, 0);
                var TradeAmount = (Account.Equity * RiskPct) / (1.5 * ATR * Symbol.PipValue);
                TradeAmount = Symbol.NormalizeVolumeInUnits(TradeAmount, RoundingMode.Down);


                ExecuteMarketOrder(TradeType.Buy, SymbolName, TradeAmount, botName + "-position", 1.5 * ATR, ATR);
            }

        }

        private bool isSevenDayLow(double currentClose)
        {
            var returnVal = true;
            for (int i = 2; i <= 7; i++)
            {
                Print("{0} day low: {1} and currentLow: {2}, difference: {3}", i - 1, Bars.Last(i).Close, currentClose, Bars.Last(i).Close - currentClose);
                if (currentClose >= Bars.Last(i).Close)
                {
                    return false;
                }
            }
            return returnVal;
        }


        private bool isSevenDayHigh(double currentClose)
        {
            var returnVal = true;
            for (int i = 2; i <= 7; i++)
            {
                if (currentClose <= Bars.Last(i).Close)
                {
                    return false;
                }
            }
            return returnVal;
        }
    }
}
