using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TSGAHost
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ServiceHost sh = new ServiceHost(typeof(TSGALib.TSGAService));
                Console.WriteLine("TSGA Sub Service Ready, Listening on 7065");
                Console.WriteLine("Hit Enter to Stop..");
                sh.Open();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: "+ex.Message);
            }
        }
    }
}
