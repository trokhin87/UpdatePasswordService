using DTO;
using Swashbuckle.AspNetCore.Filters;

namespace WebLevel.Example;

public class ResetPasswordExample : IExamplesProvider<PasswordResetDto>
{
    public PasswordResetDto GetExamples()
    {
        return new PasswordResetDto
        {
            Token = "abc123xyz",
            NewPassword = "NewSecurePassword123!"
        };
    }
}