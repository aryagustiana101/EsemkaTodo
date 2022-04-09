using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EsemkaTodo.Models;
using Microsoft.AspNetCore.Authorization;
using EsemkaTodo.Models.Filters;
using System.Text.Json;

namespace EsemkaTodo.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly EsemkaTodoContext _context;

        public TodoItemsController(EsemkaTodoContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItems>>> GetTodoItems([FromQuery] PaginationParameters @params)
        {
            string emailAuthenticate = HttpContext.User.Identity.Name;

            if (@params.Page == 0 || @params.ItemsPerPage == 0)
            {
                @params.Page = 1;
                @params.ItemsPerPage = 10;
            }

            var todoItems =  _context.TodoItems.Where(todoItems => todoItems.CreatedByEmail == emailAuthenticate);

            var paginationMetadata = new PaginationMetadata(todoItems.Count(), @params.Page, @params.ItemsPerPage);

            //Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var todoItemsPaging = await todoItems.Skip((@params.Page - 1) * @params.ItemsPerPage).Take(@params.ItemsPerPage).ToListAsync();

            return Ok(new { currentPage = paginationMetadata.CurrentPage, totalCount = paginationMetadata.TotalCount, totalPages = paginationMetadata.TotalPages, hasPrevious = paginationMetadata.HasPrevious, hasNext = paginationMetadata.HasNext, data = todoItemsPaging });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItems>> GetTodoItems(Guid id)
        {
            string emailAuthenticate = HttpContext.User.Identity.Name;

            var todoItems = await _context.TodoItems.Where(todoItems => todoItems.CreatedByEmail == emailAuthenticate && todoItems.Id == id).FirstOrDefaultAsync();

            if (todoItems == null)
            {
                return NotFound();
            }

            return todoItems;
        }

        [HttpPost]
        public async Task<ActionResult<TodoItems>> PostTodoItems(TodoItems todoItems)
        {
            string emailAuthenticate = HttpContext.User.Identity.Name;

            todoItems.Id = Guid.NewGuid();
            todoItems.CreatedByEmail = emailAuthenticate;

            _context.TodoItems.Add(todoItems);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TodoItemsExists(todoItems.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetTodoItems", new { id = todoItems.Id }, todoItems);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TodoItems>> PutTodoItems(Guid id, TodoItems todoItems)
        {
            string emailAuthenticate = HttpContext.User.Identity.Name;

            var todoItemsTarget = await _context.TodoItems.Where(_todoItems => _todoItems.CreatedByEmail == emailAuthenticate && _todoItems.Id == id).FirstOrDefaultAsync();

            if(todoItemsTarget == null)
            {
                return NotFound();
            }

            if(todoItems.Name != null)
            {
                todoItemsTarget.Name = todoItems.Name;
            }
            if (todoItems.Description != null)
            {
                todoItemsTarget.Description = todoItems.Description;
            }
            if(todoItems.IsComplete != null)
            {
                todoItemsTarget.IsComplete = todoItems.IsComplete;
            }
            if (todoItems.DueAt != null)
            {
                todoItemsTarget.DueAt = todoItems.DueAt;
            }
            if(todoItems.CompletedAt != null)
            {
                todoItemsTarget.CompletedAt = todoItems.CompletedAt;
            }
            await _context.SaveChangesAsync();

            var newTodoItems = await _context.TodoItems.Where(_todoItems => _todoItems.CreatedByEmail == emailAuthenticate && _todoItems.Id == id).FirstOrDefaultAsync();

            return newTodoItems;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItems(Guid id)
        {
            string emailAuthenticate = HttpContext.User.Identity.Name;

            var todoItems = await _context.TodoItems.Where(todoItems => todoItems.CreatedByEmail == emailAuthenticate && todoItems.Id == id).FirstOrDefaultAsync();

            if (todoItems == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItems);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TodoItemsExists(Guid id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }
    }
}
