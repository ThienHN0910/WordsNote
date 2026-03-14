using Application.Facades;
using Application.Handlers;
using Application.Helpers;
using Application.IRepositories.AS;
using Application.IRepositories.CMS;
using Application.IRepositories.DMS;
using Application.IServices.AS;
using Application.IServices.CMS;
using Application.IServices.DMS;
using Application.Services.AS;
using Application.Services.CMS;
using Application.Services.DMS;
using Application.Storage;
using Infrastructure.Repositories.AS;
using Infrastructure.Repositories.CMS;
using Infrastructure.Repositories.DMS;
using Infrastructure.Services;

namespace FeatureFusion.Extensions
{
    public static class ServiceExtensions
    {
        public static object AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Auth
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAuthRepository, AuthRepository>();

            // File & Document
            services.AddScoped<FileServices>();
            services.AddScoped<IDocRepo, DocRepo>();
            services.AddScoped<IDocService, DocService>();
            services.AddScoped<IFileUploadService, FileUploadService>();

            // Convert
            services.AddScoped<ConvertHandler>();
            services.AddScoped<IVersionRepo, VersionRepo>();
            services.AddScoped<IVersionService, VersionService>();

            // Tag
            services.AddScoped<ITagRepo, TagRepo>();
            services.AddScoped<ITagService, TagService>();

            // Annotation
            services.AddScoped<IAnnotationRepo, AnnotationRepo>();
            services.AddScoped<IAnnotationService, AnnotationService>();

            // Comment
            services.AddScoped<ICommentRepo, CommentRepo>();
            //services.AddScoped<ICommentService, CommentService>();

            //Post
            services.AddScoped<IPostRepo, PostRepo>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<PostFacade>();

            // Current User
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // User
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepo, UserRepo>();

            // Storage
            services.AddScoped<IFileStorageService, SupabaseS3StorageService>();

            services.AddSingleton<JwtTokenGenerator>();
            return services;
        }
    }
}
