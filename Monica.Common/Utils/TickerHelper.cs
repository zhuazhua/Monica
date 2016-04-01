using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Monica.Common.Pocos;
using Platinum.Common.Pocos;
using Platinum.Common.Utils;

namespace Monica.Common.Utils
{
    internal struct TradingSessionInfo
    {
        public DateTime Date { get; set; }
        public string DaySessionString { get; set; }
        public string NightSessionString { get; set; }
        public int ExchangeTimeZone { get; set; }
        public string ProductInfo { get; set; }
    }

    //Ticker: <Code>.<Exchange>
    //ProductInfo <Product>.<Exchange>
    public class TickerHelper
    {
      

        private static readonly Regex MonthRegex = new Regex("\\d{6}", RegexOptions.Compiled);

        private static readonly Regex AnthonyTickerRegex = new Regex(".*,.*,\\d{6}",RegexOptions.Compiled);

        private static readonly object SyncRoot = new Object();

        private static bool _isInit;

        private static Dictionary<string, string> _externalToInternal;

        private static Dictionary<string, GeneralTickerInfo> _generalTickerInfoDict;

        private static Dictionary<string, List<TradingSessionInfo>> _tradingSessionDict;

        private static HashSet<string> _csi300Tickers;

        private static HashSet<string> _stockFundsTickers;

        private static HashSet<string> _bondFundsTickers;

        private static HashSet<string> _currencyFundsTickers;

        #region Private

        private static TradingSessionInfo GetTradingSessionInfo(string productInfo, DateTime date)
        {
            if (_isInit == false)
                Init();
            if (_tradingSessionDict.ContainsKey(productInfo))
            {
                return _tradingSessionDict[productInfo].First(t => t.Date <= date);
            }
            else
            {
                return new TradingSessionInfo()
                {
                    Date = date,
                    DaySessionString = "093000-113000&130000-150000",
                    ExchangeTimeZone = 210,
                    NightSessionString = ""
                };
            }
        }

        public static TradingSession GetTradingSessionByProductInfo(string productInfo, string dateString = null,
            int timeZoneIndex = 210)
        {
            var date = string.IsNullOrEmpty(dateString)
                ? DateTime.Today
                : DateTime.ParseExact(dateString, GeneralConstants.DateFormat, null);
            var tradingSessionInfo = GetTradingSessionInfo(productInfo, date);
            var tradingSessions = new List<string>();
            if (string.IsNullOrEmpty(tradingSessionInfo.DaySessionString) == false)
                tradingSessions.Add(tradingSessionInfo.DaySessionString);
            if (string.IsNullOrEmpty(tradingSessionInfo.NightSessionString) == false)
                tradingSessions.Add(tradingSessionInfo.NightSessionString);
            var tradingSessionString = string.Join("&", tradingSessions);
            var tradingSession = new TradingSession(tradingSessionString,
                tradingSessionInfo.ExchangeTimeZone, date);
            tradingSession.Shift(timeZoneIndex);
            return tradingSession;
        }

        private static string GetKorenMonth(string orignalMonth)
        {
            var year = int.Parse(orignalMonth.Substring(0, 4));
            var yearChar = (char) ((year - 2005) + 'A');
            var month = int.Parse(orignalMonth.Substring(4)).ToString();
            if (month == "10")
                month = "A";
            if (month == "11")
                month = "B";
            if (month == "12")
                month = "C";
            return yearChar + month;
        }

        private static string ParseKoreanMonth(string koreanMonth)
        {
            var yearChar = koreanMonth.ElementAt(0);
            var year = (2005 + (int) (yearChar - 'A')).ToString();
            var monthString = koreanMonth.Substring(1);
            if (monthString == "A")
                monthString = "10";
            else if (monthString == "B")
                monthString = "11";
            else if (monthString == "C")
                monthString = "12";

            var month = int.Parse(monthString).ToString("D2");
            return year + month;
        }


        private static string GetCZCEMonth(string originalMonth)
        {
            return originalMonth.Substring(originalMonth.Length - 3);
        }

        private static string GetFutureMonth(string originalMonth)
        {
            return originalMonth.Substring(originalMonth.Length - 4);
        }

        private static string ParseCZCEMonth(string CZCEMonth)
        {
            return "201" + CZCEMonth;
        }

        private static string ParseFutureMonth(string futureMonth)
        {
            return "20" + futureMonth;
        }

