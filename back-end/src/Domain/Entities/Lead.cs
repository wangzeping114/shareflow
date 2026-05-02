using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Entities;

public class Lead : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;

    /// <summary>微信 / WhatsApp / 电话等联系方式</summary>
    public string ContactInfo { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public LeadStatus Status { get; private set; }

    /// <summary>负责跟进的销售员 UserId</summary>
    public Guid SalesOwnerId { get; private set; }

    /// <summary>关联的投资人客户 UserId（Client 角色）</summary>
    public Guid? ClientUserId { get; private set; }

    /// <summary>销售侧保存的客户初始密码（明文，仅销售可见）</summary>
    public string? ClientInitialPassword { get; private set; }

    public string? Notes { get; private set; }

    // 导航属性
    public User? SalesOwner { get; private set; }
    public User? ClientUser { get; private set; }

    private Lead() { }

    public static Lead Create(
        string name,
        string contactInfo,
        string? email,
        Guid salesOwnerId,
        string? notes = null)
    {
        return new Lead
        {
            Id = Guid.NewGuid(),
            Name = name,
            ContactInfo = contactInfo,
            Email = email,
            Status = LeadStatus.New,
            SalesOwnerId = salesOwnerId,
            Notes = notes
        };
    }

    public void Update(string name, string contactInfo, string? email, string? notes)
    {
        Name = name;
        ContactInfo = contactInfo;
        Email = email;
        Notes = notes;
    }

    public void ChangeStatus(LeadStatus newStatus)
    {
        Status = newStatus;
    }

    public void SetClientUserId(Guid clientUserId)
    {
        ClientUserId = clientUserId;
    }

    public void SetClientInitialPassword(string password)
    {
        ClientInitialPassword = password;
    }
}
