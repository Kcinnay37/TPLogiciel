using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionStock
{
    class Article
    {
        public int IdArticle { get; set; }
        public string Nom { get; set; }
        public string Marque { get; set; }
        public float Prix { get; set; }
        public decimal Quantite { get; set; }
        public DateTime Date { get; set; }
    }
}