        private static void LoadGeneralTickerInfos()
        {
            if (File.Exists(TickerConstants.GeneralTickerInfoPath) == false)
                throw new Exception(TickerConstants.GeneralTickerInfoPath + " does not exist.");
            _externalToInternal = new Dictionary<string, string>();
            _generalTickerInfoDict = new Dictionary<string, GeneralTickerInfo>();
            var generalTickerInfos =
                File.ReadAllLines(TickerConstants.GeneralTickerInfoPath).Skip(1)
                    .Select(line => GeneralTickerInfo.ParseFromCsv(line)).ToList();
            generalTickerInfos.ForEach(g =>
            {
                _externalToInternal.Add(g.ProductInfo, g.InternalProductInfo);
                _generalTickerInfoDict.Add(g.InternalProductInfo, g);
            });
        }

        private static void LoadIndexInfos()
        {
            if (File.Exists(TickerConstants.IndexPath) == false)
                throw new Exception(TickerConstants.IndexPath + " does not exist.");
            _csi300Tickers = new HashSet<string>();
            var lines = File.ReadAllLines(TickerConstants.IndexPath);
            foreach (var line in lines)
            {
                var datas = line.Split(',');
                if (datas[0] == "000300")
                    _csi300Tickers.Add(datas[1] + "." + datas[3]);
            }
        }

        private static void LoadFundInfos()
        {
            if (File.Exists(TickerConstants.StockFundsPath) == false)
                throw new Exception(TickerConstants.StockFundsPath + " does not exist.");
            _stockFundsTickers = new HashSet<string>();
            File.ReadAllLines(TickerConstants.StockFundsPath)
                .ToList()
                .ForEach(t => _stockFundsTickers.Add(t));

            if (File.Exists(TickerConstants.BondFundsPath) == false)
                throw new Exception(TickerConstants.BondFundsPath + " does not exist.");
            _bondFundsTickers = new HashSet<string>();
            File.ReadAllLines(TickerConstants.BondFundsPath).ToList().ForEach(t => _bondFundsTickers.Add(t));

            if (File.Exists(TickerConstants.CurrencyFundsPath) == false)
                throw new Exception(TickerConstants.CurrencyFundsPath + " does not exist.");
            _currencyFundsTickers = new HashSet<string>();
            File.ReadAllLines(TickerConstants.CurrencyFundsPath)
                .ToList()
                .ForEach(t => _currencyFundsTickers.Add(t));
        }

        private static void LoadTradingSessionInfos()
        {
            if (File.Exists(TickerConstants.TradingSessionPath) == false)
                throw new Exception(TickerConstants.TradingSessionPath + " does not exist.");
            _tradingSessionDict = new Dictionary<string, List<TradingSessionInfo>>();
            var tradingSessionDict = new Dictionary<string, List<TradingSessionInfo>>();
            var lines = File.ReadAllLines(TickerConstants.TradingSessionPath).Skip(1);
            foreach (var line in lines)
            {
                var datas = line.Split(',');
                var tradingSessionInfo = new TradingSessionInfo()
                {
                    Date = DateTimeHelper.ParseDate(datas[0]),
                    ProductInfo = datas[1],
                    DaySessionString = datas[2],
                    NightSessionString = datas[3],
                    ExchangeTimeZone = int.Parse(datas[4])
                };
                if (tradingSessionDict.ContainsKey(tradingSessionInfo.ProductInfo) == false)
                    tradingSessionDict.Add(tradingSessionInfo.ProductInfo, new List<TradingSessionInfo>());
                tradingSessionDict[tradingSessionInfo.ProductInfo].Add(tradingSessionInfo);
            }

            foreach (var pair in tradingSessionDict)
            {
                _tradingSessionDict.Add(pair.Key, pair.Value.OrderByDescending(t => t.Date).ToList());
            }
        }

        private static void Init()
        {
            lock (SyncRoot)
            {
                if (_isInit) return;
                lock (SyncRoot)
                {
                    if (_isInit == false)
                    {
                        LoadGeneralTickerInfos();
                        LoadIndexInfos();
                        LoadFundInfos();
                        LoadTradingSessionInfos();
                        _isInit = true;
                    }
                }
            }
        }

        #endregion

        #region Internal

        private static string GetCodeByInternalTicker(string internalTicker)
        {
            return internalTicker.Split(',')[1].Split('.')[0];
        }


        private static string GetExchangeByInternalTicker(string internalTicker)
        {
            return internalTicker.Split(',')[1].Split('.')[1];
        }

        public static string GetMonthByExternalTicker(string externalTicker)
        {
            return GetMonthByInternalTicker(GetInternalTicker(externalTicker));
        }
        private static string GetMonthByInternalTicker(string internalTicker)
        {
            return internalTicker.Split(',')[2];
        }

        private static string GetPrefixByInternalTicker(string internalTicker)
        {
            return internalTicker.Split(',')[0];
        }

