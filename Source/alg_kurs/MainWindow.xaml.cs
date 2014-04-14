using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using Huffman;


namespace alg_kurs
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        //делегат для progressBar1
        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        private UpdateProgressBarDelegate updatePbDelegate;
        private long InputFileLengh = 0;
        private string FileExtension;

        public MainWindow()
        {
            InitializeComponent();
            updatePbDelegate = new UpdateProgressBarDelegate(progressBar1.SetValue);
            HuffmanAlg.ProgressEvent += new Action(OnProgressEvent);
        }

        private void OnProgressEvent()
        {
            progressBar1.Value++;
            label1.Content = progressBar1.Value.ToString() + "/" + InputFileLengh + " Bytes";
            Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressBar1.Value });
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            bool AppropriateDirectoryFlag = false;
            button1.IsEnabled = false;

            //кодируем
            if ((string)button1.Content == "Encode")
            {
                if(textBox2.Text.Length>=4)
                    if (textBox2.Text.Substring(textBox2.Text.Length - 4, 4) == ".haf")
                    {
                        progressBar1.Maximum = InputFileLengh;
                        progressBar1.Value = 0;
                        if (HuffmanAlg.EncodeFile(textBox1.Text, textBox2.Text))
                        {
                            AppropriateDirectoryFlag = true;
                            BinaryReader SourceFile = new BinaryReader(File.OpenRead(textBox1.Text));
                            BinaryReader OutputFile = new BinaryReader(File.OpenRead(textBox2.Text));
                            label2.Content = "Архивация успешна, степень сжатия: " + ((double)SourceFile.BaseStream.Length / (double)OutputFile.BaseStream.Length).ToString();
                            SourceFile.Close();
                            OutputFile.Close();
                            //File.Delete(textBox2.Text);
                        }
                    }
                if(!AppropriateDirectoryFlag) label2.Content = "Неверная Директория";
            }

            //декодируем
            if ((string)button1.Content == "Decode")
            {
                if (textBox2.Text.Substring(textBox2.Text.Length - FileExtension.Length, FileExtension.Length).Equals(FileExtension))
                {
                    progressBar1.Maximum = InputFileLengh;
                    progressBar1.Value = 0;
                    if (!HuffmanAlg.DecodeFile(textBox1.Text, textBox2.Text))
                        label2.Content = "Неверная Директория";
                    else label2.Content = "Декодирование успешно";
                }
                else label2.Content = "Неверная Директория";
            }
            button1.IsEnabled = true;
        }

 


        private void button2_Click(object sender, RoutedEventArgs e)
        {
            progressBar1.Value = 0;
            label1.Content = "";
            label2.Content = "";
            OpenFileDialog InputFileDialog = new OpenFileDialog();
            InputFileDialog.Filter = "Кодируемый файл (*.*)|*.*|Декодируемый файл (*.haf)|*.haf";
            InputFileDialog.ShowDialog();
            if (InputFileDialog.FileName != "") textBox1.Text = InputFileDialog.FileName;
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            progressBar1.Value = 0;
            label1.Content = "";
            label2.Content = "";
            SaveFileDialog OutputFileDialog = new SaveFileDialog();

            //если декодируем, извлекаем из сжатого файла расширение
            if (textBox1.Text.Substring(textBox1.Text.Length - 4, 4) == ".haf")
            {
                StreamReader Rdr = new StreamReader(File.OpenRead(textBox1.Text));
                char[] CharBuffer = new char[1];
                string StringBuffer = "";
                Rdr.Read(CharBuffer, 0, 1);
                while (CharBuffer[0] != '.')
                {
                    StringBuffer += CharBuffer[0].ToString();
                    Rdr.Read(CharBuffer, 0, 1);
                }
                OutputFileDialog.Filter = "Исходный файл (*." + StringBuffer + ")|*." + StringBuffer;
                Rdr.Close();
            }

            //иначе просто добавляем расширение .haf
            else
                OutputFileDialog.Filter = "Сжатый файл (*.haf)|*.haf";
            OutputFileDialog.ShowDialog();
            if (OutputFileDialog.FileName != "") textBox2.Text = OutputFileDialog.FileName;
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            label2.Content = "";
            label1.Content = "";
            button1.IsEnabled = true;
            if (File.Exists(textBox1.Text))
            {
                bool CanBeOpenedFlag = true;
                try
                {
                    FileStream CheckStream = File.Open(textBox1.Text, FileMode.Open);
                    CheckStream.Close();
                }
                catch
                {
                    CanBeOpenedFlag = false;
                    button3.IsEnabled = false;
                    textBox2.IsEnabled = false;
                    button1.IsEnabled = false;
                    label2.Content = "Файл используется другим приложением";
                }

                if (CanBeOpenedFlag)
                {
                    button3.IsEnabled = true;
                    textBox2.IsEnabled = true;
                    button1.IsEnabled = true;

                    //вывод размера кодируемого/декодируемого файла
                    FileStream Fstream = new FileStream(textBox1.Text, FileMode.Open);
                    InputFileLengh = Fstream.Length;
                    label1.Content = "0/" + InputFileLengh + " Bytes";
                    Fstream.Close();

                    if (textBox1.Text.Substring(textBox1.Text.Length - 4, 4) != ".haf")
                    {
                        button1.Content = "Encode";
                        FileInfo Finfo = new FileInfo(textBox1.Text);
                        textBox2.Text = textBox1.Text.Substring(0, textBox1.Text.Length - Finfo.Extension.Length) + ".haf";
                    }
                    else
                    {
                        button1.Content = "Decode";
                        StreamReader Rdr = new StreamReader(File.OpenRead(textBox1.Text));
                        char[] CharBuffer = new char[1];
                        FileExtension = "";
                        Rdr.Read(CharBuffer, 0, 1);
                        while (CharBuffer[0] != '.')
                        {
                            FileExtension += CharBuffer[0].ToString();
                            Rdr.Read(CharBuffer, 0, 1);
                        }

                        textBox2.Text = textBox1.Text.Substring(0, textBox1.Text.Length - 3) + FileExtension;
                        Rdr.Close();
                    }
                }
            }
            
            else
            {
                button3.IsEnabled = false;
                textBox2.IsEnabled = false; 
            }
        }
    }
}
