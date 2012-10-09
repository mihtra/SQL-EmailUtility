using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Exchange.WebServices.Data;


namespace SQLTechSuite
{
    class Program
    {
        static void Main(string[] args)
        {
            ExchangeHelper ExchObj = new ExchangeHelper();
            ExchObj.CreateExchangeInstance();
        }
    }
}
