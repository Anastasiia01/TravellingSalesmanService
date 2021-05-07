using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TSGAMT2010.TravellingSalesman;

namespace TSGAMT2010
{
    class TSGAClient : TravellingSalesman.ITSGACallback, IDisposable
    {
        TravellingSalesman.TSGAClient proxy = null;

        public void CallRunGA(ArrayList pointsList, int numThreads)
        {
            try
            {
                proxy = new TravellingSalesman.TSGAClient(new System.ServiceModel.InstanceContext(this),
                "GAEP");
                Console.WriteLine(proxy.ToString());
                proxy.RunGA(pointsList.ToArray(), numThreads);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region IDisposable Members
        public void Dispose()
        {
            proxy.Close();
        }
        #endregion


        #region ITSGACallback Members
        public void OnComputeGA(TravellingSalesman.BestGAInfo res)
        {
            MessageBox.Show("fitness "+res.Fitness.ToString() + "\n");
        }
        #endregion

    }
}
