using FluentValidation;
using ShareFlow.Application.Project.DTOs;

namespace ShareFlow.Application.Project.Validators;

public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200).WithMessage("项目名称不能为空，且不超过 200 字。");
        RuleFor(x => x.PlatformName).NotEmpty().MaximumLength(100).WithMessage("平台名称不能为空。");
        RuleFor(x => x.TotalSlots).GreaterThan(0).WithMessage("总份数必须大于 0。");
    }
}

public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PlatformName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TotalSlots).GreaterThan(0);
    }
}

public class AddSlotRequestValidator : AbstractValidator<AddSlotRequest>
{
    public AddSlotRequestValidator()
    {
        RuleFor(x => x.SharePct)
            .GreaterThan(0).WithMessage("持股比例必须大于 0。")
            .LessThanOrEqualTo(100).WithMessage("持股比例不能超过 100%。");
    }
}
