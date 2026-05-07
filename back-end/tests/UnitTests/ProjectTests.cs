using Mapster;
using MapsterMapper;
using ShareFlow.Application.Common;
using ShareFlow.Application.Project;
using ShareFlow.Application.Project.DTOs;
using ShareFlow.Application.Project.Interfaces;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Tests;

// ═══════════════════════════════════════════════════════════
// VideoProject 领域实体测试
// ═══════════════════════════════════════════════════════════

public class VideoProjectEntityTests
{
    [Fact]
    public void Create_ValidArgs_DefaultsToDraftStatus()
    {
        var project = MakeProject();

        Assert.Equal(ProjectStatus.Draft, project.Status);
        Assert.Equal("Test Project", project.Title);
        Assert.Equal(10, project.TotalSlots);
    }

    [Fact]
    public void Create_EmptyTitle_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            VideoProject.Create("", "desc", "TikTok", ProjectSlotMode.Fixed, 10, Guid.NewGuid()));
    }

    [Fact]
    public void Create_ZeroTotalSlots_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            VideoProject.Create("Title", "desc", "TikTok", ProjectSlotMode.Fixed, 0, Guid.NewGuid()));
    }

    [Fact]
    public void Activate_FromDraft_StatusBecomesActive()
    {
        var project = MakeProject();
        project.Activate();
        Assert.Equal(ProjectStatus.Active, project.Status);
    }

    [Fact]
    public void Pause_AfterActivate_StatusBecomesPaused()
    {
        var project = MakeProject();
        project.Activate();
        project.Pause();
        Assert.Equal(ProjectStatus.Paused, project.Status);
    }

    [Fact]
    public void Close_AnyStatus_StatusBecomesClosed()
    {
        var project = MakeProject();
        project.Activate();
        project.Close();
        Assert.Equal(ProjectStatus.Closed, project.Status);
    }

    [Fact]
    public void AddSlot_WithinCapacity_SlotAddedToCollection()
    {
        var project = MakeProject(totalSlots: 5);
        var slot = project.AddSlot(10m);

        Assert.Single(project.Slots);
        Assert.Equal(SlotStatus.Available, slot.Status);
        Assert.Equal(10m, slot.SharePermille);
    }

    [Fact]
    public void AddSlot_ExceedsCapacity_ThrowsInvalidOperation()
    {
        var project = MakeProject(totalSlots: 2);
        project.AddSlot(5m);
        project.AddSlot(5m);

        Assert.Throws<InvalidOperationException>(() => project.AddSlot(5m));
    }

    [Fact]
    public void FilledSlots_CountsOnlyOccupied()
    {
        var project = MakeProject(totalSlots: 5);
        var slot1 = project.AddSlot(10m);
        var slot2 = project.AddSlot(10m);
        slot1.Reserve(Guid.NewGuid());
        slot1.Confirm();  // Occupied
        // slot2 仍 Available

        Assert.Equal(1, project.FilledSlots);
        Assert.Equal(0, project.ReservedSlots);
        Assert.Equal(4, project.AvailableSlots);
    }

    [Fact]
    public void ReservedSlots_CountsOnlyReserved()
    {
        var project = MakeProject(totalSlots: 5);
        var slot = project.AddSlot(10m);
        slot.Reserve(Guid.NewGuid());

        Assert.Equal(0, project.FilledSlots);
        Assert.Equal(1, project.ReservedSlots);
        Assert.Equal(4, project.AvailableSlots);
    }

    [Fact]
    public void Update_TotalSlotsBelowOccupied_ThrowsInvalidOperation()
    {
        var project = MakeProject(totalSlots: 5);
        var slot = project.AddSlot(10m);
        slot.Reserve(Guid.NewGuid());
        slot.Confirm(); // FilledSlots = 1

        Assert.Throws<InvalidOperationException>(() =>
            project.Update("New Title", "", "TikTok", 0)); // totalSlots=0 < filled=1
    }

    // ── helpers ──
    private static VideoProject MakeProject(int totalSlots = 10) =>
        VideoProject.Create("Test Project", "desc", "TikTok", ProjectSlotMode.Fixed, totalSlots, Guid.NewGuid());
}

// ═══════════════════════════════════════════════════════════
// ProjectSlot 领域实体测试
// ═══════════════════════════════════════════════════════════

public class ProjectSlotEntityTests
{
    [Fact]
    public void Create_ValidArgs_StatusIsAvailable()
    {
        var slot = ProjectSlot.Create(Guid.NewGuid(), 5m);
        Assert.Equal(SlotStatus.Available, slot.Status);
    }

