using NearWords.Elements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

//using System.Web.

namespace NearWords
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Timer for checking internet connection
        System.Windows.Threading.DispatcherTimer timer;
        int NumberOfWords;
        double Zoom = 200.0;
        bool Add_All_Words = false;



        public MainWindow()
        {
            InitializeComponent();
            NumberOfWords = 1;

            SetUp_TimerForInternetConnertion();
            Zoom = G_WordsViz.ActualHeight / 2;

            DisableAll();
        }

        //Get similarite function with catching internet anf input errors
        double GetSimilarity(string Word1, string Word2)
        {
            try
            {
                Word1 = Word1.Trim()
                             .ToLower();
                Word2 = Word2.Trim()
                             .ToLower();

                string Model = ((ListBoxItem)(comB_Model.SelectedItem))
                                .Content.ToString();

                string href = "http://rusvectores.org/"
                            + Model
                            + "/"
                            + Word1
                            + "__"
                            + Word2
                            + "/api/similarity/";

                WebClient client = new WebClient();
                string downloadedString;
                try
                {
                    downloadedString = client.DownloadString(href);
                    InternetConnectionIndicator.Fill = new SolidColorBrush(Colors.Green);
                }
                catch (Exception ex)
                {
                    timer.Start();
                    return -1;
                }

                string res_Similarity = downloadedString.Split(new char[] { '\t' })[0];


                return Convert.ToDouble(res_Similarity);
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        //Timer functions for checking internet connection
        void SetUp_TimerForInternetConnertion()
        {
            timer = new System.Windows.Threading.DispatcherTimer();

            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        void timerTick(object sender, EventArgs e)
        {
            double sim = GetSimilarity("собака", "кошка");
            if (sim == -1)
            {
                InternetConnectionIndicator.Fill = new SolidColorBrush(Colors.Red);
            }
            else
            {
                InternetConnectionIndicator.Fill = new SolidColorBrush(Colors.Green);
                timer.Stop();
            }
        }

        void AddNewWordToList()
        {
            double sim = GetSimilarity(tb_MainWord.Text, tb_NewWord.Text);

            //If Add_All_Words = false we not add words with sim = -1
            //If Add_All_Words = true we add words with sim = -1
            if (!Add_All_Words && sim == -1) return;

            WordInput WordElement = new WordInput(tb_NewWord.Text, sim);
            WordElement.btn_Delete.MouseUp += Btn_Delete_MouseUp;

            sp_NewWords.Children.Add(WordElement);

            NumberOfWords++;

            SortingWords();


        }

        private void Btn_Delete_MouseUp(object sender, MouseButtonEventArgs e)
        {
            NumberOfWords--;
            sp_NewWords.Children.Remove((((sender as Ellipse).Parent as Grid).Parent as UserControl));

            SortingWords();

        }

        void SortingWords()
        {
            WordInput[] WIs = new WordInput[sp_NewWords.Children.Count];
            sp_NewWords.Children.CopyTo(WIs, 0);
            WIs = (WIs.OrderByDescending(item => Convert.ToDouble(item.lbl_Similarity.Content))).ToArray();

            sp_NewWords.Children.Clear();
            foreach (var item in WIs)
            {
                sp_NewWords.Children.Add(item);
            }
            DrawCirclesOnViz();

        }

        void DrawCirclesOnViz()
        {
            G_CirclesViz.Children.Clear();
            G_LinesViz.Children.Clear();

            int CountElements = sp_NewWords.Children.Count;
            if (CountElements != 0)
            {
                double angle = 360.0 / CountElements;
                int num = 0;
                foreach (WordInput WordElement in sp_NewWords.Children)
                {
                    double sim = Convert.ToDouble(WordElement.lbl_Similarity.Content);

                    Ellipse Ell_newWord = new Ellipse();
                    Ell_newWord.Fill = WordElement.rect_Color.Fill;
                    G_CirclesViz.Children.Add(Ell_newWord);
                    Ell_newWord.Height = Ell_newWord.Width = 10;
                    double distance = ((1 - sim) * Zoom) - 5;
                    double x = Math.Cos(angle * num * (Math.PI / 180));
                    double y = Math.Sin(angle * num * (Math.PI / 180));
                    Ell_newWord.Margin = new Thickness(x * distance, 0, 0, y * distance);

                    Line line = new Line();
                    //line.VerticalAlignment = VerticalAlignment.Bottom;
                    //line.HorizontalAlignment = HorizontalAlignment.Left;
                    line.Stroke = new SolidColorBrush(Color.FromRgb(150, 150, 150));

                    G_LinesViz.Children.Add(line);

                    line.X1 = G_WordsViz.ActualHeight / 2;
                    line.Y1 = G_WordsViz.ActualHeight / 2;
                    line.X2 = G_WordsViz.ActualHeight / 2 + (x * distance / 2);
                    line.Y2 = G_WordsViz.ActualHeight / 2 - (y * distance / 2);

                    //G_LinesViz.Children.Add(line);


                    num++;
                }
            }
        }

        private void btn_AddNewWord_Click(object sender, RoutedEventArgs e)
        {
            AddNewWordToList();
        }

        private void btn_Zoom_Plus_Click(object sender, RoutedEventArgs e)
        {
            Zoom += 10;
            MiddleLine.Width = MiddleLine.Height = Zoom;
            DrawCirclesOnViz();
        }

        private void btn_Zoom_Minus_Click(object sender, RoutedEventArgs e)
        {
            Zoom -= 10;
            if (Zoom < 10) Zoom = 10;

            MiddleLine.Width = MiddleLine.Height = Zoom;
            DrawCirclesOnViz();
        }

        private void btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            sp_NewWords.Children.Clear();
            G_CirclesViz.Children.Clear();
            G_LinesViz.Children.Clear();
        }

        private void btn_Add10_Click(object sender, RoutedEventArgs e)
        {
            Get10MfxSimilarityWordsFromJsonQuery(tb_MainWord.Text);
        }

        void Get10MfxSimilarityWordsFromJsonQuery(string Word)
        {
            //try
            //{
            Word = Word.Trim()
                         .ToLower();


            string Model = ((ListBoxItem)(comB_Model.SelectedItem))
                            .Content.ToString();

            string href = "http://rusvectores.org/"
                        + Model
                        + "/"
                        + Word
                        + "/api/json/";

            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            string json;
            try
            {
                json = client.DownloadString(href);
            }
            catch (Exception ex)
            {
                timer.Start();
                return;
            }

            //string res_Similarity = downloadedString.Split(new char[] { '\t' })[0];
            dynamic jsonDe = JsonConvert.DeserializeObject(json);

            //var j1 = jsonDe[0];
            //var j2 = j1[0];
            //var j3 = j2[0];
            dynamic j = jsonDe[Model];
            dynamic jj = j.Last;


            return;
            //}
            //catch (Exception ex)
            //{
            //    return;
            //}


        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            G_WordsViz.Width = G_WordsViz.ActualHeight;
            Zoom = G_WordsViz.ActualHeight / 2;
            MiddleLine.Width = MiddleLine.Height = Zoom;
            DrawCirclesOnViz();
            //MainEllipse.Width = MainEllipse.Height = G
        }

        private void btn_CheckMainWord_Click(object sender, RoutedEventArgs e)
        {
            Color c_Wait = Color.FromRgb(238, 255, 130);
            Color c_Ok = Color.FromRgb(140, 255, 130);
            Color c_Wrong = Color.FromRgb(255, 130, 130);

            double sim = GetSimilarity(tb_MainWord.Text, "собака");

            if (sim == -1)
            {
                DisableAll();
                btn_CheckMainWord.Content = "X";
            }
            else
            {
                EnableAll();
                btn_CheckMainWord.Content = "V";
            }

            btn_CheckMainWord.Background = (sim == -1) ? new SolidColorBrush(c_Wrong) : new SolidColorBrush(c_Ok);


        }

        void EnableAll()
        {
            btn_Add10.IsEnabled = true;
            btn_AddNewWord.IsEnabled = true;
            tb_NewWord.IsEnabled = true;
        }

        void DisableAll()
        {
            btn_Add10.IsEnabled = false;
            btn_AddNewWord.IsEnabled = false;
            tb_NewWord.IsEnabled = false;
        }

        private void btn_RandomColors_Click(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();

            foreach (WordInput item in sp_NewWords.Children)
            {
                item.rect_Color.Fill = new SolidColorBrush(Color.FromRgb((byte)(rnd.Next(255)), (byte)(rnd.Next(255)), (byte)(rnd.Next(255))));
                G_WordsViz.Refresh();
            }
            DrawCirclesOnViz();
        }

        private void InternetConnectionIndicator_MouseDown(object sender, MouseButtonEventArgs e)
        {
            btn_Clear_Click(null, null);

            //Script_Words();
            Script_LongWords();


        }

        #region Scripts

        void Script_Words()
        {
            string[] Some_Words =
            {
                "Кошка",
                "Дом",
                "Машина",
                "Пес",
                "Самолет",
                "Солнце",
                "Звезда",
                "звездаааа"
            };

            foreach (var item in Some_Words)
            {
                tb_NewWord.Text = item;
                AddNewWordToList();

                RefreshAll();
            }
        }

        void Script_LongWords()
        {
            tb_MainWord.Text = "абсолютизироваться";

            #region Arrays
            #region А
            string[] str_A =
                            {
"абонент - получатель   ",
"абстракционистский     ",
"автоаутентификация     ",
"автобиографический     ",
"автобиографичность     ",
"автокинопередвижка     ",
"автокорреляционный     ",
"автоматизированный     ",
"автоматизироваться     ",
"автомобилестроение     ",
"автопромышленность     ",
"автохарактеристика     ",
"агент - сигнализатор   ",
"агент - совместитель   ",
"агиоантропотопоним     ",
"агролесомелиоратор     ",
"агролесомелиорация     ",
"агропромышленность     ",
"агрохимлаборатория     ",
"адреномиметический     ",
"акклиматизационный     ",
"акклиматизирование     ",
"аллоиммуноглобулин     ",
"аллокератопластика     ",
"аллоплазматический     ",
"аллосенсибилизация     ",
"аллотрансплантация     ",
"алфавитно - цифровой   ",
"альфа - разнообразие   ",
"альфа - стабилизатор   ",
"альфа - тестирование   ",
"анархо - синдикалист   ",
"ангиоархитектоника     ",
"ангионевротический     ",
"ангиоэнцефалопатия     ",
"англичанин - католик   ",
"англо - американский   ",
"антигемолитический     ",
"антигравитационный     ",
"антидемократически     ",
"антидиалектический     ",
"антииммуноглобулин     ",
"антикоммутирование     ",
"антиметаболический     ",
"антимилитаристский     ",
"антиотождествление     ",
"антипараллелограмм     ",
"антипатриотический     ",
"антипедагогический     ",
"антирефлексивность     ",
"антисенсибилизатор     ",
"антисенсибилизация     ",
"антисимметрический     ",
"антисимметричность     ",
"антисифилитический     ",
"антисклеротический     ",
"антиспазматический     ",
"антиферромагнетизм     ",
"антихудожественный     ",
"антропометрический     ",
"антропоморфический     ",
"аппарат - облучатель   ",
"аппроксимируемость     ",
"ареометр - пикнометр   ",
"ассемблер - эмулятор   ",
"астронавигационный     ",
"астроспектроскопия     ",
"аудиопроигрыватель     ",
"аутовакцинотерапия     ",
"аутодифференциация     ",
"аутокаталитический     ",
"аутоофтальмоскопия     ",
"ауторепродуктивный     ",
"аутосенсибилизация     ",
"аутотрансплантация     ",
"аэрофотограмметрия     ",
"аэрофототопография     "
            };
            #endregion А
            #region Б
            string[] str_B =
                {
"базарнокарабулакец          ",
"бактериологический          ",
"банк - корреспондент        ",
"баржа - нефтесборщик        ",
"барометр - высотомер        ",
"башня - концентратор        ",
"безапелляционность          ",
"бездоказательность          ",
"безлогарифмический          ",
"безмедикаментозный          ",
"безосновательность          ",
"безответственность          ",
"безотлагательность          ",
"безотносительность          ",
"безыскусственность          ",
"белгородднестровец          ",
"бензоэлектрический          ",
"бескомпромиссность          ",
"беспараметрический          ",
"бесперспективность          ",
"бесхозяйственность          ",
"бета - радиоактивный        ",
"бета - распределение        ",
"библиографирование          ",
"бидистиллированный          ",
"бизнес - образование        ",
"биоаккумулирование          ",
"биокибернетический          ",
"биоритмологический          ",
"биортонормирование          ",
"биотелеметрический          ",
"биоэквивалентность          ",
"благовоспитанность          ",
"благожелательность          ",
"благоприобретенный          ",
"благоприятствовать          ",
"благосостоятельный          ",
"благотворительница          ",
"богоотступнический          ",
"богоотступничество          ",
"большеберезниковец          ",
"бронхоальвеолярный          ",
"бронховезикулярный          ",
"быстрозамороженный          ",
"быстроизнашиваемый          ",
"быстроразгрузочный          "
            };
            #endregion Б
            #region В
            string[] str_C =
                {
"вагон - рефрижератор                  ",
"вагоностроительный                    ",
"вазоконстрикторный                    ",
"вакуум - формовочный                  ",
"вегетарианствовать                    ",
"великодушествовать                    ",
"великомученический                    ",
"вентрикулопластика                    ",
"вероотступнический                    ",
"вероотступничество     ",
"ветронепроницаемый                    ",
"ветроэлектрический                    ",
"вещественнозначный                    ",
"взаимносопряжённый                    ",
"взаимозаменяемость                    ",
"взаиморасположение                    ",
"взлётно - посадочный                  ",
"виброизмерительный                    ",
"виброэлектропривод                    ",
"видеоидентификация                    ",
"видеомагнитофонный                    ",
"видеомоделирование                    ",
"видеопредставление                    ",
"видеопроизводитель                    ",
"видеосинхронизатор                    ",
"видеосинхронизация                    ",
"вице - президентский                  ",
"вице - президентство                  ",
"ВИЧ - инфицированный                  ",
"влагонепроницаемый                    ",
"внеконституционный                    ",
"внешнеполитический                    ",
"внутриартериальный                    ",
"внутрижелудочковый                    ",
"внутримолекулярный                    ",
"внутриплацентарный                    ",
"внутрипроцессорный                    ",
"внутрирастительный                    ",
"внутриселезеночный                    ",
"воднодисперсионный                    ",
"водносуспензионный                    ",
"водогрязелечебница                    ",
"водомер - расходомер                  ",
"водораспределитель                    ",
"воздухонагреватель                    ",
"воздухопроницаемый                    ",
"воздушно - десантный                  ",
"волномер - самописец                  ",
"волокнистообразный                    ",
"вольнопрактикующий                    ",
"волюнтаристический                    ",
"восемнадцатилетний                    ",
"воспрепятствование                    ",
"восьмидесятилетний                    ",
"высококачественный                    ",
"высококоэрцитивный                    ",
"высоколегированный                    ",
"высокомолекулярный                    ",
"высоконравственный                    ",
"высокообразованный                    ",
"высокооплачиваемый                    ",
"высокопараллельный                    ",
"высокопоставленный                    "
            };
            #endregion В
            #endregion Arrays



            foreach (var item in str_A)
            {
                tb_NewWord.Text = item.Replace(" ", "");
                AddNewWordToList();

                RefreshAll();
            }

            foreach (var item in str_B)
            {
                tb_NewWord.Text = item.Replace(" ", "");
                AddNewWordToList();

                RefreshAll();
            }

            foreach (var item in str_C)
            {
                tb_NewWord.Text = item.Replace(" ", "");
                AddNewWordToList();

                RefreshAll();
            }


        }

        #endregion

        void RefreshAll()
        {
            //Work only with two lines :)
            G_WordsViz.UpdateLayout();
            G_WordsViz.Refresh();
        }

        private void tb_MainWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btn_CheckMainWord_Click(null, null);
        }

        private void tb_NewWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddNewWordToList();
        }
    }

    //Extension Methods cool thing)))))
    //Chreate method *Refresh*
    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }
}
