using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TSGALib
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class TSGAService : ITSGA
    {
        TSGA[] GAs;
        Thread[] TGAs;
        object lockobj = new object();
        object[] synchObjsB;
        object[] synchObjsA;
        Thread thXchg;
        int[] dataready;
        int[] xchgdone;
        int numThreads;
        BestGAInfo ga = new BestGAInfo();
        bool threadsFinished;
        int[,] DistMat;

        public void RunGA(ArrayList PointsList, int _numThreads)
        {
            Console.WriteLine("Received request");
            if (PointsList.Count == 0)
            {
                return;
            }
            numThreads = _numThreads;
            Console.WriteLine(numThreads);
            Console.WriteLine(PointsList[0]);
            Console.WriteLine(PointsList[1]);


            DistMat = new int[PointsList.Count, PointsList.Count];
            int dX = 0;
            int dY = 0;
            int dist;
            for (int i = 0; i < PointsList.Count; i++)
                for (int j = i; j < PointsList.Count; j++)
                {
                    if (i == j)
                        DistMat[i, j] = 0;
                    else
                    {
                        dX = Math.Abs(((Point)PointsList[i]).X - ((Point)PointsList[j]).X);
                        dY = Math.Abs(((Point)PointsList[i]).Y - ((Point)PointsList[j]).Y);
                        dist = (int)(Math.Sqrt(dX * dX + dY * dY));
                        DistMat[i, j] = dist;
                        DistMat[j, i] = dist;
                    }
                }
            

            GAs = new TSGA[numThreads];
            TGAs = new Thread[numThreads];

            synchObjsA = new object[numThreads];
            synchObjsB = new object[numThreads];
            dataready = new int[numThreads];
            xchgdone = new int[numThreads];
            object olock = new object();

            for (int i = 0; i < numThreads; i++)
            {
                synchObjsA[i] = new object();
                synchObjsB[i] = new object();
            }

            for (int i = 0; i < numThreads; i++)
            {
                GAs[i] = new TSGA(TSGAConstants.PopSize, TSGAConstants.NumCities,
                    (float)((i + 1) * 0.02), TSGAConstants.CrossoverRate, DistMat, ref lockobj, ref synchObjsA,
                    ref synchObjsB, dataready, xchgdone, i);
                //   i*0.2 is the mutation rate for each thread
                TGAs[i] = new Thread(new ThreadStart(GAs[i].RunGA_XCHG));
                TGAs[i].Start();
                //timer2.Enabled = true; send periodic replies to client 
            }

            //----start the exchange in another thread so that
            //---- user interface continues to show updates
            thXchg = new Thread(new ThreadStart(() => this.ExchangeData()));
            Thread.Sleep(100);
            thXchg.Start();
            while (!threadsFinished)
            {
                Thread.Sleep(5000);

                assignBestGAMember();
                lock (olock)
                {
                    // trigger callback in client
                    IComputeCallback callbackChannel =
                    OperationContext.Current.GetCallbackChannel<IComputeCallback>();
                    if (((ICommunicationObject)callbackChannel).State == CommunicationState.Opened)
                        callbackChannel.OnComputeGA(ga);
                }
            }
        }

        void assignBestGAMember()
        {
            int bestOne;
            int bfitness;
            if (GAs[0].bestMember.fitness < GAs[1].bestMember.fitness)
                bestOne = 0;
            else
                bestOne = 1;
            bfitness = GAs[bestOne].bestMember.fitness;

            if (numThreads >= 3)
            {
                if (GAs[2].bestMember.fitness < bfitness)
                {
                    bestOne = 2;
                    bfitness = GAs[bestOne].bestMember.fitness;
                }
                if (numThreads >= 4)
                {
                    if (GAs[3].bestMember.fitness < bfitness)
                    {
                        bestOne = 3;
                        bfitness = GAs[bestOne].bestMember.fitness;
                    }
                }
                GAMember g1 = (GAMember)GAs[bestOne].bestMember.Clone();
                //ga.mem = new int[g1.memSize];
                for (int i = 0; i < g1.memSize; i++)
                    ga.mem.Add(g1.mem[i]);
                ga.Fitness = bfitness;
            }
        }

        void ExchangeData()
        {
            int i;
            Random r = new Random(System.DateTime.Now.Millisecond);
            bool done = true;
            while (true)
            {
                for (i = 0; i < numThreads; i++)  // wait for all threads to synchronize 
                {
                    if (TGAs[i].IsAlive)
                    {
                        if (dataready[i] == 0)
                        {
                            lock (GAs[i].synchObjsA[i])
                            {
                                Monitor.Wait(GAs[i].synchObjsA[i]);
                            }
                        }
                        lock (lockobj)
                        {
                            dataready[i] = 0;
                        }
                    }
                }

                //----------exchange data between threads------
                for (i = 0; i < numThreads; i++)
                {
                    int t1 = (int)(r.NextDouble() * numThreads);
                    int t2 = (int)(r.NextDouble() * numThreads);
                    if (t1 == t2) // while (t1 == t2)
                        t2 = (int)(r.NextDouble() * numThreads);

                    GAMember temp; int m1 = 0; int m2 = 0;
                    for (int j = 0; j < TSGAConstants.NumMembersToExchange; j++)  // exchange 3 values
                    {
                        m1 = (int)(r.NextDouble() * TSGAConstants.PopSize);
                        m2 = (int)(r.NextDouble() * TSGAConstants.PopSize);
                        lock (lockobj)
                        {
                            temp = GAs[t1].pop[m1];
                            GAs[t1].pop[m1] = GAs[t2].pop[m2];
                            GAs[t2].pop[m2] = temp;
                        }
                    }
                }
                for (i = 0; i < numThreads; i++)
                {
                    lock (lockobj)
                    {
                        xchgdone[i] = 1;
                    }
                    lock (synchObjsB[i])
                    {
                        Monitor.PulseAll(synchObjsB[i]); // tell each thread to proceed
                    }
                }
                done = true;
                for (i = 0; i < numThreads; i++)
                {
                    if (TGAs[i].IsAlive)
                        done = false;
                }
                if (done == true)
                    threadsFinished = true;
                    break;  // all threads done
            }
        }
    }
}
