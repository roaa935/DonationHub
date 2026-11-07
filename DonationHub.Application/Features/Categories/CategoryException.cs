namespace DonationHub.Application.Features.Categories;

public class CategoryException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    public CategoryException(string code, int statusCode)
        : base(code)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
