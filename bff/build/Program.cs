using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using static Bullseye.Targets;
using static SimpleExec.Command;

namespace build
{
    internal static class Program
    {
        private const string PackOutput = "./artifacts";
        private const string EnvVarMissing = " environment variable is missing. Aborting.";

        private static class Targets
        {
            public const string RestoreTools = "restore-tools";
            public const string CleanBuildOutput = "clean-build-output";
            public const string CleanPackOutput = "clean-pack-output";
            public const string Build = "build";
            public const string Test = "test";
            public const string Pack = "pack";
            public const string SignPackage = "sign-package";
        }

        internal static async Task Main(string[] args)
        {
            Target(Targets.RestoreTools, () =>
            {
                Run("dotnet", "tool restore");
            });

            Target(Targets.CleanBuildOutput, () =>
            {
                Run("dotnet", "clean -c Release -v m --nologo");
            });

            Target(Targets.Build, DependsOn(Targets.CleanBuildOutput), () =>
            {
                Run("dotnet", "build -c Release --nologo");
            });

            Target(Targets.Test, DependsOn(Targets.Build), () =>
            {
                // Only running the tests on linux on the build agents because trusting the SSL Cert doesn't work there. 
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Run("dotnet", "dev-certs https --trust");
                    Run("dotnet", "test -c Release --no-build --nologo");
                }
                else
                {
                    Console.WriteLine("Skipping tests on windows and mac-os");
                }
            });

            Target(Targets.CleanPackOutput, () =>
            {
                if (Directory.Exists(PackOutput))
                {
                    Directory.Delete(PackOutput, true);
                }
            });

            Target(Targets.Pack, DependsOn(Targets.Build, Targets.CleanPackOutput), () =>
            {
                Run("dotnet", $"pack ./src/Duende.Bff/Duende.Bff.csproj -c Release -o {Directory.CreateDirectory(PackOutput).FullName} --no-build --nologo");
                Run("dotnet", $"pack ./src/Duende.Bff.EntityFramework/Duende.Bff.EntityFramework.csproj -c Release -o {Directory.CreateDirectory(PackOutput).FullName} --no-build --nologo");
                Run("dotnet", $"pack ./src/Duende.Bff.Yarp/Duende.Bff.Yarp.csproj -c Release -o {Directory.CreateDirectory(PackOutput).FullName} --no-build --nologo");
                Run("dotnet", $"pack ./src/Duende.Bff.Blazor/Duende.Bff.Blazor.csproj -c Release -o {Directory.CreateDirectory(PackOutput).FullName} --no-build --nologo");
                Run("dotnet", $"pack ./src/Duende.Bff.Blazor.Client/Duende.Bff.Blazor.Client.csproj -c Release -o {Directory.CreateDirectory(PackOutput).FullName} --no-build --nologo");
            });

            Target(Targets.SignPackage, DependsOn(Targets.Pack, Targets.RestoreTools), () =>
            {
                SignNuGet();
            });

            Target("default", DependsOn(Targets.Test, Targets.Pack));

            Target("sign", DependsOn(Targets.Test, Targets.SignPackage));

            await RunTargetsAndExitAsync(args, ex => ex is SimpleExec.ExitCodeException || ex.Message.EndsWith(EnvVarMissing));
        }

        private static void SignNuGet()
        {
            var signClientSecret = Environment.GetEnvironmentVariable("SignClientSecret");

            if (string.IsNullOrWhiteSpace(signClientSecret))
            {
                throw new Exception($"SignClientSecret{EnvVarMissing}");
            }

            foreach (var file in Directory.GetFiles(PackOutput, "*.nupkg", SearchOption.AllDirectories))
            {
                Console.WriteLine($"  Signing {file}");

                Run("dotnet",
                        "NuGetKeyVaultSignTool " +
                        $"sign {file} " +
                        "--file-digest sha256 " +
                        "--timestamp-rfc3161 http://timestamp.digicert.com " +
                        "--azure-key-vault-url https://duendecodesigninghsm.vault.azure.net/ " +
                        "--azure-key-vault-client-id 18e3de68-2556-4345-8076-a46fad79e474 " +
                        "--azure-key-vault-tenant-id ed3089f0-5401-4758-90eb-066124e2d907 " +
                        $"--azure-key-vault-client-secret {signClientSecret} " +
                        "--azure-key-vault-certificate NuGetPackageSigning"
                        ,noEcho: true);
            }
        }
    }
}