        public static string GetInternalTicker(string externalTicker)
        {
            var prefix = "";
            var month = "";
            var internalProductInfo = "";
            var externalProductInfo = "";
            var exchange = GetExchangeByTicker(externalTicker);
            var externalCode = GetCodeByTicker(externalTicker);
            switch (exchange)
            {
                case TickerConstants.SSEMarket:
                    internalProductInfo = externalCode + "." + exchange;
                    var shNumber = int.Parse(externalCode);
                    if (shNumber <= 000999)
                        prefix = TickerConstants.PrefixIndices;
                    else if (shNumber > 000999 && shNumber < 200000)
                        prefix = TickerConstants.PrefixBonds;
                    else if (shNumber >= 200000 && shNumber < 300000)
                        prefix = TickerConstants.PrefixRepos;
                    else if (shNumber >= 500000 && shNumber < 600000)
                        prefix = TickerConstants.PrefixFunds;
                    else if (shNumber >= 600000 && shNumber < 700000)
                        prefix = TickerConstants.PrefixStocks;
                    else if (shNumber >= 751000 && shNumber < 752000)
                        prefix = TickerConstants.PrefixStocks;
                    else if (shNumber >= 900000 && shNumber < 999000)
                        prefix = TickerConstants.PrefixStocks;
                    else if (shNumber >= 999000)
                        prefix = TickerConstants.PrefixIndices;
                    else
                    {
                        prefix = TickerConstants.PrefixUnknow;

                    }
                    break;
                case TickerConstants.SZSEMarket:
                    internalProductInfo = externalCode + "." + exchange;
                    var szNumber = int.Parse(externalCode);
                    if (szNumber < 10000)
                        prefix = TickerConstants.PrefixStocks;
                    else if (szNumber >= 100000 && szNumber < 130000)
                        prefix = TickerConstants.PrefixBonds;
                    else if (szNumber >= 130000 && szNumber < 140000)
                        prefix = TickerConstants.PrefixRepos;
                    else if (szNumber >= 150000 && szNumber < 190000)
                        prefix = TickerConstants.PrefixFunds;
                    else if (szNumber >= 200000 && szNumber < 210000)
                        prefix = TickerConstants.PrefixStocks;
                    else if (szNumber >= 300000 && szNumber < 310000)
                        prefix = TickerConstants.PrefixStocks;
                    else if (szNumber >= 390000)
                        prefix = TickerConstants.PrefixIndices;
                    else
                    {
                        prefix = TickerConstants.PrefixUnknow;
                    }
                    break;
                case TickerConstants.CZCEMarket:
                    prefix = TickerConstants.PrefixFutures;
                    externalProductInfo = externalCode.Substring(0, externalCode.Length - 3) + "." + exchange;
                    month = ParseCZCEMonth(externalCode.Substring(externalCode.Length - 3));
                    if (MonthRegex.IsMatch(month) == false)
                        throw new Exception("Invalid externalTicker :" + externalTicker);
                    internalProductInfo = GetFutureInternalProductInfoByProductInfo(externalProductInfo).Split(',')[1];
                    break;
                case TickerConstants.KRXMarket:
                    prefix = TickerConstants.PrefixFutures;
                    externalProductInfo = externalCode.Substring(0, 3) + "." + exchange;
                    month = ParseKoreanMonth(externalCode.Substring(3));
                    if (MonthRegex.IsMatch(month) == false)
                        throw new Exception("Invalid externalTicker :" + externalTicker);
                    internalProductInfo = GetFutureInternalProductInfoByProductInfo(externalProductInfo).Split(',')[1];
                    break;
                default:
                    prefix = TickerConstants.PrefixFutures;
                    externalProductInfo = externalCode.Substring(0, externalCode.Length - 4) + "." + exchange;
                    month = ParseFutureMonth(externalCode.Substring(externalCode.Length - 4));
                    if (MonthRegex.IsMatch(month) == false)
                        throw new Exception("Invalid externalTicker :" + externalTicker);
                    internalProductInfo = GetFutureInternalProductInfoByProductInfo(externalProductInfo).Split(',')[1];
                    break;
            }
            return GetInternalTicker(prefix, internalProductInfo, month);
        }

        private static string GetInternalTicker(string prefix, string product, string desc)
        {
            return $"{prefix},{product},{desc}";
        }

        private static string GetPrefixByInternalProductInfo(string internalProductInfo)
        {
            var datas = internalProductInfo.Split(',');
            if (datas.Length == 2)
                return datas[0];
            throw new Exception("Invalid InternalProductInfo  : " + internalProductInfo);
        }

        private static string GetInternalProduct(string internalTicker)
        {
            if (string.IsNullOrEmpty(internalTicker))
                throw new Exception("Invalid internalTicker : " + internalTicker);

            var datas = internalTicker.Split(',');
            if (datas.Length == 3)
            {
                return datas[1];
            }
            throw new Exception("Invalid internalTicker  : " + internalTicker);
        }

