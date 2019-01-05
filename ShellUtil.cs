using System;
using System.Diagnostics;

namespace Integrator
{
    public static class ShellUtil
    {
        /// <summary>
        /// 执行系统命令或脚本文件
        /// </summary>
        /// <param name="shell">系统命令或脚本文件</param>
        /// <param name="args">命令或脚本文件参数</param>
        /// <param name="isShellFile">是否为脚本文件</param>
        /// <returns></returns>
        public static bool ExecShell(string shell, string args = null, bool isShellFile = false)
        {
            var psi = new ProcessStartInfo(shell, args) {RedirectStandardOutput = true};
            try
            {
                var proc = Process.Start(psi);
                if (proc == null)
                {
                    Console.WriteLine($"Sorry,{shell} can not exec.");
                    return false;
                }

                Console.WriteLine("-------------Start read standard output--------------");
                using (var sr = proc.StandardOutput)
                {
                    while (!sr.EndOfStream)
                    {
                        Console.WriteLine(sr.ReadLine());
                    }

                    if (!proc.HasExited)
                    {
                        proc.Kill();
                    }
                }

                Console.WriteLine("---------------Read end------------------");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured,here is the exception message : {ex.Message}");
                if (ex.Message.Contains("Permission denied"))
                {
                    Console.WriteLine(isShellFile
                        ? $"Try to fix this by execute 'sudo chmod +x {shell}'"
                        : $"Try to run your command with 'sudo' prefix");
                }

                return false;
            }
        }
    }
}