using DTO;
using Swashbuckle.AspNetCore.Filters;

namespace WebLevel.Example;

public class RequestResetPasswordExample: IExamplesProvider<string>
{
    public string GetExamples() => "trokhin99@gmail.com";
}