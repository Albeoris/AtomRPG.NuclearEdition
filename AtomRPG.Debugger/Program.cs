using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Memoria.Debugger;

namespace AtomRPG.Debugger
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Game directory
                String directoyPath = args[0];

                String executablePath = directoyPath + "\\AtomRPG_x64.exe";
                String unityPath = directoyPath + "\\Unity.exe";

                if (!File.Exists(unityPath))
                {
                    File.Copy(executablePath, unityPath);
                    File.SetLastWriteTimeUtc(unityPath, File.GetLastWriteTimeUtc(executablePath));
                }
                else
                {
                    FileInfo fi1 = new FileInfo(executablePath);
                    FileInfo fi2 = new FileInfo(unityPath);
                    if (fi1.Length != fi2.Length || fi1.LastWriteTimeUtc != fi2.LastWriteTimeUtc)
                    {
                        File.Copy(executablePath, unityPath, true);
                        File.SetLastWriteTimeUtc(unityPath, fi1.LastWriteTimeUtc);
                    }
                }

                executablePath = unityPath;

                String atomRpgDataPath = Path.GetFullPath(directoyPath + "\\AtomRPG_x64_Data");
                String unityDataPath = Path.GetFullPath(directoyPath + "\\Unity_Data");

                if (!Directory.Exists(unityDataPath))
                {
                    JunctionPoint.Create(unityDataPath, atomRpgDataPath, true);
                }
                else
                {
                    try
                    {
                        foreach (String item in Directory.EnumerateFileSystemEntries(unityDataPath))
                            break;
                    }
                    catch
                    {
                        JunctionPoint.Delete(unityDataPath);
                        JunctionPoint.Create(unityDataPath, atomRpgDataPath, true);
                    }
                }

                String arguments = String.Join(" ", args.Skip(1).Select(a => '"' + a + '"'));

                ProcessStartInfo gameStartInfo = new ProcessStartInfo(executablePath, arguments) {UseShellExecute = false, WorkingDirectory = directoyPath};
                gameStartInfo.EnvironmentVariables["UNITY_GIVE_CHANCE_TO_ATTACH_DEBUGGER"] = "1";

                Process gameProcess = new Process {StartInfo = gameStartInfo};
                gameProcess.Start();


                Byte[] unicodeDllPath = PrepareDllPath();
                TimeSpan timeout = TimeSpan.FromSeconds(10);

                CancellationTokenSource cts = new CancellationTokenSource();
                Console.CancelKeyPress += (o, s) =>
                {
                    Console.WriteLine();
                    Console.WriteLine("Stopping...");
                    cts.Cancel();
                };

                Task task = Task.Factory.StartNew(() => MainLoop(unicodeDllPath, cts, timeout), cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

                Console.WriteLine("Waiting for an debug invitation.");
                Console.WriteLine("Type 'help' to show an documenantion or press Ctrl+C to exit.");

                while (!(cts.IsCancellationRequested || task.IsCompleted))
                {
                    Task<String> readLine = Task.Factory.StartNew(() => Console.ReadLine()?.ToLower(), cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
                    readLine.Wait(cts.Token);

                    switch (readLine.Result)
                    {
                        case "help":
                            Console.WriteLine();
                            Console.WriteLine("help\t\t This message.");
                            Console.WriteLine("stop\t\t Stop waiting and close the application.");
                            break;
                        case "stop":
                            Console.WriteLine();
                            Console.WriteLine("Stopping...");
                            cts.Cancel();
                            task.Wait(cts.Token);
                            break;
                        default:
                            Console.WriteLine();
                            Console.WriteLine("Unrecognized command.");
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Unexpected error has occurred.");
                Console.WriteLine(ex);
                Console.WriteLine();
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
            }
        }

        private static readonly String[] InviteMessages =
        {
            "You can attach a native debugger now if you want",
            "You can attach a managed debugger now if you want",
        };

        private static void MainLoop(Byte[] unicodeDllPath, CancellationTokenSource cts, TimeSpan timeout)
        {
            while (true)
            {
                cts.Token.ThrowIfCancellationRequested();

                try
                {
                    KeepAlive(cts, timeout);
                    WindowsObject window = WindowsObject.Wait("Debug", InviteMessages, cts.Token, out Int32 messageIndex);

                    if (messageIndex != 0)
                    {
                        window.Close();
                        KeepAlive(cts, timeout);
                        return;
                    }

                    Int32 processId = window.GetProcessId();

                    Console.WriteLine();
                    Console.WriteLine($"A new debuggable process [PID: {processId}] was found. Trying to inject DLL...");

                    using (SafeProcessHandle processHandle = new SafeProcessHandle(processId, ProcessAccessFlags.All, false))
                    using (SafeVirtualMemoryHandle memoryHandle = processHandle.Allocate(unicodeDllPath.Length, AllocationType.Commit, MemoryProtection.ReadWrite))
                    {
                        memoryHandle.Write(unicodeDllPath);

                        // Uncomment to debug
                        // System.Diagnostics.Debugger.Launch();
                        // KeepAlive(cts, TimeSpan.FromMinutes(10));

                        IntPtr loadLibraryAddress = GetLoadLibraryAddress();
                        using (SafeRemoteThread thread = processHandle.CreateThread(loadLibraryAddress, memoryHandle))
                        {
                            thread.Join();
                            window.Close();
                        }
                    }

                    KeepAlive(cts, timeout);
                    Console.WriteLine($"DLL was successfully injected to the process with PID: {processId}.");
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("Faield to inject DLL.");
                    Console.WriteLine(ex);

                    cts.Token.ThrowIfCancellationRequested();
                    Console.WriteLine("Waiting 20 seconds to try again...");
                    Console.WriteLine("Press Ctrl+C to exit...");
                    Thread.Sleep(20 * 1000);
                }
            }
        }

        private static void KeepAlive(CancellationTokenSource cts, TimeSpan timeout)
        {
            if (timeout != TimeSpan.MaxValue)
                cts.CancelAfter(timeout);
        }

        private static Byte[] PrepareDllPath()
        {
            String dllPath = Path.GetFullPath("AtomRPG.DebuggerInjection.dll");
            if (!File.Exists(dllPath))
                throw new FileNotFoundException("DLL not found: " + dllPath, dllPath);

            return Encoding.Unicode.GetBytes(dllPath);
        }

        private static IntPtr GetLoadLibraryAddress()
        {
            IntPtr kernelHandle = Kernel32.GetModuleHandle("kernel32.dll");
            if (kernelHandle == IntPtr.Zero)
                throw new Win32Exception();

            IntPtr loadLibraryAddress = Kernel32.GetProcAddress(kernelHandle, "LoadLibraryW");
            if (loadLibraryAddress == IntPtr.Zero)
                throw new Win32Exception();

            return loadLibraryAddress;
        }
    }
}