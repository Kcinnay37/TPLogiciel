using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace GestionStock
{
    public partial class Form1 : Form
    {
        Dictionary<string, Article> dictArticle = new Dictionary<string, Article>();
        
        string currItemSelected = string.Empty;

        bool sortByDate = false;

        public Form1()
        {
            InitializeComponent();
            LoadArticlesFromExcel();
            UpdateListView();
        }

        private void AddArticle()
        {
            if (textBoxName.Text.Equals(string.Empty) || textBoxBrand.Text.Equals(string.Empty)
                || textBoxPrice.Text.Equals(string.Empty) || numericUpDownQuantity.Value.Equals(0)
                || currItemSelected != string.Empty || dictArticle.ContainsKey(textBoxName.Text)) return;

            //creation de l'article
            Article article;
            try
            {
                article = new Article
                {
                    Nom = textBoxName.Text,
                    Marque = textBoxBrand.Text,
                    Prix = float.Parse(textBoxPrice.Text),
                    Quantite = numericUpDownQuantity.Value,
                    Date = monthCalendar.SelectionStart
                };
            }
            catch
            {
                article = new Article
                {
                    Nom = textBoxName.Text,
                    Marque = textBoxBrand.Text,
                    Prix = 0.0f,
                    Quantite = numericUpDownQuantity.Value,
                    Date = monthCalendar.SelectionStart
                };
            }


            //ajout dans le dictonnaire d'article
            dictArticle.Add(article.Nom, article);

            UpdateListView();

            ResetTextBoxe();

            UpdateExcelFile();
        }

        private void DeleteSelectedItem()
        {
            if(currItemSelected != string.Empty)
            {
                dictArticle.Remove(currItemSelected);

                if(listView1.SelectedItems.Count != 0)
                {
                    listView1.SelectedItems[0].Remove();
                }
                else
                {
                    int index = 0;
                    foreach(ListViewItem value in listView1.Items)
                    {
                        if(value.SubItems[0].Text.Equals(currItemSelected))
                        {
                            break;
                        }
                        index++;
                    }
                    listView1.Items.RemoveAt(index);
                    listView1.SelectedItems.Clear();
                    if(currItemSelected != string.Empty)
                    {
                        SelectedItemChanged();
                    }
                }

                UpdateExcelFile();
            }
        }

        private void SelectedItemChanged()
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem item = listView1.SelectedItems[0];
                textBoxName.Text = item.SubItems[0].Text;
                textBoxBrand.Text = item.SubItems[1].Text;
                textBoxPrice.Text = item.SubItems[2].Text;
                numericUpDownQuantity.Value = int.Parse(item.SubItems[3].Text);
                monthCalendar.SelectionStart = DateTime.Parse(item.SubItems[4].Text);

                labelItemSelected.Text = "Vous avez un item de selectonné";
                labelItemSelected.ForeColor = Color.Red;

                currItemSelected = item.SubItems[0].Text;
            }
            else
            {
                currItemSelected = string.Empty;

                labelItemSelected.Text = string.Empty;

                ResetTextBoxe();
            }
        }

        private void UpdateSeletedItem()
        {
            if (!textBoxName.Text.Equals(currItemSelected) && dictArticle.ContainsKey(textBoxName.Text)) return;

            Article article = dictArticle[currItemSelected];
            if(textBoxName.Text != string.Empty)
            {
                article.Nom = textBoxName.Text;
            }
            
            if(textBoxBrand.Text != string.Empty)
            {
                article.Marque = textBoxBrand.Text;
            }
            
            try
            {
                article.Prix = float.Parse(textBoxPrice.Text);
            }
            catch
            {
                article.Prix = 0.0f;
            }

            if(numericUpDownQuantity.Value != 0)
            {
                article.Quantite = numericUpDownQuantity.Value;
            }
            
            article.Date = monthCalendar.SelectionStart;

            dictArticle.Remove(currItemSelected);
            currItemSelected = article.Nom;
            dictArticle.Add(currItemSelected, article);

            UpdateListView();

            UpdateExcelFile();
        }

        private void ResetTextBoxe()
        {
            textBoxName.Text = string.Empty;
            textBoxBrand.Text = string.Empty;
            textBoxPrice.Text = string.Empty;
            numericUpDownQuantity.Value = 0;
            monthCalendar.SelectionStart = DateTime.Now;
        }

        private void UpdateListView()
        {
            listView1.Items.Clear();

            if(sortByDate)
            {
                List<Article> listSortByDate = new List<Article>();
                foreach (KeyValuePair<string, Article> value in dictArticle)
                {
                    Article article = value.Value;

                    if(listSortByDate.Count == 0)
                    {
                        listSortByDate.Add(article);
                        continue;
                    }

                    int index = 0;
                    for(int i = 0; i < listSortByDate.Count; i++)
                    {
                        index = i;
                        if(article.Date < listSortByDate[i].Date)
                        {
                            break;
                        }
                        index++;
                    }
                    if(index != listSortByDate.Count)
                    {
                        listSortByDate.Insert(index, article);
                    }
                    else
                    {
                        listSortByDate.Add(article);
                    }
                }

                foreach(Article article in listSortByDate)
                {
                    ListViewItem item = new ListViewItem(article.Nom);
                    item.SubItems.Add(article.Marque);
                    item.SubItems.Add(article.Prix.ToString());
                    item.SubItems.Add(article.Quantite.ToString());
                    item.SubItems.Add(article.Date.ToString());

                    listView1.Items.Add(item);
                }
            }
            else
            {
                foreach (KeyValuePair<string, Article> value in dictArticle)
                {
                    Article article = value.Value;

                    ListViewItem item = new ListViewItem(article.Nom);
                    item.SubItems.Add(article.Marque);
                    item.SubItems.Add(article.Prix.ToString());
                    item.SubItems.Add(article.Quantite.ToString());
                    item.SubItems.Add(article.Date.ToString());

                    listView1.Items.Add(item);
                }
            }

            string searchName = textBoxSearch.Text;
            if(searchName != string.Empty)
            {
                List<ListViewItem> itemToRemove = new List<ListViewItem>();
                foreach(ListViewItem item in listView1.Items)
                {
                    int i = 0;
                    foreach(char c in searchName)
                    {
                        if(searchName.Length > item.SubItems[0].Text.Length || c != item.SubItems[0].Text[i])
                        {
                            itemToRemove.Add(item);
                            break;
                        }
                        i++;
                    }
                }

                foreach (ListViewItem item in itemToRemove)
                {
                    listView1.Items.Remove(item);
                }
            }
        }

        private void LoadArticlesFromExcel()
        {
            string solutionDirectory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            string fileName = Path.Combine(solutionDirectory, "articles.xlsx");

            if (!File.Exists(fileName)) return;

            using (var workbook = new XLWorkbook(fileName))
            {
                var worksheet = workbook.Worksheet("Articles");

                int lastRow = worksheet.LastRowUsed().RowNumber();
                for (int row = 2; row <= lastRow; row++)
                {
                    Article article = new Article
                    {
                        Nom = worksheet.Cell(row, 1).Value.ToString(),
                        Marque = worksheet.Cell(row, 2).Value.ToString(),
                        Prix = float.Parse(worksheet.Cell(row, 3).Value.ToString()),
                        Quantite = int.Parse(worksheet.Cell(row, 4).Value.ToString()),
                        Date = DateTime.Parse(worksheet.Cell(row, 5).Value.ToString())
                    };

                    dictArticle.Add(article.Nom, article);
                }
            }
        }

        private void UpdateExcelFile()
        {
            // Créez un nouveau fichier Excel et ajoutez une feuille de calcul
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Articles");

                // Créez les en-têtes de colonne
                worksheet.Cell(1, 1).Value = "Nom";
                worksheet.Cell(1, 2).Value = "Marque";
                worksheet.Cell(1, 3).Value = "Prix";
                worksheet.Cell(1, 4).Value = "Quantité";
                worksheet.Cell(1, 5).Value = "Date";

                // Écrivez les articles du dictionnaire dans le fichier Excel
                int row = 2;
                foreach (var item in dictArticle.Values)
                {
                    worksheet.Cell(row, 1).Value = item.Nom;
                    worksheet.Cell(row, 2).Value = item.Marque;
                    worksheet.Cell(row, 3).Value = item.Prix;
                    worksheet.Cell(row, 4).Value = item.Quantite;
                    string date = item.Date.ToString();
                    worksheet.Cell(row, 5).Value = date;
                    row++;
                }

                // Ajustez la largeur des colonnes en fonction du contenu
                worksheet.Columns().AdjustToContents();

                // Enregistrez le fichier Excel dans le répertoire de la solution
                string solutionDirectory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
                string fileName = Path.Combine(solutionDirectory, "articles.xlsx");
                workbook.SaveAs(fileName);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            AddArticle();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedItem();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedItemChanged();
        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {
            if(currItemSelected != string.Empty)
            {
                UpdateSeletedItem();
            }
        }

        private void textBoxBrand_TextChanged(object sender, EventArgs e)
        {
            if (currItemSelected != string.Empty)
            {
                UpdateSeletedItem();
            }
        }

        private void textBoxPrice_TextChanged(object sender, EventArgs e)
        {
            if (currItemSelected != string.Empty)
            {
                UpdateSeletedItem();
            }
        }

        private void numericUpDownQuantity_ValueChanged(object sender, EventArgs e)
        {
            if (currItemSelected != string.Empty)
            {
                UpdateSeletedItem();
            }
        }

        private void monthCalendar_DateChanged(object sender, DateRangeEventArgs e)
        {
            if (currItemSelected != string.Empty)
            {
                UpdateSeletedItem();
            }
        }

        private void buttonSort_Click(object sender, EventArgs e)
        {
            sortByDate = true;
            UpdateListView();
        }

        private void buttonStopSort_Click(object sender, EventArgs e)
        {
            sortByDate = false;
            UpdateListView();
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            listView1.SelectedItems.Clear();
            UpdateListView();
        }

        private void buttonUnselectItem_Click(object sender, EventArgs e)
        {
            listView1.SelectedItems.Clear();
            if(currItemSelected != string.Empty)
            {
                SelectedItemChanged();
            }
        }
    }
}
