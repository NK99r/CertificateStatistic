using CertificateStatisticAPI.Tools;
using DailyApp.API.AutoMappers;
using SqlSugar;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//����ע������
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<SqlSugarScope>(factory => SqlSugarContext.DB);
//AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperSettings));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
