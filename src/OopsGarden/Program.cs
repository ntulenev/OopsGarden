using OopsGarden.Endpoints;
using OopsGarden.Startup;

using var app = StartupHelpers.CreateApplication(args);

if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/error");
    _ = app.UseHsts();
}

_ = app.UseHttpsRedirection();
_ = app.UseDefaultFiles();
_ = app.UseStaticFiles();
_ = app.UseAuthentication();
_ = app.UseAuthorization();
_ = app.MapOopsGardenEndpoints();

await StartupHelpers.RunAppAsync(app).ConfigureAwait(false);
