using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinecraftOnPC
{
    public partial class Form1 : Form
    {
        private long lastLogUpdateTime = 0;
        private string adbExePath = "";
        private AdbInputHandler inputHandler;
        private bool isPhoneViewActive = true;

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint WM_HOTKEY = 0x0312;
        private const int HOTKEY_F6_ID = 1;
        private const int HOTKEY_F8_ID = 2;
        private const int HOTKEY_F7_ID = 3;
        private const int HOTKEY_F9_ID = 4;

        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;

        public Form1()
        {
            InitializeComponent();
            inputHandler = null;
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RegisterHotKey(this.Handle, HOTKEY_F6_ID, 0, (uint)Keys.F6);
            RegisterHotKey(this.Handle, HOTKEY_F8_ID, 0, (uint)Keys.F8);
            RegisterHotKey(this.Handle, HOTKEY_F7_ID, 0, (uint)Keys.F7);
            RegisterHotKey(this.Handle, HOTKEY_F9_ID, 0, (uint)Keys.F9);

            if (richTextBox1 != null)
            {
                richTextBox1.Text = "=== Minecraft PTP ===\n";
                richTextBox1.AppendText("Başlamak için düğmeye basın. . .\n\n");
                richTextBox1.AppendText("Kısayollar:\n");
                richTextBox1.AppendText("  F6 -> Yansıtmayı Tamamen Kapat\n");
                richTextBox1.AppendText("  F8 -> Cihaz Sesi Aç\n");
                richTextBox1.AppendText("  F7 -> Cihaz Sesi Kıs\n");
                richTextBox1.AppendText("  F9 -> PC / Oyun Ekranı Arasında Geçiş Yap \n\n");
            }

            if (comboBox_Rezolve != null)
            {
                comboBox_Rezolve.Items.Clear();
                comboBox_Rezolve.Items.Add("1920x1080");
                comboBox_Rezolve.Items.Add("1280x720");
                comboBox_Rezolve.Items.Add("960x540");
                comboBox_Rezolve.Items.Add("854x480");
                comboBox_Rezolve.Items.Add("Orijinal (En Akıcı Mod)");
                comboBox_Rezolve.SelectedIndex = 4;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (id == HOTKEY_F6_ID)
                {
                    KapatScrcpy();
                }
                else if (id == HOTKEY_F8_ID)
                {
                    if (inputHandler != null)
                    {
                        inputHandler.SendKeyEventByName("VOLUME_UP");
                        YaziBas("[Sistem] Telefon sesi artırıldı\n");
                    }
                }
                else if (id == HOTKEY_F7_ID)
                {
                    if (inputHandler != null)
                    {
                        inputHandler.SendKeyEventByName("VOLUME_DOWN");
                        YaziBas("[Sistem] Telefon sesi kısıldı (F7)\n");
                    }
                }
                else if (id == HOTKEY_F9_ID)
                {
                    ToggleEkranGecisi();
                }
            }

            base.WndProc(ref m);
        }

        private void ToggleEkranGecisi()
        {
            IntPtr scrcpyHwnd = FindWindow(null, "MinecraftYansitma");

            if (scrcpyHwnd != IntPtr.Zero)
            {
                if (isPhoneViewActive)
                {
                    ShowWindow(scrcpyHwnd, SW_MINIMIZE);
                    isPhoneViewActive = false;
                    YaziBas("[Sistem] PC Kontrolü açıldı.\n");
                }
                else
                {
                    ShowWindow(scrcpyHwnd, SW_RESTORE);
                    SetForegroundWindow(scrcpyHwnd);
                    isPhoneViewActive = true;
                    YaziBas("[Sistem] Oyun kontrolü açıldı.\n");
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_F6_ID);
            UnregisterHotKey(this.Handle, HOTKEY_F7_ID);
            UnregisterHotKey(this.Handle, HOTKEY_F8_ID);
            UnregisterHotKey(this.Handle, HOTKEY_F9_ID);
            KapatScrcpy();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (button1 != null) button1.Enabled = false;
            if (progressBar1 != null) { progressBar1.Value = 0; progressBar1.Maximum = 100; }
            if (richTextBox1 != null)
            {
                richTextBox1.Clear();
                richTextBox1.Text = "=== Sistem Başlatılıyor ===\n";
            }

            string uygulamaYolu = AppDomain.CurrentDomain.BaseDirectory;
            string externalKlasorYolu = Path.Combine(uygulamaYolu, "external");
            string scrcpyExeYolu = Path.Combine(externalKlasorYolu, "scrcpy.exe");
            adbExePath = Path.Combine(externalKlasorYolu, "adb.exe");

            Directory.CreateDirectory(externalKlasorYolu);

            if (!File.Exists(scrcpyExeYolu))
            {
                YaziBas("[1/3] Bileşenler kontrol ediliyor. . .\n");
                try
                {
                    string scrcpyUrl = "https://github.com/Genymobile/scrcpy/releases/download/v2.4/scrcpy-win64-v2.4.zip";
                    string tempZipPath = Path.Combine(Path.GetTempPath(), "scrcpy_temp.zip");

                    await DownloadFileWithProgressAsync(scrcpyUrl, tempZipPath);
                    YaziBas("Bileşenler kontrol edildi. . .\n");

                    await Task.Run(() => ZipDosyasiniCikar(tempZipPath, externalKlasorYolu));
                    if (File.Exists(tempZipPath)) File.Delete(tempZipPath);

                    YaziBas("Kurulum başarıyla tamamlandı.\n");
                }
                catch (Exception ex)
                {
                    YaziBas("Hata: " + ex.Message + "\n");
                    if (button1 != null) button1.Enabled = true;
                    return;
                }
            }

            YaziBas("\n[2/3] USB Hata Ayıklama bağlantısı doğrulanıyor...\n");
            if (!TelefonBagliMi(adbExePath))
            {
                YaziBas("Cihaz bulunamadı. Lütfen telefonu USB ile bağlayıp Hata Ayıklama izni verin.\n");
                if (button1 != null) button1.Enabled = true;
                return;
            }

            YaziBas("\n[3/3] Uygulama başlatılıyor. . .\n");

            string rezolve = comboBox_Rezolve != null
                ? (comboBox_Rezolve.SelectedItem?.ToString() ?? "Orijinal (En Akıcı Mod)")
                : "Orijinal (En Akıcı Mod)";

            // Sağ tık sorununu çözmek için --forward-all-clicks eklendi
            // args tanımını şu şekilde değiştir:
            string args =
                "--keyboard=uhid " +
                "--mouse=uhid " +
                "--forward-all-clicks " +
                "-f " + // Tam ekran modu
                "--window-title \"MinecraftYansitma\" " + // Pencere ismini sabitledik
                "--video-bit-rate=16M " +
                "--turn-screen-off";

            if (rezolve != "Orijinal (En Akıcı Mod)")
            {
                string[] boyutlar = rezolve.Split('x');
                if (boyutlar.Length == 2 && int.TryParse(boyutlar[0], out int width))
                {
                    args += $" --max-size={width}";
                }
            }

            try
            {
                if (progressBar1 != null) progressBar1.Value = 80;

                BaslatScrcpyGorunur(scrcpyExeYolu, args, externalKlasorYolu);

                inputHandler = new AdbInputHandler(adbExePath);
                isPhoneViewActive = true;

                
                YaziBas("UHID fare/klavye modu ve sağ tık desteği aktif.\n");

                LaunchMinecraftOnPhone(adbExePath);

                if (progressBar1 != null) progressBar1.Value = 100;
            }
            catch (Exception error)
            {
                YaziBas("Sistem tetikleme hatası: " + error.Message + "\n");
            }
            finally
            {
                if (button1 != null) button1.Enabled = true;
            }
        }

        private void BaslatScrcpyGorunur(string scrcpyExeYolu, string args, string workingDirectory)
        {
            if (!File.Exists(scrcpyExeYolu))
                throw new FileNotFoundException("Bileşen bulunamadı.", scrcpyExeYolu);

            // CMD penceresi yerine doğrudan süreci başlatıyoruz
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = scrcpyExeYolu,
                Arguments = args,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            Process.Start(startInfo);
        }

        private void LaunchMinecraftOnPhone(string adbYolu)
        {
            if (!File.Exists(adbYolu)) return;

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = adbYolu,
                    Arguments = "shell monkey -p com.mojang.minecraftpe -c android.intent.category.LAUNCHER 1",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process?.WaitForExit(3000);
                }

                YaziBas("Telefonda Minecraft otomatik başlatıldı.\n");
            }
            catch (Exception ex)
            {
                YaziBas("Oyun telefonda otomatik açılamadı: " + ex.Message + "\n");
            }
        }

        private void KapatScrcpy()
        {
            try
            {
                var processes = Process.GetProcessesByName("scrcpy");
                foreach (var p in processes)
                {
                    try
                    {
                        p.Kill();
                        p.WaitForExit(1000);
                    }
                    catch { }
                }

                var cmdProcesses = Process.GetProcessesByName("cmd");
                foreach (var p in cmdProcesses)
                {
                    try
                    {
                        if (p.MainWindowTitle != null && p.MainWindowTitle.Length >= 0)
                        {
                            p.Kill();
                            p.WaitForExit(1000);
                        }
                    }
                    catch { }
                }

                YaziBas("Telefon oturumu sonlandırıldı.\n");
            }
            catch (Exception ex)
            {
                YaziBas("Kapatma işlemi tamamlanırken hata oluştu: " + ex.Message + "\n");
            }
        }

        private void YaziBas(string mesaj)
        {
            if (richTextBox1 == null) return;

            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Action(() =>
                {
                    if (richTextBox1 != null)
                    {
                        richTextBox1.AppendText(mesaj);
                        richTextBox1.ScrollToCaret();
                    }
                }));
            }
            else
            {
                richTextBox1.AppendText(mesaj);
                richTextBox1.ScrollToCaret();
            }
        }

        private void ZipDosyasiniCikar(string zipPath, string hedefKlasor)
        {
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    int toplamDosya = archive.Entries.Count;
                    int sayac = 0;

                    foreach (ZipArchiveEntry dosya in archive.Entries)
                    {
                        sayac++;
                        if (string.IsNullOrWhiteSpace(dosya.FullName)) continue;

                        string[] yolParcalari = dosya.FullName.Split(new[] { '/' }, 2);
                        if (yolParcalari.Length == 1 || string.IsNullOrEmpty(yolParcalari[1])) continue;

                        string hedefYol = Path.Combine(hedefKlasor, yolParcalari[1].Replace('/', Path.DirectorySeparatorChar));

                        if (dosya.FullName.EndsWith("/"))
                        {
                            if (!Directory.Exists(hedefYol)) Directory.CreateDirectory(hedefYol);
                        }
                        else
                        {
                            string klasor = Path.GetDirectoryName(hedefYol);
                            if (!string.IsNullOrEmpty(klasor) && !Directory.Exists(klasor))
                                Directory.CreateDirectory(klasor);

                            try { dosya.ExtractToFile(hedefYol, true); } catch { }
                        }

                        int ilerleme = (sayac * 100) / toplamDosya;
                        if (progressBar1 != null && progressBar1.InvokeRequired)
                        {
                            progressBar1.Invoke(new Action(() =>
                            {
                                if (progressBar1 != null) progressBar1.Value = ilerleme;
                            }));
                        }
                        else if (progressBar1 != null)
                        {
                            progressBar1.Value = ilerleme;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                YaziBas("Arşiv Çıkarma Hatası: " + ex.Message + "\n");
            }
        }

        private async Task DownloadFileWithProgressAsync(string url, string destinationPath)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                client.Timeout = TimeSpan.FromMinutes(15);

                try
                {
                    using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        long totalBytes = response.Content.Headers.ContentLength ?? 0;

                        using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                               fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            byte[] buffer = new byte[8192];
                            long totalRead = 0;
                            int read;

                            while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                totalRead += read;

                                if (totalBytes > 0)
                                {
                                    int progress = (int)((double)totalRead / totalBytes * 100);
                                    progress = Math.Min(100, Math.Max(0, progress));

                                    long currentTime = DateTime.Now.Ticks;
                                    if (currentTime - lastLogUpdateTime > 5000000)
                                    {
                                        lastLogUpdateTime = currentTime;
                                        if (progressBar1 != null)
                                        {
                                            if (progressBar1.InvokeRequired)
                                            {
                                                progressBar1.Invoke(new Action(() =>
                                                {
                                                    if (progressBar1 != null) progressBar1.Value = progress;
                                                }));
                                            }
                                            else
                                            {
                                                progressBar1.Value = progress;
                                            }
                                        }

                                        long mb = totalRead / 1024 / 1024;
                                        long totalMb = totalBytes / 1024 / 1024;
                                        YaziBas($"  %{progress} ({mb}MB / {totalMb}MB)\n");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    throw new Exception("Bileşen hatası oluştu: " + ex.Message);
                }
            }
        }

        private bool TelefonBagliMi(string adbYolu)
        {
            if (!File.Exists(adbYolu)) return false;

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = adbYolu,
                    Arguments = "devices",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    string cikis = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    string[] satirlar = cikis.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string satir in satirlar)
                    {
                        if (satir.Contains("\tdevice") || satir.EndsWith(" device"))
                            return true;
                    }

                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}