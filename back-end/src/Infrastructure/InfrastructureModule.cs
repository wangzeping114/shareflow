using Autofac;
using Microsoft.Extensions.Configuration;
using ShareFlow.Application.Revenue.Interfaces;
using ShareFlow.Domain.Interfaces;
using ShareFlow.Infrastructure.Revenue;
using ShareFlow.Infrastructure.Services;
using StackExchange.Redis;

namespace ShareFlow.Infrastructure;

public class InfrastructureModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<RegionContext>()
            .As<IRegionContext>()
            .SingleInstance();

        builder.Register(context =>
            ConnectionMultiplexer.Connect(
                context.Resolve<IConfiguration>().GetSection("Redis")["ConnectionString"] ?? "localhost:6379"))
            .As<IConnectionMultiplexer>()
            .SingleInstance();

        // Repositories 自动注册（后续 Epic 持续补充）
        builder.RegisterAssemblyTypes(ThisAssembly)
            .Where(t => t.Name.EndsWith("Repository"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(ThisAssembly)
            .Where(t => t.Name.EndsWith("Service"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        // Revenue 策略工厂 & AI Agent 显式注册
        builder.RegisterType<RevenueHandlerFactory>()
            .As<IPlatformRevenueHandlerFactory>()
            .InstancePerLifetimeScope();

        builder.RegisterType<ClaudeAiRevenueAgent>()
            .As<IAiRevenueSkillAgent>()
            .InstancePerLifetimeScope();
    }
}