        private static string GetInternalProductInfo(string internalTicker)
        {
            if (string.IsNullOrEmpty(internalTicker))
                throw new Exception("Invalid internalTicker : " + internalTicker);

            var datas = internalTicker.Split(',');
            if (datas.Length == 3)
                return datas[0] + "," + datas[1];
            throw new Exception("Invalid internalTicker : " + internalTicker);
        }

        private static string GetInternalProductInfoByProductInfo(string productInfo)
        {
            if (_isInit == false)
                Init();
            if (_externalToInternal.ContainsKey(productInfo))
                return _externalToInternal[productInfo];
            if (IsValidTicker(productInfo))
                return GetInternalProductInfo(GetInternalTicker(productInfo));
            else
            {
                throw new Exception($"Invalid externalProductInfo={productInfo}");
            }
        }

        private static string GetFutureInternalProductInfoByProductInfo(string productInfo)
        {
            if (_isInit == false)
                Init();
            if (_externalToInternal.ContainsKey(productInfo) == false)
                throw new Exception("Invalid externalProductInfo :" + productInfo);
            return _externalToInternal[productInfo];
        }

        private static string GetFutureProductInfoByInternalProduct(string internalProduct)
        {
            if (_isInit == false)
                Init();
            if (_generalTickerInfoDict.ContainsKey("Futures," + internalProduct) == false)
                throw new Exception("Invalid internalProduct :" + internalProduct);
            return _generalTickerInfoDict["Futures," + internalProduct].Product + "." +
                   _generalTickerInfoDict["Futures," + internalProduct].Exchange;

        }

        public static GeneralTickerInfo GetGeneralTickerInfoByInternalProductInfo(string internalProductInfo)
        {
            if (_isInit == false)
                Init();
            if (_generalTickerInfoDict.ContainsKey(internalProductInfo))
                return _generalTickerInfoDict[internalProductInfo];
            var prefix = GetPrefixByInternalProductInfo(internalProductInfo);
            var product = internalProductInfo.Split(',')[1];
            switch (prefix)
            {
                case TickerConstants.PrefixIndices:
                case TickerConstants.PrefixRepos:
                case TickerConstants.PrefixSwaps:
                case TickerConstants.PrefixStocks:
                case TickerConstants.PrefixBonds:
                    return new GeneralTickerInfo
                    {
                        Adapter = "Auto",
                        InternalProduct = product.Split('.')[0],
                        Exchange = product.Split('.')[1],
                        LocalTimezoneIndex = 210,
                        ExchangeTimezoneIndex = 210,
                        Currency = "CNY",
                        MinMove = 0.01,
                        LotSize = 100,
                        ExchangeRateXxxUsd = 0.1575,
                        PointValue = 1.0,
                        CommissionOnRate = 0.0015,
                        CommissionPerShareInXxx = 0,
                        MinCommissionInXxx = 0,
                        MaxCommissionInXxx = 10000,
                        StampDutyRate = 0,
                        Slippage = 0,
                        Product = product.Split('.')[0],
                        IsCloseTodayFree = false
                    };
                case TickerConstants.PrefixFunds:
                    return new GeneralTickerInfo
                    {

                        Adapter = "Auto",
                        InternalProduct = product.Split('.')[0],
                        Exchange = product.Split('.')[1],
                        LocalTimezoneIndex = 210,
                        ExchangeTimezoneIndex = 210,
                        Currency = "CNY",
                        MinMove = 0.001,
                        LotSize = 100,
                        ExchangeRateXxxUsd = 0.1575,
                        PointValue = 1.0,
                        CommissionOnRate = 0.0015,
                        CommissionPerShareInXxx = 0,
                        MinCommissionInXxx = 0,
                        MaxCommissionInXxx = 10000,
                        StampDutyRate = 0,
                        Slippage = 0,
                        Product = product.Split('.')[0],
                        IsCloseTodayFree = false

                    };
                default:
                    return new GeneralTickerInfo
                    {
                        Adapter = "Auto",
                        InternalProduct = product.Split('.')[0],
                        Exchange = product.Split('.')[1],
                        LocalTimezoneIndex = 210,
                        ExchangeTimezoneIndex = 210,
                        Currency = "CNY",
                        MinMove = 0.01,
                        LotSize = 100,
                        ExchangeRateXxxUsd = 0.1575,
                        PointValue = 1.0,
                        CommissionOnRate = 0.0015,
                        CommissionPerShareInXxx = 0,
                        MinCommissionInXxx = 0,
                        MaxCommissionInXxx = 10000,
                        StampDutyRate = 0,
                        Slippage = 0,
                        Product = product.Split('.')[0],
                        IsCloseTodayFree = false
                    };
            }
        }

