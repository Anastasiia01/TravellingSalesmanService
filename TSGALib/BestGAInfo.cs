using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TSGALib
{
    [DataContract]
    public class BestGAInfo
    {
        [DataMember]
        public List<int> mem { get; set; }         //= new List<int>();

        int fitness;
        [DataMember]
        public int Fitness
        {
            get { return fitness; }
            set { fitness = value; }
        }

        public BestGAInfo()
        {
            this.mem = new List<int>();
        }


    }
}
