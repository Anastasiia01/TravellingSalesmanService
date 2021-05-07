﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSGALib
{
    interface IGeneticAlgorithm
    {
        void InitializePopulation();
        void EvaluatePopulation();
        void SelectPopulation();
        void CrossoverPopulation();
        void MutatePopulation(); 
    }
}
