﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YSQ.core.Historical;
using HtmlAgilityPack;
using YSQ.core.Quotes;

namespace Folio.Logic
{
    public static class YahooAPICalls
    {

        public static List<decimal> GetStockHistoricalPrices(string ticker, DateTime startDate, DateTime endDate)
        {
            HistoricalPriceService hps = new HistoricalPriceService();
            IEnumerable<HistoricalPrice> historicalPrices = null;

            try
            {
                historicalPrices = hps.Get(ticker, startDate, endDate, Period.Daily);
            }
            catch(Exception ex)
            {
                historicalPrices = BruteForceHistoricalPrices(hps, ticker, startDate, endDate);
            }
            List<decimal> priceData = new List<decimal>();
            foreach (var price in historicalPrices)
            {
                priceData.Add(price.Price);
            }
            return priceData;
        }

        private static IEnumerable<HistoricalPrice> BruteForceHistoricalPrices(HistoricalPriceService hps, string ticker, DateTime startDate, DateTime endDate)
        {
            IEnumerable<HistoricalPrice> historicalPrice = null;
            int loopCount = 0;
            TimeSpan half = new TimeSpan(0);
            while(loopCount < 20)
            {
                try
                {
                    historicalPrice = hps.Get(ticker, startDate, endDate, Period.Daily);
                }
                catch(Exception ex)
                {
                }
                if (historicalPrice == null)
                {
                    half = new TimeSpan((endDate - startDate).Ticks / 2);
                    startDate = (startDate + half);
                }
                else
                {
                    half = new TimeSpan(half.Ticks + (half.Ticks/2));
                    startDate = (startDate + half);
                }
                loopCount++;
            }
            return historicalPrice;
        }

        //public static Dictionary<DateTime, decimal> HistoricalPricesToDict(string ticker, DateTime startDate)
        //{
        //    HistoricalPriceService hps = new HistoricalPriceService();
        //    IEnumerable<HistoricalPrice> historicalPrices = hps.Get(ticker, startDate, DateTime.UtcNow, Period.Daily);
        //    Dictionary<DateTime, decimal> priceData = new Dictionary<DateTime, decimal>();
        //    foreach (var price in historicalPrices)
        //    {
        //        priceData.Add(price.Date, price.Price);
        //    }
        //    return priceData;
        //}

        public static decimal GetCurrentStockPrice(string ticker)
        {
            HistoricalPriceService hps = new HistoricalPriceService();
            IEnumerable<HistoricalPrice> price = hps.Get(ticker, DateTime.Today.AddDays(-1), DateTime.UtcNow, Period.Daily);
            decimal currentPrice = price.ElementAt(0).Price;
            return currentPrice;
        }

        public static decimal GetStockBeta(string ticker)
        {
            HtmlWeb htmlWeb = new HtmlWeb();
            decimal beta = 0;
            HtmlDocument document = htmlWeb.Load(string.Format("http://finance.yahoo.com/q?s={0}&ql=1", ticker));
            HtmlNode node = document.GetElementbyId("table1");
            if (node != null)
            {
                int count = 0;
                IEnumerable<HtmlNode> td = node.Descendants("td");
                foreach (HtmlNode d in td)
                {
                    if (count == 5)
                    {
                        try
                        {
                            beta = Convert.ToDecimal(d.InnerHtml);
                        }
                        catch(Exception ex)
                        {
                            beta = 1;
                        }
                    }
                    count++;
                }
            }
            return beta;
        }
    }
}

