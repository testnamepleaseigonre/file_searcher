using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Windows.Forms;
using System.Threading;

namespace PD1
{
    public partial class Form1 : Form
    {
        private static List<string> finalFileList = new List<string>();
        private static List<string> finalDirectoryList = new List<string>();
        private string currentPath;
        private bool fileFound = false;
        private bool directoryFound = false;
        private bool progressBarChange = false;
        private bool pathChanged = false;
        private string timeElapsed;
        private int lenght = 0;

        public Form1()
        {
            InitializeComponent();
            fileNameTextBox.Text = "cmd";
            path.Text = "C:\\Windows\\System32";
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if(dialog.ShowDialog(this) == DialogResult.OK)
                path.Text = dialog.SelectedPath;
        }

        private void searchButton_Click(object sender, EventArgs e)
        {   
            if (String.IsNullOrWhiteSpace(path.Text) || String.IsNullOrWhiteSpace(fileNameTextBox.Text))
            {
                MessageBox.Show("Please fill in required fields!");
            }
            else
            {
                searchButton.Enabled = false;

                finalFileList.Clear();
                finalDirectoryList.Clear();
                currentPath = "";
                richTextBox1.Text = "";

                Thread th1 = new Thread(() => searchFunc(path.Text.ToString(), fileNameTextBox.Text.ToString()));
                Thread th2 = new Thread(() => printFunc(th1));

                th1.Start();
                Console.WriteLine($"Thread {th1.ManagedThreadId} [searching] started");
                th2.Start();
                Console.WriteLine($"Thread {th2.ManagedThreadId} [printing] started");
            }
        }

        private void searchFunc(string directory, string searchFileName)
        {
            var sw = Stopwatch.StartNew();

            lenght = 0;
            getProgressBarLenght(directory);

            Invoke((Action)delegate
            {
                progressBar1.Minimum = 0;
                progressBar1.Maximum = lenght;
                progressBar1.Value = 0;
                progressBar1.Step = 1;
            });

            Console.WriteLine(progressBar1.Maximum.ToString());
            findPathRecursion(directory, searchFileName);

            //Timer
            sw.Stop();
            timeElapsed = $"Time elapsed: {sw.Elapsed.TotalSeconds.ToString("0.##")}s";

            Invoke((Action)delegate
            {
                searchButton.Enabled = true;
            });
            Console.WriteLine(progressBar1.Value.ToString());
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} [searching] ended");
        }

        private void findPathRecursion(string directory, string searchFileName)
        {
            //Console.WriteLine($"Recursion Func on {Thread.CurrentThread.ManagedThreadId} thread");
            try
            {
                foreach (string dir in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
                {
                    findPathRecursion(dir, searchFileName);
                }
                progressBarChange = true;
                currentPath = directory.ToString();
                pathChanged = true;
                Thread.Sleep(50);
                if (directory == Path.GetDirectoryName(directory) + "\\" + searchFileName)
                {
                    finalDirectoryList.Add(Path.GetDirectoryName(directory).ToString());
                    directoryFound = true;
                    Thread.Sleep(50);
                }
                foreach (string file in Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly))
                {
                    if (searchFileName == Path.GetFileNameWithoutExtension(file))
                    {
                        finalFileList.Add(Path.GetDirectoryName(file).ToString());
                        fileFound = true;
                        Thread.Sleep(50);
                    }
                }
            }
            catch(UnauthorizedAccessException exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        private void printFunc(Thread th1)
        {
            while (th1.IsAlive)
            {
                if (fileFound == true)
                {
                    try
                    {
                        Invoke((Action)delegate
                        {
                            richTextBox1.Text += $"File found: {finalFileList[finalFileList.Count - 1]} \n";
                        });
                        fileFound = false;
                    }
                    catch
                    {
                        th1.Abort();
                    }
                }
                if (directoryFound == true)
                {
                    try
                    {
                        Invoke((Action)delegate
                        {
                            richTextBox1.Text += $"Directory found: {finalDirectoryList[finalDirectoryList.Count - 1]} \n";
                        });
                        directoryFound = false;
                    }
                    catch
                    {
                        th1.Abort();
                    }
                }
                if (progressBarChange == true)
                {
                    try
                    {
                        Invoke((Action)delegate
                        {
                            progressBar1.PerformStep();
                        });
                        progressBarChange = false;
                    }
                    catch
                    {
                        th1.Abort();
                    }
                }
                if (pathChanged == true)
                {
                   try
                    {
                        Invoke((Action)delegate
                        {
                            progressLabel.Text = currentPath;
                        });
                        pathChanged = false;
                    }
                    catch
                    {
                        th1.Abort();
                    }
                }
            }
            try
            {
                Invoke((Action)delegate
                {
                    if (finalDirectoryList.Count.Equals(0) && finalFileList.Count.Equals(0))
                    {
                        richTextBox1.Text = $"No files/directories found! \n";
                    }
                    richTextBox1.Text += timeElapsed;
                });
            }
            catch
            {

            }
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} [printing] ended");
        }

        private void getProgressBarLenght(string directory)
        {
            try
            {
                foreach (string dir in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
                {
                    
                    getProgressBarLenght(dir);
                    
                }
                lenght++;
            }
            catch(UnauthorizedAccessException exc)
            {
                Console.WriteLine(exc.Message);
            }
        }
    }
}