        public static string GetExchangeByInternalProductAndAdapter(string product, string adapter)
        {
            if (product == "SGTF")
                product = "TF";
            if (_isInit == false)
                Init();
            return
                _generalTickerInfoDict.Values.Single(g => g.Adapter == adapter && g.InternalProduct == product).Exchange;
        }

        public static GeneralTickerInfo GetGeneralTickerInfoByInternalTicker(string internalTicker)
        {
            var productInfo = GetInternalProductInfo(internalTicker);
            return GetGeneralTickerInfoByInternalProductInfo(productInfo);
        }

        #endregion

        #region Public

        public static bool IsChinaFuture(string ticker)
        {
            var exchange = TickerHelper.GetExchangeByTicker(ticker);
            return IsChinaFutureExchange(exchange);
        }
        public static bool IsChinaFutureExchange(string exchange)
        {
            return exchange == TickerConstants.SHFEMarket || exchange == TickerConstants.DCEMarket ||
                   exchange == TickerConstants.CZCEMarket || exchange == TickerConstants.CFFEXMarket;
        }

        public static string GetTickerByWsTicker(string wsTicker,string market)
        {
            switch (market)
            {
                case "SH":
                case "SZ":
                    return $"{wsTicker.Substring(2)}.{market}";
                default:
                    throw new NotImplementedException();
            }
        }

        public static string GetTickerByAnthonyTicker(string anthonyTicker,string adapter)
        {
            if(AnthonyTickerRegex.IsMatch(anthonyTicker) == false)
                throw new Exception($"Invalid anthony ticker={anthonyTicker}");
            var internalProduct = anthonyTicker.Split(',')[1];
            var prefix = anthonyTicker.Split(',')[0];
            var exchange = "";
            switch (prefix)
            {
                case TickerConstants.PrefixFutures:
                    exchange = GetExchangeByInternalProductAndAdapter(internalProduct, adapter);
                    break;
                case TickerConstants.PrefixStocks:
                    exchange = GetExchangeByStockCode(internalProduct);
                    break;
            }
            var internalTicker = anthonyTicker.Replace(internalProduct,internalProduct + "." + exchange);
            return GetTickerByInternalTicker(internalTicker);
        }

        public static double GetExchangeRateToUsd(string currency)
        {
            if(_isInit == false)
                Init();
            var generalTickerInfo = _generalTickerInfoDict.Values.FirstOrDefault(g => g.Currency == currency);
            if (generalTickerInfo == null)
                throw new Exception($"Invalid currency={currency}");
            return generalTickerInfo.ExchangeRateXxxUsd;
        }

        public static double GetExchangeRateToCNY(string currency)
        {
            var cny2Usd = GetExchangeRateToUsd("CNY");
            return GetExchangeRateToUsd(currency)/cny2Usd;
        }

        public static double GetExchangeRateToCNYByTicker(string ticker)
        {
            return GetExchangeRateToCNY(GetGeneralTickerInfoByTicker(ticker).Currency);
        }

        public static double GetExchangeRateToUSDByTicker(string ticker)
        {
            return GetGeneralTickerInfoByTicker(ticker).ExchangeRateXxxUsd;
        }

        public static double GetExchangeRate(string originalCurrency, string targetCurrency)
        {
            var targetExchangeRateToUsd = GetExchangeRateToUsd(targetCurrency);
            var originalExchangeRateToUsd = GetExchangeRateToUsd(originalCurrency);
            return originalExchangeRateToUsd/targetExchangeRateToUsd;
        }

        public static string GetPrefixByTicker(string ticker)
        {
            var productInfo = GetProductInfoByTicker(ticker);
            return GetPrefixByProductInfo(productInfo);
        }

