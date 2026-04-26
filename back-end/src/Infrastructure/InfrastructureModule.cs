using Autofac;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Infrastructure;

public class InfrastructureModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<RegionContext>()
            .As<IRegionContext>()
            .SingleInstance();

        // Repositories 自动注册（后续 Epic 持续补充）
        builder.RegisterAssemblyTypes(ThisAssembly)
            .Where(t => t.Name.EndsWith("Repository"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    }
}
