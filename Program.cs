using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ToDoAPI.Data;
using ToDoAPI.Dtos;
using ToDoAPI.Models;

var builder = WebApplication.CreateBuilder(args);

string sConnection = builder.Configuration.GetConnectionString("SqliteConnection");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(sConnection));

//builder.Services.AddDbContext<AppDbContext>(opt =>
//    opt.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

var app = builder.Build();



//app.UseHttpsRedirection();

app.MapGet("api/getuser", async (AppDbContext context) =>
{
    var items = await context.Users.ToListAsync();

    return Results.Ok(items);
});

app.MapPost("api/login", async (AppDbContext context, UserLoginDTO userLoginDTO) =>
{

    using (var cmd = context.Database.GetDbConnection().CreateCommand())
    {
        cmd.CommandText = "LoginUser";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        if(cmd.Connection.State != System.Data.ConnectionState.Open)
        {
            cmd.Connection.Open();
        }
        var username = new SqlParameter("@Username", userLoginDTO.UserName);
        var password = new SqlParameter("@Password", userLoginDTO.Password);
        cmd.Parameters.Add(username);
        cmd.Parameters.Add(password);
        var read = cmd.ExecuteReader();
        while (read.Read())
        {
            User user = new User();
            user.Username = userLoginDTO.UserName;
            user.Password = userLoginDTO.Password;
            if (read[0].ToString().Equals(user.Username) && read[1].ToString().Equals(user.Password))
            {
                await context.Users.AddAsync(user);
                
                return Results.Ok(userLoginDTO);
            }
            
        }
    }
    return Results.BadRequest("Sai tai khoan hoac mat khau");
    
});

app.MapPut("api/todo/{id}", async (AppDbContext context, int id, ToDo toDo) => {

    var toDoModel = await context.ToDos.FirstOrDefaultAsync(t => t.Id == id);

    if (toDoModel == null)
    {
        return Results.NotFound();
    }

    toDoModel.ToDoName = toDo.ToDoName;

    await context.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("api/todo/{id}", async (AppDbContext context, int id) => {

    var toDoModel = await context.ToDos.FirstOrDefaultAsync(t => t.Id == id);

    if (toDoModel == null)
    {
        return Results.NotFound();
    }

    context.ToDos.Remove(toDoModel);

    await context.SaveChangesAsync();

    return Results.NoContent();

});


app.Run();
