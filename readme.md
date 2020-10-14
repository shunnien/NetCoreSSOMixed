# NetCore 網站與 MVC 網站共用驗證

## 測試環境

Repository 內有三個專案

1. Mvc5 這是 MVC 5 網站，.net framework 4.6
1. NetCoreWeb 這是 dotnet core 3.1 網站
1. WebApplication1 這是 MVC 5 網站，.net framework 4.6

官方文件中說明使用 **.net framework 4.5.1+** 就可以共用，但是實際範例使用上，套件參考一直有問題，所以改用 **.net framework 4.6** 進行測試
採用 **AspNet.Identity.Core** 進行驗證共用

## MVC 網站的設定

1. 在 MVC 網站內確認 **.net framework** 為 **4.6** 版本
1. 更新 MVC 網站的 Microsoft.Owin 到對應 **.net framework4.6** 的版本
1. 在 MVC 網站內的 **Startup.Auth.cs** 變動 **UseCookieAuthentication** 的設定，如下所示

    ``` csharp
    app.UseCookieAuthentication(new CookieAuthenticationOptions
    {
        AuthenticationType = "Identity.Application",
        CookieName = ".AspNet.SharedCookie",
        LoginPath = new PathString("/Account/Login"),
        Provider = new CookieAuthenticationProvider
        {
            OnValidateIdentity =
                SecurityStampValidator
                    .OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) =>
                            user.GenerateUserIdentityAsync(manager))
        },
        TicketDataFormat = new AspNetTicketDataFormat(
            new DataProtectorShim(
                DataProtectionProvider.Create("{PATH TO COMMON KEY RING FOLDER}",
                    (builder) => { builder.SetApplicationName("SharedCookieApp"); })
                .CreateProtector(
                    "Microsoft.AspNetCore.Authentication.Cookies." +
                        "CookieAuthenticationMiddleware",
                    "Identity.Application",
                    "v2"))),
        CookieManager = new ChunkingCookieManager()
    });

    System.Web.Helpers.AntiForgeryConfig.UniqueClaimTypeIdentifier =
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
    ```

1. 在 MVC 網站內的 **IdentityModels.cs** 的設定，如下所示

    ``` csharp
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(
            UserManager<ApplicationUser> manager)
        {
            // The authenticationType must match the one defined in 
            // CookieAuthenticationOptions.AuthenticationType
            var userIdentity = 
                await manager.CreateIdentityAsync(this, "Identity.Application");

            // Add custom user claims here

            return userIdentity;
        }
    }
    ```

## dotnet core 網站的設定

在 **Startup.cs** 檔案中的 **ConfigureServices** 方法，添加以下的設定

``` csharp
services.AddDataProtection()
    .PersistKeysToFileSystem("{PATH TO COMMON KEY RING FOLDER}")
    .SetApplicationName("SharedCookieApp");

services.ConfigureApplicationCookie(options => {
    options.Cookie.Name = ".AspNet.SharedCookie";
});
```

## 參考資料

1. [Microsoft Docs cookie在 ASP.NET apps 之間共用驗證](https://docs.microsoft.com/zh-tw/aspnet/core/security/cookie-sharing?view=aspnetcore-3.1#share-authentication-cookies-with-aspnet-core-identity)
1. [Microsoft Docs Configure ASP.NET Core Data Protection](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-3.1)
1. [Katana 專案概觀](https://docs.microsoft.com/zh-tw/aspnet/aspnet/overview/owin-and-katana/an-overview-of-project-katana)
