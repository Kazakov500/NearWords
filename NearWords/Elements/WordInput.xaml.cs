using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NearWords.Elements
{
    /// <summary>
    /// Interaction logic for WordInput.xaml
    /// </summary>
    public partial class WordInput : UserControl
    {
        public WordInput(string word, double similarity)
        {
            InitializeComponent();

            Random rnd = new Random();

            lbl_Word.Content = word;
            lbl_Similarity.Content = similarity;

            rect_Color.Fill = new SolidColorBrush(Color.FromRgb((byte)(rnd.Next(255)), (byte)(rnd.Next(255)), (byte)(rnd.Next(255))));

            if (similarity == -1)
            {
                lbl_Similarity.Background = new SolidColorBrush(Color.FromRgb(255,103,103));
            }
        }
    }
}
