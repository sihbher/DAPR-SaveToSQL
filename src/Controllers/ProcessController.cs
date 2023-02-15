using DaprSaveToSql.Entities;
using DaprSaveToSql.Repository;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DaprSaveToSql.Controllers;

[ApiController]
[Route("[controller]")]
public class ProcessController : ControllerBase
{
    private readonly ILogger<ProcessController> _logger;

    public ProcessController(ILogger<ProcessController> logger)
    {
        _logger = logger;
    }


    [HttpPost("/Process")]
    public async Task<IActionResult> Run([FromBody] object body)
    {
        _logger.LogInformation("Received request");
        _logger.LogInformation(body.ToString());

        try
        {
            await InsertOrUpdateSQL(body);
            return Ok();
        }
        catch (Exception ex)
        {
            //in case of error, log it and return 500, this way the message will be retried
            _logger.LogError(ex, $"Error while processing message. Error: {ex.ToString()}");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);

        }
    }

    private async Task<bool> InsertOrUpdateSQL(object body)
    {
        //deserealize json to Telemetry object
        var order = JsonConvert.DeserializeObject<Orders>(body.ToString() ?? throw new ArgumentNullException("body is null"));
        //Get connection string from environment variable
        string connectionString= Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING") ?? throw new ArgumentNullException("SQL_CONNECTION_STRING"); 
        OrdersRepository ordersRepository = new OrdersRepository(connectionString);

        if(order == null)
            throw new ArgumentNullException("order is null");

        return await ordersRepository.AddOrUpdate(order);
    }
}
