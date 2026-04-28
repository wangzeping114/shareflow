using Mapster;
using MapsterMapper;
using ShareFlow.Application.Contracts;
using ShareFlow.Application.Contracts.DTOs;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Application.Contracts.Interfaces;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Tests;

/// <summary>
/// ContractService 单元测试。
/// 遵循命名规范：MethodName_StateUnderTest_ExpectedBehavior
/// </summary>
public class ContractServiceTests
{
    // ──────────────────────── CreateAsync ────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsNewContractId()
    {
        var (project, slot, investor) = MakeProjectSlotInvestor();
        var projectRepo = new FakeVideoProjectRepository(project);
        var userRepo = new FakeUserRepository(investor);
        var contractRepo = new FakeContractRepository();
        var service = CreateService(contractRepo, projectRepo, userRepo, RegionMode.Domestic);

        var id = await service.CreateAsync(new CreateContractRequest
        {
            ProjectId = project.Id,
            SlotId = slot.Id,
            InvestorUserId = investor.Id,
        }, Guid.NewGuid());

        Assert.NotEqual(Guid.Empty, id);
        Assert.Single(contractRepo.Contracts);
        Assert.Equal(ContractStatus.Draft, contractRepo.Contracts[0].Status);
    }

    [Fact]
    public async Task CreateAsync_OverseasRegion_UsesOverseasEnglishTemplate()
    {
        var (project, slot, investor) = MakeProjectSlotInvestor();
        var contractRepo = new FakeContractRepository();
        var service = CreateService(contractRepo,
            new FakeVideoProjectRepository(project),
            new FakeUserRepository(investor),
            RegionMode.Overseas);

        await service.CreateAsync(new CreateContractRequest
        {
            ProjectId = project.Id,
            SlotId = slot.Id,
            InvestorUserId = investor.Id,
        }, Guid.NewGuid());

        Assert.Equal("OverseasEnglish", contractRepo.Contracts[0].TemplateType);
    }

    [Fact]
    public async Task CreateAsync_DomesticRegion_UsesDomesticChineseTemplate()
    {
        var (project, slot, investor) = MakeProjectSlotInvestor();
        var contractRepo = new FakeContractRepository();
        var service = CreateService(contractRepo,
            new FakeVideoProjectRepository(project),
            new FakeUserRepository(investor),
            RegionMode.Domestic);

        await service.CreateAsync(new CreateContractRequest
        {
            ProjectId = project.Id,
            SlotId = slot.Id,
            InvestorUserId = investor.Id,
        }, Guid.NewGuid());

        Assert.Equal("DomesticChinese", contractRepo.Contracts[0].TemplateType);
    }

    [Fact]
    public async Task CreateAsync_ProjectNotFound_ThrowsBusinessException()
    {
        var service = CreateService(
            new FakeContractRepository(),
            new FakeVideoProjectRepository(),
            new FakeUserRepository());

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateAsync(new CreateContractRequest
            {
                ProjectId = Guid.NewGuid(),
                SlotId = Guid.NewGuid(),
                InvestorUserId = Guid.NewGuid(),
            }, Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_SlotNotFound_ThrowsBusinessException()
    {
        var (project, _, investor) = MakeProjectSlotInvestor();
        var service = CreateService(
            new FakeContractRepository(),
            new FakeVideoProjectRepository(project),
            new FakeUserRepository(investor));

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateAsync(new CreateContractRequest
            {
                ProjectId = project.Id,
                SlotId = Guid.NewGuid(), // 不存在的槽位
                InvestorUserId = investor.Id,
            }, Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_InvestorNotFound_ThrowsBusinessException()
    {
        var (project, slot, _) = MakeProjectSlotInvestor();
        var service = CreateService(
            new FakeContractRepository(),
            new FakeVideoProjectRepository(project),
            new FakeUserRepository()); // 空用户库

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateAsync(new CreateContractRequest
            {
                ProjectId = project.Id,
                SlotId = slot.Id,
                InvestorUserId = Guid.NewGuid(),
            }, Guid.NewGuid()));
    }

    // ──────────────────────── GenerateSignLinkAsync ────────────────────────

    [Fact]
    public async Task GenerateSignLinkAsync_ExistingContract_ReturnsSignUrlAndExpiry()
    {
        var contract = Contract.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "DomesticChinese");
        var contractRepo = new FakeContractRepository(contract);
        var service = CreateService(contractRepo);

        var result = await service.GenerateSignLinkAsync(contract.Id);

        Assert.Contains("/esign/", result.SignUrl);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        Assert.Equal(ContractStatus.Sent, contractRepo.Contracts[0].Status);
    }

    [Fact]
    public async Task GenerateSignLinkAsync_ContractNotFound_ThrowsBusinessException()
    {
        var service = CreateService(new FakeContractRepository());

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.GenerateSignLinkAsync(Guid.NewGuid()));
    }

    // ──────────────────────── GetByIdAsync ────────────────────────

    [Fact]
    public async Task GetByIdAsync_ContractNotFound_ThrowsBusinessException()
    {
        var service = CreateService(new FakeContractRepository());

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.GetByIdAsync(Guid.NewGuid()));
    }

