using Microsoft.AspNetCore.Mvc;
using Northwind.Common.EntityModels.SqlServer;
using Northwind.WebApi.Repositories;

namespace Northwind.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomersController : Controller
{
    private readonly ICustomerRepository _repository;

    public CustomersController(ICustomerRepository repository)
    {
        _repository = repository;
    }

    //GetCustomers(string? country)
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Customer>))]
    public async Task<IEnumerable<Customer>> GetCustomers(string? country)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return await _repository.RetrieveAllAsync();
        }

        return (await _repository.RetrieveAllAsync()).Where(c => c.Country == country);
    }

    //GetCustomer(id)
    [HttpGet("{id}", Name = nameof(GetCustomer))]
    [ProducesResponseType(200, Type = typeof(Customer))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCustomer(string id)
    {
        Customer? customer = await _repository.RetrieveAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    //Create
    //716 pagina
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(Customer))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Customer customer)
    {
        //null check -> Bad Request
        if (customer == null)
        {
            return BadRequest();
        }

        Customer? addedCustomer = await _repository.CreateAsync(customer);

        if (addedCustomer == null)
        {
            return BadRequest("Something went to create a customer from the repository ");
        }
        else
        {
            return CreatedAtRoute(
                routeName: nameof(GetCustomer),
                routeValues: new {id = addedCustomer.CustomerId.ToLower()},
                value: addedCustomer);
        }
    }

    //Update
    //716
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(string id, [FromBody] Customer customer)
    {
        id = id.ToUpper();
        customer.CustomerId = customer.CustomerId.ToUpper();

        if (customer == null || customer.CustomerId != id) return BadRequest();

        Customer? existing = await _repository.RetrieveAsync(id);
        if (existing == null) return NotFound();

        await _repository.UpdateAsync(id, customer);
        return new NoContentResult(); // 204
    }

    //Delete
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(string id)
    {
        if (id == "bad")
        {
            ProblemDetails problemDetails = new()
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://localhost:5001/customers/failed-to-delete",
                Title = $"Customer ID {id} found but failed to delete.",
                Detail = "More details like Company Name, Country and so on.",
                Instance = HttpContext.Request.Path
            };
            return BadRequest(problemDetails); // 400 Bad Request
        }

        Customer? existing = await _repository.RetrieveAsync(id);
        if (existing == null) return NotFound();

        bool? deleted = await _repository.DeleteAsync(id);

        if (deleted.HasValue && deleted.Value)
        {
            return new NoContentResult();
        }
        else
        {
            return BadRequest($"Customer {id} was found but failed to delete.");
        }
    }
}