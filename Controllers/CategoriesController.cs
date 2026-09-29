using FrClassifier.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FrClassifier.Controllers;

[ApiController]
[Route("api/ifrs18-categories")]
public sealed class CategoriesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoryDto>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<CategoryDto>> Get() => Ok(CategoryContract.All);
}