using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
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

namespace DiskMaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        List<FichierTemp> fichiers = new List<FichierTemp>();

        public MainWindow()
        {

            InitializeComponent();
            //chemin vers "Downloaded Program Files"
            DirectoryInfo downloadedProgramFiles = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Downloaded Program Files");
            //chemin vers "Temporary Internet Files"
            DirectoryInfo internetCache = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache));
            //chemin vers "Windows error reports and feedback"
            DirectoryInfo wer = new DirectoryInfo(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Microsoft\Windows\WER"
);
            //chemin vers "DirectX Shader Cache"
            DirectoryInfo dxCache = new DirectoryInfo(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Microsoft\DirectX Shader Cache"
);
            //chemin vers "Delivery Optimization Files"
            DirectoryInfo delivery = new DirectoryInfo(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Microsoft\Windows\DeliveryOptimization"
);
            //chemin vers "Recycle Bin"
            DirectoryInfo recycleBin = new DirectoryInfo(@"C:\$Recycle.Bin");
            //chemin vers "Temporary Files"
            DirectoryInfo temp = new DirectoryInfo(System.IO.Path.GetTempPath());
            //chemin vers "Thumbnails"
            DirectoryInfo thumbnails = new DirectoryInfo(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Microsoft\Windows\Explorer"
);

            fichiers.Add(new FichierTemp { Nom = "Downloaded Program Files", Dossier = downloadedProgramFiles });
            fichiers.Add(new FichierTemp { Nom = "Temporary Internet Files", Dossier = internetCache });
            fichiers.Add(new FichierTemp { Nom = "Windows Error Reports", Dossier = wer });
            fichiers.Add(new FichierTemp { Nom = "DirectX Shader Cache", Dossier = dxCache });
            fichiers.Add(new FichierTemp { Nom = "Delivery Optimization Files", Dossier = delivery });
            fichiers.Add(new FichierTemp { Nom = "Recycle Bin", Dossier = recycleBin });
            fichiers.Add(new FichierTemp { Nom = "Temporary Files", Dossier = temp });
            fichiers.Add(new FichierTemp { Nom = "Thumbnails", Dossier = thumbnails });
            ldb.ItemsSource = fichiers;
            AnalyzeDirectories();

        }

        private void systemFiles_Click(object sender, RoutedEventArgs e)
        {
            // éviter doublons
            if (fichiers.Any(f => f.Nom == "Windows Update Cleanup"))
                return;

            try
            {
                // Windows Update Cleanup
                DirectoryInfo windowsUpdate = new DirectoryInfo(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\SoftwareDistribution\Download");

                // Microsoft Defender
                DirectoryInfo defender = new DirectoryInfo(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Windows Defender");

                // Drivers
                DirectoryInfo drivers = new DirectoryInfo(
                    Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\DriverStore\FileRepository");

                // Language files
                DirectoryInfo lang = new DirectoryInfo(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\System32\en-US");

                fichiers.Add(new FichierTemp { Nom = "Windows Update Cleanup", Dossier = windowsUpdate });
                fichiers.Add(new FichierTemp { Nom = "Microsoft Defender Antivirus", Dossier = defender });
                fichiers.Add(new FichierTemp { Nom = "Device driver packages", Dossier = drivers });
                fichiers.Add(new FichierTemp { Nom = "Language Resource Files", Dossier = lang });

                ldb.Items.Refresh();

                AnalyzeDirectories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur fichiers système: {ex.Message}");
            }
        }

        //calculer la taille d'un dossier
        public long GetDirectorySize(DirectoryInfo directoryInfo)
        {
            long size = 0;

            try
            {
                FileInfo[] files = directoryInfo.GetFiles();
                foreach (FileInfo file in files)
                {
                    size += file.Length;
                }

                DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
                foreach (DirectoryInfo subDirectory in subDirectories)
                {
                    size += GetDirectorySize(subDirectory);
                }
            }
            catch
            {
            }

            return size;
        }

        //calculer la taille d'un fichier
        public long GetFileSize(FileInfo fileInfo)
        {
            return fileInfo.Length;
        }
        // vider un dossier
        public void ClearDirectory(DirectoryInfo directoryInfo)
        {
            // Supprimer tous les fichiers dans le dossier
            FileInfo[] files = directoryInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                file.Delete();
            }
            // Supprimer tous les sous-dossiers
            DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
            foreach (DirectoryInfo subDirectory in subDirectories)
            {
                ClearDirectory(subDirectory);
                subDirectory.Delete();
            }
        }
        // vider un fichier
        public void ClearFile(FileInfo fileInfo)
        {
            fileInfo.Delete();
        }

        private void quitter_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void nettoyer_Click(object sender, RoutedEventArgs e)
        {
            date.Content = $"Dernier nettoyage: {DateTime.Now}";
            //enregistrer la date du dernier nettoyage dans un fichier texte
            string lastclean = date.Content.ToString();
            File.WriteAllText("lastclean.txt", lastclean);

            CheckNews();
            github.Visibility = Visibility.Visible;
            //Clipboard.Clear();

            try
            {
                foreach (var f in fichiers)
                {
                    if (f.Dossier.Exists)
                    {
                        try
                        {
                            ClearDirectory(f.Dossier);
                        }
                        catch { }
                    }
                }
                AnalyzeDirectories();


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du nettoyage: {ex.Message}");
            }
        }

        private void github_Click(object sender, RoutedEventArgs e)
        {
            // Ouvrir le lien GitHub dans le navigateur par défaut ;
            Process.Start(new ProcessStartInfo("https://github.com/gamehooperstudio")
            {
                UseShellExecute = true
            });
            github.Visibility = Visibility.Hidden;

        }
        //analyser les fichiers d'un dossier
        public void AnalyzeDirectories()
        {
            long totalSize = 0;

            try
            {
                foreach (var f in fichiers)
                {
                    if (f.Dossier.Exists)
                    {
                        long size = GetDirectorySize(f.Dossier);
                        f.Taille = FormatSize(size);

                        totalSize += size;
                    }
                    else
                    {
                        f.Taille = "Introuvable";
                    }
                }

                espace.Content = $"Espace libérable: {FormatSize(totalSize)}";

                ldb.Items.Refresh();

                if (File.Exists("lastclean.txt"))
                    date.Content = File.ReadAllText("lastclean.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'analyse: {ex.Message}");
            }
        }
        public string FormatSize(long size)
        {
            double len = size;
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
        public class FichierTemp
        {
            public string Nom { get; set; }
            public DirectoryInfo Dossier { get; set; }
            public string Taille { get; set; }
        }

        private void explorer_Click(object sender, RoutedEventArgs e)
        {
            if (ldb.SelectedItem is not FichierTemp selected)
                return;

            if (selected.Dossier == null || !selected.Dossier.Exists)
                return;

            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{selected.Dossier.FullName}\"")
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture du dossier : {ex.Message}");
            }
        }

        private void assistanceStockage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("ms-settings:storagesense")
                {
                    UseShellExecute = true
                });
            }
            catch
            {
                try
                {
                    Process.Start(new ProcessStartInfo("ms-settings:storage")
                    {
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Impossible d'ouvrir l'assistance stockage : {ex.Message}");
                }
            }
        }

        public void CheckNews()
        {
            string newsUrl = "https://raw.githubusercontent.com/gamehooperstudio/DiskMaster/refs/heads/main/news.txt";
            using (WebClient client = new WebClient())
            {
                try
                {
                    string newsContent = client.DownloadString(newsUrl);
                    if (!string.IsNullOrEmpty(newsContent))
                    {
                        MessageBox.Show($"Nouvelles mises à jour :\n\n{newsContent}", "DiskMaster - Nouvelles", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    // Ignorer les erreurs de téléchargement
                }

            }
        }
    }
}