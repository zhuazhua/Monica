using System.Collections.Generic;

namespace Monica.Common.Pocos
{
    public delegate void FutureContractInfoCallbackHandler(FutureContractInfo info);

    public class FutureContractInfo:TickerPoco
    { 
        public string StartDate { get; set; }
        public string ExpireDate { get; set; }

        public string ToCsv()
        {
            return $"{Ticker},{StartDate},{ExpireDate}";
        }

        public static FutureContractInfo PraseFromCsv(IList<string> data)
        {
            var instrument = new FutureContractInfo
            {
                Ticker = data[0],
                StartDate = data[1],
                ExpireDate = data[2],
            };
            return instrument;
        }

        public static FutureContractInfo PraseFromCsv(string line)
        {
            var datas = line.Split(',');
            return PraseFromCsv(datas);
        }

        public override string ToString()
        {
            return ToCsv();
        }
    }
}
