using System;
using System.Data;
using ClosedXML.Excel;


namespace gen
{
    class Program
    {
        static void Main(string[] args)
        {
            int populacja = 1000;
            int liczbaIteracji = 100;
            int liczbaPrzedmiotow = 30;
            double szansaNaMutacje = 0.0001;
            Random rnd = new Random();

            // Losowanie problemu plecakowego
            int n = liczbaPrzedmiotow;
            int[,] Przedmioty = new int[n, 2];
            for (int i = 0; i < n; i++){
                for (int j = 0; j < 2; j++){
                    Przedmioty[i,j] = rnd.Next() % 10 + 1;
                }
            }

            int pojemnosc = 2*liczbaPrzedmiotow;

            // Inicjalizacja
            int m = populacja;
            int[,] tablicaPopulacji = new int[m, n];
            for (int i = 0; i < m;i++){
                for (int j = 0; j < n; j++)
                {
                    int random = rnd.Next() % 10+1;
                    if(random >= 2){
                        tablicaPopulacji[i, j] = 0;
                    }
                    else {
                        tablicaPopulacji[i, j] = 1;
                    }
                }
            }

            // Rozwiązanie dokładne
            Dynamic_PP Dpp = new Dynamic_PP(pojemnosc, Przedmioty, liczbaPrzedmiotow);
            Console.WriteLine("Wynik dokładny, wartość plecaka = " + Dpp.value);

            ProblemPlecakowy[] pp = new ProblemPlecakowy[6];

            // Jeden punkt krzyżowania

            // Ruletkowa
            Console.WriteLine("\nRuletkowa, jeden punkt krzyżowania:");

            pp[0] = new ProblemPlecakowy(populacja, liczbaIteracji, "RouletteSelection", "OnePointCrossing", szansaNaMutacje, liczbaPrzedmiotow, Przedmioty, pojemnosc, tablicaPopulacji);
            while(pp[0].currentIteration < pp[0].maxIterations){
                pp[0].NextGeneration();
            }
            Console.WriteLine(pp[0].printHistory());
            Console.WriteLine("Część wyniku dokładnego: " + (pp[0].bestChromosomeEver[liczbaIteracji-1]/Dpp.value)*100 + "%");

            // Rankingowa
            Console.WriteLine("\nRankingowa, jeden punkt krzyżowania:");

            pp[1] = new ProblemPlecakowy(populacja, liczbaIteracji, "RankSelection", "OnePointCrossing", szansaNaMutacje, liczbaPrzedmiotow, Przedmioty, pojemnosc, tablicaPopulacji);
            while(pp[1].currentIteration < pp[1].maxIterations){
                pp[1].NextGeneration();
            }
            Console.WriteLine(pp[1].printHistory());
            Console.WriteLine("Część wyniku dokładnego: " + (pp[1].bestChromosomeEver[liczbaIteracji-1]/Dpp.value)*100 + "%");

            // Turniejowa
            Console.WriteLine("\nTurniejowa, jeden punkt krzyżowania:");

            pp[2] = new ProblemPlecakowy(populacja, liczbaIteracji, "TournamentSelection", "OnePointCrossing", szansaNaMutacje, liczbaPrzedmiotow, Przedmioty, pojemnosc, tablicaPopulacji);
            while(pp[2].currentIteration < pp[2].maxIterations){
                pp[2].NextGeneration();
            }
            Console.WriteLine(pp[2].printHistory());
            Console.WriteLine("Część wyniku dokładnego: " + (pp[2].bestChromosomeEver[liczbaIteracji-1]/Dpp.value)*100 + "%");


            // Dwa punkty krzyżowania

            // Ruletkowa
            Console.WriteLine("\nRuletkowa, dwa punkty krzyżowania:");

            pp[3] = new ProblemPlecakowy(populacja, liczbaIteracji, "RouletteSelection", "TwoPointCrossing", szansaNaMutacje, liczbaPrzedmiotow, Przedmioty, pojemnosc, tablicaPopulacji);
            while(pp[3].currentIteration < pp[3].maxIterations){
                pp[3].NextGeneration();
            }
            Console.WriteLine(pp[3].printHistory());
            Console.WriteLine("Część wyniku dokładnego: " + (pp[3].bestChromosomeEver[liczbaIteracji-1]/Dpp.value)*100 + "%");

            // Rankingowa
            Console.WriteLine("\nRankingowa, dwa punkty krzyżowania:");

            pp[4] = new ProblemPlecakowy(populacja, liczbaIteracji, "RankSelection", "TwoPointCrossing", szansaNaMutacje, liczbaPrzedmiotow, Przedmioty, pojemnosc, tablicaPopulacji);
            while(pp[4].currentIteration < pp[4].maxIterations){
                pp[4].NextGeneration();
            }
            Console.WriteLine(pp[4].printHistory());
            Console.WriteLine("Część wyniku dokładnego: " + (pp[4].bestChromosomeEver[liczbaIteracji-1]/Dpp.value)*100 + "%");

            // Turniejowa
            Console.WriteLine("\nTurniejowa, dwa punkty krzyżowania:");

            pp[5] = new ProblemPlecakowy(populacja, liczbaIteracji, "TournamentSelection", "TwoPointCrossing", szansaNaMutacje, liczbaPrzedmiotow, Przedmioty, pojemnosc, tablicaPopulacji);
            while(pp[5].currentIteration < pp[5].maxIterations){
                pp[5].NextGeneration();
            }
            Console.WriteLine(pp[5].printHistory());
            Console.WriteLine("Część wyniku dokładnego: " + (pp[5].bestChromosomeEver[liczbaIteracji-1]/Dpp.value)*100 + "%");


            // zapis danych do excela
            XLWorkbook workBook = new XLWorkbook();
            DataTable[] dt = new DataTable[6];
            for(int i = 0; i < 6; i++){
                dt[i] = new DataTable();
                dt[i] = ZapisDoExcela(pp[i], i, liczbaIteracji, populacja, liczbaPrzedmiotow, szansaNaMutacje);
                workBook.Worksheets.Add(dt[i]);
            }
            workBook.SaveAs("dane.xlsx");
        }
        static public DataTable ZapisDoExcela(ProblemPlecakowy pp, int index, int liczbaIteracji, int populacja, int liczbaPrzedmiotow, double szansaNaMutacje){
            DataTable dt = new DataTable(); 
            dt.Clear();
            dt.Columns.Add("Iteracja");
            dt.Columns.Add("Srednie przystosowanie populacji");
            dt.Columns.Add("Najlepsze przystosowanie");
            for(int i = 0; i < liczbaIteracji; i++){
                DataRow row = dt.NewRow();
                row["Iteracja"] = i.ToString();
                row["Srednie przystosowanie populacji"] = pp.averageFitness[i].ToString();
                row["Najlepsze przystosowanie"] = pp.bestChromosome[i].ToString();
                dt.Rows.Add(row);
            }
            dt.TableName = "danePP"+index+"-"+populacja+"-"+liczbaIteracji+"-"+liczbaPrzedmiotow+"-"+szansaNaMutacje;
            return dt;
        }
    }
}