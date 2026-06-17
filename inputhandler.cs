using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace MinecraftOnPC
{
    public class AdbInputHandler
    {
        private string adbExePath;
        private Dictionary<int, string> keyCodeMap;

        public AdbInputHandler(string adbPath)
        {
            adbExePath = adbPath;
            keyCodeMap = new Dictionary<int, string>();
            InitializeKeyCodeMap();
        }

        private void InitializeKeyCodeMap()
        {
            keyCodeMap.Add(8, "67");
            keyCodeMap.Add(13, "66");
            keyCodeMap.Add(27, "4");
            keyCodeMap.Add(32, "62");
            keyCodeMap.Add(37, "21");
            keyCodeMap.Add(38, "19");
            keyCodeMap.Add(39, "22");
            keyCodeMap.Add(40, "20");
            keyCodeMap.Add(46, "112");
            keyCodeMap.Add(9, "61");
            keyCodeMap.Add(116, "82");
            keyCodeMap.Add(123, "123");
        }

        public void SendKeyEvent(int keyCode)
        {
            if (keyCodeMap.ContainsKey(keyCode))
            {
                string androidKeyCode = keyCodeMap[keyCode];
                ExecuteAdb($"shell input keyevent {androidKeyCode}");
            }
        }

        public void SendTextInput(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            string sanitized = text.Replace("'", "\\'").Replace("\"", "\\\"");
            ExecuteAdb($"shell input text '{sanitized}'");
        }

        public void SendTap(int x, int y)
        {
            ExecuteAdb($"shell input tap {x} {y}");
        }

        public void SendSwipe(int x1, int y1, int x2, int y2, int duration = 300)
        {
            ExecuteAdb($"shell input swipe {x1} {y1} {x2} {y2} {duration}");
        }

        public void SendLongPress(int x, int y, int duration = 500)
        {
            ExecuteAdb($"shell input swipe {x} {y} {x} {y} {duration}");
        }

        public void SendKeyEventByName(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
                return;

            string keyCode = keyName switch
            {
                "HOME" => "3",
                "BACK" => "4",
                "MENU" => "82",
                "POWER" => "26",
                "VOLUME_UP" => "24",
                "VOLUME_DOWN" => "25",
                "ENTER" => "66",
                "DEL" => "67",
                "SPACE" => "62",
                "ESC" => "111",
                _ => ""
            };

            if (!string.IsNullOrEmpty(keyCode))
            {
                ExecuteAdb($"shell input keyevent {keyCode}");
            }
        }

        private void ExecuteAdb(string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = adbExePath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit(1000);
                    }
                }
            }
            catch
            {
            }
        }
    }
}