using FluentValidation;
using ShareFlow.Application.Auth.DTOs;

namespace ShareFlow.Application.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(64);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}