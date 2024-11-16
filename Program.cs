using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=app.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()  
                  .AllowAnyMethod() 
                  .AllowAnyHeader();
        });
});


var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

    app.UseSwagger();
    app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/products", async (AppDbContext db) =>
{
    var products = await db.Products
    .OrderBy(x => x.Status)
    .ToListAsync();
    return Results.Ok(products);
});

app.MapGet("/products/{id}", async (int id, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.MapPost("/products", async (Product product, AppDbContext db) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{product.Id}", product);
});


app.MapPut("/products", async (Product updated, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(updated.Id);

    if(product == null)
        return Results.NotFound();


    product.Name = updated.Name;
    product.Status = updated.Status;

    await db.SaveChangesAsync();

    return Results.Created($"/products/{product.Id}", product);

});


app.MapDelete("/products/{id}", async (int id, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);

    if(product == null)
        return Results.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync();

    return Results.NoContent();
});


app.MapGet("/foo", () =>
{
    return "200";
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}


app.Run();