        public static string GetPrefixByProductInfo(string productInfo)
        {
            var prefix = "";
            var exchange = GetExchangeByProductInfo(productInfo);
            var product = GetProductByProductInfo(productInfo);
            switch (exchange)
            {
                case TickerConstants.SSEMarket:
                    var shNumber = int.Parse(product);
                    if (shNumber <= 000999)
                        prefix = TickerConstants.PrefixIndices;
                    else if (shNumber > 000999 && shNumber < 200000)
                        prefix = TickerConstants.PrefixBonds;
                    else if (shNumber >= 200000 && shNumber < 300000)
                        prefix = TickerConstants.PrefixRepos;
                    else if (shNumber >= 500000 && shNumber < 600000)
                        prefix = TickerConstants.PrefixFunds;
                    else if (shNumber >= 600000 && shNumber < 700000)
                        prefix = TickerConstants.PrefixStocks;
                    else if (shNumber >= 751000 && shNumber < 752000)
                        prefix = TickerConstants.PrefixStocks;
                    else if (shNumber >= 900000 && shNumber < 999000)
                        prefix = TickerConstants.PrefixStocks;
                    else if (shNumber >= 999000)
                        prefix = TickerConstants.PrefixIndices;
                    else
                    {
                        prefix = TickerConstants.PrefixUnknow;
                    }
                    break;
                case TickerConstants.SZSEMarket:
                    var szNumber = int.Parse(product);
                    if (szNumber < 10000)
                        prefix = TickerConstants.PrefixStocks;
                    else if (szNumber >= 100000 && szNumber < 130000)
                        prefix = TickerConstants.PrefixBonds;
                    else if (szNumber >= 130000 && szNumber < 140000)
                        prefix = TickerConstants.PrefixRepos;
                    else if (szNumber >= 150000 && szNumber < 190000)
                        prefix = TickerConstants.PrefixFunds;
                    else if (szNumber >= 200000 && szNumber < 210000)
                        prefix = TickerConstants.PrefixStocks;
                    else if (szNumber >= 300000 && szNumber < 310000)
                        prefix = TickerConstants.PrefixStocks;
                    else if (szNumber >= 390000)
                        prefix = TickerConstants.PrefixIndices;
                    else
                    {
                        prefix = TickerConstants.PrefixUnknow;
                    }
                    break;
                default:
                    prefix = TickerConstants.PrefixFutures;
                    break;
            }
            return prefix;
        }


        public static bool IsFuture(string ticker)
        {
            if (GetPrefixByTicker(ticker) == TickerConstants.PrefixFutures)
                return true;
            return false;
        }


        public static bool IsIndexFuture(string ticker)
        {
            return ticker.Contains("IF") || ticker.Contains("IH") || ticker.Contains("IC");
        }

        public static bool IsBondFuture(string ticker)
        {
            return (ticker.Contains("TF") || ticker.Contains("TT")) && ticker.Contains(TickerConstants.CFFEXMarket);
        }

        public static string GetAnthonyTickerByTicker(string ticker)
        {
            var internalTicker = GetInternalTicker(ticker);
            var exchange = GetExchangeByInternalTicker(internalTicker);

            var anthonyTicker = "";
            if (IsIndex(ticker))
                anthonyTicker = $"Stocks,Idx{ticker.Replace($".{exchange}", "")}";
            else if (IsFuture(ticker))
                anthonyTicker = internalTicker.Replace($".{exchange}", "");
            else
            {
                anthonyTicker = $"Stocks,{ticker.Replace($".{exchange}", "")}";
            }
            return anthonyTicker;
        }

        public static bool IsStock(string ticker)
        {
            if (GetPrefixByTicker(ticker) == TickerConstants.PrefixStocks)
                return true;
            return false;
        }


        public static bool IsBond(string ticker)
        {
            if (GetPrefixByTicker(ticker) == TickerConstants.PrefixBonds)
                return true;
            return false;
        }


        public static bool IsFund(string ticker)
        {
            if (GetPrefixByTicker(ticker) == TickerConstants.PrefixFunds)
                return true;
            return false;
        }

        public static bool IsIndex(string ticker)
        {
            if (GetPrefixByTicker(ticker) == TickerConstants.PrefixIndices)
                return true;
            return false;
        }

        public static bool IsStockFund(string ticker)
        {
            if (_isInit == false)
                Init();
            return _stockFundsTickers.Contains(ticker);
        }

        public static bool IsBondFund(string ticker)
        {
            if (_isInit == false)
                Init();
            return _bondFundsTickers.Contains(ticker);
        }

        public static bool IsCurrencyFund(string ticker)
        {
            if (_isInit == false)
                Init();
            return _currencyFundsTickers.Contains(ticker);
        }

        public static double GetPointValueByTicker(string ticker)
        {
            var tickerInfo = GetGeneralTickerInfoByTicker(ticker);
            return tickerInfo.PointValue;
        }

        public static bool IsInCSI300(string externalTicker)
        {
            if (_isInit == false)
                Init();
            if (_csi300Tickers.Contains(externalTicker))
                return true;
            return false;
        }

        public static string GetTickerByFilename(string filename)
        {
            return filename.Split('\\').Last().Replace(".csv", "");
        }

        public static string GetProductInfoByFilename(string filename)
        {
            return GetProductInfoByTicker(GetTickerByFilename(filename));
        }

        public static double GetCommission(string ticker, double price, double volume)
        {
            var tickerInfo = GetGeneralTickerInfoByTicker(ticker);
            return Math.Max(tickerInfo.CommissionOnRate*price*volume*tickerInfo.PointValue,
                tickerInfo.CommissionPerShareInXxx*volume);
        }

