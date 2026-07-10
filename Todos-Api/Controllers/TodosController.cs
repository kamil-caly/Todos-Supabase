using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Todos_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController(IConfiguration configuration) : ControllerBase
{
    private const string EnsureTodosTableSql = """
        create table if not exists public."Todos"
        (
            id bigserial primary key,
            created_at timestamptz not null default timezone('utc', now()),
            description text not null,
            done boolean not null default false
        );
        """;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TodoItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetAll(CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        var todos = await connection.QueryAsync<TodoItem>(new CommandDefinition(
            """
            select
                id as Id,
                description as Description,
                done as Done,
                created_at as CreatedAt
            from public."Todos"
            order by id;
            """,
            cancellationToken: cancellationToken));

        return Ok(todos);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(TodoItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoItem>> GetById(long id, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        var todo = await connection.QuerySingleOrDefaultAsync<TodoItem>(new CommandDefinition(
            """
            select
                id as Id,
                description as Description,
                done as Done,
                created_at as CreatedAt
            from public."Todos"
            where id = @Id;
            """,
            new { Id = id },
            cancellationToken: cancellationToken));

        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TodoItem), StatusCodes.Status201Created)]
    public async Task<ActionResult<TodoItem>> Create([FromBody] UpsertTodoRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        var todo = await connection.QuerySingleAsync<TodoItem>(new CommandDefinition(
            """
            insert into public."Todos" (description, done)
            values (@Description, @Done)
            returning
                id as Id,
                description as Description,
                done as Done,
                created_at as CreatedAt;
            """,
            new
            {
                request.Description,
                request.Done
            },
            cancellationToken: cancellationToken));

        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(TodoItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoItem>> Update(long id, [FromBody] UpsertTodoRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        var todo = await connection.QuerySingleOrDefaultAsync<TodoItem>(new CommandDefinition(
            """
            update public."Todos"
            set
                description = @Description,
                done = @Done
            where id = @Id
            returning
                id as Id,
                description as Description,
                done as Done,
                created_at as CreatedAt;
            """,
            new
            {
                Id = id,
                request.Description,
                request.Done
            },
            cancellationToken: cancellationToken));

        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(
            "delete from public.\"Todos\" where id = @Id;",
            new { Id = id },
            cancellationToken: cancellationToken));

        return rowsAffected == 0 ? NotFound() : NoContent();
    }

    private async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("Supabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Supabase' was not found.");
        }

        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(EnsureTodosTableSql, cancellationToken: cancellationToken));
        return connection;
    }

    public sealed class UpsertTodoRequest
    {
        public string? Description { get; init; }

        public bool Done { get; init; }
    }

    public sealed class TodoItem
    {
        public long Id { get; init; }

        public string? Description { get; init; }

        public bool Done { get; init; }

        public DateTimeOffset CreatedAt { get; init; }
    }
}
