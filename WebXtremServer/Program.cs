
using FmCndoeServer.Models;

var builder = WebApplication.CreateBuilder(args);


// 读取节点信息配置
var configuration = builder.Configuration;
var nodeInfo = configuration.GetSection("NodeInfo").Get<NodeInfo>() ;
if (nodeInfo != null)
    builder.Services.AddSingleton(nodeInfo);
else
    throw new Exception("配置文件不包含NodeInfo");
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseWebSockets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseSwagger();
////启用中间件服务对swagger-ui，指定Swagger JSON终结点
app.UseSwaggerUI();

app.Run();