    // ──────────────────────── Factory / Setup Helpers ────────────────────────

    private static ContractService CreateService(
        FakeContractRepository? contractRepo = null,
        FakeVideoProjectRepository? projectRepo = null,
        FakeUserRepository? userRepo = null,
        RegionMode regionMode = RegionMode.Domestic)
    {
        var config = new TypeAdapterConfig();
        new ShareFlow.Application.Contracts.ContractMappingConfig().Register(config);
        return new ContractService(
            contractRepo ?? new FakeContractRepository(),
            projectRepo ?? new FakeVideoProjectRepository(),
            userRepo ?? new FakeUserRepository(),
            new FakeRegionContext(regionMode),
            new FakeContractTemplateService(),
            new Mapper(config));
    }

    private static (VideoProject project, ProjectSlot slot, User investor) MakeProjectSlotInvestor()
    {
        var project = VideoProject.Create(
            "测试项目", string.Empty, "TikTok", ProjectSlotMode.Fixed, 10, Guid.NewGuid(), 100_000m);

        var slot = ProjectSlot.Create(project.Id, 100m); // 100‰

        // 通过反射将 slot 插入 project.Slots（绕过 EF 导航属性的 private setter）
        var slotsField = typeof(VideoProject)
            .GetField("_slots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (slotsField?.GetValue(project) is List<ProjectSlot> slotList)
            slotList.Add(slot);

        var investor = User.Create(
            "investor1", "investor@test.com", "Pass@123456", "Investor One", UserRole.Client);

        return (project, slot, investor);
    }

    // ──────────────────────── Fake Implementations ────────────────────────

    private sealed class FakeContractRepository(params Contract[] contracts) : IContractRepository
    {
        public List<Contract> Contracts { get; } = contracts.ToList();

        public Task<Contract?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Contracts.FirstOrDefault(c => c.Id == id));

        public Task<Contract?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Contracts.FirstOrDefault(c => c.Id == id));

        public Task<Contract?> GetBySignTokenAsync(string token, CancellationToken ct = default)
            => Task.FromResult(Contracts.FirstOrDefault(c => c.SignToken == token));

        public Task<(IReadOnlyList<Contract> Items, int Total)> GetPagedAsync(
            Guid? projectId, ContractStatus? status, string? projectTitle, int page, int pageSize, CancellationToken ct = default)
        {
            IReadOnlyList<Contract> items = Contracts;
            return Task.FromResult((items, items.Count));
        }

        public Task AddAsync(Contract contract, CancellationToken ct = default)
        {
            Contracts.Add(contract);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Contract contract, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeVideoProjectRepository(params VideoProject[] projects) : IVideoProjectRepository
    {
        private readonly List<VideoProject> _projects = projects.ToList();

        public Task<VideoProject?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_projects.FirstOrDefault(p => p.Id == id));

        public Task<VideoProject?> GetByIdWithSlotsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_projects.FirstOrDefault(p => p.Id == id));

        public Task<(IReadOnlyList<VideoProject> Items, int Total)> GetPagedAsync(
            string? platformFilter, ProjectStatus? statusFilter, int page, int pageSize, CancellationToken ct = default)
        {
            IReadOnlyList<VideoProject> items = _projects;
            return Task.FromResult((items, items.Count));
        }

        public Task AddAsync(VideoProject project, CancellationToken ct = default)
        {
            _projects.Add(project);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(VideoProject project, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeUserRepository(params User[] users) : IUserRepository
    {
        private readonly List<User> _users = users.ToList();

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

        public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
            => Task.FromResult(_users.FirstOrDefault(u => u.Username == username));

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult(_users.FirstOrDefault(u => u.Email == email));

        public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
            => Task.FromResult(_users.Any(u => u.Username == username));

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult(_users.Any(u => u.Email == email));

        public Task AddAsync(User user, CancellationToken ct = default)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User user, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<string>>([]);

        public Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<User>>(_users.Where(u => u.Role == role).ToList());
    }

    private sealed class FakeRegionContext(RegionMode mode) : IRegionContext
    {
        public RegionMode Mode => mode;
        public string DefaultCurrency => mode == RegionMode.Overseas ? "USD" : "CNY";
        public string DefaultLocale => mode == RegionMode.Overseas ? "en-US" : "zh-CN";
        public IReadOnlyList<string> EnabledPlatforms => [];
        public bool IsYouTubeOAuthEnabled => false;
    }

    private sealed class FakeContractTemplateService : IContractTemplateService
    {
        public string Render(string templateType, ContractTemplateData data)
            => $"[TEMPLATE:{templateType}] Project={data.ProjectTitle} Investor={data.InvestorName}";

        public string GetRaw(string templateType) => $"[RAW:{templateType}]";
    }
}