        public static List<GeneralTickerInfo> GetGeneralTickerInfos()
        {
            return _generalTickerInfoDict.Values.ToList();
        }

        public static List<GeneralTickerInfo> GetGeneralTickerInfosByAdapter(string adapter)
        {
            if (_isInit == false)
                Init();
            return _generalTickerInfoDict.Values.Where(g => g.Adapter == adapter).ToList();
        }

        public static GeneralTickerInfo GetGeneralTickerInfoByTicker(string ticker)
        {
            var internalTicker = GetInternalTicker(ticker);
            return GetGeneralTickerInfoByInternalTicker(internalTicker);
        }

        public static GeneralTickerInfo GetGeneralTickerInfoByProductInfo(string productInfo)
        {
            if (_isInit == false)
                Init();
            var internalProductInfo = GetInternalProductInfoByProductInfo(productInfo);
            if (_generalTickerInfoDict.ContainsKey(internalProductInfo))
                return _generalTickerInfoDict[internalProductInfo];
            var prefix = GetPrefixByInternalProductInfo(internalProductInfo);
            var product = internalProductInfo.Split(',')[1];
            switch (prefix)
            {
                case TickerConstants.PrefixIndices:
                case TickerConstants.PrefixRepos:
                case TickerConstants.PrefixSwaps:
                case TickerConstants.PrefixStocks:
                case TickerConstants.PrefixBonds:
                    return new GeneralTickerInfo
                    {
                        Adapter = "Auto",
                        InternalProduct = product.Split('.')[0],
                        Exchange = product.Split('.')[1],
                        LocalTimezoneIndex = 210,
                        ExchangeTimezoneIndex = 210,
                        Currency = "CNY",
                        MinMove = 0.01,
                        LotSize = 100,
                        ExchangeRateXxxUsd = 0.1575,
//                        DaySessionString = "093000-113000&130000-150000",
//                        NightSessionString = "",
                        PointValue = 1.0,
                        CommissionOnRate = 0.0015,
                        CommissionPerShareInXxx = 0,
                        MinCommissionInXxx = 0,
                        MaxCommissionInXxx = 10000,
                        StampDutyRate = 0,
                        Slippage = 0,
                        Product = product.Split('.')[0],
                        IsCloseTodayFree = false
                    };
                case TickerConstants.PrefixFunds:
                    return new GeneralTickerInfo
                    {

                        Adapter = "Auto",
                        InternalProduct = product.Split('.')[0],
                        Exchange = product.Split('.')[1],
                        LocalTimezoneIndex = 210,
                        ExchangeTimezoneIndex = 210,
                        Currency = "CNY",
                        MinMove = 0.001,
                        LotSize = 100,
                        ExchangeRateXxxUsd = 0.1575,
//                        DaySessionString = "093000-113000&130000-150000",
//                        NightSessionString = "",
                        PointValue = 1.0,
                        CommissionOnRate = 0.0015,
                        CommissionPerShareInXxx = 0,
                        MinCommissionInXxx = 0,
                        MaxCommissionInXxx = 10000,
                        StampDutyRate = 0,
                        Slippage = 0,
                        Product = product.Split('.')[0],
                        IsCloseTodayFree = false

                    };
                default:
                    return null;
            }
        }

