using EmailServiceAPI.Models;
using FluentValidation;

namespace EmailServiceAPI.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email address is required")
                .EmailAddress()
                .WithMessage("Please provide a valid email address")
                .MaximumLength(320)
                .WithMessage("Email address is too long");
        }
    }
}