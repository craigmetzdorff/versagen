using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Versagen.IO;

namespace Versagen.ASPNET.SignalR
{
    public static class VersaServiceConfigExtensions
    {
        public static VersagenServiceConfig AddSignalRWriters<THub>(this VersagenServiceConfig config, string JavaScriptReceiveFunction = "ReceiveVersagenMessage") where THub:Hub
        {
            config.VersagenServices.AddScoped<SignalRWriterDirectory<THub>>()
                .AddTransient<IVersaWriterDirectory>(p =>
                    p.GetRequiredService<SignalRWriterDirectory<THub>>())
                .AddSingleton<SignalRWriterDirectoryBackingStore>()
                .AddSingleton(c => new VersaSignalRConfig {NameOfWriterFunction = JavaScriptReceiveFunction});
            return config;
        }
    }
}
