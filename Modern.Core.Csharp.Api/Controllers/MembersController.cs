using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Modern.Core.Csharp.Api.Controllers
{
    public record CreateMemberRequest(
        string MemberId,
        string FirstName,
        string LastName,
        DateOnly DateOfBirth,
        string Gender,
        string MemberStatus = "INCOMPLETE"
    );

    [ApiController]
    [Route("api/[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly string _connectionString;
        private static readonly string[] AllowedStatuses = { "ACTIVE", "INACTIVE", "INCOMPLETE" };

        public MembersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("QnxtCefConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        [HttpPost]
        public async Task<IActionResult> CreateMember([FromBody] CreateMemberRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MemberId) ||
                string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest(new { message = "MemberId, FirstName, and LastName are required." });
            }

            var normalizedStatus = string.IsNullOrWhiteSpace(request.MemberStatus)
                ? "INCOMPLETE"
                : request.MemberStatus.Trim().ToUpperInvariant();

            if (!AllowedStatuses.Contains(normalizedStatus))
            {
                return BadRequest(new { message = "MemberStatus must be ACTIVE, INACTIVE, or INCOMPLETE." });
            }

            const string sql = @"
                INSERT INTO public.member (member_id, first_name, last_name, date_of_birth, gender, member_status)
                VALUES (@MemberId, @FirstName, @LastName, @DateOfBirth, @Gender, @MemberStatus);";

            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.ExecuteAsync(sql, new
                {
                    MemberId = request.MemberId.Trim(),
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    DateOfBirth = request.DateOfBirth.ToDateTime(TimeOnly.MinValue),
                    Gender = request.Gender.Trim().ToUpperInvariant(),
                    MemberStatus = normalizedStatus
                });

                return CreatedAtAction(nameof(GetMemberById), new { id = request.MemberId }, request);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // Unique violation
            {
                return Conflict(new { message = $"Member ID '{request.MemberId}' already exists." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMemberById(string id)
        {
            const string sql = @"
                SELECT 
                    member_id AS MemberId,
                    first_name AS FirstName,
                    last_name AS LastName,
                    date_of_birth AS DateOfBirth,
                    gender AS Gender,
                    member_status AS MemberStatus
                FROM public.member
                WHERE member_id = @MemberId;";

            await using var connection = new NpgsqlConnection(_connectionString);
            var member = await connection.QuerySingleOrDefaultAsync(sql, new { MemberId = id });

            if (member is null)
                return NotFound(new { message = $"Member '{id}' not found." });

            return Ok(member);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllMembers()
        {
            const string sql = @"
        SELECT 
            member_id AS MemberId,
            first_name AS FirstName,
            last_name AS LastName,
            date_of_birth AS DateOfBirth,
            gender AS Gender,
            member_status AS MemberStatus
        FROM public.member
        ORDER BY created_utc DESC;";

            await using var connection = new NpgsqlConnection(_connectionString);
            var members = await connection.QueryAsync(sql);
            return Ok(members);
        }
    }
}
