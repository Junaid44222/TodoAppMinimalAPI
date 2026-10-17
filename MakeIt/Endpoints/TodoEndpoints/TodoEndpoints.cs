using System;
using MakeIt.Data;
using MakeIt.Dtos.TodoDtos;
using MakeIt.Mappings;
using Microsoft.EntityFrameworkCore;

namespace MakeIt.Endpoints.TodoEndpoints;

public static class TodoEndpoints
{
    private const string GroupName = "todos";

    public static RouteGroupBuilder MapTodoEndpoints(this WebApplication app)
    {
        var group = app
                    .MapGroup(GroupName)
                    .WithParameterValidation();

        // get all todos
        group.MapGet("/", GetTodos);

        // Create todo in database
        group.MapPost("/", CreateTodo);

        // put
        group.MapPut("/{id}", UpdateTodo);

        // delete
        group.MapDelete("/{id}", DeleteTodo);

        return group;
    }

    private static async Task<IResult> GetTodos(MakeItContext dbContext)
    {
        var result = await dbContext.Todos
                        .Select(todo => todo.ToDto())
                        .AsNoTracking()
                        .ToListAsync();

        return Results.Ok(result);
    }

    private static async Task<IResult> CreateTodo(MakeItContext dbContext, CreateTodoDto createTodoDto)
    {
        var newTodo = createTodoDto.ToEntity();

        await dbContext.Todos.AddAsync(newTodo);
        await dbContext.SaveChangesAsync();


        TodoDto returnTodo = newTodo.ToDto();

        return Results.Created(uri: (string?)null, value: returnTodo);

    }

    private static async Task<IResult> UpdateTodo(int id, MakeItContext dbContext, UpdateTodoDto updateTodoDto)
    {
        var existingTodo = await dbContext.Todos.FindAsync(id);

        if (existingTodo == null)
        {
            return Results.NoContent();
        }

        dbContext.Entry(existingTodo)
                .CurrentValues
                .SetValues(updateTodoDto.ToEntity(id, existingTodo.CreatedAt));

        await dbContext.SaveChangesAsync();

        return Results.Ok();
    }

    private static async Task<IResult> DeleteTodo(int id, MakeItContext dbContext)
    {
        await dbContext.Todos
            .Where(todo => todo.Id == id)
            .ExecuteDeleteAsync();

        return Results.NoContent();
    }

}
