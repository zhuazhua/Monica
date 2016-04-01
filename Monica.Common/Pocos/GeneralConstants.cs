using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Monica.Common.Pocos
{
    public class GeneralConstants
    {
        public const string DateFormat = "yyyyMMdd";
        public const string DateTimeFormat = "yyyyMMdd HH:mm:ss";
        public const string TimeFormat = "HH:mm:ss";
        public const string DoubleFormat = "F3";
        public const string HolidayPath = @"Data\Holiday.csv";
        public const string TimeZoneIndexPath = @"Data\TimeZoneIndex.csv";

    }

    public class TickerConstants
    {
        public const string StockFundsPath = @"Data\StockFunds.csv";
        public const string BondFundsPath = @"Data\BondFunds.csv";
        public const string CurrencyFundsPath = @"Data\CurrencyFunds.csv";
        public const string TradingSessionPath = @"Data\TradingSession.csv";
        public const string IndexPath = @"Data\Index.csv";
        public const string GeneralTickerInfoPath = @"Data\GeneralTickerInfo.csv";
        public const string PrefixStocks = "Stocks";
        public const string PrefixFutures = "Futures";
        public const string PrefixIndices = "Indices";
        public const string PrefixBonds = "Bonds";
        public const string PrefixSwaps = "Swaps";
        public const string PrefixFunds = "Funds";
        public const string PrefixRepos = "Repos";
        public const string PrefixUnknow = "PrefixUnknow";
        public const string CSI300Ticker = "999300.SH";
        public const string NonCSI300Ticker = "999301.SH";
        public static Regex SpotsRegex = new Regex(@"\d{6}", RegexOptions.Compiled);
        public static Regex FuturesRegex = new Regex(@"101[A-Z0-9]{2}|[a-zA-Z]+[0-9]{3}|[a-zA-Z]+[0-9]{4}", RegexOptions.Compiled);
        public static Regex FuturesRegexCZCE = new Regex(@"[a-zA-Z]+[0-9]{3}", RegexOptions.Compiled);
        public static Regex FuturesRegexKRX = new Regex(@"101[A-Z0-9]{2}", RegexOptions.Compiled);
        public const string SSEMarket = "SH";
        public const string SZSEMarket = "SZ";
        public const string DCEMarket = "DCE";
        public const string SHFEMarket = "SHFE";
        public const string CZCEMarket = "CZCE";
        public const string CFFEXMarket = "CFFEX";
        public const string HKEXMarket = "HKEX";
        public const string SGXQMarket = "SGXQ";
        public const string KRXMarket = "KRX";
        public const string BMDMarket = "BMD";
        public const string TOCOMMarket = "TOCOM";
        public const string CMEMarket = "CME";
        public const string CMECBTMarket = "CME_CBT";
        public const string TFEXMarket = "TFEX";
        public const string SFEMarket = "SFEMarket";
        public const string MarketUnkown = "NaN";
        public static string[] AllMarkets = {
                "SH", "SZ", "DCE", "SHFE", "CZCE", "CFFEX", "HKEX", "SGXQ", "KRX", "BMD", "TOCOM", "CME", "CME_CBT", "TFEX", "SFE"
            };
        public static string[] ChFutureMarkets = { "DCE", "SHFE", "CZCE", "CFFEX" };
        public static string[] AsFutureMarkets = { "HKEX", "SGXQ", "KRX", "BMD", "TOCOM", "CME", "CME_CBT", "TFEX", "SFE" };
        public static string[] ChSecutiryMarkets = { "SH", "SZ" };

    }
}
