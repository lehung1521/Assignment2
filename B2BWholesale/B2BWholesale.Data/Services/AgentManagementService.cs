using System.Text.RegularExpressions;
using B2BWholesale.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.Data.Services;

public sealed class AgentManagementService : IAgentManagementService
{
    private static readonly Regex PhonePattern = new(@"^[0-9]{9,11}$", RegexOptions.Compiled);
    private readonly AppDbContext _dbContext;

    public AgentManagementService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyList<AgentRecord> GetAgents() => _dbContext.Agents
        .AsNoTracking()
        .OrderBy(agent => agent.AgentName)
        .Select(agent => ToRecord(agent))
        .ToList();

    public AgentRecord? GetAgent(int agentId)
    {
        var agent = _dbContext.Agents.AsNoTracking().FirstOrDefault(x => x.AgentID == agentId);
        return agent is null ? null : ToRecord(agent);
    }

    public AgentRecord CreateAgent(AgentInput input)
    {
        var values = Validate(input, null);
        var agent = new Agent
        {
            AgentName = values.AgentName,
            Address = values.Address,
            Phone = values.Phone
        };
        _dbContext.Agents.Add(agent);
        SaveAgent("An agent with this phone number already exists or the agent violates a database constraint.");
        return ToRecord(agent);
    }

    public AgentRecord UpdateAgent(int agentId, AgentInput input)
    {
        var agent = _dbContext.Agents.FirstOrDefault(x => x.AgentID == agentId)
            ?? throw new KeyNotFoundException($"Agent {agentId} was not found.");
        var values = Validate(input, agentId);
        agent.AgentName = values.AgentName;
        agent.Address = values.Address;
        agent.Phone = values.Phone;
        SaveAgent("The agent could not be saved because the phone is already registered or a database constraint was violated.");
        return ToRecord(agent);
    }

    public void DeleteAgent(int agentId)
    {
        var agent = _dbContext.Agents.FirstOrDefault(x => x.AgentID == agentId)
            ?? throw new KeyNotFoundException($"Agent {agentId} was not found.");
        _dbContext.Agents.Remove(agent);
        try
        {
            _dbContext.SaveChanges();
        }
        catch (DbUpdateException)
        {
            throw new ServiceValidationException("This agent cannot be deleted because the agent has order history.");
        }
    }

    private (string AgentName, string Address, string Phone) Validate(AgentInput input, int? currentAgentId)
    {
        ArgumentNullException.ThrowIfNull(input);
        var name = input.AgentName?.Trim() ?? string.Empty;
        var address = input.Address?.Trim() ?? string.Empty;
        var phone = input.Phone?.Trim() ?? string.Empty;

        if (name.Length == 0 || name.Length > 100)
        {
            throw new ServiceValidationException("Agent name is required and must be at most 100 characters.");
        }

        if (address.Length == 0 || address.Length > 200)
        {
            throw new ServiceValidationException("Agent address is required and must be at most 200 characters.");
        }

        if (!PhonePattern.IsMatch(phone))
        {
            throw new ServiceValidationException("Phone must contain 9 to 11 digits.");
        }

        if (_dbContext.Agents.Any(x =>
            x.Phone == phone && (!currentAgentId.HasValue || x.AgentID != currentAgentId.Value)))
        {
            throw new ServiceValidationException("An agent with this phone number already exists.");
        }

        return (name, address, phone);
    }

    private void SaveAgent(string constraintMessage)
    {
        try
        {
            _dbContext.SaveChanges();
        }
        catch (DbUpdateException)
        {
            throw new ServiceValidationException(constraintMessage);
        }
    }

    private static AgentRecord ToRecord(Agent agent) =>
        new(agent.AgentID, agent.AgentName, agent.Address, agent.Phone);
}
