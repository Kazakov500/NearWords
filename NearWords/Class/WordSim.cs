using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearWords.Class
{
    public class WordSim
    {
        public string Word;
        public double Sim;

        public WordSim(string word, double sim)
        {
            Word = word
                .Trim();
            Sim = sim;
        }
    }
}