    [Fact]
    public void Create_ZeroSharePermille_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ProjectSlot.Create(Guid.NewGuid(), 0m));
    }

    [Fact]
    public void Reserve_AvailableSlot_StatusBecomesReserved()
    {
        var slot = ProjectSlot.Create(Guid.NewGuid(), 5m);
        var clientId = Guid.NewGuid();
        slot.Reserve(clientId);

        Assert.Equal(SlotStatus.Reserved, slot.Status);
        Assert.Equal(clientId, slot.ClientUserId);
    }

    [Fact]
    public void Reserve_AlreadyReserved_ThrowsInvalidOperation()
    {
        var slot = ProjectSlot.Create(Guid.NewGuid(), 5m);
        slot.Reserve(Guid.NewGuid());
        Assert.Throws<InvalidOperationException>(() => slot.Reserve(Guid.NewGuid()));
    }

    [Fact]
    public void Confirm_ReservedSlot_StatusBecomesOccupied()
    {
        var slot = ProjectSlot.Create(Guid.NewGuid(), 5m);
        slot.Reserve(Guid.NewGuid());
        slot.Confirm();
        Assert.Equal(SlotStatus.Occupied, slot.Status);
    }

    [Fact]
    public void Confirm_NotReserved_ThrowsInvalidOperation()
    {
        var slot = ProjectSlot.Create(Guid.NewGuid(), 5m);
        Assert.Throws<InvalidOperationException>(() => slot.Confirm());
    }

    [Fact]
    public void Release_AnyStatus_ClearsClientAndBecomesAvailable()
    {
        var slot = ProjectSlot.Create(Guid.NewGuid(), 5m);
        slot.Reserve(Guid.NewGuid());
        slot.Release();

        Assert.Equal(SlotStatus.Available, slot.Status);
        Assert.Null(slot.ClientUserId);
    }
}

// ═══════════════════════════════════════════════════════════
// PagedQuery 边界校验测试
// ═══════════════════════════════════════════════════════════

public class PagedQueryTests
{
    private sealed record TestQuery : PagedQuery;

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(3, 3)]
    public void Page_Boundary_ClampsToMinOne(int input, int expected)
    {
        var q = new TestQuery { Page = input };
        Assert.Equal(expected, q.Page);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(201, 200)]
    [InlineData(50, 50)]
    public void PageSize_Boundary_ClampsToRange(int input, int expected)
    {
        var q = new TestQuery { PageSize = input };
        Assert.Equal(expected, q.PageSize);
    }
}

// ═══════════════════════════════════════════════════════════
// ProjectService 应用层测试
// ═══════════════════════════════════════════════════════════

public class ProjectServiceTests
{
    // ── GetByIdAsync ──────────────────────────────────────
    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsBusinessException()
    {
        var (svc, _, _) = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => svc.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsMappedDto()
    {
        var project = MakeProject();
        var (svc, repo, _) = CreateService();
        var projectRepo = (FakeVideoProjectRepository)repo;
        projectRepo.AddWithSlots(project);

        var dto = await svc.GetByIdAsync(project.Id);

        Assert.Equal(project.Title, dto.Title);
        Assert.Equal("Draft", dto.Status);
    }

    // ── CreateAsync ──────────────────────────────────────
    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsNewGuid()
    {
        var (svc, repo, _) = CreateService();
        var projectRepo = (FakeVideoProjectRepository)repo;
        var request = new CreateProjectRequest
        {
            Title = "New Project",
            Description = "desc",
            PlatformName = "TikTok",
            SlotMode = ProjectSlotMode.Fixed,
            TotalSlots = 5,
        };

        var id = await svc.CreateAsync(request, Guid.NewGuid());

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(projectRepo.GetByIdPlain(id));
    }

    // ── ChangeStatusAsync ─────────────────────────────────
    [Fact]
    public async Task ChangeStatusAsync_ActivateFromDraft_StatusBecomesActive()
    {
        var project = MakeProject();
        var (svc, repo, _) = CreateService();
        var projectRepo = (FakeVideoProjectRepository)repo;
        projectRepo.AddPlain(project);

        await svc.ChangeStatusAsync(project.Id,
            new ChangeProjectStatusRequest { NewStatus = ProjectStatus.Active }, Guid.NewGuid());

        Assert.Equal(ProjectStatus.Active, projectRepo.GetByIdPlain(project.Id)!.Status);
    }

    [Fact]
    public async Task ChangeStatusAsync_NotFound_ThrowsBusinessException()
    {
        var (svc, _, _) = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() =>
            svc.ChangeStatusAsync(Guid.NewGuid(),
                new ChangeProjectStatusRequest { NewStatus = ProjectStatus.Active }, Guid.NewGuid()));
    }

    [Fact]
    public async Task ChangeStatusAsync_UnsupportedStatus_ThrowsBusinessException()
    {
        var project = MakeProject();
        var (svc, repo, _) = CreateService();
        var projectRepo = (FakeVideoProjectRepository)repo;
        projectRepo.AddPlain(project);

        await Assert.ThrowsAsync<BusinessException>(() =>
            svc.ChangeStatusAsync(project.Id,
                new ChangeProjectStatusRequest { NewStatus = ProjectStatus.Draft }, Guid.NewGuid()));
    }

