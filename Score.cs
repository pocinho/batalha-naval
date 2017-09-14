using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatalhaNavalCmd
{
    class Score
    {
        public int Tentativas { get; set; }
        public int ScoreTotal { get; set; }
        public string NomeJogador { get; set; }

        public Score(int score_total, int tentativas, string nome)
        {
            this.Tentativas = tentativas;
            this.ScoreTotal = score_total;
            this.NomeJogador = nome;
        }

        public override string ToString()
        {
            return ScoreTotal + ";" + Tentativas + ";" + NomeJogador;
        }
    }
}
