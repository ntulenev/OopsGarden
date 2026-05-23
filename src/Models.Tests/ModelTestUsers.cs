namespace Models.Tests;

internal static class ModelTestUsers
{
    public static AppUser Create(DateTimeOffset createdAt = default) =>
        AppUser.Create(
            UserEmail.From("user@example.com"),
            DisplayName.From("User"),
            PasswordHash.From("hash"),
            LanguageCode.From("en"),
            createdAt);
}
