using Spire.Xls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MergeExcel
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            string route = SelectedFolder();
            //string route = "C:\\Archivos";
            if (route != "")
            {
                InitializeComponent(route);
                CreateStructure();
                ExecuteMonitor();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Método que crea la estructura del proyecto
        /// Este crea carpetas Procesados y No Procesados
        /// </summary>
        public void CreateStructure(){
            string fileInput = input.Text.ToString();
            string processFiles = fileInput + "\\Procesado";
            string processNoFiles = fileInput + "\\No Aplicable";

            ///----- CARPETA PROCESADOS

            ///----- Si el directorio existe entonces lo eliminamos 
            ///----- con sus archivos dentro
            DirectoryInfo folder = new DirectoryInfo(processFiles);

            if (folder.Exists)
            {
                FileInfo[] files = folder.GetFiles();
                foreach (FileInfo file in files)
                {
                    file.Delete();
                }

                //----- Revisa si existen sub carpetas y las elimina
                DirectoryInfo[] folders = folder.GetDirectories();
                foreach (DirectoryInfo subfolder in folders)
                {
                    subfolder.Delete(true);//Se elimina aunque esté vacío
                }
            }

            ///----- Crea el directorio
            Directory.CreateDirectory(processFiles);

            ///----- CARPETA NO PROCESADOS
            
            ///----- Si el directorio existe entonces lo eliminamos 
            ///----- con sus archivos dentro
            DirectoryInfo folder2 = new DirectoryInfo(processNoFiles);

            if (folder2.Exists)
            {
                FileInfo[] files = folder2.GetFiles();
                foreach (FileInfo file in files)
                {
                    file.Delete();
                }

                //----- Revisa si existen sub carpetas y las elimina
                DirectoryInfo[] folders = folder2.GetDirectories();
                foreach (DirectoryInfo subfolder in folders)
                {
                    subfolder.Delete(true);//Se elimina aunque esté vacío
                }
            }

            ///----- Crea el directorio
            Directory.CreateDirectory(processNoFiles);

            /*
            //----- Elimina al archivo master
            if (File.Exists(fileInput + "\\FileOutput.xls"))
            {
                File.Delete(fileInput + "\\FileOutput.xls");
            }
            */
        }

        public string SelectedFolder()
        {
            string route = "";
            while (route=="")
            {
                using (FolderBrowserDialog objFolder = new FolderBrowserDialog())
                {
                    if (objFolder.ShowDialog() == DialogResult.OK)
                    {
                        route = objFolder.SelectedPath;
                    }
                }
            }

            return route;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //MergeXls();
        }

        public void ExecuteMonitor() {
            string fileInput = input.Text.ToString();
            using (FileSystemWatcher watcher = new FileSystemWatcher(fileInput)) {
                monitor.Path = fileInput;
                monitor.Created += MergeXLSFile;
                monitor.EnableRaisingEvents = true;
                //System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
            }
        }

        private void MergeXLSFile(object sender, FileSystemEventArgs e)
        {
            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    if (e.Name == "~$FileOutput.xls") {
                        break;
                    }
                    using (Stream st = File.Open(e.FullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        if (st != null)
                        {
                            System.Diagnostics.Trace.WriteLine(string.Format("Salida: ", e.FullPath));
                            break;
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
            ///----- Si se crea un nuevo archivo
            ///----- Si es xls se procede a moverlo a la carpeta Procesados
            ///----- Si no no es xls se mueve a No Procesados
            string fileInput = input.Text.ToString();

            if (e.Name != "FileOutput.xls")
            {
                if (e.Name != "~$FileOutput.xls")
                {
                    if (File.Exists(e.FullPath) && e.Name != "~$FileOutput.xls")
                    {
                        string extension = Path.GetExtension(e.FullPath);
                        if (extension == ".xls")
                        {
                            if (File.Exists(fileInput + "\\Procesado\\" + e.Name))
                            {
                                File.Delete(fileInput + "\\Procesado\\" + e.Name);
                            }
                            File.Move(e.FullPath, fileInput + "\\Procesado\\" + e.Name);
                            Thread.Sleep(1000);
                            MergeXls(fileInput + "\\Procesado");

                        }
                        else
                        {
                            if (File.Exists(fileInput + "\\No Aplicable\\" + e.Name))
                            {
                                File.Delete(fileInput + "\\No Aplicable\\" + e.Name);
                            }
                            File.Move(e.FullPath, fileInput + "\\No Aplicable\\" + e.Name);
                        }
                    }
                }
            }
        }

        public void MergeXls(string route) {
            Workbook newWorkbook = new Workbook();
            newWorkbook.Worksheets.Clear();

            Workbook tempWorkbook = new Workbook();


            string fileInput = input.Text.ToString();

            if (File.Exists(fileInput + "\\FileOutput.xls")) {
                File.Delete(fileInput + "\\FileOutput.xls");
            }

            DirectoryInfo di = new DirectoryInfo(route);
            FileInfo[] files = di.GetFiles("*.xls");

            foreach (FileInfo file in files)
            {
                tempWorkbook.LoadFromFile(file.FullName);
                foreach (Worksheet sheet in tempWorkbook.Worksheets)
                {
                    newWorkbook.Worksheets.AddCopy(sheet, WorksheetCopyType.CopyAll);
                }

            }
            newWorkbook.SaveToFile(fileInput + "/FileOutput.xls", ExcelVersion.Version2007);

            //MoveNoProcess();
        }
        public void MoveNoProcess()
        {
            File.Delete(input.Text.ToString() + "\\FileOutput.xls");
            string sourceF = input.Text.ToString();
            string destinF = input.Text.ToString() + "\\No Aplicable\\";
            //----- Mueve los archivos no procesados
            if (File.Exists(input.Text.ToString() + "\\No Aplicable\\"))
            {
                Directory.Move(input.Text.ToString(), input.Text.ToString() + "\\No Aplicable\\");
            }
            else
            {
                Directory.CreateDirectory(input.Text.ToString() + "\\No Aplicable\\");
                Directory.Move(sourceF, destinF);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void monitor_Changed(object sender, FileSystemEventArgs e)
        {

        }
    }
}
