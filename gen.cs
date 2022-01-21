using System;
using System.Collections.Generic;
using System.Linq;

namespace gen
{
    class ProblemPlecakowy
    {
        static Random rnd = new Random();
        public int[,] populationTable;
        string selection;
        public int[] selected;
        int m;
        public int maxIterations;
        public int currentIteration;
        public double[] averageFitness;
        public double[] bestChromosome;
        public double[] bestChromosomeEver;
        public int[] bestSolutionEver;
        int n;
        public int[,] Tems;
        int C;
        public double[,] fitness;
        string crossing;
        public int[] crossPoints;
        double chance;
        public ProblemPlecakowy(int population, int iterations, string selectionFunction, string crossingFunction, double chanceToMutate, int numberOfItems, int[,] Items, int capacity, int[,] popTab) : base()
        {
            m = population;
            maxIterations = iterations;
            currentIteration = 0;
            n = numberOfItems;
            crossing = crossingFunction;
            selection = selectionFunction;
            chance = chanceToMutate;
            Tems = Items;
            C = capacity;
            populationTable = popTab;

            bestSolutionEver = new int[n];
            averageFitness = new double[maxIterations];
            bestChromosome = new double[maxIterations];
            bestChromosomeEver = new double[maxIterations];
            crossPoints = new int[2];
        }
        public void NextGeneration(){
            // Obliczenie przystosowania populacji
            fitness = calculateFitnessArray();

            // selekcja 
            switch(selection){
                case "RouletteSelection":
                    selected = RouletteSelection();
                    break;

                case "RankSelection":
                    selected = RankSelection();
                    break;

                case "TournamentSelection":
                    selected = TournamentSelection();
                    break;
            }

            // krzyżowanie
            switch(crossing){
                case "OnePointCrossing":
                    populationTable = OnePointCrossing();
                    break;
                    
                case "TwoPointCrossing":
                    populationTable = TwoPointCrossing();
                    break;
            }

            // Mutacja
            mutation();

            currentIteration++;
        }
        public double[,] calculateFitnessArray(){
            // obliczenie wartości, wagi i przystosowania każdego plecaka
            double fitSum = 0;
            double maxFit = 0;
            int maxFitIndex = 0;
            double[,] fit = new double[m,3];
             for (int i = 0; i < m;i++){
                for (int j = 0; j < n; j++)
                {
                    if(populationTable[i,j] == 1){
                        // wartość
                        fit[i,0] += Tems[j,0];
                        // waga
                        fit[i,1] += Tems[j,1];
                    }
                }
                // przystosowanie
                fit[i,2] = fitnessFunction(fit[i,0], fit[i,1]);
            
                fitSum += fit[i,2];
                if (maxFit < fit[i,2]){
                    maxFit = fit[i,2];
                    maxFitIndex = i;
                }
            }

            // srednie przystosowanie populacji
            averageFitness[currentIteration] = fitSum / m;

            // najlepszy osobnik w tej populacji
            bestChromosome[currentIteration] = maxFit;

            // najlepszy dotychczasowy osobnik
            if(currentIteration == 0){
                bestChromosomeEver[currentIteration] = maxFit;
            }
            else {
                if(bestChromosomeEver[currentIteration-1] > maxFit){
                    bestChromosomeEver[currentIteration] = bestChromosomeEver[currentIteration-1];
                }
                else {
                    bestChromosomeEver[currentIteration] = maxFit;
                    for(int i = 0; i < n; i++){
                        bestSolutionEver[i] = populationTable[maxFitIndex, i];
                    }
                }
            }
            return fit;
        }
        public double fitnessFunction(double value, double cap){
            if (cap <= C && cap != 0){
                return value;
            } else {
                return 0;
            }
        }
        public int[] RouletteSelection(){
            double[] P = new double[m];
            double fitSum = 0;
            for (int i = 0; i < m; i++){
                fitSum += fitness[i,2];
            }
            for (int i = 0; i < m; i++){
                double fx = fitness[i,2];
                P[i] = fx / fitSum;
            }

            // tworzenie ruletki
            double[] roulette = new double[m];
            for (int i=0; i < m; i++){
                for(int j = 0; j<m; j++){
                    if(P[j] != 0){
                        if(i!=0){
                            roulette[i] = P[i] + roulette[i-1];
                        }
                        else {
                            roulette[i] = P[i];
                        }
                    }
                }
            }

            // losowanie
            int[] selection = new int[m];
            double rand;
            for(int j = 0; j<m; j++){
            rand = rnd.NextDouble();
                for(int i = 0; i < m; i++){
                    if(rand <= roulette[i]){
                        selection[j] =  i;
                        break;
                    }
                }
            }
            return selection;
        }
        public int[] RankSelection(){
            double[] P = new double[m];
            double[][] rank = new double[m][];
            for (int i = 0; i < m; i++){
                rank[i] = new double[2]{i, fitness[i,2]};
            }

            double[] temp = new double[2];
            temp = rank[0];

            // sortowanie rankingu
            for (int i = 0; i < m; i++)
                for (int j = i+1; j < m; j++){
                    if (rank[i][1] > rank[j][1]){
                        temp = rank[i];
                        rank[i] = rank[j];
                        rank[j] = temp;
                    }
                }

            // obliczanie prawdopodobienstwa
            int popIndex;
            double rankSum = m*(m+1)/2;
            for (int i = 0; i < m; i++){
                popIndex = Convert.ToInt16(rank[i][0]);
                P[popIndex] = (i+1) / rankSum;
            }

            // tworzenie ruletki
            double[] roulette = new double[m];
            roulette[0] = P[0];
            for (int i=1; i < m; i++){
                roulette[i] = P[i] + roulette[i-1];
            }

            // losowanie
            int[] selection = new int[m];
            double rand;
            for(int j = 0; j<m; j++){
            rand = rnd.NextDouble();
                for(int i = 0; i < m; i++){
                    if(rand <= roulette[i]){
                        selection[j] =  i;
                        break;
                    }
                }
            }
            return selection;
        }
        public int[] TournamentSelection(){
            int[] selected = new int[m];
            int k = 4;
            double[][] tournament;
            double random;

            for (int t = 0; t < m; t++){
                // rozpoczęcie turnieju
                tournament = new double[k][];
                for (int i = 0; i < k; i++){
                    tournament[i] = new double[2]{-1, 0};
                }

                // losowanie zawodników do turnieju
                bool duplication;
                for (int i = 0; i < k;){
                    duplication = false;
                    random = Convert.ToDouble(rnd.Next() % m);
                    for(int j = 0; j < k; j++){
                        // jesli pole w turnieju jest puste
                        if(tournament[j][0] == -1){
                            break;
                        }
                        // jesli zawodnik juz zostal wylosowany do tego turnieju
                        if(random == tournament[j][0]){
                            duplication = true;
                            break;
                        }
                    }
                    if (duplication == true){
                        // powtórne losowanie
                        continue;
                    }
                    tournament[i][0] = random;
                    tournament[i][1] = fitness[Convert.ToInt16(random),2];
                    i++;
                }

                // rozegranie turnieju (sortowanie)
                double[] temp = new double[2];
                temp = tournament[0];

                for (int i = 0; i < k; i++)
                    for (int j = i+1; j < k; j++){
                        if (tournament[i][1] < tournament[j][1]){
                            temp = tournament[i];
                            tournament[i] = tournament[j];
                            tournament[j] = temp;
                        }
                    }

                // wyłonienie zwyciężcy
                selected[t] = Convert.ToInt16(tournament[0][0]); 
            }
            return selected;
        }
        public int[,] OnePointCrossing(){
            // losowanie punktu krzyżowania
            int point = 0;
            do
            {
                point = rnd.Next() % n;
            } while (point >= n-1);
            crossPoints[0] = point;
            crossPoints[1] = point;

            // krzyżowanie
            int[,] newPop = new int[m,n];
            for(int i = 0; i < m; i++){
                if(i % 2 == 0){
                    for (int j = 0; j < point; j++){
                        newPop[i,j] = populationTable[selected[i],j];
                    }
                    for (int j = 0; point+j < n; j++){
                        newPop[i,point+j] = populationTable[selected[i+1],point+j];
                    }
                } 
                else {
                    for (int j = 0; j < point; j++){
                        newPop[i,j] = populationTable[selected[i],j];
                    }
                    for (int j = 0; point+j < n; j++){
                        newPop[i,point+j] = populationTable[selected[i-1],point+j];
                    }
                }
            }
            return newPop;
        }
        public int[,] TwoPointCrossing(){
            // losowanie punktu krzyżowania
            int point = 0;
            int point1 = 0;
            do
            {
                point = rnd.Next() % n;
                point1 = rnd.Next() % n;
            } while (point >= point1 || point == 0);
            crossPoints[0] = point;
            crossPoints[1] = point1;

            // krzyżowanie
            int[,] newPop = new int[m,n];
            for(int i = 0; i < m; i++){
                if(i % 2 == 0){
                    for (int j = 0; j < point; j++){
                        newPop[i,j] = populationTable[selected[i],j];
                    }
                    for (int j = 0; point+j < point1; j++){
                        newPop[i,point+j] = populationTable[selected[i+1],point+j];
                    }
                    for (int j = 0; point1+j < n; j++){
                        newPop[i,point1+j] = populationTable[selected[i],point1+j];
                    }
                } 
                else {
                    for (int j = 0; j < point; j++){
                        newPop[i,j] = populationTable[selected[i],j];
                    }
                    for (int j = 0; point+j < point1; j++){
                        newPop[i,point+j] = populationTable[selected[i-1],point+j];
                    }
                    for (int j = 0; point1+j < n; j++){
                        newPop[i,point1+j] = populationTable[selected[i],point1+j];
                    }
                }
            }
            return newPop;
        }
        public void mutation(){
            // zamienia liczby 0.0001 na 1000
            char[] number = chance.ToString().ToCharArray();
            Array.Reverse(number);
            string maximum = new string(number.Take(number.Count() - 2).ToArray());

            // losowanie i zmiana bitu
            for(int i = 0; i < m; i++)
                for(int j = 0; j < n; j++){
                    int random = rnd.Next(0, Int16.Parse(maximum));
                    if (random == 1){
                        populationTable[i, j] = 1 - populationTable[i, j];
                    }
                }

        }
        public string printPopulation(){
            List<string> pop = new List<string>();
            for (int i = 0; i < m;i++){
                pop.Add(i + "- [ ");
                for (int j = 0; j < n; j++)
                {
                    pop.Add(populationTable[i,j].ToString());
                    pop.Add(", ");
                }
                // pop.Add("v: ");
                // pop.Add(fitness[i,0].ToString());
                // pop.Add(", c: ");
                // pop.Add(fitness[i,1].ToString());
                pop.Add(" f: ");
                pop.Add(fitness[i,2].ToString());
                pop.Add("]\n");
            }
            return String.Join("", pop);
        }
        public string printProblem(){
            List<string> prob = new List<string>();
            for (int i = 0; i < n;i++){
                prob.Add("[v: ");
                prob.Add(Tems[i,0].ToString());
                prob.Add(", c: ");
                prob.Add(Tems[i,1].ToString());
                prob.Add("]\n");
            }
            prob.Add("C= " + C);
            return String.Join("", prob);
        }
        public string printSelected(){
            List<string> sel = new List<string>();
            sel.Add("[");
            for (int i = 0; i < m;i++){
                sel.Add(selected[i].ToString());
            }
            sel.Add("]");
            return String.Join("", sel);
        }
        public string printHistory(){
            List<string> hist = new List<string>();
            hist.Add("average:\n");
            for (int i = 0; i < maxIterations; i++)
            {
                hist.Add(averageFitness[i].ToString());
                hist.Add(", ");
            }
            hist.Add("\nbest:\n");
            for (int i = 0; i < maxIterations; i++)
            {
                hist.Add(bestChromosome[i].ToString());
                hist.Add(", ");
            }
            hist.Add("\nbest ever:\n");
            for (int i = 0; i < maxIterations; i++)
            {
                hist.Add(bestChromosomeEver[i].ToString());
                hist.Add(", ");
            }
            hist.Add("\nsolution:\n[ ");
            for (int i = 0; i < n; i++)
            {
                hist.Add(bestSolutionEver[i].ToString());
                hist.Add(", ");
            }
            hist.Add("]");
            return String.Join("", hist);
        }
    }
}