        public static bool IsValidTicker(string ticker)
        {
            try
            {
                GetInternalTicker(ticker);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetExchangeByTicker(string ticker)
        {
            if (ticker.Split('.').Length < 2)
                throw new Exception("Invalid externalTicker : " + ticker);
            var exchange = ticker.Split('.')[1];
            return exchange;
        }

        public static string GetExchangeByCodeAndAdapter(string code, string adapter)
        {
            return GetExchangeByProductAndAdapter(Regex.Replace(code, @"\d*", ""),
                adapter);
        }

        public static string GetExchangeByProductAndAdapter(string product, string adapter)
        {
            if (_isInit == false)
                Init();
            return _generalTickerInfoDict.Values.Single(g => g.Product == product && g.Adapter == adapter).Exchange;
        }

        public static string GetCodeByTicker(string ticker)
        {
            if (ticker.Split('.').Length < 2)
                throw new Exception("Invalid externalTicker : " + ticker);
            var code = ticker.Split('.')[0];
            return code;
        }

        public static string GetProductInfoByTicker(string ticker)
        {
            var generalTickerInfo = GetGeneralTickerInfoByTicker(ticker);
            return generalTickerInfo.Product + "." + generalTickerInfo.Exchange;
        }

        public static string GetProductInfoByInternalProductAndAdapter(string internalProduct, string adapter)
        {
            if (_isInit == false)
                Init();
            return
                _generalTickerInfoDict.Values.SingleOrDefault(
                    item => item.InternalProduct == internalProduct && item.Adapter == adapter)?.ProductInfo;
        }

        public static string GetExchangeByStockCode(string code)
        {
            int codeNum;
            int.TryParse(code, out codeNum);
            if (codeNum == 0)
                return null;
            if ((600000 <= codeNum && codeNum < 700000) || codeNum >= 900000)
                return TickerConstants.SSEMarket;
            if (codeNum < 10000 || (200000 <= codeNum && codeNum < 210000) || (300000 <= codeNum && codeNum < 310000))
                return TickerConstants.SZSEMarket;
            return null;
        }

        public static TradingSession GetTradingSessionByTicker(string ticker, string dateString = null,
            int timeZoneIndex = 210)
        {
            var productInfo = GetProductInfoByTicker(ticker);
            return GetTradingSessionByProductInfo(productInfo, dateString, timeZoneIndex);
        }


        public static TradingSession GetDaySessionByProductInfo(string productInfo, string dateString = null,
            int timeZoneIndex = 210)
        {
            var date = string.IsNullOrEmpty(dateString)
                ? DateTime.Today
                : DateTime.ParseExact(dateString, GeneralConstants.DateFormat, null);
            var tradingSessionInfo = GetTradingSessionInfo(productInfo, date);
            if (string.IsNullOrEmpty(tradingSessionInfo.DaySessionString))
                return null;
            var tradingSession = new TradingSession(tradingSessionInfo.DaySessionString,
                tradingSessionInfo.ExchangeTimeZone, date);
            tradingSession.Shift(timeZoneIndex);
            return tradingSession;
        }


        public static TradingSession GetDaySessionByTicker(string ticker, string dateString = null,
            int timeZoneIndex = 210)
        {
            var productInfo = GetProductInfoByTicker(ticker);
            return GetDaySessionByProductInfo(productInfo, dateString, timeZoneIndex);
        }


        public static TradingSession GetNightSessionByProductInfo(string productInfo, string dateString = null,
            int timeZoneIndex = 210)
        {
            var date = string.IsNullOrEmpty(dateString)
                ? DateTime.Today
                : DateTime.ParseExact(dateString, GeneralConstants.DateFormat, null);
            var tradingSessionInfo = GetTradingSessionInfo(productInfo, date);
            if (string.IsNullOrEmpty(tradingSessionInfo.NightSessionString))
                return null;
            var tradingSession = new TradingSession(tradingSessionInfo.NightSessionString,
                tradingSessionInfo.ExchangeTimeZone, date);
            tradingSession.Shift(timeZoneIndex);
            return tradingSession;
        }

        public static TradingSession GetNightSessionByTicker(string ticker, string dateString = null,
            int timeZoneIndex = 210)
        {
            var productInfo = GetProductInfoByTicker(ticker);
            return GetNightSessionByProductInfo(productInfo, dateString, timeZoneIndex);
        }

        public static string GetTicker(string code, string exchange)
        {
            return code + "." + exchange;
        }

        public static string GetTickerByInternalTicker(string internalTicker)
        {
            var prefix = GetPrefixByInternalTicker(internalTicker);
            var code = GetCodeByInternalTicker(internalTicker);
            var exchange = GetExchangeByInternalTicker(internalTicker);
            var internalProduct = GetInternalProduct(internalTicker);
            if (prefix == TickerConstants.PrefixFutures)
            {
                var originalMonth = GetMonthByInternalTicker(internalTicker);
                var externalProductInfo = GetFutureProductInfoByInternalProduct(internalProduct);
                var externalCode = externalProductInfo.Split('.')[0];
                var month = "";
                switch (exchange)
                {
                    case TickerConstants.KRXMarket:
                        month = GetKorenMonth(originalMonth);
                        break;
                    case TickerConstants.CZCEMarket:
                        month = GetCZCEMonth(originalMonth);
                        break;
                    default:
                        month = GetFutureMonth(originalMonth);
                        break;
                }

                return externalCode + month + "." + exchange;
            }
            return code + "." + exchange;
        }

        public static string GetExchangeByProductInfo(string productInfo)
        {
            return productInfo.Split('.')[1];
        }

        public static string GetProductByProductInfo(string productInfo)
        {
            return productInfo.Split('.')[0];
        }

        public static List<string> GetTickersByProductInfo(List<string> tickers, string productInfo)
        {
            return tickers.Where(t => GetProductInfoByTicker(t) == productInfo).ToList();
        }

        public static List<string> GetTickersByProductInfos(List<string> tickers, List<string> productInfos)
        {
            return tickers.Where(t => productInfos.Contains(GetProductInfoByTicker(t))).ToList();
        }
    }

    #endregion
}
