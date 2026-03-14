namespace Application.IServices.AS;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
