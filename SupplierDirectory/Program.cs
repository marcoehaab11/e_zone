using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SupplierDirectory.Domain;
using SupplierDirectory.Infrastructure;

Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("logs/supplier-.log", rollingInterval: RollingInterval.Day).CreateBootstrapLogger();
try {
 var builder=WebApplication.CreateBuilder(args); builder.Host.UseSerilog((ctx,_,cfg)=>cfg.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console().WriteTo.File("logs/supplier-.log",rollingInterval:RollingInterval.Day));
 var dataProtectionPath=Path.Combine(builder.Environment.ContentRootPath,"App_Data","DataProtection-Keys"); Directory.CreateDirectory(dataProtectionPath); builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath)).SetApplicationName("SupplierDirectory");
 builder.Services.AddDbContext<AppDbContext>(o=>o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sql=>sql.EnableRetryOnFailure(maxRetryCount:3,maxRetryDelay:TimeSpan.FromSeconds(5),errorNumbersToAdd:null)));
 builder.Services.AddIdentity<ApplicationUser,IdentityRole>(o=> { o.Password.RequiredLength=10; o.Password.RequireDigit=true; o.Password.RequireUppercase=true; o.Password.RequireNonAlphanumeric=true; }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders(); builder.Services.ConfigureApplicationCookie(o => { o.LoginPath = "/login"; o.LogoutPath = "/logout"; o.AccessDeniedPath = "/login"; });
 var jwt=builder.Configuration.GetSection("Jwt"); var key=jwt["Key"] ?? throw new InvalidOperationException("JWT key is required.");
 builder.Services.AddAuthentication(o=> {o.DefaultAuthenticateScheme=IdentityConstants.ApplicationScheme;o.DefaultChallengeScheme=IdentityConstants.ApplicationScheme;}).AddJwtBearer(o=>o.TokenValidationParameters=new(){ValidateIssuer=true,ValidIssuer=jwt["Issuer"],ValidateAudience=true,ValidAudience=jwt["Audience"],ValidateIssuerSigningKey=true,IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),ValidateLifetime=true});
 builder.Services.AddAuthorization(); builder.Services.AddControllersWithViews(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true); builder.Services.AddRazorPages(); builder.Services.AddScoped<IFileStorageService,LocalFileStorageService>(); builder.Services.AddRateLimiter(o=>o.AddFixedWindowLimiter("api",x=>{x.PermitLimit=100;x.Window=TimeSpan.FromMinutes(1);}));
 builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen(o=> {o.SwaggerDoc("v1",new OpenApiInfo{Title="Supplier Directory API",Version="v1"});o.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme{Type=SecuritySchemeType.Http,Scheme="bearer",BearerFormat="JWT"});o.AddSecurityRequirement(new OpenApiSecurityRequirement{{new OpenApiSecurityScheme{Reference=new OpenApiReference{Type=ReferenceType.SecurityScheme,Id="Bearer"}},[]}});});
 var app=builder.Build(); app.UseSerilogRequestLogging(); app.UseExceptionHandler(e=>e.Run(async c=>{c.Response.StatusCode=500;if(c.Request.Path.StartsWithSegments("/api")) await c.Response.WriteAsJsonAsync(new {success=false,message="حدث خطأ غير متوقع"});else c.Response.Redirect("/Home/Error");})); if(!app.Environment.IsDevelopment()) app.UseHsts(); app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseSwagger(); app.UseSwaggerUI(); app.UseRouting(); app.UseRateLimiter(); app.UseAuthentication(); app.UseAuthorization(); app.Use(async (ctx,next)=> {ctx.Response.Headers.Append("X-Content-Type-Options","nosniff");ctx.Response.Headers.Append("X-Frame-Options","SAMEORIGIN");await next();}); app.MapControllers().RequireRateLimiting("api"); app.MapRazorPages(); app.MapControllerRoute("default","{controller=Dashboard}/{action=Index}/{id?}");
 using(var scope=app.Services.CreateScope()) { var db=scope.ServiceProvider.GetRequiredService<AppDbContext>(); await db.Database.MigrateAsync(); await SeedData.InitializeAsync(scope.ServiceProvider,builder.Configuration); } await app.RunAsync();
} catch(Exception ex) when (ex is not Microsoft.Extensions.Hosting.HostAbortedException) { Log.Fatal(ex,"Application terminated unexpectedly"); } finally { await Log.CloseAndFlushAsync(); }


