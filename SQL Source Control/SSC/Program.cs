using Autofac;
using CommandLine;
using SSC.Abstractions;
using SSC.Providers;
using SSC.Providers.JSONFile;
using SSC.Providers.Postgres;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SSC
{
    public class Program
    {
        private static IContainer Container { get; set; }

        public class BaseOptions
        {
            [Option("source-db", Group = "sources")]
            public string SourceDB { get; set; }

            [Option("source-file", Group = "sources")]
            public string SourceFile { get; set; }

            [Option("target-db", Group = "targets")]
            public string TargetDB { get; set; }

            [Option("target-file", Group = "targets")]
            public string TargetFile { get; set; }
        }

        [Verb("apply")]
        public class ApplyOptions : BaseOptions
        {
            
        }

        [Verb("diff")]
        public class DiffOptions : BaseOptions
        {
            
        }

        static async Task Main(string[] args)
        {
            var parsedArguments = Parser.Default.ParseArguments<
                ApplyOptions,
                DiffOptions
                >(args);

            await parsedArguments.WithParsedAsync<ApplyOptions>(async opts =>
                {
                    var builder = new ContainerBuilder();

                    if (opts.SourceFile != null)
                    {
                        builder.RegisterInstance<IDBConfigProvider>(new FileProvider(opts.SourceFile));
                    }

                    if (opts.TargetFile != null)
                    {
                        builder.RegisterInstance<IDBConfigTarget>(new FileTarget(opts.TargetFile));
                    }

                    if (opts.SourceDB != null)
                    {
                        builder.RegisterInstance<IDBConfigProvider>(new PostgresDatabaseProvider(opts.SourceDB));
                    }

                    if (opts.TargetDB != null)
                    {
                        builder.RegisterInstance<IDBConfigTarget>(new FileTarget(opts.TargetDB));
                    }

                    Program.Container = builder.Build();

                    using (var scope = Program.Container.BeginLifetimeScope())
                    {
                        var provider = scope.Resolve<IDBConfigProvider>();

                        var config = await provider.GetDatabaseConfig();

                        var target = scope.Resolve<IDBConfigTarget>();

                        await target.TryApplyChanges(config);
                    }

                });

            await parsedArguments.WithParsedAsync<DiffOptions>(async opts =>
                {
                    var builder = new ContainerBuilder();

                    if (opts.SourceFile != null)
                    {
                        builder.RegisterInstance<IDBConfigProvider>(new FileProvider(opts.SourceFile));
                    }

                    if (opts.TargetFile != null)
                    {
                        builder.RegisterInstance<IDBConfigTarget>(new FileTarget(opts.TargetFile));
                    }

                    if (opts.SourceDB != null)
                    {
                        builder.RegisterInstance<IDBConfigProvider>(new PostgresDatabaseProvider(opts.SourceDB));
                    }

                    if (opts.TargetDB != null)
                    {
                        builder.RegisterInstance<IDBConfigTarget>(new PostgresDatabaseTargetImpl(opts.TargetDB));
                    }

                    builder.RegisterInstance<IDBConfigDiffer>(new PostgresDatabaseDiffProvider());

                    Program.Container = builder.Build();

                    using (var scope = Program.Container.BeginLifetimeScope())
                    {
                        var provider = scope.Resolve<IDBConfigProvider>();
                        var sourceConfig = await provider.GetDatabaseConfig();

                        var target = scope.Resolve<IDBConfigTarget>();
                        var targetConfig = await target.GetDatabaseConfig();

                        var differ = scope.Resolve<IDBConfigDiffer>();
                        using (var stream = await differ.DiffConfigs(sourceConfig, targetConfig))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            await stream.CopyToAsync(Console.OpenStandardOutput());
                        }
                    }
                });
            ;
        }
    }
}