    // ── AddSlotAsync ──────────────────────────────────────
    [Fact]
    public async Task AddSlotAsync_ValidProject_ReturnsSlotDto()
    {
        var project = MakeProject(totalSlots: 5);
        var (svc, repo, _) = CreateService();
        var projectRepo = (FakeVideoProjectRepository)repo;
        projectRepo.AddWithSlots(project);

        var slotDto = await svc.AddSlotAsync(project.Id, new AddSlotRequest { SharePct = 10m });

        Assert.Equal(10m, slotDto.SharePct);
        Assert.Equal("Available", slotDto.Status);
    }

    [Fact]
    public async Task AddSlotAsync_ProjectAtCapacity_ThrowsInvalidOperation()
    {
        var project = MakeProject(totalSlots: 1);
        project.AddSlot(5m); // 已满
        var (svc, repo, _) = CreateService();
        var projectRepo = (FakeVideoProjectRepository)repo;
        projectRepo.AddWithSlots(project);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.AddSlotAsync(project.Id, new AddSlotRequest { SharePct = 5m }));
    }

    [Fact]
    public async Task AddSlotAsync_NotFound_ThrowsBusinessException()
    {
        var (svc, _, _) = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() =>
            svc.AddSlotAsync(Guid.NewGuid(), new AddSlotRequest { SharePct = 5m }));
    }

    // ── GetListAsync ──────────────────────────────────────
    [Fact]
    public async Task GetListAsync_ReturnsPagedResult()
    {
        var (svc, repo, _) = CreateService();
        var projectRepo = (FakeVideoProjectRepository)repo;
        projectRepo.AddPlain(MakeProject("Alpha"));
        projectRepo.AddPlain(MakeProject("Beta"));

        var result = await svc.GetListAsync(new ProjectQueryRequest { Page = 1, PageSize = 10 });

        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
    }

    // ── helpers ──────────────────────────────────────────
    private static VideoProject MakeProject(string title = "Test", int totalSlots = 10) =>
        VideoProject.Create(title, "desc", "TikTok", ProjectSlotMode.Fixed, totalSlots, Guid.NewGuid());

    private static (IProjectService Svc, IVideoProjectRepository Repo, IProjectSlotRepository SlotRepo)
        CreateService()
    {
        var repo = new FakeVideoProjectRepository();
        var slotRepo = new FakeProjectSlotRepository();
        var config = new TypeAdapterConfig();
        new ProjectMappingConfig().Register(config);
        var mapper = new Mapper(config);
        var svc = new ProjectService(repo, slotRepo, mapper);
        return (svc, repo, slotRepo);
    }
}

// ═══════════════════════════════════════════════════════════
// Fake Repositories
// ═══════════════════════════════════════════════════════════

internal sealed class FakeVideoProjectRepository : IVideoProjectRepository
{
    private readonly Dictionary<Guid, VideoProject> _store = new();
    // 带已加载 Slots 供 GetByIdWithSlotsAsync 返回
    private readonly Dictionary<Guid, VideoProject> _withSlots = new();

    public void AddPlain(VideoProject p) => _store[p.Id] = p;
    public void AddWithSlots(VideoProject p) { _store[p.Id] = p; _withSlots[p.Id] = p; }
    public VideoProject? GetByIdPlain(Guid id) => _store.GetValueOrDefault(id);

    public Task<VideoProject?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_store.GetValueOrDefault(id));

    public Task<VideoProject?> GetByIdWithSlotsAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_withSlots.TryGetValue(id, out var p) ? p : _store.GetValueOrDefault(id));

    public Task<(IReadOnlyList<VideoProject> Items, int Total)> GetPagedAsync(
        string? titleFilter, ProjectStatus? statusFilter, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _store.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(titleFilter))
            q = q.Where(x => x.Title.Contains(titleFilter));
        if (statusFilter.HasValue)
            q = q.Where(x => x.Status == statusFilter.Value);

        var list = q.ToList();
        IReadOnlyList<VideoProject> items = list
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult((items, list.Count));
    }

    public Task AddAsync(VideoProject p, CancellationToken ct = default)
    {
        _store[p.Id] = p;
        _withSlots[p.Id] = p;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(VideoProject p, CancellationToken ct = default)
    {
        _store[p.Id] = p;
        return Task.CompletedTask;
    }
}

internal sealed class FakeProjectSlotRepository : IProjectSlotRepository
{
    private readonly List<ProjectSlot> _slots = [];

    public Task<ProjectSlot?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_slots.FirstOrDefault(s => s.Id == id));

    public Task<IReadOnlyList<ProjectSlot>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ProjectSlot>>(_slots.Where(s => s.ProjectId == projectId).ToList());

    public Task AddAsync(ProjectSlot slot, CancellationToken ct = default)
    {
        _slots.Add(slot);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ProjectSlot slot, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<IReadOnlyList<ProjectSlot>> GetByIdsAsync(IEnumerable<Guid> slotIds, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ProjectSlot>>(_slots.Where(s => slotIds.Contains(s.Id)).ToList());

    public Task UpdateRangeAsync(IEnumerable<ProjectSlot> slots, CancellationToken ct = default)
        => Task.CompletedTask;
}
