using System;
using System.Linq;

namespace gen
{

    class Dynamic_PP
    {
        public int value;
        public Dynamic_PP(int capacity, int[,] Items, int numberOfItems) : base() {
            int n = numberOfItems;
            int C = capacity;

            int[,] Aij = new int[C+1, n+1];
            int x1, x2, Cj, Vj;

            for (int i = 1; i <= C; i++){
                if(Items[0,1] <= i){
                    Aij[i,1] = Items[0,0];
                }
                for(int j = 1; j <= n; j++){
                    Cj = Items[j-1,1];
                    Vj = Items[j-1,0];
                    x1 = 0;
                    if(i - Cj >= 0){
                        x1 = Aij[i-Cj,j-1] + Vj;
                    }
                    x2 = Aij[i,j-1];
                    Aij[i,j] = Math.Max(x1, x2);
                }
            }
            value = Aij.Cast<int>().Max();
        }
    }
}