using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TSGALib
{
    interface IComputeCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnComputeGA(BestGAInfo res);
    }

    [ServiceContract(CallbackContract = typeof(IComputeCallback))]
    public interface ITSGA
    {
        [OperationContract(IsOneWay = true)]
        void RunGA(ArrayList PointsList, int numThreads);
    }
}
